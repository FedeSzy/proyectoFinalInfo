using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabajoFinal_Fink_Szymsio_Fio
{
    public class Partida
    {
        private Jugador jugador1;
        private Jugador jugador2;
        private int turnoActual;
        private List<Ronda> rondas;
        private string estado;

        public Partida(Jugador jugador1, Jugador jugador2, int turnoActual, List<Ronda> rondas, string estado)
        {
            this.jugador1 = jugador1;
            this.jugador2 = jugador2;
            this.turnoActual = turnoActual;
            this.rondas = rondas;
            this.estado = estado;
        }

        public void repartirCartas()
        {
            List<Jugador> jugadores = new List<Jugador>();
            jugadores.add(jugador1);
            jugadores.add(jugador2);
            gestorDeCartas.repartirCartas(jugadores);
        }

        public void iniciarRonda()
        {
            Ronda ronda;
            string atributo;
            Jugador ganador;
            ronda = new Ronda(rondas.size() + 1);
            rondas.add(ronda);
            jugador1.elegirCarta(jugador1.getMazo().getCartas().get(0));
            Console.WriteLine("Tu carta: " + jugador1.getCartaElegida().getPersonaje().getNombre());
            atributo = ronda.elegirAtributo();
            if (atributo == "DESCONECTADO")
            {
                estado = "DESCONECTADO";
                Console.WriteLine("El rival se desconectó");
            }
            else
            {
                ganador = ronda.compararCartas();
                ronda.finalizarRonda();
                if (ganador == jugador1)
                {
                    turnoActual = 1;
                }
                else if (ganador == jugador2)
                {
                    turnoActual = 2;
                }
                if (jugador1.getMazo().getCartas().isEmpty() || jugador2.getMazo().getCartas().isEmpty())
                {
                    estado = "FINALIZADA";
                }
            }
        }

        public Jugador determinarGanador()
        {
            Jugador ganador;
            if (estado == "DESCONECTADO")
            {
                // el rival se desconectó: se rinde y gana automáticamente el jugador que permanece
                ganador = jugador1;
            }
            else if (jugador1.getMazo().getCartas().isEmpty())
            {
                ganador = jugador2;
            }
            else
            {
                ganador = jugador1;
            }
            return ganador;
        }

        public void finalizar()
        {
            Jugador ganador;
            ganador = determinarGanador();
            Console.WriteLine("Fin de la partida. Ganador: " + ganador.getNombre());
            Console.WriteLine("Rondas ganadas: " + jugador1.getPuntos());
            estado = "FINALIZADA";
        }

        public void iniciar()
        {
            string archivo = "cartas.txt";
            bool sincronizado;
            estado = "INICIANDO";
            gestorDeCartas.cargarCartas(archivo);
            sincronizado = comunicacion.sincronizar();
            if (!sincronizado)
            {
                Console.WriteLine("No se pudo sincronizar con el otro jugador");
                estado = "ERROR";
            }
            else
            {
                repartirCartas();
                // el jugador selector de la primera ronda se sortea al azar (1 o 2)
                turnoActual = sortearSelector();
                estado = "EN_JUEGO";
                while (estado == "EN_JUEGO")
                {
                    iniciarRonda();
                }
                finalizar();
            }
        }
    }
}
