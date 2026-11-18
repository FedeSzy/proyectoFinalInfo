using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TrabajoFinal_Fink_Szymsio_Fio
{
    // Carga las cartas desde la carpeta de personajes, las mezcla y las reparte.
    // Los archivos con errores se informan y se descartan (casos de prueba de TI).
    public class GestorDeCartas
    {
        public const int CARTAS_POR_JUGADOR = 7;
        public const int MINIMO_DE_CARTAS = 2;

        private List<Carta> cartasDisponibles;
        private List<ArchivoPersonajeException> errores;
        private List<string> avisos;

        public GestorDeCartas()
        {
            cartasDisponibles = new List<Carta>();
            errores = new List<ArchivoPersonajeException>();
            avisos = new List<string>();
        }

        // Lee todos los .txt de la carpeta. Devuelve la cantidad de cartas válidas cargadas.
        // Los archivos se recorren ordenados por nombre para que ambas computadoras numeren igual las cartas.
        public int cargarCartas(string carpeta)
        {
            string[] archivos;
            LectorDePersonajes lector = new LectorDePersonajes();

            cartasDisponibles = new List<Carta>();
            errores = new List<ArchivoPersonajeException>();
            avisos = new List<string>();

            if (!Directory.Exists(carpeta))
            {
                errores.Add(new ArchivoPersonajeException(carpeta, "No existe la carpeta de personajes."));
            }
            else
            {
                archivos = Directory.GetFiles(carpeta, "*.txt");
                Array.Sort(archivos, StringComparer.Ordinal);
                foreach (string archivo in archivos)
                {
                    cargarArchivo(lector, archivo);
                }
            }
            return cartasDisponibles.Count;
        }

        private void cargarArchivo(LectorDePersonajes lector, string archivo)
        {
            Personaje personaje;
            try
            {
                personaje = lector.leer(archivo);
                if (buscarCarta(personaje.getNombre()) != null)
                {
                    errores.Add(new ArchivoPersonajeException(archivo,
                        "Ya existe otro personaje llamado \"" + personaje.getNombre() + "\". Se ignora este archivo."));
                }
                else
                {
                    if (Path.GetFileNameWithoutExtension(archivo) != personaje.getNombre())
                        avisos.Add(Path.GetFileName(archivo) + ": el archivo debería llamarse \"" + personaje.getNombre() + ".txt\".");
                    cartasDisponibles.Add(new Carta(cartasDisponibles.Count + 1, personaje));
                }
            }
            catch (ArchivoPersonajeException e)
            {
                errores.Add(e);
            }
        }

        public bool hayCartasSuficientes()
        {
            return cartasDisponibles.Count >= MINIMO_DE_CARTAS;
        }

        // Cantidad de cartas que recibe cada jugador: 7, o la parte entera de total / jugadores si no alcanzan.
        public int calcularCartasPorJugador(int cantidadJugadores)
        {
            int cantidad = 0;
            if (cantidadJugadores > 0)
            {
                cantidad = CARTAS_POR_JUGADOR;
                if (cartasDisponibles.Count < CARTAS_POR_JUGADOR * cantidadJugadores)
                    cantidad = cartasDisponibles.Count / cantidadJugadores;
            }
            return cantidad;
        }

        // Devuelve las cartas disponibles en un orden aleatorio (no modifica la lista original).
        public List<Carta> mezclarCartas(Random azar)
        {
            Mazo mazoAuxiliar = new Mazo(cartasDisponibles);
            mazoAuxiliar.mezclar(azar);
            return mazoAuxiliar.getCartas();
        }

        // Mezcla y reparte la misma cantidad de cartas a cada jugador. Si el total es impar, sobra una.
        // Con el mismo generador (misma semilla) el resultado es idéntico en las dos computadoras.
        public void repartirCartas(List<Jugador> jugadores, Random azar)
        {
            int cantidadPorJugador = calcularCartasPorJugador(jugadores.Count);
            Mazo mazoGeneral = new Mazo(mezclarCartas(azar));

            foreach (Jugador jugador in jugadores)
            {
                foreach (Carta carta in mazoGeneral.repartir(cantidadPorJugador))
                {
                    jugador.getMazo().agregarCarta(carta);
                }
            }
        }

        public Carta buscarCarta(int id)
        {
            Carta encontrada = null;
            int i = 0;
            while (i < cartasDisponibles.Count && encontrada == null)
            {
                if (cartasDisponibles[i].getId() == id)
                    encontrada = cartasDisponibles[i];
                i++;
            }
            return encontrada;
        }

        public Carta buscarCarta(string nombre)
        {
            Carta encontrada = null;
            int i = 0;
            while (i < cartasDisponibles.Count && encontrada == null)
            {
                if (cartasDisponibles[i].getNombre().ToLower() == nombre.Trim().ToLower())
                    encontrada = cartasDisponibles[i];
                i++;
            }
            return encontrada;
        }

        // Resumen (huella) de las cartas cargadas: si las dos computadoras obtienen el mismo resumen,
        // tienen exactamente los mismos personajes, en el mismo orden y con los mismos valores.
        public string calcularResumen()
        {
            StringBuilder texto = new StringBuilder();
            foreach (Carta carta in cartasDisponibles)
            {
                texto.Append(carta.getId()).Append('|').Append(carta.getNombre());
                foreach (int valor in carta.getPersonaje().getAtributos())
                    texto.Append('|').Append(valor);
                texto.Append('\n');
            }
            byte[] huella = SHA256.HashData(Encoding.UTF8.GetBytes(texto.ToString()));
            return Convert.ToHexString(huella).Substring(0, 16);
        }

        public List<Carta> getCartasDisponibles()
        {
            return new List<Carta>(cartasDisponibles);
        }

        public List<ArchivoPersonajeException> getErrores()
        {
            return new List<ArchivoPersonajeException>(errores);
        }

        public List<string> getAvisos()
        {
            return new List<string>(avisos);
        }

        // Informa por consola cada archivo con error y, si hubo alguno, las reglas de estructura.
        public void mostrarErrores()
        {
            if (errores.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("Se encontraron errores en los archivos de personajes:");
                foreach (ArchivoPersonajeException error in errores)
                {
                    Console.WriteLine(" * " + Path.GetFileName(error.getArchivo()) + ": " + error.Message);
                }
                Console.WriteLine("Esos personajes NO se van a usar en la partida.");
                LectorDePersonajes.mostrarReglasDeEstructura();
            }
            foreach (string aviso in avisos)
            {
                Console.WriteLine(" Aviso: " + aviso);
            }
        }
    }
}
