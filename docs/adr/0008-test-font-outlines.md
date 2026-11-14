# ADR-0008 — Test bağımlılığı: System.Drawing.Common (yalnızca test)

- Durum: **Kabul edildi**
- Tarih: 2026-11-14

## Bağlam

Gerçek harf dış konturları, sentetik şekillerden farklı olarak çok sayıda kübik bezier,
ince boyunlar ve iç içe delikler içerir. Geometri çekirdeğinin bu gerçek veriyle
sınanması P1 kabul kriteridir. CorelDRAW'ı test sırasında açmak yasaktır (V2 seviyesi
CorelDRAW'sız çalışır).

GDI+ (`System.Drawing.Drawing2D.GraphicsPath.AddString`) Windows'ta kurulu fontların
gerçek dış konturlarını üretir. Bu, test girdisi için yeterlidir.

## Karar

`System.Drawing.Common` **8.0.0** yalnızca `Corelad.Geometry.Tests` projesinde kullanılır.
Üretim projeleri (`Corelad.Geometry`, `Corelad.Dock`) bu pakete bağımlı olmaz.

## Gerekçe

| Aday | Lisans | Değerlendirme |
|---|---|---|
| **System.Drawing.Common 8.0.0** | MIT | Windows'ta GDI+ ile gerçek font konturu; test projesi `net8.0-windows` |
| Kendi font ayrıştırıcısı | — | Gereksiz kapsam; test için değer üretmez |
| CorelDRAW COM ile font → eğri | — | Testler CorelDRAW açmadan çalışmalı (ADR-0004) |
| Sentetik şekiller | — | Gerçek bezier karmaşıklığını temsil etmez |

**Bakım notu:** Microsoft tarafından bakımı sürdürülüyor. 8.0.0, `net8.0-windows` hedefiyle
uyumlu; Windows dışında `EnableUnixSupport` gerekir, ancak proje Windows'a kilitli (ADR-0001).

## Sonuçlar

- Test projesi `net8.0` → `net8.0-windows` olarak değişti.
- Gerçek Arial konturlarıyla `A/B/O/P/R/İ/j/Ç/Ğ/MUSTAFA` davranışı doğrulanıyor.
- Testler font sürümüne duyarlıdır; beklenen sayılar Windows Arial içindir. Farklı font
  sürümünde test kırılırsa bu **regresyon değil ortam farkıdır** ve açıkça raporlanır.
- Test bağımlılığı üretim çıktısına, addon paketine veya `Corelad.Geometry` referanslarına
  girmez; `THIRD-PARTY-NOTICES.md` yalnızca test kapsamında anar.

## Doğrulanmamışlar

- Farklı Windows/font sürümlerinde beklenen delik/aday sayılarının kararlılığı (P2'de
  golden fixture ile sabitlenecek).
