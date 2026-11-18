using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabajoFinal_Fink_Szymsio_Fio
{
    public class Ronda
    {
        private int numero;
        private string atributoElegido;
        private int puntosJugador1;
        private int puntosJugador2;

        public Ronda(int numero, string atributoElegido, int puntosJugador1, int puntosJugador2)
        {
            this.numero = numero;
            this.atributoElegido = atributoElegido;
            this.puntosJugador1 = puntosJugador1;
            this.puntosJugador2 = puntosJugador2;
        }

        public string elegirAtributo()
        {
            string atributo = "";
            string datos;
            comunicacion.enviarCarta(jugador1.getCartaElegida());
            datos = comunicacion.recibirDatos();
            if (datos == "DESCONECTADO")
            {
                atributo = "DESCONECTADO";
            }
            else
            {
                jugador2.elegirCarta(reconstruirCarta(datos));
                if (esMiTurnoDeElegir())
                {
                    do
                    {
                        Console.WriteLine("Elegí un atributo: fuerza, velocidad, resistencia, inteligencia, energia o habilidad");
                        atributo = Console.ReadLine();
                    } while (!esAtributoValido(atributo));
                    comunicacion.enviarAtributo(atributo);
                }
                else
                {
                    Console.WriteLine("Esperando el atributo elegido por el rival");
                    atributo = comunicacion.recibirDatos();
                }
            }
            atributoElegido = atributo;
            return atributo;
        }

        public Jugador compararCartas()
        {
            Jugador ganador = null;
            int resultado;
            resultado = jugador1.getCartaElegida().compararAtributo(jugador2.getCartaElegida(), atributoElegido);
            if (resultado > 0)
            {
                ganador = jugador1;
            }
            else if (resultado < 0)
            {
                ganador = jugador2;
            }
            else
            {
                // empate: ganador queda en null
            }
            return ganador;
        }

        public void finalizarRonda()
        {
            Jugador ganador;
            Jugador perdedor;
            Carta cartaGanadora;
            Carta cartaPerdedora;
            ganador = compararCartas();
            if (ganador == null)
            {
                Console.WriteLine("Empate: cada jugador conserva su carta");
                jugador1.getMazo().quitarCarta(jugador1.getCartaElegida());
                jugador1.getMazo().agregarCarta(jugador1.getCartaElegida());
                jugador2.getMazo().quitarCarta(jugador2.getCartaElegida());
                jugador2.getMazo().agregarCarta(jugador2.getCartaElegida());
            }
            else
            {
                if (ganador == jugador1)
                {
                    perdedor = jugador2;
                    puntosJugador1 = puntosJugador1 + 1;
                }
                else
                {
                    perdedor = jugador1;
                    puntosJugador2 = puntosJugador2 + 1;
                }
                cartaGanadora = ganador.getCartaElegida();
                cartaPerdedora = perdedor.getCartaElegida();
                perdedor.getMazo().quitarCarta(cartaPerdedora);
                ganador.getMazo().quitarCarta(cartaGanadora);
                // el ganador absorbe la carta del rival y ambas van al final de su mazo
                ganador.getMazo().agregarCarta(cartaGanadora);
                ganador.getMazo().agregarCarta(cartaPerdedora);
                ganador.sumarPunto();
                Console.WriteLine("Ganador de la ronda " + numero + ": " + ganador.getNombre());
            }
        }
    }

}
