# ADR-0005 — Kanonik birim ve tolerans politikası

- Durum: **Kabul edildi**
- Tarih: 2026-11-14

## Bağlam

CorelDRAW belge birimi kullanıcıya göre değişir (mm, inç, punto…). Nesting levha ölçüleri ve
Smart Divide geometrisi aynı birimde karşılaştırılmalıdır. Yüzen nokta hataları, teğet/sınır
durumlarında yanlış containment kararına yol açabilir.

## Karar

1. **Çekirdekte tek birim: mm** (`double`). Birim dönüşümü yalnızca Dock katmanında, tek noktada.
2. Tolerans değerleri tek bir yapılandırmada (`GeometryTolerances`) toplanır; sabit sayı dağıtılmaz.

| Parametre | Değer |
|---|---|
| Karşılaştırma epsilon | `1e-4` mm |
| Containment sınır bandı | `1e-3` mm (bu bant → belirsiz) |
| Tessellation | `min(0.05 mm, spacing/10)` |
| Açı | `1e-6` rad, normalize |
| Alan (göreli) | `1e-6` |

3. Epsilon bandında kalan containment sonucu **belirsiz** kabul edilir ve `Problem` üretir;
   sessizce bir tarafa atanmaz.

## Gerekçe

- Kutu harf üretiminde 0.1 mm mertebesi anlamlıdır; 1e-4 mm karşılaştırma gürültüyü bastırır.
- Tek dönüşüm noktası, "iki yerde farklı ölçek" hatalarını yapısal olarak engeller.
- Sınır bandı, teğet konturlarda yanlış ebeveyn atamasını önler.

## Sonuçlar

- UI'da birim seçimi serbest; çekirdek her zaman mm.
- Testler mm cinsinden yazılır.
- Tolerans değişikliği tüm regresyon setini etkiler → ADR güncellemesi gerekir.
