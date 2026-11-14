using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Corel.Interop.VGCore;

namespace Corelad.SmokeTest
{
    /// <summary>
    /// CorelDRAW otomasyon örneğini ProgID üzerinden başlatır/bağlanır.
    ///
    /// Neden: interop assembly'sindeki <c>ApplicationClass</c> CLSID'i bu kurulumda
    /// kayıtlı değil; kurulum <c>CorelDRAW.Application.27</c> ProgID'sini
    /// <c>{F35B0002-...}</c> CLSID'ine bağlıyor. Bu yüzden <c>new Application()</c>
    /// yerine kayıtlı ProgID kullanılır.
    /// </summary>
    internal static class CorelApplicationFactory
    {
        private static readonly string[] PreferredProgIds =
        {
            "CorelDRAW.Application.27",
            "CorelDRAW.Application",
        };

        public static Application Create()
        {
            var failures = new List<string>();

            for (int i = 0; i < PreferredProgIds.Length; i++)
            {
                string progId = PreferredProgIds[i];
                Type type;
                try
                {
                    type = Type.GetTypeFromProgID(progId, false);
                }
                catch (Exception exception)
                {
                    failures.Add(progId + ": ProgID okunamadı (" + exception.Message + ")");
                    continue;
                }

                if (type == null)
                {
                    failures.Add(progId + ": ProgID kayıtlı değil");
                    continue;
                }

                try
                {
                    var instance = Activator.CreateInstance(type);
                    var application = instance as Application;
                    if (application != null)
                    {
                        Console.WriteLine("CorelDRAW otomasyonu bağlandı: " + progId);
                        return application;
                    }

                    failures.Add(progId + ": nesne IVGApplication arayüzüne çevrilemedi");
                }
                catch (COMException exception)
                {
                    failures.Add(progId + ": COM hatası 0x" + exception.HResult.ToString("X8") + " — " + exception.Message);
                }
            }

            throw new InvalidOperationException(
                "CorelDRAW otomasyon örneği başlatılamadı. Denenenler: " + string.Join(" | ", failures));
        }
    }
}
