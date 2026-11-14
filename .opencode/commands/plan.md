---
description: Değişiklik öncesi analiz, dosya haritası, risk ve küçük adımlardan oluşan plan üretir
agent: build
---

`@plan-first` skill'ini yükle ve şu görev için uygulama planı hazırla:

$ARGUMENTS

Plan şu bölümleri içermeli ve **hiçbir dosya değiştirilmemeli**:

1. **Analiz** — istek tek cümle, ilgili gereksinim ID'leri (`SD-`/`NST-`/`GEN-`/`INT-`/`UX-`).
2. **Varsayımlar** — `[VARSAYIM]` etiketiyle; belirsizlik varsa dur ve sor.
3. **Dosya haritası** — yol, sorumluluk, yeni/değişecek. Katman sınırını kontrol et.
4. **Riskler** — etki + azaltma.
5. **Adımlar** — her adım tek doğrulanabilir sonuç + doğrulama yöntemi.
6. **Kapsam dışı** — bu işte yapılmayacaklar.
7. **Doğrulama planı** — derleme / test / gerçek CorelDRAW.

Sonunda onay iste. Onay gelmeden uygulamaya geçme.
