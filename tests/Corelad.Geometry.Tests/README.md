# tests/Corelad.Geometry.Tests

`net8` + xUnit. CorelDRAW açmadan çalışır (ADR-0004).

P0 durumunda test yok; P1'de `fixtures/synthetic/` girdileriyle başlayacak.

Planlanan test grupları:

| Grup | İçerik |
|---|---|
| Model | Bezier okuma/yazma, yön normalizasyonu, mm dönüşümü |
| Containment | `O`, `B`, `A`, `R` delik ağacı; `İ` ayrık parça |
| Problemlar | Teğet, kendini kesen, açık kontur, çoklu ebeveyn |
| Determinizm | Aynı girdi → aynı çıktı |
| Regresyon | `fixtures/golden/` karşılaştırması (tolerans 1e-4 mm) |
| Nesting | NFP doğruluğu, çakışma = 0, sınır/spacing ihlali = 0 (P3) |

Çalıştırma:

```bash
dotnet test tests/Corelad.Geometry.Tests/Corelad.Geometry.Tests.csproj
```
