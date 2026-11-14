---
name: Project Memory
description: Use when starting or ending a work session, when a decision is made, or when progress, blockers, assumptions, or scope change. Keeps long-lived project state in docs/ so context survives across sessions.
---

# Project Memory

Amaç: uzun süreli kararlar, gereksinimler ve ilerleme **konuşmada değil dosyada** yaşasın.
Bu skill Karpathy'nin "bağlamı dosyada tut, varsayımı saklama" yaklaşımının bu projeye uyarlamasıdır.

## Ne zaman kullanılır

- Oturum başında: mevcut durumu yükle.
- Oturum sonunda: durumu yaz.
- Bir karar verildiğinde: ADR yaz veya güncelle.
- Kapsam, varsayım, risk veya engel değiştiğinde.

## Okuma sırası (oturum başı)

1. `docs/STATUS.md` — faz, engeller, sıradaki adım
2. `docs/PROJECT.md` — kapsam, varsayımlar, riskler
3. `docs/REQUIREMENTS.md` — ilgili gereksinim ID'leri
4. `docs/ARCHITECTURE.md` — dokunulacak katman
5. İlgili `docs/adr/*` — geri alınamaz kararlar

Tüm dosyaları baştan sona okuma; yalnızca görevle ilgili bölümleri oku.

## Yazma kuralları

| Bilgi türü | Nereye | Nereye **değil** |
|---|---|---|
| Faz durumu, sıradaki adım, engel | `docs/STATUS.md` | README, commit mesajı |
| Ürün kapsamı, varsayım, risk | `docs/PROJECT.md` | STATUS |
| Test edilebilir gereksinim | `docs/REQUIREMENTS.md` | serbest metin |
| Geri alınamaz mimari karar | yeni `docs/adr/NNNN-*.md` | ARCHITECTURE gövdesi |
| Doğrulanmış/doğrulanmamış iddia | `docs/VERIFICATION.md` | sohbet |
| Terim tanımı | `docs/glossary.md` | kod yorumu (tanım için) |

## Kurallar

1. **Varsayımı yaz, saklama.** "Muhtemelen böyle" → `[VARSAYIM]` etiketiyle dosyaya.
2. **Durum dosyasını gerçekle senkron tut.** Bitmemiş işi "tamamlandı" yazma.
3. **ADR değiştirilmez, yeni ADR ile geçersiz kılınır.** Eski ADR'ye "Superseded by ADR-XXXX" notu düş.
4. **Tarih ve faz etiketi kullan.** Her STATUS girişi tarih taşır.
5. **Gizli bilgi yazma.** Anahtar, parola, müşteri verisi dokümana girmez.
6. **Kısa tut.** STATUS bir durum panosudur, roman değil.

## Oturum sonu kontrol listesi

- [ ] `docs/STATUS.md` güncel mi (faz, sıradaki adım, engel)?
- [ ] Yeni karar varsa ADR yazıldı mı?
- [ ] Doğrulanmamış iddia `docs/VERIFICATION.md` §5.2'ye eklendi mi?
- [ ] Değişen dosyalar raporlandı mı?
- [ ] Kapsam dışına çıkan iş varsa not düşüldü mü?

## Yasaklar

- Sohbette kalan karar bırakmak.
- STATUS'u ilerleme gibi göstermek için şişirmek.
- Aynı bilgiyi üç dosyada farklı biçimde tutmak (tek kaynak: yukarıdaki tablo).
