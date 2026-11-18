using System;
using System.Diagnostics;
using System.IO;

namespace TrabajoFinal_Fink_Szymsio_Fio
{
    // Basada en la clase PartidaCompartida del TP ClienteServidor.
    //   Servidor: C:\JuegoGuerra\Guerra\Guerra.txt   (la carpeta se comparte con el nombre Guerra)
    //   Cliente:  \\NOMBRE-PC-SERVIDOR\Guerra\Guerra.txt
    // Cambios: el cliente también puede escribir la ruta completa de la carpeta (para probar en una
    // sola PC), y si no se puede compartir la carpeta se muestran los pasos en lugar de cortar el programa.
    public class PartidaCompartida
    {
        private const string CARPETA_LOCAL = @"C:\JuegoGuerra\Guerra";
        private const string NOMBRE_COMPARTIDO = "Guerra";
        private const string NOMBRE_ARCHIVO = "Guerra.txt";

        private string rutaArchivo;
        private string nombreServidor;
        private ArchivoTexto archivoTexto;

        public PartidaCompartida(bool esServidor, string nombreServidor)
        {
            if (esServidor)
            {
                this.nombreServidor = Environment.MachineName;
                rutaArchivo = Path.Combine(CARPETA_LOCAL, NOMBRE_ARCHIVO);
            }
            else if (nombreServidor.Contains("\\") || nombreServidor.Contains("/"))
            {
                this.nombreServidor = nombreServidor;
                rutaArchivo = Path.Combine(nombreServidor, NOMBRE_ARCHIVO);
            }
            else
            {
                this.nombreServidor = nombreServidor;
                rutaArchivo = "\\\\" + nombreServidor + "\\" + NOMBRE_COMPARTIDO + "\\" + NOMBRE_ARCHIVO;
            }

            archivoTexto = new ArchivoTexto(rutaArchivo);
        }

        // =========================================
        // INFORMACION
        // =========================================

        public string obtenerNombreServidor()
        {
            return nombreServidor;
        }

        public string obtenerRutaArchivo()
        {
            return rutaArchivo;
        }

        // =========================================
        // CONFIGURACION DEL SERVIDOR
        // =========================================

        public void prepararServidor()
        {
            Directory.CreateDirectory(CARPETA_LOCAL);

            try
            {
                if (!existeRecursoCompartido())
                    compartirCarpeta(NOMBRE_COMPARTIDO, CARPETA_LOCAL);
            }
            catch (Exception)
            {
                Console.WriteLine("No se pudo compartir la carpeta automáticamente. Para hacerlo a mano:");
                Console.WriteLine(" - Clic derecho en " + CARPETA_LOCAL + " > Propiedades > Compartir > Uso compartido avanzado.");
                Console.WriteLine(" - Compartir con el nombre " + NOMBRE_COMPARTIDO + " y dar permiso de lectura y escritura.");
            }
        }

        private bool existeRecursoCompartido()
        {
            bool existe = false;

            ProcessStartInfo psi = new ProcessStartInfo("net", "share")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            using (Process proceso = Process.Start(psi))
            {
                string resultado = proceso.StandardOutput.ReadToEnd();
                proceso.WaitForExit();
                existe = resultado.Contains(NOMBRE_COMPARTIDO);
            }
            return existe;
        }

        private void compartirCarpeta(string nombreCompartido, string rutaCarpeta)
        {
            string argumentos = $"share {nombreCompartido}=\"{rutaCarpeta}\" /grant:Todos,FULL";

            ProcessStartInfo psi = new ProcessStartInfo("net", argumentos)
            {
                CreateNoWindow = true,
                UseShellExecute = true,
                Verb = "runas"
            };

            using (Process proceso = Process.Start(psi))
            {
                proceso.WaitForExit();

                if (proceso.ExitCode != 0)
                {
                    throw new Exception("No se pudo compartir la carpeta.");
                }
            }
        }

        // =========================================
        // OPERACIONES CON EL ARCHIVO
        // =========================================

        public bool existeArchivo()
        {
            return archivoTexto.existe();
        }

        public string leer()
        {
            return archivoTexto.leer();
        }

        public bool guardar(string contenido)
        {
            return archivoTexto.guardar(contenido);
        }
    }
}
