# AGENTS.md — Corelad

CorelDRAW 2026 (x64, v27.x) içinde çalışan, kutu harf üretimine yönelik iki araç geliştiriyoruz:

1. **Smart Divide** — `Ctrl+L` ile birleştirilmiş yazıyı, her harfin dış konturu + iç konturları
   birlikte kalacak şekilde mantıksal harflere ayırır.
2. **Gerçek Kontur Nesting** — kapalı eğrileri gerçek konturlarına göre levhaya yerleştirir
   (bounding-box değil).

Bu dosya OpenCode için bağlayıcı proje talimatıdır. Çelişki durumunda öncelik sırası:
**kullanıcı talimatı > bu dosya > skill'ler > genel alışkanlıklar.**

---

## 1. Kesin sınırlar (ihlal edilemez)

- **Kaynak çizimi otomatik kaydetme.** Kullanıcı açıkça istemedikçe `Save`/`SaveAs` çağrılmaz.
- **Kaynak nesneleri varsayılan olarak silme, gizleme veya değiştirme.** Yeni nesne üret.
  Kaynağı kaldırma yalnızca kullanıcı onaylı bir seçenekse ve tek undo ile geri alınabiliyorsa.
- **Sessiz geometri onarımı yok.** Kendini kesen, açık, teğet, belirsiz ebeveynli veya
  çakışan konturlar düzeltilmiş gibi davranılmaz; `Problem` olarak raporlanır.
- **Kaba poligon çıktısı yok.** Poligonlaştırma yalnızca analiz/yerleşim matematiğinde kullanılan
  türetilmiş kopyada olur. CorelDRAW'a yazılan geometri **orijinal bezier** + rijit transformdur.
- **GPL / LGPL / AGPL kodunu projeye kopyalama.** Ayrıntı: `docs/adr/0003-license-strategy.md`.
  `deepnest` çekirdeği GPLv3, `libnest2d` LGPL-3.0 — kod alınmaz, yalnızca yayınlanmış fikirler okunur.
- **Corel lisanslı ikilileri repoya koyma.** `Corel.Interop.*.dll`, `VGCore.dll`, `*.tlb` commit edilmez.
  Derleme zamanında kurulu CorelDRAW'dan referans verilir; çalışma zamanında yüklenir.
- **`cmd.exe`'ye dokunma.** Depo kökündeki `cmd.exe` kullanıcıya ait, ilgisiz bir Windows kopyasıdır.
  Silme, taşıma, değiştirme, "temizlik" kapsamına alma.
- **Global OpenCode ayarına dokunma.** `~/.config/opencode/opencode.json` bu projenin kapsamı dışındadır.
- **Yönetici izni gerektiren işlem yapma.** CorelDRAW `Programs64\Addons\` altına yazma, servis
  değiştirme, execution policy değiştirme yok. Geliştirme çıktısı `artifacts/` altında tutulur.
- **Doğrulanmamışı doğrulanmış sunma.** Etiketleme kuralı: bölüm 5.

## 2. Mimari sınırlar

```
src/Corelad.Geometry   → saf geometri. CorelDRAW, WPF, System.Windows referansı YOK.
src/Corelad.Dock       → net48 WPF addon + CorelDRAW COM entegrasyonu. Algoritma YOK.
tests/Corelad.Geometry.Tests → net8 xUnit. CorelDRAW açmadan çalışır.
```

- `Corelad.Geometry` **hiçbir** CorelDRAW tipine bağımlı olmaz. Girdi/çıktı POCO'dur.
- `Corelad.Dock` içinde containment, NFP, offset veya arama algoritması yazılmaz; yalnızca
  COM ↔ POCO dönüşümü, UI, ilerleme/iptal ve undo grubu yönetimi bulunur.
- `Corelad.Dock` **UI thread'inde** yalnızca COM çağrısı yapar. Uzun hesap `Task` + `CancellationToken`
  ile arka planda, saf veri kopyası üzerinde çalışır.
- Bağımlılık politikası: yalnızca **permissive** lisanslı paketler (MIT/BSD/Apache-2.0/BSL-1.0).
  Yeni paket eklemeden önce lisans + bakım durumu ADR'ye yazılır.

## 3. Birim ve tolerans politikası

- Kanonik hesap birimi **mm** (double). CorelDRAW belge birimi UI'da gösterilir, çekirdeğe mm girer.
- Dönüşüm tek noktada yapılır; çekirdekte birim parametresi taşınmaz.
- Varsayılan geometrik tolerans: **1e-4 mm** (karşılaştırma), tessellation toleransı
  **min(0.05 mm, spacing/10)**. Sihirli sayı yazma; `docs/ARCHITECTURE.md` §6'daki politika kullanılır.
- Yüzen nokta karşılaştırmaları epsilon ile yapılır; `==` kullanılmaz.

## 4. Komutlar

```bash
dotnet restore Corelad.sln
dotnet build   Corelad.sln -c Debug
dotnet test    tests/Corelad.Geometry.Tests/Corelad.Geometry.Tests.csproj
```

Not: Bu makinede PowerShell script çalıştırma kısıtlıdır. PowerShell betiği gerekiyorsa
`cmd /c "..."` ile çağır veya doğrudan `dotnet`/`git` kullan.

## 5. Doğrulama etiketleri

Her iddiayı şu etiketlerden biriyle raporla:

| Etiket | Anlamı |
|---|---|
| `[DOĞRULANDI: birim testi]` | CorelDRAW'sız otomatik test geçti |
| `[DOĞRULANDI: derleme]` | Yalnızca derleme başarılı |
| `[DOĞRULANDI: CorelDRAW v27.2]` | Gerçek uygulamada çalıştırıldı, kanıt eklendi |
| `[DOĞRULANMADI]` | Kod var ama hedef ortamda denenmedi |
| `[VARSAYIM]` | Kanıt yok, çalışma hipotezi |

"Çalışıyor" demek için `[DOĞRULANDI: CorelDRAW v27.2]` gerekir. Derleme geçmesi yeterli değildir.

## 6. İş akışı

1. İsteği analiz et; varsayımları ve belirsizlikleri açıkça yaz.
2. Değişecek dosyaları ve riskleri **önce** listele.
3. Büyük değişiklikte onay bekle; küçük ve geri alınabilir adımlarla ilerle.
4. Her adımda doğrula (derleme + test + gerekiyorsa gerçek uygulama).
5. `docs/STATUS.md` ve gerekliyse ADR'yi güncelle.
6. Değişen dosyaları, kararları ve sınırlamaları raporla.

Karar verici belge hiyerarşisi:
`docs/PROJECT.md` → `docs/REQUIREMENTS.md` → `docs/ARCHITECTURE.md` → `docs/adr/*` → `docs/STATUS.md`.

## 7. Doküman haritası

| Dosya | İçerik |
|---|---|
| `docs/PROJECT.md` | Vizyon, kapsam, varsayımlar, riskler |
| `docs/REQUIREMENTS.md` | Numaralı gereksinimler + kabul kriterleri |
| `docs/ARCHITECTURE.md` | Katmanlar, veri modeli, iş parçacığı, undo |
| `docs/VERIFICATION.md` | Doğrulama seviyeleri, doğrulanmış/doğrulanmamış kayıt |
| `docs/STATUS.md` | Faz durumu, kararlar, engeller, sıradaki adım |
| `docs/glossary.md` | Ortak terimler |
| `docs/adr/` | Mimari karar kayıtları |
| `docs/research/source-evaluation.md` | Dış kaynak uyumluluk + lisans incelemesi |
| `fixtures/README.md` | Test girdisi sözleşmesi |
