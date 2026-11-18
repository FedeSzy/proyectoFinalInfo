using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabajoFinal_Fink_Szymsio_Fio
{
    // Error que se produce cuando un archivo de personaje no se puede leer o no respeta el formato.
    public class ArchivoPersonajeException : Exception
    {
        private string archivo;

        public ArchivoPersonajeException(string archivo, string mensaje) : base(mensaje)
        {
            this.archivo = archivo;
        }

        public string getArchivo()
        {
            return archivo;
        }
    }

    // Lee UN archivo de personaje con el formato acordado en TI (Actividad 5):
    //
    //   NOMBRE
    //   {nombre}
    //   DESCRIPCION
    //   {descripción, puede ocupar varias líneas}
    //   ASCIIART
    //   {dibujo}
    //   FINASCIIART
    //   ATRIBUTOS
    //   FUERZA;{int}
    //   VELOCIDAD;{int}
    //   RESISTENCIA;{int}
    //   INTELIGENCIA;{int}
    //   ENERGIA;{int}
    //   HABILIDAD;{int}
    //
    // La lectura está dividida en un método por sección; la validación de los puntos la hace
    // ValidadorDeAtributos (a través del constructor de Personaje).
    public class LectorDePersonajes
    {
        public const string MARCA_NOMBRE = "NOMBRE";
        public const string MARCA_DESCRIPCION = "DESCRIPCION";
        public const string MARCA_ASCII = "ASCIIART";
        public const string MARCA_FIN_ASCII = "FINASCIIART";
        public const string MARCA_ATRIBUTOS = "ATRIBUTOS";
        public const char SEPARADOR = ';';

        private string ruta;
        private List<string> lineas;
        private int posicion;

        public Personaje leer(string ruta)
        {
            string nombre;
            string descripcion;
            string asciiArt;
            int[] valores;

            this.ruta = ruta;
            this.lineas = leerLineas(ruta);
            this.posicion = 0;

            nombre = leerNombre();
            descripcion = leerDescripcion();
            asciiArt = leerAsciiArt();
            valores = leerAtributos();
            return construirPersonaje(nombre, descripcion, asciiArt, valores);
        }

        private List<string> leerLineas(string ruta)
        {
            List<string> resultado = new List<string>();
            try
            {
                using (StreamReader lector = new StreamReader(ruta, Encoding.UTF8))
                {
                    string linea = lector.ReadLine();
                    while (linea != null)
                    {
                        resultado.Add(linea);
                        linea = lector.ReadLine();
                    }
                }
            }
            catch (FileNotFoundException)
            {
                throw new ArchivoPersonajeException(ruta, "El archivo no existe.");
            }
            catch (DirectoryNotFoundException)
            {
                throw new ArchivoPersonajeException(ruta, "La carpeta del archivo no existe.");
            }
            catch (UnauthorizedAccessException)
            {
                throw new ArchivoPersonajeException(ruta, "No hay permisos para leer el archivo.");
            }
            catch (IOException e)
            {
                throw new ArchivoPersonajeException(ruta, "No se pudo leer el archivo: " + e.Message);
            }
            return resultado;
        }

        private string leerNombre()
        {
            string nombre;
            esperarMarca(MARCA_NOMBRE);
            nombre = siguienteLinea();
            if (nombre == null || nombre.Trim() == "" || esMarca(nombre))
                throw error("Falta el nombre del personaje debajo de " + MARCA_NOMBRE + ".");
            return nombre.Trim();
        }

        private string leerDescripcion()
        {
            List<string> partes = new List<string>();
            string linea;

            esperarMarca(MARCA_DESCRIPCION);
            linea = verLineaActual();
            while (linea != null && linea.Trim() != MARCA_ASCII)
            {
                if (esMarca(linea))
                    throw error("Se encontró " + linea.Trim() + " cuando se esperaba " + MARCA_ASCII + ".");
                partes.Add(linea.Trim());
                posicion++;
                linea = verLineaActual();
            }

            string descripcion = string.Join(" ", partes).Trim();
            if (descripcion == "")
                throw error("Falta la descripción del personaje debajo de " + MARCA_DESCRIPCION + ".");
            return descripcion;
        }

        private string leerAsciiArt()
        {
            StringBuilder dibujo = new StringBuilder();
            string linea;

            esperarMarca(MARCA_ASCII);
            linea = siguienteLinea();
            while (linea != null && linea.Trim() != MARCA_FIN_ASCII)
            {
                dibujo.AppendLine(linea);
                linea = siguienteLinea();
            }
            if (linea == null)
                throw error("Falta la línea " + MARCA_FIN_ASCII + " que cierra el dibujo.");
            return dibujo.ToString();
        }

        private int[] leerAtributos()
        {
            int[] valores = new int[Personaje.ATRIBUTOS.Length];
            bool[] encontrados = new bool[Personaje.ATRIBUTOS.Length];
            string linea;

            esperarMarca(MARCA_ATRIBUTOS);
            linea = siguienteLinea();
            while (linea != null)
            {
                if (linea.Trim() != "")
                    leerLineaDeAtributo(linea, valores, encontrados);
                linea = siguienteLinea();
            }

            for (int i = 0; i < encontrados.Length; i++)
            {
                if (!encontrados[i])
                    throw error("Falta el atributo " + Personaje.ATRIBUTOS[i].ToUpper() + ".");
            }
            return valores;
        }

        private void leerLineaDeAtributo(string linea, int[] valores, bool[] encontrados)
        {
            string[] datos = linea.Split(SEPARADOR);
            int posicionAtributo;
            int valor;

            if (datos.Length != 2)
                throw error("La línea \"" + linea.Trim() + "\" no respeta el formato ATRIBUTO;VALOR.");

            posicionAtributo = Personaje.posicionDeAtributo(datos[0]);
            if (posicionAtributo == -1)
                throw error("\"" + datos[0].Trim() + "\" no es un atributo válido.");
            if (encontrados[posicionAtributo])
                throw error("El atributo " + datos[0].Trim().ToUpper() + " está repetido.");
            if (!int.TryParse(datos[1].Trim(), out valor))
                throw error("El valor de " + datos[0].Trim().ToUpper() + " (\"" + datos[1].Trim() + "\") no es un número entero.");

            valores[posicionAtributo] = valor;
            encontrados[posicionAtributo] = true;
        }

        private Personaje construirPersonaje(string nombre, string descripcion, string asciiArt, int[] valores)
        {
            Personaje personaje;
            try
            {
                personaje = new Personaje(nombre, descripcion, asciiArt,
                    valores[0], valores[1], valores[2], valores[3], valores[4], valores[5]);
            }
            catch (ArgumentException e)
            {
                throw error(e.Message);
            }
            return personaje;
        }

        // ------------------------------------------------------------
        // Auxiliares para recorrer las líneas
        // ------------------------------------------------------------

        // Saltea líneas vacías y exige que la siguiente línea sea la marca indicada.
        private void esperarMarca(string marca)
        {
            string linea = siguienteLineaNoVacia();
            if (linea == null)
                throw error("El archivo termina antes de la sección " + marca + ".");
            if (linea.Trim() != marca)
                throw error("Se esperaba la línea " + marca + " y se encontró \"" + linea.Trim() + "\".");
        }

        private string siguienteLinea()
        {
            string linea = null;
            if (posicion < lineas.Count)
            {
                linea = lineas[posicion];
                posicion++;
            }
            return linea;
        }

        private string siguienteLineaNoVacia()
        {
            string linea = siguienteLinea();
            while (linea != null && linea.Trim() == "")
                linea = siguienteLinea();
            return linea;
        }

        private string verLineaActual()
        {
            string linea = null;
            if (posicion < lineas.Count)
                linea = lineas[posicion];
            return linea;
        }

        private bool esMarca(string linea)
        {
            string texto = linea.Trim();
            return texto == MARCA_NOMBRE || texto == MARCA_DESCRIPCION || texto == MARCA_ASCII
                || texto == MARCA_FIN_ASCII || texto == MARCA_ATRIBUTOS;
        }

        private ArchivoPersonajeException error(string mensaje)
        {
            return new ArchivoPersonajeException(ruta, mensaje);
        }

        public static void mostrarReglasDeEstructura()
        {
            Console.WriteLine("""
            ------------------------------------------------------------
             REGLAS DE ESTRUCTURA DEL ARCHIVO DE PERSONAJE
            ------------------------------------------------------------
             Nombre del archivo: {nombreDelPersonaje}.txt (carpeta Personajes)

             NOMBRE
             {nombre del personaje}
             DESCRIPCION
             {descripción del personaje}
             ASCIIART
             {dibujo ASCII}
             FINASCIIART
             ATRIBUTOS
             FUERZA;{entero}
             VELOCIDAD;{entero}
             RESISTENCIA;{entero}
             INTELIGENCIA;{entero}
             ENERGIA;{entero}
             HABILIDAD;{entero}

             Reglas de los valores:
             - La suma de los seis atributos debe ser exactamente 3000.
             - Cada atributo debe valer entre 250 y 750.
             - Al menos un atributo debe tener 650 puntos o más.
             - Al menos un atributo debe tener 350 puntos o menos.
             - No se permite asignar 500 puntos a los seis atributos.
            ------------------------------------------------------------
            """);
        }
    }
}
