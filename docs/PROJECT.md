# PROJECT — Corelad

> Bu belge projenin kalıcı bağlamıdır. Kararlar değiştiğinde burada değil, ADR'de gerekçesiyle güncellenir;
> burada yalnızca güncel özet tutulur.

## 1. Vizyon

Kutu harf (channel letter) üretiminde CorelDRAW'da elle yapılan iki zahmetli işi güvenilir biçimde
otomatikleştirmek:

- yazının harflere ayrılması (iç/dış kontur bütünlüğü korunarak),
- parçaların gerçek konturlarına göre levhaya yerleştirilmesi (fire azaltma).

Başarı ölçütü "güzel görünmesi" değil, **üretilebilir, doğrulanmış ve geri alınabilir** çıktı üretmesidir.

## 2. Ürün kapsamı

### 2.1 Smart Divide
- Birleştirilmiş tek nesneyi (veya seçimi) mantıksal harflere ayırma.
- Her mantıksal harf = 1 dış kontur + kendisine ait iç konturlar (delikler).
- Gerçek bezier geometrisinin korunması.
- Containment ilişkisinin geometriyle (yalnızca kontur yönüyle değil) belirlenmesi.
- Belirsiz/hatalı geometride durma ve kullanıcıya karar sunma.
- Türkçe karakterler ve ayrık parçalar (`İ` noktası, `j`, `ö/ü` noktaları) desteği.

### 2.2 Nesting
- Gerçek kontur yerleştirmesi (NFP tabanlı).
- Levha ölçüsü ve ayrı kenar payları; parça arası güvenli mesafe.
- Tek/çoklu levha; dönüş kısıtları (sabit, 90° adımlar, kullanıcı açı listesi).
- Renk/katman/gövde grubuna göre ayırma.
- Opsiyonel: küçük parçaları büyük parçaların iç boşluklarına yerleştirme.
- Sabitlenmiş (elle konumlandırılmış) parçaların korunması.
- Süre bütçeli arama; önizleme, iptal, yeniden deneme.
- Uygulama öncesi bağımsız çakışma/sınır doğrulaması.

### 2.3 Arayüz
- CorelDRAW içinde dock (WPF).
- Önizleme → onay → tek adımda geri alınabilir uygulama akışı.
- Uzun hesaplarda kilitlenmeyen UI, ilerleme ve iptal.

## 3. Kapsam dışı (şimdilik)

- CorelDRAW 2024/2025 ve macOS desteği.
- Bitmap, PowerClip, efekt uygulanmış nesnelerin otomatik çözümlenmesi (işaretlenir, atlanır).
- CNC G-code / makine sürücüsü üretimi.
- Otomatik kaydetme, bulut, telemetri, uzak hesap.
- Fiyatlandırma / üretim emri yönetimi.

## 4. Paydaşlar ve roller

| Rol | Sorumluluk |
|---|---|
| Ürün sahibi (kullanıcı) | Kapsam, öncelik, kabul |
| Geliştirici (OpenCode) | Analiz, uygulama, doğrulama, dokümantasyon |
| Son kullanıcı (üretim operatörü) | Gerçek çizimlerle kabul testi |

## 5. Kısıtlar

- Windows x64, CorelDRAW 2026 (v27.x) kurulu olmalı.
- Addon `net48` olmak zorunda (CorelDRAW süreci .NET Framework tabanlı).
- CorelDRAW COM nesne modeli UI thread'ine duyarlıdır.
- Corel SDK API referansı üyelik korumalı; bazı semantikler ampirik doğrulanacak.
- Lisans: kapalı kaynak olasılığı → yalnızca permissive bağımlılıklar.
- Bu geliştirme makinesinde PowerShell script kısıtlı; Visual Studio ve MSBuild yok (dotnet CLI yeterli).

## 6. Varsayımlar

| # | Varsayım | Durum |
|---|---|---|
| A1 | Hedef yalnızca CorelDRAW 2026 x64 (v27.x), TR arayüz | Kabul (ADR-0001) |
| A2 | Kurulum yönetici izniyle `Programs64\Addons\` altına kopyalama ile | Varsayım |
| A3 | Ürün kapalı kaynak/ticari olabilir | Kabul (ADR-0003) |
| A4 | Belge birimi kullanıcıya bağlı; kanonik hesap birimi mm | Kabul (ADR-0005) |
| A5 | Parçalar çoğunlukla kapalı bezier konturlar | Varsayım |
| A6 | Tipik 10–500 parça, levha ~1000×2000 mm | Varsayım (performans hedefi) |
| A7 | Tamamen yerel çalışma; ağ/telemetri yok | Kabul |

## 7. Başarı kriterleri

| # | Kriter | Ölçüm |
|---|---|---|
| S1 | `MUSTAFA` örneğinde her harf tek mantıksal nesne, delikler korunmuş | Gerçek CorelDRAW'da nesne/node/alan raporu |
| S2 | Kaynak nesne sayısı ve geometrisi uygulama sonrası değişmemiş | Önce/sonra karşılaştırma |
| S3 | Aynı girdi için çıktı deterministik | Tekrarlı çalıştırma, aynı çıktı |
| S4 | Belirsiz geometri sessizce "çözülmemiş", raporlanmış | Problem listesi testi |
| S5 | Nesting çıktısında çakışma = 0, sınır ihlali = 0, spacing ihlali = 0 | Bağımsız doğrulayıcı |
| S6 | 200 parça için arama süresi bütçeye uyar, iptal < 200 ms | Ölçüm |
| S7 | Uygulama tek undo ile geri alınabilir | Gerçek CorelDRAW testi |

## 8. Riskler

| # | Risk | Etki | Azaltma |
|---|---|---|---|
| R1 | Corel COM semantiği belgelenmemiş (`AppendCurveSegment` kontrol noktaları) | Yanlış geometri | P1'de yaz-oku-karşılaştır testi; doğrulanana kadar `[VARSAYIM]` |
| R2 | `IsPointInside` teğet/sınır durumlarında güvenilmez | Yanlış ebeveyn | Tolerans bandı + belirsizlik raporu |
| R3 | NFP üretimi konkav parçalarda pahalı | UI gecikmesi | Önbellek, sadeleştirme, süre bütçesi, iptal |
| R4 | XSLT UI değişikliği kullanıcı workspace'ini bozabilir | Kurulum sorunu | F8/workspace yedeği prosedürü; ayrı test profili |
| R5 | Addon yükleme yolu sürüm bazlı | Kurulum kırılması | Kurulum betiği + sürüm kontrolü (P5) |
| R6 | Corel ikililerinin yeniden dağıtım hakkı belirsiz | Hukuki | DLL dağıtılmaz; kurulu uygulamadan yükleme |
| R7 | Belge birimi/ölçek hatası | Yanlış ölçü | Tek dönüşüm noktası + birim testleri |
