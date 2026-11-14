# ADR-0004 — Geometri çekirdeğinin ayrılması

- Durum: **Kabul edildi**
- Tarih: 2026-11-14

## Bağlam

Smart Divide ve Nesting'in doğruluğu, CorelDRAW açmadan test edilebilir olmalıdır. Ayrıca aynı çekirdek
ileride başka bir CAD/vektör hedefine bağlanabilir. COM tipleri test edilebilirliği ve taşınabilirliği kırar.

## Karar

İki ayrı derleme:

| Proje | Hedef | İçerik | Yasak |
|---|---|---|---|
| `Corelad.Geometry` | `netstandard2.0` | Bezier modeli, containment, tessellation, Clipper2 sarmalayıcı, NFP, yerleştirici, doğrulayıcı | CorelDRAW, WPF, `System.Windows`, dosya sistemi, UI |
| `Corelad.Dock` | `net48` + WPF | COM ↔ POCO dönüşümü, ViewModel, XAML, ilerleme/iptal, undo | Geometri algoritması |

Bağımlılık yönü **tek yönlüdür**: `Dock → Geometry`. Tersi yasaktır.

## Gerekçe

- Çekirdek `net8` test projesinden referans edilebilir → hızlı, Corel'siz test döngüsü.
- `netstandard2.0`, hem `net48` hem `net8` ile uyumludur (P0'da derleme ile doğrulandı).
- Sorumluluk sınırı net: algoritma hatası çekirdekte, entegrasyon hatası Dock'ta aranır.

## Sonuçlar

- POCO sözleşmesi (`docs/ARCHITECTURE.md` §3) iki katman arasında tek arayüzdür.
- COM nesneleri asla çekirdeğe geçmez; kopyalanmış saf veri geçer.
- Çekirdeğe yeni özellik eklerken Corel kavramı sızdırmamak zorunludur.

## Doğrulama

`Corelad.Geometry.csproj` içinde `Corel` içeren hiçbir `Reference`/`PackageReference` bulunmamalıdır
(P0'da yapısal olarak sağlandı; CI kontrolü P2'de eklenecek).
