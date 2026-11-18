using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabajoFinal_Fink_Szymsio_Fio
{
    public class GestorDeCartas
    {
        private List<Carta> cartasDisponibles;

        public GestorDeCartas(List<Carta> cartasDisponibles)
        {
            this.cartasDisponibles = cartasDisponibles;
        }

        public void cargarCartas(string archivo)
        {
            bool legible;
            legible = archivoEsLegible(archivo);
            if (!legible)
            {
                Console.WriteLine("Error en el archivo: " + archivo);
                mostrarReglasDeEstructura();
                Console.WriteLine("La partida se da por terminada");
                terminarPrograma();
            }
            else
            {
                cartasDisponibles = leerCartasDelArchivo(archivo);
            }
        }

        public void mezclarCartas()
        {
            Mazo mazoAuxiliar = new Mazo();
            foreach (Carta carta in cartasDisponibles)
            {
                mazoAuxiliar.agregarCarta(carta);
            }
            mazoAuxiliar.mezclar();
            cartasDisponibles = mazoAuxiliar.getCartas();
        }

        public void repartirCartas(List<Jugador> jugadores)
        {
            int cantidadPorJugador = 7;
            int totalCartas;
            Mazo mazoGeneral = new Mazo();
            totalCartas = cartasDisponibles.Count();
            if (totalCartas < 14)
            {
                cantidadPorJugador = totalCartas / 2;
            }
            mezclarCartas();
            foreach (Carta c in cartasDisponibles)
            {
                mazoGeneral.agregarCarta(c);
            }
            foreach (Jugador jugador in jugadores)
            {
                foreach (Carta carta in mazoGeneral.repartir(cantidadPorJugador))
                {
                    jugador.getMazo().agregarCarta(carta);
                }
            }
        }
    }

}
