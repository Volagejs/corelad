# Corelad.SmokeTest

Gerçek CorelDRAW 2026 (x64, v27.x) üzerinde P1 dikey dilimini ve P2 desteklenmeyen nesne
kontrolünü doğrulayan konsol harness'ı.
`net48`, x64, STA.

## Güvenlik kuralları

- **Hiçbir belge kaydedilmez.** Yeni belge `Dirty = false` + `Close()` ile kapatılır.
- Kullanıcının açık belgelerine dokunulmaz; `Application.Quit()` çağrılmaz.
- Corel interop DLL'i repoya/çıktıya kopyalanmaz (`Private=false`); çalışma zamanında
  `Programs64\Assemblies\Corel.Interop.VGCore.dll`'den yüklenir.
- Addon klasörüne yazılmaz.

## Kullanım

```powershell
dotnet build tools\Corelad.SmokeTest\Corelad.SmokeTest.csproj -c Debug
tools\Corelad.SmokeTest\bin\Debug\Corelad.SmokeTest.exe
```

CorelDRAW farklı bir klasöre kuruluysa:

```powershell
$env:COREL_PROGRAMS64 = 'D:\Corel\CorelDRAW Graphics Suite\27\Programs64'
```

Çıkış kodu `0` = tüm kontroller PASS, `1` = FAIL.

## API probu

`CreateCurveFromArray` / `SubPath.Closed` / kontrol noktası semantiğini ölçer; belgeyi
kaydetmeden kapatır:

```powershell
tools\Corelad.SmokeTest\bin\Debug\Corelad.SmokeTest.exe --probe
```

## Senaryo

1. `CorelDRAW.Application.27` ProgID'si ile otomasyona bağlan (interop coclass CLSID'i
   bu kurulumda kayıtlı değil).
2. Yeni belge, birim = mm.
3. GDI+ Arial `MUSTAFA` dış konturlarını yaz (9 subpath).
4. Geri oku; segment türleri + uç/kontrol noktaları 1e-3 mm içinde eşleşmeli.
5. Kaynak imzasını al.
6. `SmartDivideService` çalıştır: 7 aday, 2 delik, 0 Error, 7 yeni şekil beklenir.
7. Kaynak imzası değişmemiş olmalı.
8. `Undo(1)`: yalnızca kaynak kalmalı.
9. Belgeyi kaydetmeden kapat.

P2 ilavesi; dikdörtgen, eğri içeren grup, grup içindeki eğri, `cdrCurveShape` PowerClip
çerçevesi ve PowerClip içindeki eğri oluşturur. Her durum `UnsupportedObject` ile atlanmalı;
Smart Divide hiçbir yeni şekil yazmamalıdır. Belge yalnızca harness'ın oluşturduğu geçici
belgedir ve kaydedilmeden kapatılır.

Beklenen çıktı: `SMOKE SONUCU: PASS`.
