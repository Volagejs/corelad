# src/Corelad.Dock

CorelDRAW 2026 (v27.x) addon'u + WPF dock. Hedef `net48` (ADR-0002).

P1'de COM adaptörü ve Smart Divide servisi tamamlandı. P2'de bağımsız sorun raporu WPF paneli
eklendi; addon giriş noktası, XSLT ile dock kaydı ve CorelDRAW içinde açılması henüz yapılmadı.
Hiçbir dosya CorelDRAW eklenti klasörüne kopyalanmadı.

Yapı:

```
Corel/
  CorelUnits.cs          belge birimi <-> mm (tek dönüşüm noktası)
  CorelTolerances.cs     COM round-trip / imza toleransları
  CorelCurveReader.cs    Shape.Curve -> ContourSet (salt okuma)
  CorelCurveWriter.cs    ContourSet -> yeni Curve (round-trip doğrulamalı)
  CorelShapeSnapshot.cs  kaynak değişmedi kanıtı (deterministik imza)
  SmartDivideService.cs  analiz + tek undo grubu
  SmartDivideRunResult.cs sonuç/rapor modeli
  SmartDivideProblemReportAdapter.cs çalışma sonucunu panel Problem listesine çevirir
Addon.cs                 giriş noktası, UI kaydı (P5)
ViewModels/              durum, komutlar, ilerleme/iptal (P2/P5)
Views/                   ProblemReportPanel (P2); dock'a yerleştirme (P5)
Assets/                  AppUI.xslt, UserUI.xslt, Coreldrw.addon (P5)
```

Kurallar:
- Geometri algoritması burada yazılmaz; `Corelad.Geometry` çağrılır.
- Tüm COM çağrıları UI thread'inde; hesap arka planda POCO kopyasıyla.
- `Corel.Interop.VGCore.dll` referansı kurulu CorelDRAW'dan alınır, repoya konmaz (`Private=false`).
- Kaynak nesneler varsayılan olarak değiştirilmez; uygulama tek undo grubunda.
- Kaynak şekil bağımsız bir `cdrCurveShape` değilse (grup/PowerClip veya iç nesnesi dahil)
  hiçbir şey yazılmaz (`UnsupportedObject`).
- Yazma öncesi detached `Curve` geri okunur; geometri eşleşmezse hiçbir şey eklenmez.

Ayrıntılı sözleşme: `docs/P1-integration-plan.md`.
