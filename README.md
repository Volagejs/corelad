# Corelad

CorelDRAW 2026 (x64, v27.x) içinde çalışan, kutu harf üretimine yönelik araç seti.

| Modül | Amaç |
|---|---|
| **Smart Divide** | Birleştirilmiş (`Ctrl+L`) yazıyı, her harfin dış konturu ve iç konturları birlikte kalacak şekilde mantıksal harflere ayırır. |
| **Gerçek Kontur Nesting** | Kapalı eğrileri gerçek konturlarına göre levhaya yerleştirir; bounding-box yerleştirmesi değildir. |

## Durum

**P1 — çalışan dikey dilim tamamlandı.** Geometri çekirdeği + COM adaptörü;
gerçek CorelDRAW 27.2'de smoke testi PASS. Dock UI/addon kurulumu henüz yok (P5).
Ayrıntı: [`docs/STATUS.md`](docs/STATUS.md), [`docs/P1-integration-plan.md`](docs/P1-integration-plan.md).

**P2 sertleştirmesi sürüyor.** Geometri testleri şu an 52 test içeriyor; Türkçe karakter
regresyonu ve grup/PowerClip güvenlik kontrolleri var. Bağımsız sorun raporu WPF paneli derleniyor;
CorelDRAW dock'una bağlanması ve gerçek üretim çizimiyle denenmesi henüz yapılmadı.

## Yapı

```
AGENTS.md                     OpenCode proje talimatı (bağlayıcı)
opencode.jsonc                Proje düzeyi OpenCode ayarı
docs/                         Gereksinim, mimari, ADR, durum, doğrulama
.opencode/                    Proje skill'leri ve komutları
src/Corelad.Geometry/         Saf geometri çekirdeği (CorelDRAW bağımsız)
src/Corelad.Dock/             net48 WPF addon + COM adaptörü (UI P5'te)
tests/                        Geometri testleri (CorelDRAW açmadan)
tools/Corelad.SmokeTest/      Gerçek CorelDRAW smoke harness'ı
fixtures/                     Test girdisi sözleşmesi
```

## Derleme

```bash
dotnet restore Corelad.sln
dotnet build   Corelad.sln -c Debug
dotnet test    tests/Corelad.Geometry.Tests/Corelad.Geometry.Tests.csproj

# Gerçek CorelDRAW smoke (yeni belge açar, kaydetmeden kapatır)
tools\Corelad.SmokeTest\bin\Debug\Corelad.SmokeTest.exe
```

## Lisans yaklaşımı

Yalnızca permissive lisanslı bağımlılıklar (Clipper2 / BSL-1.0). GPL/LGPL kod alınmaz.
Ayrıntı: [`docs/adr/0003-license-strategy.md`](docs/adr/0003-license-strategy.md).

## Katkı kuralları

`AGENTS.md` bağlayıcıdır. Doğrulanmamış davranış "çalışıyor" olarak sunulmaz;
`docs/VERIFICATION.md` etiketleme kuralını tanımlar.
