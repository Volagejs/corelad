---
name: Plan First
description: Use before any non-trivial implementation, refactor, or file-creation task. Requires analysis, explicit assumptions, affected-file list, risks, and small verifiable steps before code is written.
---

# Plan First

Amaç: kod yazmadan önce **analiz → varsayım → dosya haritası → risk → küçük adımlar**.
Superpowers'ın planlama disiplininin bu projeye uyarlamasıdır; agresif "her şeyden önce skill"
kuralı alınmamıştır.

## Ne zaman kullanılır

- Yeni özellik, yeni modül, yeni bağımlılık.
- Birden fazla dosyayı etkileyen değişiklik.
- Geri alınması zor işlem (COM yazma, undo grubu, şema değişikliği).
- Kullanıcı "nasıl yapacağız?" diye sorduğunda.

Tek satırlık düzeltme veya salt okuma için tam plan gerekmez; yine de değişecek dosyayı söyle.

## Adımlar

### 1. Analiz
- İsteği tek cümlede yeniden yaz. Yanlış anladıysan burada ortaya çıkar.
- İlgili gereksinim ID'lerini listele (`SD-`, `NST-`, `GEN-`, `INT-`, `UX-`).
- Bilinmeyenleri `[VARSAYIM]` olarak ayır.

### 2. Dosya haritası
Her dosya için: **yol — sorumluluk — yeni/değişecek**.
Katman sınırını kontrol et (AGENTS.md §2): algoritma `Corelad.Geometry`'de, COM/UI `Corelad.Dock`'ta.

### 3. Risk tablosu
| Risk | Etki | Azaltma |
|---|---|---|

En az bir risk yaz. "Risk yok" genellikle analiz eksikliğidir.

### 4. Adımlar
Her adım tek bir doğrulanabilir sonuç üretir:

```
1. [adım] → doğrula: [komut / test / gözlem]
2. [adım] → doğrula: ...
```

Adım boyutu: bir incelemeci "bunu onaylıyorum, şunu reddediyorum" diyebilmeli.

### 5. Onay
Büyük değişiklikte kullanıcı onayı bekle. Onay gelmeden dosya oluşturma/değiştirme.
Onay metni şunları içermeli: değişecek dosyalar, kapsam dışı, doğrulama yöntemi.

## Kural: plan ≠ uygulama
Plan onaylandıktan sonra adımları sırayla uygula; plandan saparsan **dur ve bildir**.
Sessizce kapsam genişletme yasak.

## Bu projede ek kurallar

- CorelDRAW'a yazan her adımda "kaynak korunuyor mu?" ve "tek undo ile dönüyor mu?" sorulur.
- Doğrulanmamış COM davranışı plana `[VARSAYIM]` olarak girer ve P1'de ampirik teste bağlanır.
- Yeni bağımlılık → lisans kontrolü + ADR olmadan plana girmez.
