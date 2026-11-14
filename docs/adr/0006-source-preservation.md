# ADR-0006 — Kaynak koruma ve kopyala-üret yaklaşımı

- Durum: **Kabul edildi**
- Tarih: 2026-11-14

## Bağlam

Kullanıcı, Smart Divide ve Nesting'in kaynak çizimi bozmasından veya veri kaybından endişe ediyor.
CorelDRAW'da `Ctrl+K`/`Ctrl+L` gibi işlemler kaynağı tüketir; geri alma tek adımda her zaman güvenilir değildir.

## Karar

1. Varsayılan davranış: **kaynak nesnelere dokunmadan yeni nesne üretmek.**
2. Smart Divide: seçim `Duplicate` edilir, kopya üzerinde `ConvertToCurves` ve analiz yapılır;
   çıktı **yeni** nesnelerdir. Kaynak seçim olduğu gibi kalır.
3. Nesting: yalnızca **rijit transform** (öteleme + dönüş) uygulanır; geometri yeniden üretilmez.
4. Uygulama tek `BeginCommandGroup`/`EndCommandGroup` içinde → tek `Ctrl+Z`.
5. Kaynağı gizleme/kaldırma yalnızca **açık kullanıcı seçeneği** ile ve aynı undo grubunda.
6. Otomatik kaydetme yok.

## Gerekçe

- Veri kaybı, üretim ortamında en pahalı hatadır.
- Kaynak korunduğunda "yanlış ayar → tekrar dene" akışı güvenli olur.
- Tek undo grubu, kullanıcıya geri dönüş garantisi verir.

## Sonuçlar

- Belge nesne sayısı artar; kullanıcıya "kaynak gizle" seçeneği sunulur (varsayılan kapalı).
- Çıktı nesneleri kaynakla ilişkilendirilebilir (grup adı / object data) — P2'de değerlendirilecek.
- Performans: kopyalama maliyeti kabul edilebilir (seçim başına bir kez).

## Doğrulanmamışlar

- `Layer.CreateCurve` ile oluşturulan nesnelerin undo grubunda tek adımda geri alınması → P1 testi.
