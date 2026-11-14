# ADR-0002 — Entegrasyon yolu: net48 C# addon + WPF dock

- Durum: **Kabul edildi**
- Tarih: 2026-11-14

## Bağlam

CorelDRAW içinde kalıcı bir panel (dock) deneyimi, uzun süren hesaplarda kilitlenmeyen UI, tek adımda
geri alınabilir uygulama ve test edilebilir bir geometri çekirdeği gerekiyor.
CorelDRAW süreci .NET Framework tabanlıdır; süreç içinde .NET 8 yüklenemez.

Kurulu üçüncü taraf kanıt: `Addons\VolageCutTools\` → `.NETFramework,Version=v4.8` + WPF derlemesi,
`CorelDrw.addon` işaret dosyasıyla yüklenen çalışan bir dock addon'u.

## Karar

- **C# addon + WPF dock**, hedef çerçeve **net48**.
- Addon klasörü: `<Corel>\Programs64\Addons\Corelad\` + boş `Coreldrw.addon`.
- UI: `AppUI.xslt` + `UserUI.xslt`.
- Corel interop derleme zamanında **referans** olarak kurulu uygulamadan alınır; repoya/dağıtıma konmaz.

## Gerekçe

| Seçenek | Değerlendirme |
|---|---|
| VBA (GMS) | Hızlı ama test edilemez, UI zayıf, bakımı zor → yalnızca yedek |
| VSTA | Kullanıcı makinesinde VSTA runtime/IDE bağımlılığı; dışarıda test zor |
| COM dış uygulama | Dock deneyimi yok; süreçler arası UI |
| **net48 addon + WPF** | Corel içinde dock, `dotnet build` ile derlenebilir, olay dinleme, undo grubu, üçüncü taraf kanıtı |
| HTML web docker | Eski JS motoru, senkron `window.external`, ağır hesapta UI kilitlenmesi, dosya sistemi güvenlik uyarısı |

## Sonuçlar

- `Corelad.Dock` net48; `Corelad.Geometry` netstandard2.0 (hem net48 hem net8'den referans edilebilir).
- Tüm COM çağrıları UI thread'inde; hesap arka planda POCO kopyasıyla.
- Visual Studio gerekmiyor; `dotnet` SDK 8 ile net48 derlenebildiği P0'da doğrulandı.
- Debugger deneyimi VS kadar iyi olmayabilir; kabul edilen sınırlama.

## Doğrulanmamışlar

- XSLT'nin v27.2'de dock'u doğru açması.
- Kullanıcı bazlı addon yolu.
- `Layer.CreateCurve` + undo grubu etkileşimi.

## Alternatifler

| Alternatif | Neden reddedildi |
|---|---|
| VSTA | Dağıtım/test kırılganlığı |
| HTML docker (birincil) | Performans ve güvenlik |
| WebView2 tabanlı dock | Değerlendirilebilir; P5'te yeniden bakılabilir, şimdilik kapsam dışı |
