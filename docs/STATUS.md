# STATUS — Corelad

Son güncelleme: 2026-11-14

## 1. Faz durumu

| Faz | Ad | Durum | Not |
|---|---|---|---|
| P0 | Proje iskeleti + dokümantasyon | **TAMAMLANDI** | Üretim kodu yok, CorelDRAW kurulumu yok |
| P1 | Çalışan dikey dilim (seçim → analiz → geri yaz) | **TAMAMLANDI** | V4 smoke PASS (v27.2.0.135); UI/addon yok (P5) |
| P2 | Smart Divide sertleştirme | **DEVAM EDİYOR** | Türkçe regresyon, UnsupportedObject smoke, rapor paneli/adaptörü; test ve gerçek fixture bekliyor |
| P3 | Nesting v1 (tek levha) | Bekliyor | — |
| P4 | Nesting v2 (çoklu levha, delik içi, bütçe) | Bekliyor | — |
| P5 | UX + paketleme | Bekliyor | — |

## 2. Teslimatlar

### 2.1 Dokümantasyon
- `README.md` — giriş, yapı, komutlar
- `AGENTS.md` — bağlayıcı OpenCode proje talimatı
- `docs/PROJECT.md` — vizyon, kapsam, varsayımlar, riskler
- `docs/REQUIREMENTS.md` — SD/NST/GEN/INT/UX gereksinimleri + kabul kriterleri
- `docs/ARCHITECTURE.md` — katmanlar, veri modeli, akışlar, tolerans politikası
- `docs/VERIFICATION.md` — doğrulama seviyeleri, etiketler, doğrulanmamış kayıt
- `docs/glossary.md` — terimler
- `docs/research/source-evaluation.md` — dış kaynak uyumluluk + lisans
- `docs/adr/0001`–`0007` — mimari kararlar
- `fixtures/README.md` — test girdisi sözleşmesi

### 2.2 OpenCode uyarlamaları
- `opencode.jsonc` — yalnızca `instructions: ["AGENTS.md"]`
- `.opencode/skills/project-memory/SKILL.md`
- `.opencode/skills/plan-first/SKILL.md`
- `.opencode/skills/verify-before-claim/SKILL.md`
- `.opencode/skills/ui-ux-desktop/SKILL.md`
- `.opencode/commands/plan.md`, `status.md`, `verify.md`

### 2.3 Çözüm iskeleti
- `Corelad.sln`
- `src/Corelad.Geometry/Corelad.Geometry.csproj` (netstandard2.0, bağımlılıksız)
- `src/Corelad.Dock/Corelad.Dock.csproj` (net48, WPF; Corel referansı **koşullu**)
- `tests/Corelad.Geometry.Tests/Corelad.Geometry.Tests.csproj` (net8.0-windows, xUnit)

### 2.4 P1 teslimatları
- `docs/P1-integration-plan.md` — okunacak/yazılacak nesneler + belge koruma sözleşmesi
- `Corelad.Geometry` çekirdeği:
  - `Model/` — `Point2`, `Rect2`, `BezierSegment`, `Contour`, `ContourSet`, `SegmentKind`, `ContourRole`
  - `Analysis/` — `BezierFlattener`, `FlattenedContour`, `Polygon2`, `ContainmentAnalyzer`,
    `ContourClassification`, `LetterCandidate`, `SmartDivideAnalysis`
  - `Diagnostics/` — `Problem`, `ProblemCode`, `ProblemSeverity`
  - `GeometryTolerances` — tüm eşikler tek yerde
- `Corelad.Dock/Corel/` COM adaptörü:
  - `CorelUnits`, `CorelTolerances`, `CorelCurveReader`, `CorelCurveWriter`,
    `CorelShapeSnapshot`, `SmartDivideService`, `SmartDivideRunResult`
- `tools/Corelad.SmokeTest/` — net48 x64 STA konsol smoke harness'ı (+ `--probe` API probu)
- `tests/Corelad.Geometry.Tests/` — P1'de 51 test; P2 ile güncel toplam 52 test, GDI+ font desteği
- `tests/Corelad.Dock.Tests/` — sorun raporu ViewModel/DTO/metin için 37 test; CorelDRAW COM başlatmaz
- P2 rapor sunumu: `Corel/SmartDivideProblemReportAdapter`, `Diagnostics/ProblemReport*`,
  `ViewModels/ProblemReportViewModel`, `Views/ProblemReportPanel` (bağımsız WPF bileşeni; dock host'a bağlanmadı)

### 2.5 P1 sonuçları

| Kontrol | Sonuç |
|---|---|
| `dotnet build Corelad.sln -c Debug` | **PASS** — 0 uyarı, 0 hata |
| `dotnet test` (geometri) | **PASS** — 51/51, 0 başarısız |
| V4 smoke (gerçek CorelDRAW v27.2.0.135) | **PASS** — 7 aday, 2 delik, 0 Error; kaynak imzası değişmedi; `Undo(1)` yeni 7 nesneyi kaldırdı |
| P2 Türkçe karakter regresyonu | **PASS** — `ÇĞİıÖŞÜ` konturları kaybolmadan, deterministik analiz edildi; 52/52 test |
| P2 UnsupportedObject smoke | **PASS** — dikdörtgen, grup, grup içi eğri, PowerClip çerçevesi ve içindeki eğri atlandı; hiçbiri çıktı yazmadı; atlama hatası rapora çevrildi |
| P2 sorun raporu bileşeni | **DERLENDİ** — WPF paneli ve run-result adaptörü çözüm içinde derleniyor; CorelDRAW dock'unda açılma doğrulanmadı |
| P2 rapor modeli testleri | **PASS** — `Corelad.Dock.Tests` 37/37; ViewModel, indeks gösterimi, metinler ve vurgu isteği |
| Kaynak belge kaydı | **Yapılmadı** — `Dirty = false` + `Close()` |
| Addon klasörüne yazma | **Yapılmadı** |
| Corel interop çıktıya kopyalama | **Yapılmadı** (`Private=false`) |

## 3. Sonuçlar

| Kontrol | Sonuç |
|---|---|
| `dotnet build Corelad.sln -c Debug` | **PASS** — 0 uyarı, 0 hata (4.05 sn) |
| `dotnet test` | **PASS (0 test)** — P0'da bilinçli olarak test yok; çıkış kodu 0 |
| `Corelad.Geometry` Corel/WPF referansı | **Yok** — `Private=false`, interop çıktıya kopyalanmadı |
| `Corelad.Dock` çıktısı | `Corelad.Dock.dll` + `Corelad.Geometry.dll`; `Corel.Interop*` **kopyalanmadı** |
| CorelDRAW klasörüne yazma | **Yapılmadı** |
| Global OpenCode ayarı | **Değiştirilmedi** |
| `cmd.exe` | **Dokunulmadı** |
| Git | **Başlatılmadı** |
| Üretim kodu (P0) | **Yazılmadı** — P1'de çekirdek + COM adaptörü eklendi (§2.4) |

## 4. Kararlar (özet)

| # | Karar | ADR |
|---|---|---|
| 1 | Hedef CorelDRAW 2026 x64 (v27.x) | ADR-0001 |
| 2 | C# addon + WPF dock (net48) | ADR-0002 |
| 3 | Permissive-only lisans; Clipper2 + kendi NFP | ADR-0003 |
| 4 | Geometri çekirdeği ayrı, Corel bağımsız | ADR-0004 |
| 5 | Kanonik birim mm, tek dönüşüm noktası | ADR-0005 |
| 6 | Kaynak korunur; kopyala-üret yaklaşımı | ADR-0006 |
| 7 | Yalnızca Clipper2 (BSL-1.0) bağımlılığı | ADR-0007 |
| 8 | `System.Drawing.Common` yalnızca test bağımlılığı | ADR-0008 |

## 5. Engeller / açık sorular

| # | Konu | Durum |
|---|---|---|
| B1 | Ürün lisans modeli (kapalı/açık) | Varsayımla ilerlendi: kapalı olabilir → permissive-only |
| B2 | CorelDRAW 2024/2025 desteği | Kapsam dışı varsayıldı |
| B3 | Gerçek `MUSTAFA.cdr` fixture | Hâlâ yok; GDI+ Arial konturlarıyla ilerlendi (P2'de gerçek dosya) |
| B4 | Corel SDK API sayfaları üyelik korumalı | Ampirik olarak çözüldü (probe + smoke); ayrıntı: ARCHITECTURE §11.1 |

## 6. Sıradaki adım

**P1 kapandı; P2 devam ediyor.**

P1'de tamamlananlar:
1. `Corelad.Geometry` saf çekirdek + containment ağacı + 51 test.
2. `Corelad.Dock/Corel` COM adaptörü: salt-okuma, mm dönüşümü, round-trip doğrulamalı yazma, tek undo.
3. Gerçek CorelDRAW v27.2'de `MUSTAFA` smoke testi (V4 PASS).
4. `CreateCurveFromArray` + `SubPath.Closed` semantiği ampirik olarak doğrulandı.

P2 çalışması: `ÇĞİıÖŞÜ` için deterministik font konturu regresyonu eklendi; gerçek CorelDRAW'da
dikdörtgen, grup, grup içi eğri, PowerClip çerçevesi ve PowerClip içindeki eğrinin
`UnsupportedObject` ile atlandığı ve şekil eklemediği doğrulandı. Türkçe sorun raporu paneli ve
çalışma sonucu adaptörü eklendi; 37 rapor modeli testi geçti. Panel henüz addon dock'una
bağlanmadı. Kalanlar: gerçek `MUSTAFA.cdr` ile V5 ve Corel'de panel/vurgu entegrasyonu. Gerçek
fixture yok; Nesting'e geçilmedi.

P1'de **yapılmayanlar:** NFP/nesting, önizleme UI'si, belirsizlik onay ekranı, addon kurulumu.

## 7. Değişiklik günlüğü

| Tarih | Faz | Özet |
|---|---|---|
| 2026-11-14 | P0 | İskelet, dokümantasyon, ADR'ler, OpenCode uyarlamaları oluşturuldu |
| 2026-11-14 | P1 | Geometri çekirdeği, 51 test, COM adaptörü, smoke harness; V4 PASS; ADR-0008 eklendi |
| 2026-11-14 | P2 | Türkçe regresyonu, grup/PowerClip smoke, sorun paneli/adaptörü ve 37 Dock testi; P2 sürüyor |
