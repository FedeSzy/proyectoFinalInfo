using System.IO;
using System.Threading;

namespace TrabajoFinal_Fink_Szymsio_Fio
{
    // Basada en la clase ArchivoTexto del TP ClienteServidor.
    // Cambio: los reintentos tienen un límite, así el programa no se queda trabado para siempre
    // si la carpeta compartida deja de estar disponible (leer devuelve null y guardar devuelve false).
    public class ArchivoTexto
    {
        private const int MAXIMO_INTENTOS = 10;

        private string rutaArchivo;

        public ArchivoTexto(string rutaArchivo)
        {
            this.rutaArchivo = rutaArchivo;
        }

        public string getRuta()
        {
            return rutaArchivo;
        }

        public bool existe()
        {
            return File.Exists(rutaArchivo);
        }

        // Devuelve el contenido del archivo, o null si no se pudo leer
        public string leer()
        {
            string contenido = null;
            bool terminado = false;
            int intentos = 0;

            while (!terminado)
            {
                try
                {
                    using (FileStream archivo = new FileStream(rutaArchivo, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (StreamReader lector = new StreamReader(archivo))
                        {
                            contenido = lector.ReadToEnd();
                        }
                    }

                    terminado = true;
                }
                catch (IOException)
                {
                    intentos++;
                    if (intentos >= MAXIMO_INTENTOS)
                        terminado = true;
                    else
                        Thread.Sleep(200);
                }
                catch (UnauthorizedAccessException)
                {
                    terminado = true;
                }
            }

            return contenido;
        }

        // Escribe primero un archivo temporal y después lo copia, para no dejar el archivo a medio escribir.
        // Devuelve false si no se pudo guardar.
        public bool guardar(string contenido)
        {
            bool guardado = false;
            bool terminado = false;
            int intentos = 0;

            while (!terminado)
            {
                try
                {
                    string archivoTemporal = rutaArchivo + ".tmp";

                    using (FileStream archivo = new FileStream(archivoTemporal, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        using (StreamWriter escritor = new StreamWriter(archivo))
                        {
                            escritor.Write(contenido);
                        }
                    }

                    File.Copy(archivoTemporal, rutaArchivo, true);
                    File.Delete(archivoTemporal);

                    guardado = true;
                    terminado = true;
                }
                catch (IOException)
                {
                    intentos++;
                    if (intentos >= MAXIMO_INTENTOS)
                        terminado = true;
                    else
                        Thread.Sleep(200);
                }
                catch (UnauthorizedAccessException)
                {
                    terminado = true;
                }
            }

            return guardado;
        }
    }
}
