# ADR-0001 — Hedef platform: CorelDRAW 2026 x64 (v27.x)

- Durum: **Kabul edildi**
- Tarih: 2026-11-14
- Karar veren: Ürün sahibi (onay), OpenCode (öneri)

## Bağlam

Geliştirme makinesinde CorelDRAW Graphics Suite **27 (2026)**, sürüm **27.2**, TR dil, x64 kurulu.
`Corel.Interop.VGCore.dll` 27.2.0.135 ve `policy.27.0/27.1/27.2.*` yönlendirme dosyaları mevcut.
COM (`CorelDRAW.Application.27`) ve typelib (`CorelDRAW.tlb`, `VGCoreAuto.tlb`) kayıtlı.

## Karar

Birincil ve tek hedef: **CorelDRAW 2026 x64 (v27.x)**.

## Gerekçe

- Tek sürüm hedefi, test matrisini ve addon yükleme karmaşıklığını küçük tutar.
- Interop sürümü sabitlenebilir; kırılma riski azalır.
- Eski sürümlerin COM yüzeyi farklı olabilir; doğrulanmamış uyumluluk iddiası yasak.

## Sonuçlar

- `Corelad.Dock` yalnızca v27 interop'una göre derlenir.
- Farklı sürümde çalıştırılırsa anlaşılır hata verilir (INT-005).
- 2024/2025 desteği istenirse yeni ADR + ayrı test matrisi gerekir.

## Alternatifler

| Alternatif | Neden reddedildi |
|---|---|
| v24–v27 geniş desteği | Doğrulanmamış uyumluluk, test yükü |
| Sürümden bağımsız geç bağlama | Kırılgan, tip güvenliği kaybı |
