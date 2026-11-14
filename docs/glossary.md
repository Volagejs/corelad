# Glossary — Corelad

| Terim | Tanım | Karıştırılmaması gereken |
|---|---|---|
| **Mantıksal harf** | Bir dış kontur ve ona bağlı deliklerden oluşan tek anlamlı birim | CorelDRAW'ın "nesne" sayısı; bir harf birden fazla alt kontur içerebilir |
| **Dış kontur / gövde** | Harfin dış sınırını oluşturan kapalı kontur (`Role = Outer`) | Delik |
| **Delik / iç kontur** | Gövdenin içinde kalan, malzemenin çıkarıldığı kapalı kontur (`Role = Hole`) | Ayrı bir harf parçası (`İ` noktası gibi) |
| **Alt kontur (subpath)** | Tek bir `Curve` içindeki bağlı segment zinciri | Segment (tek bezier parçası) |
| **Segment** | İki düğüm arasındaki tek bezier veya çizgi parçası | Alt kontur |
| **Containment** | Bir konturun diğerinin **içinde** olması; gerçek nokta-içinde testiyle belirlenir | Kontur yönü (saat yönü) — yön yalnızca çapraz kontroldür |
| **Derinlik** | Containment ağacındaki seviye; 0 = en dış, 1 = delik, 2 = delik içindeki ada | Nesne sırası (Z-order) |
| **Ada** | Çift derinlikte, deliğin içindeki katı bölge (ör. `B` harfinin orta göbeği) | Delik |
| **Ayrık parça** | Gövdeyle geometrik olarak bağlı olmayan, aynı harfe ait parça (`İ` noktası, `ö` noktaları) | Delik |
| **Problem** | Sessizce çözülmeyen geometri durumu; `Severity` ile raporlanır | Hata istisnası (exception) |
| **Tessellation** | Bezier'in analiz için poligon zincirine yaklaştırılması; **yalnızca türetilmiş kopya** | Çıktı geometrisi (çıktı bezier'dir) |
| **Offset / şişirme** | Konturun normal yönünde mesafe kadar büyütülmesi/küçültülmesi | Spacing (parça arası mesafe politikası) |
| **Spacing** | İki parça arasında garanti edilen minimum boşluk | Kenar payı (levha kenarına mesafe) |
| **NFP (No-Fit Polygon)** | İki parçanın değmeden durabileceği konumların sınırı; referans noktasının yasak bölgesi | Bounding box |
| **BLF (Bottom-Left Fill)** | Parçayı mümkün olduğunca sola ve aşağıya yerleştiren sezgisel yöntem | NFP (BLF, NFP'yi kullanır) |
| **Levha (sheet)** | Malzemenin fiziksel sınırı; parçalar bu alana yerleşir | Sayfa (CorelDRAW page) |
| **Kenar payı (margin)** | Levha kenarıyla parça arasında bırakılan boşluk; dört kenar ayrı olabilir | Spacing |
| **Fire (waste)** | Levhada kullanılmayan alan oranı | Levha sayısı |
| **Rijit transform** | Yalnızca öteleme + dönüş içeren dönüşüm; ölçek yok | Serbest afin dönüşüm |
| **Undo grubu** | `BeginCommandGroup`/`EndCommandGroup` ile tek `Ctrl+Z`'de geri alınan işlem kümesi | Ayrı ayrı geri alınan işlemler |
| **Kanonik birim** | Çekirdeğin içinde kullanılan tek birim: mm | Belge birimi (CorelDRAW'da değişebilir) |
| **Fixture** | Test girdisi; kontur seti veya `.cdr`/`.svg` dosyası | Örnek çıktı (golden) |
| **Golden çıktı** | Regresyon testinde beklenen sonuç dosyası | Fixture |
| **Permissive lisans** | MIT/BSD/Apache-2.0/BSL-1.0 gibi, kaynak kodu kapalı üründe kullanmaya izin veren lisans | Copyleft (GPL/LGPL/AGPL) |
