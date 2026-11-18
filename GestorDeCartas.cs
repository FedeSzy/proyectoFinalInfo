using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabajoFinal_Fink_Szymsio_Fio
{
    // Carga las cartas desde la carpeta Cartas (un archivo .txt por personaje), las mezcla y las reparte.
    //
    // Formato de cada archivo:
    //   NOMBRE
    //   {nombre}
    //   DESCRIPCION
    //   {descripción}
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
    public class GestorDeCartas
    {
        private const int CARTAS_POR_JUGADOR = 7;

        private List<Carta> cartasDisponibles;
        private List<string> errores;

        // Datos del archivo que se está leyendo
        private List<string> lineas;
        private int posicion;

        public GestorDeCartas()
        {
            cartasDisponibles = new List<Carta>();
            errores = new List<string>();
        }

        // Lee todos los archivos de la carpeta. Los archivos con errores se guardan en la lista de errores.
        public void cargarCartas(string carpeta)
        {
            cartasDisponibles = new List<Carta>();
            errores = new List<string>();

            if (!Directory.Exists(carpeta))
            {
                errores.Add(carpeta + ": no existe la carpeta de cartas.");
            }
            else
            {
                string[] archivos = Directory.GetFiles(carpeta, "*.txt");
                Array.Sort(archivos);
                for (int i = 0; i < archivos.Length; i++)
                {
                    try
                    {
                        Carta carta = leerCarta(archivos[i]);
                        if (buscarCarta(carta.getNombre()) != null)
                            errores.Add(Path.GetFileName(archivos[i]) + ": ya existe otra carta llamada " + carta.getNombre() + ".");
                        else
                            cartasDisponibles.Add(carta);
                    }
                    catch (Exception e)
                    {
                        errores.Add(Path.GetFileName(archivos[i]) + ": " + e.Message);
                    }
                }
            }
        }

        // Cada parte del archivo se lee con su propio método. Si algo está mal se lanza una
        // excepción con el motivo, que cargarCartas guarda junto con el nombre del archivo.
        private Carta leerCarta(string ruta)
        {
            lineas = leerLineas(ruta);
            posicion = 0;

            string nombre = leerNombre();
            string descripcion = leerDescripcion();
            string asciiArt = leerAsciiArt();
            List<int> valores = leerAtributos();
            validarValores(valores);

            Personaje personaje = new Personaje(nombre, descripcion, asciiArt,
                valores[0], valores[1], valores[2], valores[3], valores[4], valores[5]);
            return new Carta(cartasDisponibles.Count + 1, personaje);
        }

        private List<string> leerLineas(string ruta)
        {
            List<string> resultado = new List<string>();
            if (!File.Exists(ruta))
                throw new Exception("el archivo no existe.");
            using (StreamReader lector = new StreamReader(ruta))
            {
                string linea = lector.ReadLine();
                while (linea != null)
                {
                    resultado.Add(linea);
                    linea = lector.ReadLine();
                }
            }
            return resultado;
        }

        private string leerNombre()
        {
            esperarMarca("NOMBRE");
            string nombre = siguienteLinea();
            if (nombre == null || nombre.Trim() == "")
                throw new Exception("falta el nombre del personaje.");
            return nombre.Trim();
        }

        private string leerDescripcion()
        {
            esperarMarca("DESCRIPCION");
            string descripcion = siguienteLinea();
            if (descripcion == null || descripcion.Trim() == "")
                throw new Exception("falta la descripción del personaje.");
            return descripcion.Trim();
        }

        private string leerAsciiArt()
        {
            string asciiArt = "";
            esperarMarca("ASCIIART");
            string linea = siguienteLinea();
            while (linea != null && linea.Trim() != "FINASCIIART")
            {
                asciiArt += linea + Environment.NewLine;
                linea = siguienteLinea();
            }
            if (linea == null)
                throw new Exception("falta la línea FINASCIIART.");
            return asciiArt;
        }

        private List<int> leerAtributos()
        {
            List<int> valores = new List<int>();
            bool[] encontrados = new bool[Personaje.ATRIBUTOS.Length];

            for (int i = 0; i < Personaje.ATRIBUTOS.Length; i++)
                valores.Add(0);

            esperarMarca("ATRIBUTOS");
            string linea = siguienteLinea();
            while (linea != null)
            {
                if (linea.Trim() != "")
                {
                    string[] datos = linea.Split(';');
                    if (datos.Length != 2)
                        throw new Exception("la línea \"" + linea + "\" no respeta el formato ATRIBUTO;VALOR.");

                    int posicionAtributo = Personaje.posicionDeAtributo(datos[0]);
                    if (posicionAtributo == -1)
                        throw new Exception(datos[0] + " no es un atributo válido.");
                    if (encontrados[posicionAtributo])
                        throw new Exception("el atributo " + datos[0] + " está repetido.");

                    try
                    {
                        valores[posicionAtributo] = int.Parse(datos[1].Trim());
                    }
                    catch (FormatException)
                    {
                        throw new Exception("el valor de " + datos[0] + " no es un número entero.");
                    }
                    encontrados[posicionAtributo] = true;
                }
                linea = siguienteLinea();
            }

            for (int i = 0; i < encontrados.Length; i++)
            {
                if (!encontrados[i])
                    throw new Exception("falta el atributo " + Personaje.ATRIBUTOS[i].ToUpper() + ".");
            }
            return valores;
        }

        private void validarValores(List<int> valores)
        {
            List<string> problemas = Personaje.validarAtributos(valores);
            if (problemas.Count > 0)
            {
                string mensaje = "";
                foreach (string problema in problemas)
                    mensaje += problema + " ";
                throw new Exception(mensaje.Trim());
            }
        }

        // Saltea las líneas vacías y verifica que la siguiente línea sea la marca indicada
        private void esperarMarca(string marca)
        {
            string linea = siguienteLinea();
            while (linea != null && linea.Trim() == "")
                linea = siguienteLinea();

            if (linea == null)
                throw new Exception("el archivo termina antes de la línea " + marca + ".");
            if (linea.Trim() != marca)
                throw new Exception("se esperaba la línea " + marca + " y se encontró \"" + linea.Trim() + "\".");
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

        // Junta todas las cartas, las mezcla y reparte la misma cantidad a cada jugador:
        // 7 si alcanzan, si no la mitad (si la cantidad es impar, la última no se reparte)
        public void repartirCartas(List<Jugador> jugadores)
        {
            int cantidadPorJugador = CARTAS_POR_JUGADOR;
            Mazo mazoGeneral = new Mazo();

            if (cartasDisponibles.Count < CARTAS_POR_JUGADOR * jugadores.Count)
                cantidadPorJugador = cartasDisponibles.Count / jugadores.Count;

            foreach (Carta carta in cartasDisponibles)
                mazoGeneral.agregarCarta(carta);
            mazoGeneral.mezclar();

            foreach (Jugador jugador in jugadores)
            {
                foreach (Carta carta in mazoGeneral.repartir(cantidadPorJugador))
                {
                    jugador.getMazo().agregarCarta(carta);
                }
            }
        }

        public Carta buscarCarta(string nombre)
        {
            Carta encontrada = null;
            foreach (Carta carta in cartasDisponibles)
            {
                if (carta.getNombre() == nombre)
                    encontrada = carta;
            }
            return encontrada;
        }

        public List<Carta> getCartasDisponibles()
        {
            return cartasDisponibles;
        }

        public bool hayErrores()
        {
            return errores.Count > 0;
        }

        public List<string> getErrores()
        {
            return errores;
        }

        // Muestra el nombre de cada archivo con error y las reglas de estructura, para que se pueda arreglar
        public void mostrarErrores()
        {
            Console.WriteLine();
            Console.WriteLine("ERROR: hay archivos de cartas que no se pueden leer.");
            foreach (string error in errores)
                Console.WriteLine(" - " + error);
            mostrarReglasDeEstructura();
        }

        public static void mostrarReglasDeEstructura()
        {
            Console.WriteLine("""

                REGLAS DE ESTRUCTURA DEL ARCHIVO
                El archivo debe llamarse {nombreDelPersonaje}.txt y estar en la carpeta Cartas.

                NOMBRE
                {nombre del personaje}
                DESCRIPCION
                {descripción del personaje}
                ASCIIART
                {dibujo ascii}
                FINASCIIART
                ATRIBUTOS
                FUERZA;{número entero}
                VELOCIDAD;{número entero}
                RESISTENCIA;{número entero}
                INTELIGENCIA;{número entero}
                ENERGIA;{número entero}
                HABILIDAD;{número entero}

                - La suma de los seis atributos debe ser exactamente 3000.
                - Cada atributo debe valer entre 250 y 750.
                - Al menos un atributo debe tener 650 puntos o más.
                - Al menos un atributo debe tener 350 puntos o menos.
                - No se permite asignar 500 puntos a los seis atributos.
                """);
        }
    }
}
