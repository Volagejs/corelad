---
description: Derleme, test ve kural kontrollerini çalıştırıp etiketli doğrulama raporu üretir
agent: build
---

`@verify-before-claim` skill'ini yükle ve şu kapsam için doğrulama çalıştır:

$ARGUMENTS

Sıra:

1. `dotnet build Corelad.sln -c Debug`
2. `dotnet test tests/Corelad.Geometry.Tests/Corelad.Geometry.Tests.csproj`
3. Katman sınırı kontrolü:
   - `src/Corelad.Geometry` içinde Corel/WPF referansı var mı?
   - `src/Corelad.Dock` içinde geometri algoritması var mı?
4. Kural kontrolü: `Save`/`SaveAs`, kaynak silme/değiştirme, sabit kodlanmış tolerans, sessiz onarım.
5. Gerekiyorsa gerçek CorelDRAW doğrulaması — **yapılmadıysa bunu açıkça yaz**.

Rapor formatı:

```
Değişen dosyalar:
  - ...
Doğrulama:
  - derleme: PASS/FAIL
  - testler: X/Y
  - CorelDRAW: yapıldı/yapılmadı
Sınırlamalar:
  - ...
Sıradaki adım:
  - ...
```

Her iddiaya tek bir etiket ver: `[DOĞRULANDI: birim testi]`, `[DOĞRULANDI: derleme]`,
`[DOĞRULANDI: CorelDRAW v27.2]`, `[DOĞRULANMADI]`, `[VARSAYIM]`.
