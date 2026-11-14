# P1 — CorelDRAW Entegrasyon Planı

Son güncelleme: 2026-11-14
Durum: **UYGULANDI** — V4 smoke PASS (CorelDRAW 27.2.0.135); ampirik sonuçlar §8'de.

Bu belge, P1 dikey diliminin CorelDRAW tarafını bağlar. Geometri çekirdeği
(`Corelad.Geometry`) CorelDRAW'dan bağımsızdır ve burada değişmez (ADR-0004).

---

## 1. Kapsam

Tek bir **seçili `cdrCurveShape`** üzerinde:

1. Şeklin alt konturlarını (subpath) oku → POCO (`ContourSet`).
2. Saf geometri çekirdeğinde containment analizini çalıştır.
3. Her mantıksal harf adayını (dış gövde + containment ile bağlı delikler) **yeni bir
   şekil** olarak yaz.
4. Tüm yazma işlemini **tek undo grubunda** topla.
5. Kaynak şekli değiştirme, silme, gizleme veya belgeyi kaydetme.

P1'de **yok:** Nesting, önizleme UI'si, renk/stil ataması, otomatik ayrık parça bağlama,
grup/PowerClip/bitmap işleme, addon kurulumu.

---

## 2. Okunacak nesneler (yalnızca okuma)

| Nesne | Erişim | Kullanım |
|---|---|---|
| `Application.ActiveDocument` | Okuma | Belge birimi, `FromUnits`/`ToUnits`, undo |
| `Document.ActivePage` / `Document.ActiveLayer` | Okuma | Yeni şekillerin yazılacağı katman |
| `Document.Unit` | Okuma | Belge birimi (mm'ye dönüşüm için) |
| `Shape` (seçili) | Okuma | `Type`, `Curve` |
| `Shape.Curve` → `SubPaths` | **Salt okuma** | Alt kontur listesi (1 tabanlı) |
| `SubPath.Closed` | Okuma | Kapalılık; kapalı değilse hata, onarım yok |
| `SubPath.Segments` | **Salt okuma** | Segment zinciri |
| `Segment.Type` | Okuma | `cdrLineSegment` / `cdrCurveSegment` |
| `Segment.StartNode` / `EndNode` | Okuma | Uç noktalar |
| `Segment.StartingControlPointX/Y`, `EndingControlPointX/Y` | Okuma | Kübik kontrol noktaları |

**Dokunulmayanlar:** kaynak şeklin geometrisi, dolgusu, konturu, konumu, katmanı, adı;
diğer şekiller; sayfa ayarları; belge dosyası.

Okuma sırasında hiçbir `Set`/`Delete`/`ConvertToCurves` çağrısı yapılmaz. Kaynak şekil
`cdrCurveShape` değilse `UnsupportedObject` ile **hiç yazmadan** dönülür.

### 2.1 Koordinat ve birim

- CorelDRAW koordinatları belge birimindedir; kanonik çekirdek birimi **mm**'dir (ADR-0005).
- Okuma: `mm = Document.FromUnits(value, cdrUnit.cdrMillimeter)`.
- Yazma: `value = Document.ToUnits(mm, cdrUnit.cdrMillimeter)`.
- Y ekseni CorelDRAW'da da yukarı pozitiftir; eksen çevrimi yapılmaz.
- Yuvarlama yapılmaz; `double` aynen taşınır.

### 2.2 Kapalı kontur kuralı

- CorelDRAW kapalı bir subpath'te kapanış segmentini açıkça listelemeyebilir.
- Okuyucu: son segmentin bitiş noktası başlangıca eşit değilse ve `Closed = true` ise
  **düz çizgi** kapanış segmenti ekler. Bu, Corel'in "kapalı" semantiğinin doğal
  karşılığıdır; geometri onarımı değildir.
- `Closed = false` kontur çekirdeğe `IsClosed = false` olarak girer ve `OpenContour`
  (Error) üretir; **otomatik kapatılmaz**.

---

## 3. Yazılacak nesneler (yalnızca yeni)

| Nesne | Çağrı | Not |
|---|---|---|
| `Document.CreateCurveFromArray` | Yeni `Curve` oluşturur | `CurveElement[]` ile tam bezier |
| `Layer.CreateCurve(curve)` | Yeni `Shape` oluşturur | Kaynağa bağlı değil |
| `Document.BeginCommandGroup` / `EndCommandGroup` | Undo grubu | Tek `Ctrl+Z` |

- Her aday **tek şekil** olarak yazılır: dış gövde + delikleri aynı `Curve` içinde
  ayrı subpath'lerdir. Böylece harf, CorelDRAW'da tek nesne olarak taşınır.
- Geometri **orijinal bezier**'dir; düzleştirilmiş poligon yalnızca analiz kopyasıdır ve
  asla belgeye yazılmaz.
- Yazma, kaynakla aynı koordinatlarda yapılır (P1'de kaydırma yok). Kaynak şekil
  görünür kalır; kullanıcı isterse elle siler (tek undo ile geri alınabilir).
- Yazma öncesi `Curve` nesnesi doğrulanır: subpath sayısı, `Closed` ve segment sayısı
  beklenenle eşleşmezse **hiçbir şekil belgeye eklenmez**, açık hata döner.

### 3.1 Undo sözleşmesi

```
BeginCommandGroup("Corelad Smart Divide")
  ... N adet Layer.CreateCurve ...
EndCommandGroup()
```

- Hata olursa `Undo(1)` ile grup geri alınır ve hata yukarı taşınır.
- Başarıda kullanıcı tek `Ctrl+Z` ile **tüm** yeni şekilleri kaldırır.
- `Document.Undo(1)`'in grup davranışı smoke testte ampirik doğrulanır.

---

## 4. Belge koruma adımları

1. **Save/SaveAs/SaveAsCopy çağrılmaz.** P1 kodunda bu metotlar hiç kullanılmaz.
2. Kaynak şekil ve diğer nesneler üzerinde yazma çağrısı yoktur.
3. Tüm belge değişiklikleri tek command group içindedir; iptal/başarısızlıkta `Undo(1)`.
4. Smoke test ayrı bir **yeni belge** açar; iş sonunda `Document.Dirty = false` yapılır ve
   `Document.Close()` ile **kaydetmeden** kapatılır.
5. Smoke test çalıştırılmadan önce kullanıcının açık belgesi varsa: yeni belge
   `Application.CreateDocument()` ile ayrı pencerede açılır; mevcut belgeye dokunulmaz.
6. Smoke harness `Application.Quit()` **çağırmaz**; yalnızca kendi açtığı belgeyi kapatır.
   Böylece kullanıcının açık CorelDRAW oturumu sonlanmaz.

> Not: CorelDRAW uygulaması zaten açıksa `new Application()` mevcut örneğe bağlanır
> (COM single-instance). Bu nedenle smoke harness **kendi açtığı belgeyi** izler ve
> yalnızca onu kapatır.

---

## 5. Smoke doğrulama senaryosu (V4)

1. CorelDRAW 2026 x64 başlat (veya mevcut örneğe bağlan).
2. Yeni belge, birim = mm.
3. GDI+ Arial `MUSTAFA` dış konturlarını (9 subpath) `CreateCurveFromArray` +
   `Layer.CreateCurve` ile **kaynak** olarak yaz.
4. Kaynağı geri oku; segment sayısı, kontrol noktaları ve kapalılık round-trip eşleşmeli
   (tolerans: `ComparisonEpsilonMm`).
5. Kaynak imzasını (subpath/segment/nokta özeti) al.
6. `SmartDivideService.Run(doc, source)` çağır.
7. Beklenen: 7 aday, 2 delik, 0 Error; 7 yeni şekil; kaynak imzası **değişmedi**.
8. `Document.Undo(1)`: yeni 7 şekil gitti, kaynak duruyor.
9. Belgeyi kaydetmeden kapat.

Her adım konsola `PASS`/`FAIL` + ölçülen değerlerle yazılır. Sonuç etiketi:
`[DOĞRULANDI: CorelDRAW v27.2]` yalnızca tüm adımlar geçerse kullanılır; aksi halde
`[DOĞRULANMADI]` ve hata ayrıntısı `docs/VERIFICATION.md`'ye işlenir.

---

## 6. Doğrulanmamış varsayımlar (smoke ile test edilecek)

| # | Varsayım | Doğrulama |
|---|---|---|
| A1 | `CreateCurveFromArray` + `cdrElementStart/Line/Curve/Control` dizisi tam bezier üretir | Round-trip kontrol noktaları |
| A2 | `Layer.CreateCurve` çizimi aktif sayfaya ekler | Şekil sayısı |
| A3 | `BeginCommandGroup`/`EndCommandGroup` tek undo üretir | `Undo(1)` sonrası sayım |
| A4 | Kapalı subpath `Closed = true` döner | Okuma kontrolü |
| A5 | `Document.FromUnits/ToUnits` mm dönüşümü | Uzunluk karşılaştırması |
| A6 | `new Application()` çalışan örneğe bağlanır | Bağlantı testi |

A1–A6'dan biri başarısız olursa kod "düzeltilmiş gibi" davranmaz; smoke FAIL verir ve
alternatif (ör. `CreateSubPath` + `AppendCurveSegment2`) ayrı ADR/plan ile değerlendirilir.

---

## 7. Dosya sorumlulukları

```
src/Corelad.Dock/Corel/CorelUnits.cs          mm <-> belge birimi
src/Corelad.Dock/Corel/CorelTolerances.cs     round-trip / imza toleransları
src/Corelad.Dock/Corel/CorelCurveReader.cs    Shape.Curve -> ContourSet (salt okuma)
src/Corelad.Dock/Corel/CorelCurveWriter.cs    ContourSet -> yeni Curve (round-trip doğrulamalı)
src/Corelad.Dock/Corel/CorelShapeSnapshot.cs  kaynak değişmedi doğrulaması
src/Corelad.Dock/Corel/SmartDivideService.cs  analiz + undo grubu + sonuç
src/Corelad.Dock/Corel/SmartDivideRunResult.cs sonuç/rapor modeli
tools/Corelad.SmokeTest/                      smoke harness + --probe (net48 x64 STA)
```

`Corelad.Dock` içinde containment/NFP/offset algoritması yazılmaz; yalnızca COM ↔ POCO,
undo ve raporlama vardır (AGENTS.md §2).

---

## 8. Uygulama sonrası ampirik sonuçlar (CorelDRAW v27.2.0.135)

Probe (`Corelad.SmokeTest.exe --probe`) ve smoke çalıştırmasının sonuçları:

| # | Bulgu | Sonuç |
|---|---|---|
| A1 | `CreateCurveFromArray` + `cdrElementStart/Line/Curve/Control` tam bezier üretir | **DOĞRU** — round-trip kontrol noktaları eşleşti |
| A2 | `CreateCurveFromArray` kapalı subpath üretir | **YANLIŞ** — `Closed=false`; yazıcı `SubPath.Closed = true` atar |
| A2b | `Closed=true`, başlangıç = bitiş ise | Yinelenen uç düğümü atılır, segment sayısı korunur |
| A2c | `Closed=true`, başlangıç ≠ bitiş ise | Düz çizgi kapanış segmenti eklenir |
| A3 | `Layer.CreateCurve` + `BeginCommandGroup`/`EndCommandGroup` tek undo | **DOĞRU** — `Undo(1)` 7 şekli kaldırdı |
| A4 | Çizgi segmentinde kontrol noktası okuma | COM hatası; yalnızca `cdrCurveSegment` için okunur |
| A5 | `FromUnits/ToUnits` mm dönüşümü | **DOĞRU** — belge birimi mm iken kimlik; round-trip 1e-3 mm içinde |
| A6 | `new Application()` | **BAŞARISIZ** — interop coclass CLSID'i kayıtlı değil; `CorelDRAW.Application.27` ProgID'si kullanıldı |

**Smoke sonucu:** 7 aday, 2 delik, 0 Error; 7 yeni şekil; kaynak imzası değişmedi;
`Undo(1)` yeni nesneleri kaldırdı; belge kaydedilmeden kapandı. Ayrıntı: `docs/VERIFICATION.md` §5.1.
