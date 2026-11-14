# REQUIREMENTS — Corelad

Durum kodları: `MUST` (zorunlu), `SHOULD` (güçlü tercih), `MAY` (opsiyonel).
Her gereksinim test edilebilir bir kabul kriteri taşır. Kod: `SD` Smart Divide, `NST` Nesting,
`GEN` ortak, `INT` entegrasyon, `UX` arayüz.

---

## 1. Smart Divide

| ID | Öncelik | Gereksinim | Kabul kriteri |
|---|---|---|---|
| SD-001 | MUST | Seçili birleştirilmiş nesneyi mantıksal harflere ayırır | `MUSTAFA` girdisinde 7 mantıksal harf nesnesi |
| SD-002 | MUST | Her harfin iç konturları (delikler) o harfe bağlı kalır | `A, B, O, P, R, Ğ, Ö` örneklerinde delik sayısı korunur |
| SD-003 | MUST | Dış/iç ilişkisi gerçek geometrik containment ile belirlenir | İç nokta testi tabanlı; yön (`IsClockwise`) tek başına karar vermez |
| SD-004 | MUST | Bezier geometrisi korunur | Çıktı segment tipleri ve kontrol noktaları girdiyle eşleşir (tolerans 1e-4 mm) |
| SD-005 | MUST | Kaynak nesne varsayılan olarak korunur | Uygulama sonrası kaynak sayısı/geometrisi değişmez |
| SD-006 | MUST | Önizleme ve tek adımda geri alınabilir uygulama | `BeginCommandGroup`/`EndCommandGroup`; tek `Undo` geri alır |
| SD-007 | MUST | Belirsiz geometri sessizce çözülmez | Kendini kesen/açık/teğet/çoklu ebeveyn → `Problem` kaydı + kullanıcı kararı |
| SD-008 | MUST | Türkçe karakterler desteklenir | `Ç Ğ İ ı Ö Ş Ü` seti ile regresyon testi |
| SD-009 | SHOULD | `İ`, `j`, `ö/ü` gibi ayrık nokta parçaları aday olarak eşleştirilir | Eşleştirme öneri olarak sunulur, otomatik birleştirme varsayılan kapalı |
| SD-010 | SHOULD | Harf sırası okuma yönüne göre belirlenir | Soldan sağa sezgisel sıra; kullanıcı düzeltebilir |
| SD-011 | MAY | Kaynağı gizleme/seçenekle kaldırma | Varsayılan kapalı; açıksa tek undo ile geri döner |

## 2. Nesting

| ID | Öncelik | Gereksinim | Kabul kriteri |
|---|---|---|---|
| NST-001 | MUST | Yerleştirme gerçek konturla yapılır, bounding-box ile değil | İç içe geçen konkav parçalarda bbox çakışmasına rağmen geçerli yerleşim |
| NST-002 | MUST | Levha ölçüsü + birim + ayrı kenar payları (üst/alt/sol/sağ) | Sınır ihlali = 0 |
| NST-003 | MUST | Parçalar arası güvenli mesafe uygulanır | Bağımsız doğrulayıcı minimum mesafeyi onaylar |
| NST-004 | MUST | Tek ve çoklu levha yerleşimi | Levha sayısı raporlanır, parça kaybı = 0 |
| NST-005 | MUST | Dönüş kısıtları: sabit / 90° adımlar / kullanıcı açı listesi | Kısıt dışı açı üretilmez |
| NST-006 | SHOULD | Renk/katman/gövde grubuna göre ayırma | Gruplar aynı levhada karışmaz (seçimle) |
| NST-007 | MAY | Küçük parçaların büyük parça deliklerine yerleştirilmesi | Etkinse delik içi yerleşim doğrulanır; varsayılan kapalı |
| NST-008 | MUST | Sabitlenmiş parçalar korunur | Sabit parçaların konumu değişmez, çakışma kontrol edilir |
| NST-009 | MUST | Süre bütçeli arama; levha sayısı ve fire azaltılır | Bütçe aşımı yok; iptal < 200 ms |
| NST-010 | MUST | Önizleme, iptal, yeniden deneme | Her üç akış da çalışır; iptalde modele yazma yok |
| NST-011 | MUST | Uygulama öncesi bağımsız doğrulama | Çakışma/boundary/spacing ihlali varsa uygulama engellenir |
| NST-012 | MUST | Uygulama yalnızca rijit transform yazar | Bezier kontrol noktaları yalnız öteleme+dönüş ile değişir |

## 3. Ortak / genel

| ID | Öncelik | Gereksinim | Kabul kriteri |
|---|---|---|---|
| GEN-001 | MUST | Kanonik birim mm; tek dönüşüm noktası | Birim testleri |
| GEN-002 | MUST | Açık tolerans politikası (bkz. ARCHITECTURE §6) | Sabit kodlanmış eşik yok |
| GEN-003 | MUST | Hatalı/eksik geometri sessizce düzeltilmez | `Problem` listesi döner |
| GEN-004 | MUST | Sonuç deterministik | Aynı girdi → aynı çıktı (seed sabit) |
| GEN-005 | MUST | Otomatik kaydetme yok | `Save*` çağrısı yok (kod incelemesi) |
| GEN-006 | MUST | Geometri çekirdeği CorelDRAW'dan bağımsız derlenir | `Corelad.Geometry` Corel referansı içermez |

## 4. Entegrasyon

| ID | Öncelik | Gereksinim | Kabul kriteri |
|---|---|---|---|
| INT-001 | MUST | Addon `net48` + WPF dock olarak CorelDRAW içinde açılır | Gerçek CorelDRAW v27.2'de dock görünür |
| INT-002 | MUST | Tüm COM çağrıları UI thread'inde | Çapraz thread istisnası yok |
| INT-003 | MUST | Uzun işler arka planda, iptal edilebilir | UI donma < 100 ms |
| INT-004 | MUST | Seçim değişikliği olayı dinlenir | `SelectionChange` ile UI güncellenir |
| INT-005 | SHOULD | Sürüm uyumsuzluğunda anlaşılır hata | v27 dışı çalıştırmada uyarı |
| INT-006 | MUST | Corel ikilileri repoda/dağıtımda taşınmaz | Paket içeriği denetimi |

## 5. Arayüz / UX

| ID | Öncelik | Gereksinim | Kabul kriteri |
|---|---|---|---|
| UX-001 | MUST | Akış: Seçim → Ayarlar → Önizleme → Uygula | Tek ekranda net durum |
| UX-002 | MUST | Problem listesi tıklanınca ilgili geometri vurgulanır | Seçim senkron |
| UX-003 | MUST | Boş seçim / geçersiz seçim / hesap / hata durumları ayrı gösterilir | Durum matrisi testi |
| UX-004 | MUST | İlerleme + iptal görünür; iptal sonrası model temiz | Test |
| UX-005 | SHOULD | Klavye ile tam kullanım, odak sırası görünür | Erişilebilirlik kontrol listesi |
| UX-006 | SHOULD | Türkçe metinlerde taşma/kısaltma yok | 125% ve 150% DPI kontrolü |
| UX-007 | MUST | Yıkıcı işlem öncesi onay, varsayılan güvenli seçenek | Onay diyaloğu |

---

## 6. Açık sorular

| # | Soru | Etkilediği faz | Varsayılan (onay yoksa) |
|---|---|---|---|
| OQ-1 | Ürün kapalı kaynak ticari mi? | Lisans | Kapalı kaynak kabul edilir; permissive-only |
| OQ-2 | CorelDRAW 2024/2025 desteği gerekiyor mu? | INT | Yalnızca v27 |
| OQ-3 | Arayüz WPF dock mu? | UX | WPF dock |
| OQ-4 | Smart Divide sonrası kaynak gizlensin mi? | SD-011 | Hayır, korunur |
| OQ-5 | `İ` noktası otomatik birleştirilsin mi? | SD-009 | Hayır, onay istenir |
| OQ-6 | Kod imzalama sertifikası gerekli mi? | P5 | Gerekli değil varsayımı |
| OQ-7 | Gerçek `MUSTAFA.cdr` fixture sağlanacak mı? | P1 | Sentetik fixture ile ilerlenir |
