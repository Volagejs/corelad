# ADR-0007 — Geometri bağımlılığı: Clipper2 (BSL-1.0)

- Durum: **Kabul edildi**
- Tarih: 2026-11-14

## Bağlam

Nesting ve Smart Divide şu işlemlere ihtiyaç duyar:

- kapalı konturların birleşimi/kesişimi/farkı (boolean),
- güvenli mesafe için offset (şişirme/küçültme),
- kendi kendini kesen konturların temizlenmesi (union),
- Minkowski toplamı ile NFP üretimi,
- analiz için poligon temizleme/sadeleştirme.

## Karar

`Clipper2` **2.0.0** (NuGet, `netstandard2.0`, **BSL-1.0**) tek dış geometri bağımlılığıdır.

## Gerekçe

| Aday | Lisans | Değerlendirme |
|---|---|---|
| **Clipper2** | BSL-1.0 | Boolean + offset + Minkowski + triangulation; C# API; `netstandard2.0` → net48 uyumlu; permissive |
| libnest2d | LGPL-3.0 | Hazır NFP ama lisans riski (ADR-0003) |
| deepnest | GPLv3 (çekirdek) | Lisans uyumsuz |
| Kendi boolean motoru | — | Gereksiz risk ve maliyet |

**Bakım notu:** `AngusJohnson/Clipper2` arşivlendi; geliştirme `PolyClipping/Clipper2` altında
(BSL-1.0, aktif). 2.0.0 sürümü kararlı ve yayınlanmış durumda. Gelecekte aktif fork'a geçiş
yeni bir ADR gerektirir.

## Sonuçlar

- Offset/boolean/Minkowski tek ve test edilmiş bir motordan gelir.
- **NFP üretimi ve yerleştirme/arama kendi kodumuzdur**; Clipper2 yalnızca geometri primitiflerini sağlar.
- `THIRD-PARTY-NOTICES.md` BSL-1.0 atıf metnini içerecek.
- Tamsayı koordinat ölçeği (Clipper2 integer çalışır) sabit bir ölçek faktörüyle yönetilir;
  mm → tam sayı dönüşümü tek noktada, toleransla uyumlu seçilir (P1'de test edilecek).

## Doğrulanmamışlar

- 2.0.0'ın `net48` çalışma zamanında davranışı (P0'da yalnızca paket mevcudiyeti kontrol edildi).
- Büyük konturlarda integer ölçek taşması sınırı (P3'te ölçülecek).
