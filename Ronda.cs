using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabajoFinal_Fink_Szymsio_Fio
{
    // Un enfrentamiento entre la primera carta de cada mazo.
    // jugador1 es el jugador de ESTA computadora y jugador2 el rival.
    public class Ronda
    {
        public const string RENDIDO = "RENDIDO";

        private int numero;
        private Jugador jugador1;
        private Jugador jugador2;
        private Comunicacion comunicacion;
        private bool soySelector;
        private List<string> atributosDisponibles;
        private string atributoElegido;
        private Jugador ganador;

        public Ronda(int numero, Jugador jugador1, Jugador jugador2, Comunicacion comunicacion, bool soySelector, List<string> atributosDisponibles)
        {
            this.numero = numero;
            this.jugador1 = jugador1;
            this.jugador2 = jugador2;
            this.comunicacion = comunicacion;
            this.soySelector = soySelector;
            this.atributosDisponibles = atributosDisponibles;
            this.atributoElegido = "";
            this.ganador = null;
        }

        // Juega la ronda completa. Devuelve Comunicacion.OK si terminó bien, RENDIDO si el jugador
        // abandonó, o el problema de comunicación (DESCONECTADO, DESINCRONIZADO o ERROR).
        public string jugar()
        {
            string resultado;

            // Las cartas se juegan en el orden en que fueron repartidas
            jugador1.elegirCarta(jugador1.getMazo().verPrimera());
            jugador2.elegirCarta(jugador2.getMazo().verPrimera());

            Console.WriteLine("Tu carta:");
            jugador1.getCartaElegida().mostrar();

            if (soySelector)
                resultado = jugarComoSelector();
            else
                resultado = jugarComoRival();
            return resultado;
        }

        // El selector elige el atributo, envía su carta y espera la carta del rival
        private string jugarComoSelector()
        {
            string resultado = comunicacion.esperarTurno(numero);
            if (resultado == Comunicacion.OK)
            {
                string atributo = elegirAtributo();
                if (atributo == RENDIDO)
                {
                    resultado = RENDIDO;
                }
                else if (!comunicacion.enviarAtributo(atributo) || !comunicacion.enviarCarta(jugador1.getCartaElegida()))
                {
                    resultado = Comunicacion.ERROR;
                }
                else
                {
                    atributoElegido = atributo;
                    Console.WriteLine("Esperando la carta de " + jugador2.getNombre() + "...");
                    resultado = comunicacion.esperarTurno(numero);
                    if (resultado == Comunicacion.OK)
                        resultado = verificarCartaRival(comunicacion.recibirCarta());
                    if (resultado == Comunicacion.OK)
                        finalizarRonda();
                }
            }
            return resultado;
        }

        // El otro jugador confía en el atributo elegido por el selector y responde con su carta
        private string jugarComoRival()
        {
            string resultado;
            string atributo;

            Console.WriteLine("Esperando que " + jugador2.getNombre() + " elija el atributo...");
            resultado = comunicacion.esperarTurno(numero);
            if (resultado == Comunicacion.OK)
            {
                atributo = comunicacion.recibirAtributo();
                if (!atributosDisponibles.Contains(atributo))
                    resultado = Comunicacion.DESINCRONIZADO;
                else
                    resultado = verificarCartaRival(comunicacion.recibirCarta());

                if (resultado == Comunicacion.OK)
                {
                    atributoElegido = atributo;
                    Console.WriteLine(jugador2.getNombre() + " eligió el atributo " + atributo.ToUpper() + ".");
                    finalizarRonda();
                    if (!comunicacion.enviarCarta(jugador1.getCartaElegida()))
                        resultado = Comunicacion.ERROR;
                }
            }
            return resultado;
        }

        // La carta que mandó el rival tiene que ser la primera de su mazo; si no, los datos no
        // corresponden al turno esperado.
        private string verificarCartaRival(string nombreRecibido)
        {
            string resultado = Comunicacion.OK;
            if (nombreRecibido != jugador2.getCartaElegida().getNombre())
                resultado = Comunicacion.DESINCRONIZADO;
            return resultado;
        }

        // Se elige con el menú. Solo aparecen los atributos que no están bloqueados.
        public string elegirAtributo()
        {
            Menu menu = new Menu();
            string atributo;

            foreach (string disponible in atributosDisponibles)
                menu.agregarOpcion(disponible.ToUpper() + " (" + jugador1.getCartaElegida().getValor(disponible) + ")");
            menu.agregarOpcion("Rendirse");

            Console.WriteLine();
            int elegida = menu.elegirOpcion("Elegí el atributo");
            if (elegida == atributosDisponibles.Count)
                atributo = RENDIDO;
            else
                atributo = atributosDisponibles[elegida];
            return atributo;
        }

        // Solo se puede usar un atributo disponible en esta ronda
        public bool establecerAtributo(string atributo)
        {
            bool valido = atributosDisponibles.Contains(atributo);
            if (valido)
                atributoElegido = atributo;
            return valido;
        }

        // Devuelve el jugador con el valor mayor en el atributo elegido, o null si empatan
        public Jugador compararCartas()
        {
            Jugador resultadoGanador = null;
            int resultado;
            resultado = jugador1.getCartaElegida().compararAtributo(jugador2.getCartaElegida(), atributoElegido);
            if (resultado > 0)
            {
                resultadoGanador = jugador1;
            }
            else if (resultado < 0)
            {
                resultadoGanador = jugador2;
            }
            return resultadoGanador;
        }

        // El ganador absorbe la carta del rival y las dos van al final de su mazo.
        // Si empatan, cada carta vuelve al final del mazo de su dueño.
        public void finalizarRonda()
        {
            Jugador perdedor;
            Carta cartaGanadora;
            Carta cartaPerdedora;

            ganador = compararCartas();
            if (ganador == null)
            {
                jugador1.getMazo().quitarCarta(jugador1.getCartaElegida());
                jugador1.getMazo().agregarCarta(jugador1.getCartaElegida());
                jugador2.getMazo().quitarCarta(jugador2.getCartaElegida());
                jugador2.getMazo().agregarCarta(jugador2.getCartaElegida());
            }
            else
            {
                if (ganador == jugador1)
                    perdedor = jugador2;
                else
                    perdedor = jugador1;

                cartaGanadora = ganador.getCartaElegida();
                cartaPerdedora = perdedor.getCartaElegida();
                perdedor.getMazo().quitarCarta(cartaPerdedora);
                ganador.getMazo().quitarCarta(cartaGanadora);
                ganador.getMazo().agregarCarta(cartaGanadora);
                ganador.getMazo().agregarCarta(cartaPerdedora);
                ganador.sumarPunto();
            }
        }

        public int getNumero()
        {
            return numero;
        }

        public string getAtributoElegido()
        {
            return atributoElegido;
        }

        public Jugador getGanador()
        {
            return ganador;
        }

        public bool getSoySelector()
        {
            return soySelector;
        }
    }
}
