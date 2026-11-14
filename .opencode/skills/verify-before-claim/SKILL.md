---
name: Verify Before Claim
description: Use after implementing anything, before reporting work as done, or before saying something "works". Separates build success from real CorelDRAW verification and requires explicit labels for unverified behavior.
---

# Verify Before Claim

Amaç: "derlendi" ile "çalışıyor" arasındaki farkı korumak.
Kaynak: ECC `verification-loop` fikrinin bu projenin .NET/CorelDRAW yığınına uyarlaması.

## Ne zaman kullanılır

- Bir özellik/düzeltme tamamlandığında.
- Rapora "tamamlandı", "çalışıyor", "doğru" yazmadan önce.
- Performans veya doğruluk iddiası yazmadan önce.

## Doğrulama sırası

1. **Derleme**
   ```bash
   dotnet build Corelad.sln -c Debug
   ```
   Başarısızsa dur; devam etme.

2. **Birim testleri**
   ```bash
   dotnet test tests/Corelad.Geometry.Tests/Corelad.Geometry.Tests.csproj
   ```
   Sayıları raporla: X geçti / Y başarısız / Z atlandı.

3. **Katman sınırı**
   - `Corelad.Geometry` içinde Corel/WPF referansı var mı? (olmamalı)
   - `Corelad.Dock` içinde geometri algoritması var mı? (olmamalı)

4. **Kural kontrolü**
   - `Save`/`SaveAs` çağrısı eklenmiş mi? (olmamalı)
   - Kaynak nesne siliniyor/değiştiriliyor mu? (varsayılan olmamalı)
   - Sessiz geometri onarımı var mı? (olmamalı)
   - Sabit kodlanmış tolerans var mı? (olmamalı, ADR-0005)

5. **Hedef ortam (gerekiyorsa)**
   - COM/UI değişikliği varsa **gerçek CorelDRAW v27.2**'de denenmeden "çalışıyor" denmez.
   - Denenmediyse: `[DOĞRULANMADI]` yaz ve nedenini belirt.

## Etiketler (AGENTS.md §5)

`[DOĞRULANDI: birim testi]` · `[DOĞRULANDI: derleme]` · `[DOĞRULANDI: CorelDRAW v27.2]` ·
`[DOĞRULANMADI]` · `[VARSAYIM]`

Her iddia **tam olarak bir** etiket taşır. Etiketsiz iddia yazma.

## Rapor şablonu

```
Değişen dosyalar:
  - <yol> — <ne değişti>
Doğrulama:
  - derleme: PASS/FAIL
  - testler: X/Y
  - CorelDRAW: yapıldı/yapılmadı
Sınırlamalar:
  - <doğrulanmamış varsayım>
Sıradaki adım:
  - <tek cümle>
```

## Yasaklar

- Başarısız testi yorumla gizlemek.
- "Muhtemelen çalışır" demek; ya `[VARSAYIM]` yaz ya test et.
- Test edilmemiş yolu "tamamlandı" işaretlemek.
- Performans iddiasını ölçüm olmadan yazmak.
