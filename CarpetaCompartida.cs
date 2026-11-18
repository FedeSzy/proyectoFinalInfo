using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabajoFinal_Fink_Szymsio_Fio
{
    // Arma las rutas de la carpeta compartida (SSI) y, en Windows, intenta compartirla en la red.
    //
    //   Servidor (ruta local):    C:\JuegoGuerra\Guerra\Guerra.txt
    //   Cliente  (ruta de red):   \\NOMBRE-PC-SERVIDOR\Guerra\Guerra.txt
    public class CarpetaCompartida
    {
        public const string NOMBRE_RECURSO = "Guerra";
        public const string CARPETA_LOCAL_WINDOWS = @"C:\JuegoGuerra\Guerra";

        public static string carpetaLocalPorDefecto()
        {
            string carpeta;
            if (OperatingSystem.IsWindows())
                carpeta = CARPETA_LOCAL_WINDOWS;
            else
                carpeta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "JuegoGuerra", NOMBRE_RECURSO);
            return carpeta;
        }

        // Si el usuario escribió una ruta completa se usa tal cual; si escribió el nombre de una
        // computadora se arma la ruta de red \\PC\Guerra.
        public static string rutaDelServidor(string nombreOCarpeta)
        {
            string ruta = nombreOCarpeta.Trim();
            if (!ruta.Contains('\\') && !ruta.Contains('/'))
                ruta = @"\\" + ruta + @"\" + NOMBRE_RECURSO;
            return ruta;
        }

        // Crea la carpeta y, en Windows, verifica que esté compartida. Devuelve false si no se pudo crear.
        public static bool preparar(string carpeta)
        {
            bool preparada = false;
            try
            {
                Directory.CreateDirectory(carpeta);
                preparada = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("No se pudo crear la carpeta " + carpeta + ": " + e.Message);
            }

            if (preparada && OperatingSystem.IsWindows() && carpeta == CARPETA_LOCAL_WINDOWS)
            {
                if (!estaCompartida())
                    intentarCompartir(carpeta);
            }
            return preparada;
        }

        private static bool estaCompartida()
        {
            bool compartida = false;
            try
            {
                ProcessStartInfo info = new ProcessStartInfo("net", "share");
                info.CreateNoWindow = true;
                info.UseShellExecute = false;
                info.RedirectStandardOutput = true;
                using (Process proceso = Process.Start(info))
                {
                    string salida = proceso.StandardOutput.ReadToEnd();
                    proceso.WaitForExit();
                    compartida = salida.Contains(NOMBRE_RECURSO + " ");
                }
            }
            catch (Exception)
            {
                compartida = false;
            }
            return compartida;
        }

        // Pide permisos de administrador para ejecutar: net share Guerra="C:\JuegoGuerra\Guerra" /grant:Todos,FULL
        private static void intentarCompartir(string carpeta)
        {
            Console.WriteLine("La carpeta todavía no está compartida. Se va a pedir permiso de administrador para compartirla...");
            try
            {
                ProcessStartInfo info = new ProcessStartInfo("net", "share " + NOMBRE_RECURSO + "=\"" + carpeta + "\" /grant:Todos,FULL");
                info.CreateNoWindow = true;
                info.UseShellExecute = true;
                info.Verb = "runas";
                using (Process proceso = Process.Start(info))
                {
                    proceso.WaitForExit();
                    if (proceso.ExitCode == 0)
                        Console.WriteLine("Carpeta compartida como \\\\" + Environment.MachineName + "\\" + NOMBRE_RECURSO);
                    else
                        mostrarInstrucciones(carpeta);
                }
            }
            catch (Exception)
            {
                mostrarInstrucciones(carpeta);
            }
        }

        public static void mostrarInstrucciones(string carpeta)
        {
            Console.WriteLine();
            Console.WriteLine("No se pudo compartir la carpeta automáticamente. Para hacerlo a mano:");
            Console.WriteLine(" 1. Clic derecho en " + carpeta + " > Propiedades > Compartir > Uso compartido avanzado.");
            Console.WriteLine(" 2. Marcar \"Compartir esta carpeta\" con el nombre " + NOMBRE_RECURSO + ".");
            Console.WriteLine(" 3. En Permisos, dar Control total (o Cambiar) al usuario que usará la otra PC.");
            Console.WriteLine(" 4. Verificar también los permisos NTFS en la pestaña Seguridad.");
            Console.WriteLine(" (Alternativa en una consola de administrador: net share " + NOMBRE_RECURSO + "=\"" + carpeta + "\" /grant:Todos,FULL)");
            Console.WriteLine();
        }
    }
}
