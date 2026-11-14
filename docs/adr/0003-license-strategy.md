# ADR-0003 — Lisans stratejisi: yalnızca permissive; GPL/LGPL kodu alınmaz

- Durum: **Kabul edildi**
- Tarih: 2026-11-14

## Bağlam

Ürünün kapalı kaynak/ticari olma olasılığı var. İncelenen nesting kaynaklarının lisansları:

| Kaynak | Lisans | Kod taşınabilir mi? |
|---|---|---|
| `deepnest-next/deepnest` | Üst düzey MIT, ancak `LICENSES.md`: `deepnest.js` = **GPLv3** | **Hayır** |
| `jack000/SVGNest` | MIT | Evet (ama JS/Node; yine de taşınmayacak) |
| `Dexus/svgnest` | MIT | Evet (düşük olgunluk) |
| `tamasmeszaros/libnest2d` | **LGPL-3.0** | Sadece ayrı süreç/DLL + yeniden bağlama hakkı ile; header-only kullanım risklidir |
| `AngusJohnson/Clipper2` | **BSL-1.0** | Evet (permissive) |

## Karar

1. **Yalnızca permissive lisanslı kod** (MIT / BSD / Apache-2.0 / BSL-1.0) projeye girer.
2. **GPL/LGPL/AGPL kaynak kodu kopyalanmaz**, header-only dahi alınmaz.
3. Algoritma **fikirleri** (NFP, Minkowski, BLF, genetik arama) yayınlanmış literatürden bağımsız uygulanır;
   fikirler telif hakkına tabi değildir, kod tabidir.
4. Üçüncü taraf kod alıntılanırsa: kaynak dosyada atıf + `THIRD-PARTY-NOTICES.md` girişi + ADR.
5. Corel lisanslı ikililer (`Corel.Interop.*.dll`, `VGCore.dll`, `*.tlb`) **dağıtılmaz**;
   derleme zamanı referans, çalışma zamanı kurulu uygulamadan yükleme.

## Gerekçe

- GPLv3 viral lisansı, kapalı kaynak ürünü imkânsız kılar.
- LGPL, dinamik bağlama + yeniden bağlama hakkı gerektirir; header-only şablon kütüphanesi bu sınırı
  belirsizleştirir. Hukuki risk, mimari faydadan büyüktür.
- Clipper2 (BSL-1.0) ihtiyacın tamamını (boolean, offset, Minkowski, temizleme) karşılıyor.

## Sonuçlar

- `deepnest` yalnızca **okunur** (davranış/parametre fikri); kodu commit edilmez.
- Nesting çekirdeği kendi yazılır; Clipper2 tek dış geometri bağımlılığıdır.
- Her yeni bağımlılıkta lisans kontrolü zorunlu (AGENTS.md §2).

## Uyulacak kontrol listesi (her bağımlılık)

- [ ] SPDX kimliği net mi?
- [ ] Kapalı kaynak dağıtımda kullanılabilir mi?
- [ ] Atıf/notice gerekiyor mu?
- [ ] Bakım durumu (son commit, arşiv, aktif fork) nedir?
- [ ] NuGet/paket kaynağı güvenilir mi?

## Doğrulanmamışlar

- Corel SDK/EULA kapsamında interop DLL'lerinin müşteri makinesine dağıtımı: **hukuki teyit gerekli**.
  Bu nedenle DLL dağıtılmıyor.
