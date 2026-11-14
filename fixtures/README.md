# Fixtures — test girdisi sözleşmesi

Fixture'lar, geometri çekirdeğini CorelDRAW açmadan doğrulamak için kullanılan **girdi** dosyalarıdır.
Golden çıktılar (beklenen sonuçlar) `fixtures/golden/` altında tutulur.

## 1. P0 durumu

Klasör yapısı hazır; **fixture içeriği henüz eklenmedi** (P1'de üretilecek).
Gerçek `MUSTAFA.cdr` sağlanana kadar sentetik konturlarla ilerlenir.

```
fixtures/
├── README.md            (bu dosya)
├── synthetic/           (P1) JSON kontur setleri
├── source/              (opsiyonel) gerçek .cdr / .svg girdiler
└── golden/              (P1) beklenen çıktı JSON'ları
```

## 2. JSON kontur formatı (önerilen, P1'de kesinleşecek)

```jsonc
{
  "name": "O-basic",
  "units": "mm",
  "contours": [
    {
      "id": "outer-0",
      "closed": true,
      "segments": [
        { "kind": "cubic", "p0": [0, 0], "p1": [10, 0], "p2": [10, 10], "p3": [0, 10] }
      ]
    }
  ],
  "expected": {
    "logicalLetters": 1,
    "outerCount": 1,
    "holeCount": 1,
    "problems": []
  }
}
```

- Koordinatlar **mm**, Y yukarı pozitif (çekirdek kuralı). CorelDRAW Y aşağı olduğu için dönüşüm
  Dock katmanında yapılır — bu fark **fixture'larda test edilir**.
- `kind`: `line` | `cubic` (kontrol noktaları dahil).
- `expected.problems`: `ProblemCode` değerleri.

## 3. Zorunlu fixture seti (P1–P2)

| Ad | Amaç | Beklenen |
|---|---|---|
| `A-basic` | Tek delikli harf | 1 harf, 1 delik |
| `B-two-holes` | İki delikli harf | 1 harf, 2 delik |
| `O-basic` | Basit halka | 1 harf, 1 delik |
| `P-basic` | Üst delik | 1 harf, 1 delik |
| `R-basic` | Delik + bacak | 1 harf, 1 delik |
| `I-dot` | Ayrık nokta (`İ`) | 2 gövde adayı → onay ister |
| `j-dot` | Ayrık nokta | Aday eşleştirme |
| `c-cedilla` | `Ç` çengeli | 1 harf, doğru containment |
| `g-breve` | `Ğ` | 1 harf |
| `S-curves` | Yüksek eğrilik | Bezier korunumu |
| `tangent-touch` | İki kontur teğet | `TangentialContact` problemi |
| `self-intersect` | Kendini kesen kontur | `SelfIntersection` problemi |
| `open-contour` | Açık kontur | `OpenContour` problemi |
| `nested-3-level` | 3 seviye iç içe | Doğru derinlik/ada |
| `MUSTAFA` | Birleşik yazı | 7 mantıksal harf |

## 4. Golden çıktı kuralları

- Golden dosya, çekirdeğin **tam** çıktısını (kontur sayısı, rol, derinlik, problem listesi,
  alan, node sayısı) içerir.
- Güncelleme yalnızca bilinçli bir davranış değişikliğinde ve ADR/STATUS notuyla yapılır.
- Golden karşılaştırma toleransı: `1e-4` mm (ADR-0005).

## 5. Gerçek dosya kullanımı

- Gerçek `.cdr` dosyaları repoya **commit edilmez** (boyut + gizlilik). `fixtures/source/.gitignore`
  ile hariç tutulur; kullanıcı yerel olarak sağlar.
- Gerçek CorelDRAW testinde kullanılan dosyanın adı ve sürümü `docs/STATUS.md`'ye yazılır.
