# ARCHITECTURE — Corelad

## 1. Genel bakış

```
┌─────────────────────────────────────────────────────────────────────┐
│ CorelDRAW 2026 v27.2 (x64) — ana süreç, STA/UI thread                 │
│                                                                     │
│  Corelad.Dock (net48, WPF, addon)                                   │
│   ├─ Addon.cs            : giriş noktası, UI kaydı                   │
│   ├─ Corel/              : COM ↔ POCO dönüşümü, undo grubu          │
│   ├─ ViewModels/         : MVCVM, komutlar, durum                   │
│   └─ Views/              : XAML dock                                │
│                                                                     │
│  UI thread: yalnızca COM okuma/yazma + görsel güncelleme            │
└───────────────┬─────────────────────────────────────────────────────┘
                │  POCO (saf veri, kopya)
                ▼
┌─────────────────────────────────────────────────────────────────────┐
│ Corelad.Geometry (netstandard2.0) — CorelDRAW bağımsız              │
│  Model/        : BezierSegment, Contour, ContourSet, Part, Sheet    │
│  Analysis/     : containment ağacı, tessellation, doğrulama         │
│  Clipping/     : Clipper2 sarmalayıcı (offset, boolean, temizleme)  │
│  Nesting/      : NFP üretimi, yerleştirici, arama, doğrulayıcı      │
│  Diagnostics/  : Problem, Severity, sonuç raporları                 │
└───────────────┬─────────────────────────────────────────────────────┘
                ▲
                │ referans (yalnız test)
┌───────────────┴─────────────────────────────────────────────────────┐
│ tests/Corelad.Geometry.Tests (net8, xUnit)                          │
│  CorelDRAW açmadan çalışır; golden fixture + özellik testleri        │
└─────────────────────────────────────────────────────────────────────┘
```

**Değişmez kural:** `Corelad.Geometry` içinde `Corel.Interop.*`, `System.Windows`, `PresentationCore`
referansı bulunmaz. Bu, çekirdeğin test edilebilirliğini ve taşınabilirliğini korur.

## 2. Katman sorumlulukları

| Katman | Yapar | Yapmaz |
|---|---|---|
| `Corelad.Dock` | Seçim okuma, POCO üretme, önizleme çizimi, undo grubu, ilerleme/iptal, ayar saklama | Geometri algoritması, NFP, containment kararı |
| `Corelad.Geometry` | Bezier modeli, containment, tessellation, offset/boolean, NFP, arama, doğrulama | COM, UI, dosya sistemi, Corel kavramları |
| Testler | Sentetik doğrulama, özellik testleri, regresyon | Gerçek CorelDRAW davranışı (o ayrı seviye) |

## 3. Veri modeli (P1'de kodlandı)

```
Point2        { double X, Y }                        // mm
BezierSegment { Point2 P0, P1, P2, P3, SegmentKind } // kübik; Kind = Line ise P1=P0, P2=P3
Contour       { IReadOnlyList<BezierSegment> Segments, bool IsClosed,
                ContourRole Role /* Outer | Hole | Unknown */,
                int SourceShapeId, int SourceSubPathIndex }
ContourSet    { IReadOnlyList<Contour> Contours }    // bir nesnenin tüm alt konturları
Part          { int Id, ContourSet Boundary, IReadOnlyList<ContourSet> Holes,
                Matrix2D Transform, bool IsLocked, string GroupKey }
Sheet         { double WidthMm, double HeightMm, EdgeMargins Margins,
                double SpacingMm, IReadOnlyList<Part> Parts }
Placement     { int PartId, Matrix2D Transform /* yalnız rijit */ }
Problem       { ProblemCode Code, Severity Severity, int? ShapeId, int? SubPathIndex,
                string Message, GeometryRef? Highlight }
```

- `Matrix2D` yalnızca öteleme + dönüş (rijit) kullanılır; ölçek CorelDRAW'a yazılmaz.
- Her `Contour` kaynak indeksini taşır; hata raporu kullanıcıya kadar izlenebilir olur.
- `Unknown` rolü, belirsizliğin **açıkça taşındığı** durumdur; sessizce `Outer`/`Hole` yapılmaz.

## 4. Smart Divide akışı (P1 uygulaması)

1. Seçili tek ve bağımsız `cdrCurveShape` doğrulanır. Grup/PowerClip çerçevesi veya içindeki
   nesne, bitmap/efekt **açılmaz**; `UnsupportedObject` ile hiç yazmadan dönülür. Kaynak
   `ConvertToCurves` ile değiştirilmez.
2. Alt konturlar `Curve.SubPaths` üzerinden **salt okuma** ile alınır; her segmentin uç ve
   kontrol noktaları POCO'ya çevrilir. Belge birimi → mm dönüşümü tek noktada (`CorelUnits`).
3. Her alt kontur için analiz kopyası üretilir (adaptif bezier düzleştirme). **Güvenilir iç
   nokta** kendi tarama çizgisi yedeğimizle bulunur; ağırlık merkezi konkavda yetersizdir.
4. Containment matrisi: sınır noktası + nokta-içinde testi (ağırlık merkezi yerine), sınır
   teması ve kesişme ayrıca sınıflandırılır. `IsClockwise` **kullanılmaz** (yön otorite değildir).
5. Ağaç: çift derinlik = dış gövde, tek derinlik = delik, çift derinlik = ada. Ayrık parçalar
   (İ noktası, j noktası, Ç/Ğ işaretleri) **bağlanmaz**; yalnızca `DetachedComponent` uyarısı.
6. Belirsizlik tespiti → `Problem` (bkz. §7). `Error` varsa hiçbir nesne yazılmaz.
7. Analiz `Problem` kayıtları ve `SmartDivideRunResult` rapor adaptöründen `ProblemReportViewModel`'e
   taşınır. WPF paneli Türkçe başlık/açıklama ve indeksleri gösterir; seçilen sorun için yalnızca
   kontur vurgu isteği üretir. Corel'de vurgu uygulama ve paneli dock kabuğuna yerleştirme henüz yoktur.
8. Uygulama: tüm aday eğrileri **detached** olarak kurulur ve yaz→oku round-trip ile doğrulanır.
   Doğrulama geçerse `BeginCommandGroup` → aday başına bir `Layer.CreateCurve` (gövde + delikler
   aynı curve içinde) → `EndCommandGroup`. Kaynak şekle dokunulmaz; tek `Ctrl+Z` tüm yeni
   nesneleri kaldırır.

## 5. Nesting akışı

1. Seçim → `Part` listesi (Smart Divide containment çıktısı yeniden kullanılır).
2. Analiz kopyası: adaptif tessellation (tolerans = min(0.05 mm, spacing/10)).
3. Parçalar `spacing/2` kadar şişirilir; NFP bu şişmiş sınırlarla üretilir → mesafe garantisi.
4. Sabit parçalar önce yerleştirilir; dönüş seti kısıta göre uygulanır.
5. FFD sıralama + BLF; ardından süre bütçesi içinde yeniden sıralama/yerel arama.
6. Delik içi yerleştirme (etkinse): boşluğun `InflatePaths(-spacing/2)` sonucu içinde tam kalma testi.
7. Bağımsız doğrulayıcı: çift bazında `Intersect` alanı ≈ 0, levha sınırı, spacing.
8. Önizleme; onayda yalnızca rijit transform + `BeginCommandGroup`.

## 6. Birim ve tolerans politikası

| Kavram | Değer | Not |
|---|---|---|
| Kanonik birim | mm (double) | Dönüşüm yalnızca Dock katmanında |
| Karşılaştırma epsilon | `1e-4` mm | `double` için yeterli; belge hassasiyeti ~1e-3 mm |
| Tessellation | `min(0.05 mm, spacing/10)` | Yalnızca analiz kopyası |
| Açı toleransı | `1e-6` rad | Dönüş normalize edilir |
| Containment sınır bandı | `1e-3` mm | Bu bantta kalan sonuç **belirsiz** sayılır |
| Alan karşılaştırma | göreli `1e-6` | Offset/boolean sonrası |

Sabit sayı kullanılmaz; bu değerler tek bir `GeometryTolerances` yapılandırmasında toplanır (P1).

## 7. Hata ve belirsizlik modeli

| Kod | Önem | Durum | Davranış |
|---|---|---|---|
| `OpenContour` | Error | Kapalı olmayan / zinciri kopuk kontur | İşaretle, otomatik kapatma yok |
| `SelfIntersection` | Error | Kendini kesen kontur | İşaretle, otomatik onarım yok |
| `CrossingContours` | Error | İki konturun sınırları kesişiyor | Containment belirsiz; birleştirme yok |
| `TangentialContact` | Warning | Teğet / tolerans bandında temas | Belirsiz; otomatik eşleştirme yok |
| `AmbiguousParent` | Error | Birden fazla olası ebeveyn | Adayları göster, otomatik seçme |
| `DetachedComponent` | Warning | Gövdeye geometrik bağlı olmayan parça | **Bağlama yok**; yalnızca rapor (İ/j/Ç/Ğ) |
| `NoInteriorPoint` | Error | Güvenilir iç nokta yok (çok ince) | İşaretle |
| `DegenerateContour` | Error | Sıfır alan / üçten az nokta | İşaretle |
| `TooComplex` | Warning | Düzleştirme derinlik sınırı | Analiz yaklaşık; geometri değişmez |
| `NestedIsland` | Warning | Delik içinde ayrı gövde | Ayrı parça olarak değerlendir |
| `UnsupportedObject` | Error | Grup/PowerClip/bitmap/efekt | Hiç yazmadan atla + raporla |
| `OutsideSheet` | Error | Levha dışına taşma (Nesting) | Uygulamayı engelle |
| `Collision` | Error | Çakışma (Nesting) | Uygulamayı engelle |
| `WriteValidationFailed` | Error | Yazma öncesi round-trip uyuşmazlığı | Hiçbir nesne yazılmaz |

**Kural:** `Severity = Error` varsa uygulama butonu kapalıdır veya yalnızca "sorunlu parçaları atla"
seçeneğiyle ilerlenir; atlanan parçalar raporlanır.

## 8. İş parçacığı ve iptal

- COM erişimi yalnızca UI thread'inde. Arka plan görevine **kopyalanmış POCO** verilir.
- `CancellationToken` her arama döngüsünde kontrol edilir; hedef iptal gecikmesi < 200 ms.
- `IProgress<T>` ile ilerleme; UI güncellemesi throttle edilir (≥ 100 ms).
- UI thread'inde uzun süreli işlem yasak; `Dispatcher.Invoke` ile senkron bekleme yapılmaz.

## 9. Önizleme ve undo

- Önizleme, belgeye kalıcı yazmadan yapılır (geçici katman veya on-screen çizim).
- Uygulama tek `BeginCommandGroup`/`EndCommandGroup` içinde; kullanıcı tek `Ctrl+Z` ile döner.
- Kaynak nesnelere dokunulmaz; istisna: kullanıcı açıkça "kaynağı kaldır" seçerse, aynı undo grubunda.

## 10. Bağımlılık politikası

- Yalnızca permissive lisans (MIT/BSD/Apache-2.0/BSL-1.0).
- Seçili: `Clipper2` 2.0.0 (BSL-1.0, `netstandard2.0`).
- Planlanan: kendi NFP/yerleştirici katmanımız (dış nesting kütüphanesi yok).
- Yeni bağımlılık → ADR + lisans kontrolü + bakım durumu + boyut etkisi.

## 11. CorelDRAW entegrasyon sözleşmesi

- Addon klasörü: `<Corel>\Programs64\Addons\Corelad\` + boş `Coreldrw.addon`.
- UI: `AppUI.xslt` (öğe tanımı) + `UserUI.xslt` (menü/dock yerleşimi).
- Derleme: `Corel.Interop.VGCore.dll` **referans** olarak kurulu CorelDRAW'dan; repoya kopyalanmaz.
- Sürüm: `policy.27.x.Corel.Interop.VGCore.dll` yönlendirmesi nedeniyle v27.x içinde çalışır.

### 11.1 P1/P2'de ampirik olarak doğrulananlar (v27.2.0.135)

- Otomasyon başlatma: interop'taki `ApplicationClass` CLSID'i (`{4BBE0002-…}`) bu kurulumda
  kayıtlı değildir. Kurulum `CorelDRAW.Application.27` ProgID'sini `{F35B0002-…}` CLSID'ine
  bağlar; bu nedenle otomasyon **ProgID üzerinden** başlatılır (`CorelApplicationFactory`).
- `Document.CreateCurveFromArray(CurveElement[], n)`: `cdrElementStart` / `cdrElementLine` /
  `cdrElementCurve` / `cdrElementControl` dizisi tam kübik bezier üretir. **Ancak subpath
  `Closed = false` gelir**; kapanış ayrıca `SubPath.Closed = true` ile işaretlenmelidir.
  Bu, başlangıç = bitiş olan yolda yinelenen uç düğümünü atar ve segment sayısını korur.
- `SubPath.Closed = true`, başlangıç ≠ bitiş ise **düz çizgi** kapanış segmenti ekler.
- `Segment.StartingControlPointX/Y` yalnızca `cdrCurveSegment` için okunabilir; çizgi
  segmentinde COM hatası verir. Okuyucu bu ayrımı yapar.
- `Layer.CreateCurve(Curve)` yeni şekli aktif katmana ekler; `Document.Undo(1)` bir
  `BeginCommandGroup`/`EndCommandGroup` içindeki tüm yeni şekilleri kaldırır.
- Belge kaydedilmeden kapatma: `Document.Dirty = false` + `Document.Close()`.
- Smart Divide yalnızca nesne türünü kontrol etmez: `PowerClip`, `PowerClipParent` ve
  `ParentGroup` ilişkileri olan eğriyi de `UnsupportedObject` ile atlar. Dikdörtgen, grup,
  grup içindeki eğri, PowerClip çerçevesi ve PowerClip içindeki eğri smoke harness'ta
  denendi; her çağrıda şekil sayısı değişmedi.

### 11.2 Doğrulanmamış varsayımlar

- Kullanıcı bazlı addon yolu ve dock'un XSLT ile v27.2'de açılması (P5).
- Gerçek üretim `MUSTAFA.cdr` fixture'ı (P2).
