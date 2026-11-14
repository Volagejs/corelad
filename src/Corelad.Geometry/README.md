# src/Corelad.Geometry

Saf geometri çekirdeği. **CorelDRAW, WPF, `System.Windows` referansı eklenmez** (ADR-0004).

P0 durumunda yalnızca proje iskeleti vardır; üretim kodu P1'de eklenecek.

Planlanan klasörler (P1):

```
Model/        Point2, BezierSegment, Contour, ContourSet, Part, Sheet, Placement
Analysis/     containment ağacı, tessellation, güvenilir iç nokta
Clipping/     Clipper2 sarmalayıcı (offset, boolean, temizleme)
Nesting/      NFP üretimi, yerleştirici, arama, bağımsız doğrulayıcı
Diagnostics/  Problem, ProblemCode, Severity, raporlar
```

Kurallar:
- Kanonik birim mm; toleranslar `GeometryTolerances` içinde tek yerde (ADR-0005).
- Belirsiz geometri `Problem` olarak döner; sessiz onarım yok.
- Poligonlaştırma yalnızca analiz kopyasında; dışa verilen model bezier'dir.
