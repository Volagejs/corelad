# Dış Kaynak Değerlendirmesi — uyumluluk ve lisans

İnceleme tarihi: 2026-11-14
Ortam: OpenCode **v2.3.0** (V2), CorelDRAW **v27.2** x64, .NET SDK 8 + .NET Framework 4.8

> "Uyumlu" = biçim olarak OpenCode tarafından yüklenebilir. Hiçbir kaynak "olduğu gibi çalışır"
> kategorisinde değildir; hepsi uyarlama gerektirdi.

## 1. Beceri / yöntem paketleri

| Kaynak | Ne / hangi ajan için | OpenCode V2 uyumu | Lisans | Projede kullanım |
|---|---|---|---|---|
| **Karpathy** — `multica-ai/andrej-karpathy-skills` | Tek `CLAUDE.md` + `skills/karpathy-guidelines/SKILL.md`; LLM kodlama hatalarını azaltan davranış kuralları. Claude Code için. | `SKILL.md` + `name`/`description` frontmatter **birebir uyumlu**. `CLAUDE.md` OpenCode tarafından **okunmaz** (OpenCode `AGENTS.md` okur). | **Lisans dosyası yok** (GitHub license API 404); dosyada `license: MIT` yazıyor ama bağlayıcı lisans metni yok. | **Fikir düzeyinde uyarlandı** → `.opencode/skills/project-memory/` + `AGENTS.md`. Metin kopyalanmadı. |
| **Superpowers** — `obra/superpowers` | Analiz → plan → küçük doğrulanabilir adımlar, TDD, sistematik debug, kod inceleme. | **Resmî destek var**: `.opencode/plugins/superpowers.js` + `.opencode/INSTALL.md`, V2 için `>= 2.0.4`. Ancak plugin her yanıttan önce skill çağrısı zorluyor ve Windows'ta git-tabanlı kurulum sorunlu. | **MIT** ✅ | **Plugin kurulmadı.** Planlama/doğrulama özü uyarlandı → `.opencode/skills/plan-first/`. |
| **Matt Pocock** — `mattpocock/skills` | 27 mühendislik skill'i (TDD, review, ADR, triage). TypeScript ekosistemi odaklı. | Biçim uyumlu, OpenCode kurulumu yok. İçerik TS/vitest varsayıyor. | **MIT** ✅ | **Eklenmedi** — yığın C#/.NET. Kural: yalnızca TS/arayüz teknolojisi kullanılırsa. |
| **Everything Claude Code** — `affaan-m/ECC` (eski `worldflowai/everything-claude-code`) | 26 agent, 26 command, hook, 100+ skill. | `.opencode/` var **ama şema V1**: `plugin`, `agent` (tekil), `skills.paths`. V2'de `plugins`, `agents`, `skills: []`. Plugin `dist/` derlemesi yoksa sessizce atlanıyor. | **MIT** ✅ (eski `worldflowai` fork'unda lisans **yok** ❌) | **Kopyalanmadı.** Yalnızca doğrulama döngüsü fikri uyarlandı → `.opencode/skills/verify-before-claim/`. |
| **UI/UX Pro Max** — `nextlevelbuilder/ui-ux-pro-max-skill` | UI/UX tasarım zekâsı: stiller, paletler, tipografi, UX kuralları. | **Resmî OpenCode desteği** (`platforms/opencode.json`). Ancak içerik/script'ler web/CSS/Tailwind/Python odaklı. | **MIT** ✅ | **Tam kurulum yapılmadı.** Dil-bağımsız UX ilkeleri uyarlandı → `.opencode/skills/ui-ux-desktop/`. Algoritma bu beceriye bırakılmadı. |

## 2. Nesting algoritma referansları

| Kaynak | Yaklaşım | Lisans | Bakım | Karar |
|---|---|---|---|---|
| `deepnest-next/deepnest` | SVGNest türevi; Minkowski/NFP + genetik arama; C hızlandırma | **Karma**: üst `LICENSE` MIT, ama `LICENSES.md`: `deepnest.js` = **GPLv3**; `svgnest.js`/`svgparser.js` = MIT; `util/clipper`, `minkowski.*` = Boost | Aktif (2026-10) | **Kod alınmadı** (GPLv3 viral). Yalnızca algoritma fikirleri. |
| `jack000/SVGNest` | Orijinal akademik uygulama | **MIT** | 2024'te durdu | Lisans açısından temiz referans; JS/Node olduğu için taşınmadı. |
| `Dexus/svgnest` | Küçük fork | **MIT** | 2024-12, düşük olgunluk | Yalnızca okuma. |
| `tamasmeszaros/libnest2d` | C++11, NFP + first-fit; PrusaSlicer'da üretimde | **LGPL-3.0** | Son push 2024-05 | **Kullanılmadı** — header-only türev eser riski (ADR-0003). |
| `AngusJohnson/Clipper2` | Boolean, offset, Minkowski, triangulation (C++/C#/Delphi) | **BSL-1.0** | Orijinal arşivli; `PolyClipping/Clipper2` aktif | **Seçildi** (ADR-0007). NuGet 2.0.0, `netstandard2.0`. |

### Seçim gerekçesi (NFP)
- Gerçek kontur yerleştirmesi NFP gerektirir; bounding-box yetersizdir.
- Clipper2 `MinkowskiSum` + `InflatePaths` + `Union/Difference` ile NFP ve delik yerleştirme aynı araçla çözülür.
- GPL/LGPL kullanmadan aynı sınıf sonuç elde edilebilir.
- Risk: konkav parçalarda NFP maliyeti → önbellek, sadeleştirme, süre bütçesi (ARCHITECTURE §5).

## 3. CorelDRAW SDK

- SDK **ücretsiz**, ancak dokümantasyon/API referansı **üyelik** gerektiriyor; sayfalar Incapsula korumalı.
- Addon yükleme: `Programs64\Addons\<Ad>\` + boş `Coreldrw.addon` (wiki makale 173/174 ile doğrulandı).
- Dock UI: `AppUI.xslt` + `UserUI.xslt`; `window.external.Application` (HTML docker) veya doğrudan COM (C#).
- **Lisans:** Corel EULA + SDK koşulları. Interop DLL'leri repoya/dağıtıma konmuyor.
- **P1'de ampirik doğrulanan:** `CreateCurveFromArray` + `SubPath.Closed`, `Layer.CreateCurve`,
  undo grubu, birim dönüşümü (ARCHITECTURE §11.1).
- **Doğrulanmamış:** kullanıcı bazlı addon yolu, XSLT'nin v27.2 davranışı.

## 4. Uyarlanan / uyarlanmayan özet

| Kaynak | Uyarlanan | Uyarlanmayan (gerekçe) |
|---|---|---|
| Karpathy | Varsayımı söyle, basit tut, cerrahi değişiklik, doğrulanabilir hedef + kalıcı durum dosyaları | `CLAUDE.md` metni (lisanssız + yanlış dosya adı) |
| Superpowers | Plan önce, küçük adımlar, doğrulama, TDD özü | Plugin, hook'lar, agresif "her şeyden önce skill" kuralı |
| Matt Pocock | — | Tamamı (TS değil) |
| ECC | Doğrulama döngüsü, karar/bağlam ayrımı, `/plan`-`/checkpoint` fikri | V1 şemalı config, plugin, 26 agent |
| UI/UX Pro Max | 8pt grid, durum matrisi, kontrast, klavye erişimi, boş/hata/meşgul | Web stack dosyaları, CSS/Tailwind script'leri |
| deepnest / SVGNest / libnest2d | Algoritma fikirleri | Kaynak kod (lisans) |
| Clipper2 | Bağımlılık olarak kullanılacak | — |

## 5. Çelişki çözümleri (özet)

| Çelişki | Çözüm |
|---|---|
| Superpowers "her yanıttan önce skill" ↔ kullanıcı "önce plan, onay bekle" | Kullanıcı + Karpathy kazanır |
| Superpowers TDD + sık commit ↔ repo git değil | Testler geometri çekirdeğinde; git kullanıcı onayına bağlı |
| ECC config V1 ↔ OpenCode V2 | ECC config kopyalanmadı |
| ECC 26 agent + Superpowers 14 skill | Tek yöntem (plan-first + verify) seçildi |
| UI/UX Pro Max web ↔ WPF | Stack dosyaları alınmadı |
| Matt Pocock TS ↔ C# | Eklenmedi |
| deepnest GPLv3 ↔ kapalı kaynak | Kod alınmadı |
| "kaba poligon yok" ↔ NFP poligon ister | Poligon yalnızca analiz kopyasında |
| "kaynağı silme" ↔ kullanıcı "yerine koy" bekleyebilir | Varsayılan koruma, seçenek opsiyonel |
| SVG "kesin çözüm" varsayımı ↔ doğrulanmadı | SVG birincil yol değil; doğrudan COM |
