using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TrabajoFinal_Fink_Szymsio_Fio
{
    // Acceso de bajo nivel al archivo de partida compartida (Guerra.txt).
    // El archivo guarda una línea CLAVE=VALOR por dato.
    //
    // Cómo se evitan inconsistencias (SSI, Actividad 5):
    //  - Para ESCRIBIR se abre el archivo con FileShare.None: mientras una computadora lo modifica,
    //    nadie más puede abrirlo (ni para leer ni para escribir). Lectura, modificación y escritura
    //    se hacen con el archivo bloqueado, así no se pierden datos escritos por el otro jugador.
    //  - Para LEER se abre con FileShare.Read: no se puede leer un archivo a medio escribir.
    //  - Si el archivo está ocupado o la red no responde, se reintenta cada cierto tiempo.
    //    Nunca se asume que una operación terminó bien: los métodos devuelven null / false si fallan.
    public class ArchivoCompartido
    {
        private const int MILISEGUNDOS_ENTRE_INTENTOS = 200;

        private string ruta;
        private object candado;   // evita que dos hilos del mismo programa usen el archivo a la vez

        public ArchivoCompartido(string ruta)
        {
            this.ruta = ruta;
            this.candado = new object();
        }

        public string getRuta()
        {
            return ruta;
        }

        public bool existe()
        {
            bool resultado = false;
            try
            {
                resultado = File.Exists(ruta);
            }
            catch (Exception)
            {
                resultado = false;
            }
            return resultado;
        }

        // Devuelve los datos del archivo o null si no se pudo leer (no existe, no hay red, sin permisos...).
        // Si el archivo está ocupado por la otra computadora reintenta durante unos instantes.
        public Dictionary<string, string> leer()
        {
            Dictionary<string, string> datos = null;
            bool terminado = false;
            int intentos = 0;

            lock (candado)
            {
                while (!terminado)
                {
                    try
                    {
                        using (FileStream archivo = new FileStream(ruta, FileMode.Open, FileAccess.Read, FileShare.Read))
                        using (StreamReader lector = new StreamReader(archivo, Encoding.UTF8))
                        {
                            datos = interpretar(lector.ReadToEnd());
                        }
                        terminado = true;
                    }
                    catch (FileNotFoundException)
                    {
                        terminado = true;
                    }
                    catch (DirectoryNotFoundException)
                    {
                        terminado = true;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        terminado = true;
                    }
                    catch (IOException)
                    {
                        // Archivo ocupado o problema de red: se reintenta un par de segundos.
                        intentos++;
                        if (intentos >= 10)
                            terminado = true;
                        else
                            Thread.Sleep(MILISEGUNDOS_ENTRE_INTENTOS);
                    }
                }
            }
            return datos;
        }

        // Crea (o reemplaza) el archivo con los datos indicados.
        public bool crear(Dictionary<string, string> datos, int segundosMaximos)
        {
            return escribir(datos, true, segundosMaximos);
        }

        // Agrega o modifica las claves indicadas, conservando el resto del contenido.
        public bool actualizar(Dictionary<string, string> cambios, int segundosMaximos)
        {
            return escribir(cambios, false, segundosMaximos);
        }

        private bool escribir(Dictionary<string, string> cambios, bool reemplazarTodo, int segundosMaximos)
        {
            bool escrito = false;
            DateTime limite = DateTime.Now.AddSeconds(segundosMaximos);
            bool seguir = true;

            lock (candado)
            {
                while (seguir)
                {
                    try
                    {
                        escribirBloqueado(cambios, reemplazarTodo);
                        escrito = true;
                        seguir = false;
                    }
                    catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
                    {
                        if (DateTime.Now >= limite)
                            seguir = false;
                        else
                            Thread.Sleep(MILISEGUNDOS_ENTRE_INTENTOS);
                    }
                }
            }
            return escrito;
        }

        // Lee, modifica y vuelve a escribir el archivo sin soltarlo en el medio (FileShare.None).
        private void escribirBloqueado(Dictionary<string, string> cambios, bool reemplazarTodo)
        {
            FileMode modo = reemplazarTodo ? FileMode.Create : FileMode.OpenOrCreate;
            UTF8Encoding utf8SinBOM = new UTF8Encoding(false);

            using (FileStream archivo = new FileStream(ruta, modo, FileAccess.ReadWrite, FileShare.None))
            {
                Dictionary<string, string> datos = new Dictionary<string, string>();
                if (!reemplazarTodo)
                {
                    using (StreamReader lector = new StreamReader(archivo, utf8SinBOM, false, 1024, true))
                    {
                        datos = interpretar(lector.ReadToEnd());
                    }
                }
                foreach (KeyValuePair<string, string> cambio in cambios)
                {
                    datos[cambio.Key] = limpiar(cambio.Value);
                }

                archivo.SetLength(0);
                archivo.Position = 0;
                using (StreamWriter escritor = new StreamWriter(archivo, utf8SinBOM, 1024, true))
                {
                    escritor.Write(convertirATexto(datos));
                    escritor.Flush();
                }
                archivo.Flush(true);
            }
        }

        public static Dictionary<string, string> interpretar(string contenido)
        {
            Dictionary<string, string> datos = new Dictionary<string, string>();
            string[] lineas = contenido.Replace("\r", "").Split('\n');
            foreach (string linea in lineas)
            {
                int igual = linea.IndexOf('=');
                if (igual > 0)
                {
                    datos[linea.Substring(0, igual).Trim()] = linea.Substring(igual + 1).Trim();
                }
            }
            return datos;
        }

        private static string convertirATexto(Dictionary<string, string> datos)
        {
            StringBuilder texto = new StringBuilder();
            foreach (KeyValuePair<string, string> dato in datos)
            {
                texto.Append(dato.Key).Append('=').Append(dato.Value).Append('\n');
            }
            return texto.ToString();
        }

        // Un valor no puede tener saltos de línea porque rompería el formato CLAVE=VALOR.
        private static string limpiar(string valor)
        {
            string resultado = "";
            if (valor != null)
                resultado = valor.Replace("\r", " ").Replace("\n", " ").Trim();
            return resultado;
        }
    }
}
