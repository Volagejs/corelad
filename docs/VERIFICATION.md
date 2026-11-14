# VERIFICATION — Corelad

Doğrulama, "derlendi" ile "çalışıyor" arasındaki farkı korumak için vardır. Her iddia etiketlenir.

## 1. Etiketler

| Etiket | Kanıt | Ne zaman kullanılır |
|---|---|---|
| `[DOĞRULANDI: birim testi]` | `dotnet test` çıktısı | Saf geometri davranışı |
| `[DOĞRULANDI: derleme]` | `dotnet build` çıktısı | Yalnızca derlenebilirlik |
| `[DOĞRULANDI: CorelDRAW v27.2]` | Gerçek uygulamada çalıştırma + ekran/çıktı kanıtı | Entegrasyon ve kullanıcı akışı |
| `[DOĞRULANMADI]` | Kod var, hedef ortamda denenmedi | Yeni entegrasyon kodu |
| `[VARSAYIM]` | Kanıt yok | API semantiği, sürüm davranışı |

Kural: Bir rapor satırı birden fazla etiket taşıyamaz. "Derleme geçti" cümlesi tek başına
"çalışıyor" anlamına gelmez.

## 2. Doğrulama seviyeleri

| Seviye | Kapsam | Araç | Sıklık |
|---|---|---|---|
| V0 | Doküman tutarlılığı (ID çakışması, kırık bağlantı) | Elle / basit kontrol | Her doküman değişikliği |
| V1 | Derleme | `dotnet build Corelad.sln` | Her commit |
| V2 | Geometri birim testleri | `dotnet test` | Her commit |
| V3 | Özellik/fuzz testleri (alan korunumu, determinizm) | xUnit + üretici | Günlük / sürüm öncesi |
| V4 | COM smoke testi (CorelDRAW açık) | Manuel veya betik | Faz sonu |
| V5 | Kabul testi (gerçek üretim çizimi) | Operatör + geliştirici | Sürüm öncesi |

## 3. Faz kapıları

| Faz | Geçiş koşulu |
|---|---|
| P0 → P1 | V0 + V1 yeşil; doküman ve iskelet derleniyor |
| P1 → P2 | V4: gerçek CorelDRAW'da seçim okundu ve nesne geri yazıldı; kaynak değişmedi |
| P2 → P3 | V2/V3: Smart Divide regresyon seti yeşil; TR karakter seti geçti |
| P3 → P4 | V4: tek levha nesting çalıştı; bağımsız doğrulayıcı 0 ihlal |
| P4 → P5 | V3: çoklu levha + iptal + süre bütçesi testleri yeşil |
| P5 → sürüm | V4/V5: temiz makinede kurulum ve operatör kabulü |

## 4. Test katmanları

| Katman | Neyi test eder | Örnek |
|---|---|---|
| Birim | Tek fonksiyon/segment | Bezier uzunluğu, offset tek kontur |
| Bileşen | Containment ağacı, NFP üretimi | `O` harfi → 1 gövde + 1 delik |
| Özellik | Değişmezler | Toplam çocuk alanı ≤ ebeveyn alanı; determinizm |
| Regresyon | Golden fixture | `MUSTAFA` çıktısı JSON ile karşılaştırma |
| Entegrasyon | COM ↔ POCO | Seçimi oku → POCO → geri yaz |
| Kabul | Kullanıcı akışı | Önizleme → uygula → undo |

## 5. Doğrulanmış / doğrulanmamış kayıt (canlı)

### 5.1 Doğrulanmış

| Tarih | İddia | Etiket | Kanıt |
|---|---|---|---|
| 2026-11-14 | `net48` hedefi bu makinede `dotnet build` ile derleniyor | `[DOĞRULANDI: derleme]` | P0 derleme çıktısı (STATUS §2) |
| 2026-11-14 | CorelDRAW 2026 v27.2 x64 kurulu, `Corel.Interop.VGCore.dll` 27.2.0.135 mevcut | `[DOĞRULANDI: ortam envanteri]` | İnceleme raporu §2 |
| 2026-11-14 | `Corelad.Geometry` CorelDRAW/WPF referansı olmadan derleniyor | `[DOĞRULANDI: derleme]` | P0 derleme çıktısı |
| 2026-11-14 | `Corelad.Dock` çıktısına `Corel.Interop*` kopyalanmıyor (`Private=false`) | `[DOĞRULANDI: derleme]` | `bin\Debug` içerik listesi |
| 2026-11-14 | Geometri çekirdeği 51/51 test geçiyor (0 başarısız) | `[DOĞRULANDI: birim testi]` | `dotnet test` — 20 ms |
| 2026-11-14 | A/B/O/P/R gerçek Arial konturlarında 1 aday + doğru delik sayısı | `[DOĞRULANDI: birim testi]` | `FontOutlineTests` |
| 2026-11-14 | `MUSTAFA` 9 kontur → 7 aday, 2 delik, 0 problem | `[DOĞRULANDI: birim testi]` | `FontOutlineTests.WordMustafa_HasSevenLogicalLettersAndTwoHoles` |
| 2026-11-14 | İ/j/Ğ ayrık parça uyarısı; birleştirme yok | `[DOĞRULANDI: birim testi]` | `DetachedComponentTests`, `FontOutlineTests` |
| 2026-11-14 | Ç gerçek Arial'de `CrossingContours` (Error) → uygulama engelli | `[DOĞRULANDI: birim testi]` | `FontOutlineTests` tanı çıktısı |
| 2026-11-14 | `CreateCurveFromArray` açık subpath üretir; `Closed = true` yinelenen uç düğümünü atar | `[DOĞRULANDI: CorelDRAW v27.2]` | `Corelad.SmokeTest --probe` (Probe 1–3) |
| 2026-11-14 | Çizgi segmentinde kontrol noktası okumak COM hatası verir; okuyucu yalnızca `cdrCurveSegment`'te okur | `[DOĞRULANDI: CorelDRAW v27.2]` | `--probe` çıktısı |
| 2026-11-14 | `MUSTAFA` yaz→oku round-trip: 9 subpath, tüm segment/kontrol noktaları eşleşti | `[DOĞRULANDI: CorelDRAW v27.2]` | Smoke `PASS Round-trip…` |
| 2026-11-14 | Smart Divide: 7 yeni şekil, kaynak imzası değişmedi, belge kaydedilmedi | `[DOĞRULANDI: CorelDRAW v27.2]` | Smoke `PASS` satırları |
| 2026-11-14 | Tek `Undo(1)` command group içindeki 7 yeni şekli kaldırdı, kaynak kaldı | `[DOĞRULANDI: CorelDRAW v27.2]` | Smoke `PASS Undo(1)…` |
| 2026-11-14 | Arial `ÇĞİıÖŞÜ` konturları testte kaybolmadan ve deterministik analiz ediliyor | `[DOĞRULANDI: birim testi]` | `FontOutlineTests.TurkishCharacterSet_IsAnalyzedDeterministicallyWithoutLosingContours`; 52/52 |
| 2026-11-14 | `cdrRectangleShape` Smart Divide tarafından `UnsupportedObject` olarak atlanıyor, yeni şekil yazılmıyor | `[DOĞRULANDI: CorelDRAW v27.2]` | Corel smoke P2 kontrolü; şekil sayısı 2'de kaldı |

**V2 notu:** P0'da test projesi bilinçli olarak 0 test içeriyordu; P1'de 51 test ile V2 kapısı
karşılandı. Testler CorelDRAW açmadan çalışır; gerçek font konturları GDI+ ile üretilir (ADR-0008).

**V4 kanıt özeti (2026-11-14, CorelDRAW 27.2.0.135):**

```
PASS  Yeni belge oluşturuldu
PASS  Kaynak eğri belgeye yazıldı / cdrCurveShape / şekil sayısı +1
PASS  Round-trip geometri eşleşmesi (9 subpath)
PASS  7 aday, 2 delik, 0 Error, 7 yeni cdrCurveShape, şekil sayısı +7
PASS  Kaynak geometrisi değişmedi (imza aynı)
PASS  Undo(1) sonrası yalnızca kaynak kaldı
PASS  cdrRectangleShape UnsupportedObject olarak atlandı, şekil eklenmedi
Belge kaydedilmeden kapatıldı.
SMOKE SONUCU: PASS
```

### 5.2 Doğrulanmamış (açıkça işaretli)

| İddia | Durum | Doğrulama planı |
|---|---|---|
| `Curve.AppendCurveSegment2` kontrol noktası semantiği | **Kullanılmıyor** | Yerine `CreateCurveFromArray` + `SubPath.Closed` doğrulandı |
| `SubPath.IsPointInside` teğet durumda doğru sonuç verir | **Kullanılmıyor** | Kendi nokta-içinde + sınır bandı testlerimiz kullanılıyor |
| XSLT ile dock v27.2'de açılır | `[DOĞRULANMADI]` | P5: gerçek CorelDRAW |
| Kullanıcı bazlı addon yolu destekleniyor | `[VARSAYIM]` | P5: `%APPDATA%` altında deneme |
| Gerçek üretim `MUSTAFA.cdr` fixture'ı | `[DOĞRULANMADI]` | P2: kullanıcı dosyasıyla V4/V5 |
| Grup/PowerClip seçiminde davranış | `[DOĞRULANMADI]` | P2: `UnsupportedObject` smoke |
| SVG round-trip ölçek/yön korunumu | `[DOĞRULANMADI]` | Yalnızca gerekirse; SVG birincil yol değil |
| Corel ikililerinin müşteri makinesinde yeniden dağıtımı | `[VARSAYIM]` | Hukuki teyit; DLL dağıtılmıyor |

## 6. Raporlama şablonu

```
Değişen dosyalar:
  - <yol> — <ne değişti>
Doğrulama:
  - V1 derleme: PASS/FAIL (komut + özet)
  - V2 testler: X/Y geçti
  - V4 CorelDRAW: yapıldı/yapılmadı — kanıt
Sınırlamalar:
  - <doğrulanmamış varsayım>
Sıradaki adım:
  - <tek cümle>
```

## 7. Yasaklar

- Test edilmemiş kodu "tamamlandı" olarak işaretlemek.
- Başarısız testi yorumla gizlemek.
- Gerçek CorelDRAW'da denenmemiş COM davranışını "çalışıyor" diye raporlamak.
- Doğrulama etiketi olmadan performans/doğruluk iddiası yazmak.
