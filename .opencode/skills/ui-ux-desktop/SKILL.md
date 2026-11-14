---
name: UI UX Desktop
description: Use when designing or reviewing the WPF dock layout, states, labels, spacing, keyboard access, or error/empty/busy presentation. Desktop-specific UX rules only; never use this skill for geometry or nesting algorithm decisions.
---

# UI UX Desktop

Amaç: WPF dock arayüzünün akışını, görsel tutarlılığını ve kullanılabilirliğini iyileştirmek.
Kaynak: UI/UX Pro Max'in **dil-bağımsız** ilkelerinin masaüstü/WPF uyarlaması.
Web stack (CSS, Tailwind, React) dosyaları alınmamıştır.

## Kapsam sınırı (kesin)

- Bu skill **yalnızca arayüz** içindir.
- Geometri, containment, NFP, yerleştirme, tolerans kararları **bu skill'e bırakılmaz**.
  Onlar `docs/ARCHITECTURE.md` ve `Corelad.Geometry` konusudur.

## 1. Akış

Dock tek ekranda şu sırayı göstermeli:

```
Seçim durumu → Ayarlar → Önizleme → Uygula
```

- Kullanıcı hangi adımda olduğunu her zaman görebilmeli.
- "Uygula" yalnızca önizleme geçerliyken etkin olur.
- Yıkıcı seçenekler (kaynağı kaldır) varsayılan **kapalı** ve görsel olarak ayrılmış olur.
- Smart Divide ve Nesting **ayrı sekmeler**; ayarlar karışmaz.

## 2. Durum matrisi (her ekran için zorunlu)

| Durum | Gösterim | Eylem |
|---|---|---|
| Boş seçim | Açıklayıcı metin + örnek | "Uygula" kapalı |
| Geçersiz seçim | Hangi nesne neden uygun değil | Atlama seçeneği |
| Analiz ediliyor | İlerleme + iptal | İptal edilebilir |
| Problemler var | Liste, tıklanınca vurgu | Parçayı atla / düzelt / iptal |
| Hazır | Önizleme + özet (parça, levha, fire) | Uygula |
| Uygulanıyor | Kısa ilerleme | Bekle |
| Tamamlandı | Özet + "Geri al" ipucu | — |
| Hata | Ne oldu + ne yapmalı | Kopyala/raporla |

Eksik durum = eksik tasarım.

## 3. Görsel tutarlılık

- **8 pt grid**: boşluklar 8/16/24; iç boşluk 12–16.
- Tek vurgu rengi; tehlike için tek kırmızı tonu. Renk tek başına anlam taşımaz (renk körlüğü).
- Yazı ölçeği: 12 (gövde), 14 (vurgu), 11 (yardım). Daha fazla boyut ekleme.
- Sayısal alanlar sağa hizalı, birim etiketi alanın yanında (mm/inç).
- Odak halkası her etkileşimli öğede görünür.

## 4. Erişilebilirlik

- Tüm işlevler klavyeyle erişilebilir; Tab sırası görsel sırayla aynı.
- `Enter` = birincil eylem, `Esc` = iptal/kapat.
- Kısayollar çakışmasın: `Ctrl+Z` her zaman CorelDRAW undo.
- Ekran okuyucu için `AutomationProperties.Name` zorunlu (özellikle ikon butonlar).
- Kontrast: normal metin ≥ 4.5:1.

## 5. Metin

- Türkçe: büyük harf dönüşümünde `i → İ` hatası yapma; görsel dönüşüm yerine doğru metin yaz.
- Kısa, eylem odaklı etiket: "Uygula", "Önizle", "İptal".
- Hata metni: **ne oldu + neden + ne yapmalı**. Teknik kod varsa ayrıntıya sakla.
- Sayı biçimi yerelleştirilmiş (ondalık ayırıcı), birim açık.

## 6. Performans hissi

- 100 ms üzeri her işlemde görsel geri bildirim.
- İlerleme belirsizse "belirsiz ilerleme" göster, sahte yüzde verme.
- İptal her zaman erişilebilir; iptal sonrası arayüz temiz duruma döner.

## 7. Anti-desenler (yapma)

- Bounding-box tabanlı "yerleşti" göstergesi (gerçek kontur değilse yanıltıcı).
- Sessizce düzeltilmiş geometriyi "hazır" göstermek.
- Aynı ekranda 20+ ayar; ileri düzey ayarları katla.
- Yıkıcı işlemi birincil buton yapmak.
- Onaysız otomatik kaydetme.

## 8. İnceleme kontrol listesi

- [ ] Sekiz durumun tamamı tasarlandı mı?
- [ ] "Uygula" yalnızca geçerli önizlemede etkin mi?
- [ ] Kaynak koruma varsayılanı görünür mü?
- [ ] Klavye ile tam akış tamamlanıyor mu?
- [ ] 125% ve 150% DPI'da taşma yok mu?
- [ ] Türkçe metinler kısalmıyor/bozulmuyor mu?
- [ ] Hata mesajı eylem öneriyor mu?
