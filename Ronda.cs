using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabajoFinal_Fink_Szymsio_Fio
{
    // Un enfrentamiento puntual entre la primera carta de cada mazo.
    // jugador1 es siempre el jugador de ESTA computadora y jugador2 el rival.
    public class Ronda
    {
        public const string OK = "#OK";
        public const string RENDIDO = "#RENDIDO";
        public const string PALABRA_RENDIRSE = "rendirse";

        private int numero;
        private Jugador jugador1;
        private Jugador jugador2;
        private Comunicacion comunicacion;
        private bool soySelector;
        private List<string> atributosDisponibles;
        private string atributoElegido;
        private Jugador ganador;
        private bool finalizada;

        public Ronda(int numero, Jugador jugador1, Jugador jugador2, Comunicacion comunicacion,
                     bool soySelector, List<string> atributosDisponibles)
        {
            this.numero = numero;
            this.jugador1 = jugador1;
            this.jugador2 = jugador2;
            this.comunicacion = comunicacion;
            this.soySelector = soySelector;
            this.atributosDisponibles = new List<string>(atributosDisponibles);
            this.atributoElegido = "";
            this.ganador = null;
            this.finalizada = false;
        }

        // Cada jugador juega la primera carta de su mazo. La carta propia se envía al rival y se
        // recibe la del rival, verificando que sea la que corresponde según el reparto.
        // Devuelve OK o un resultado especial de Comunicacion (DESCONECTADO, ERROR, DESINCRONIZADO).
        public string prepararCartas()
        {
            string resultado = OK;
            Carta miCarta = jugador1.getMazo().verPrimera();
            Carta cartaRival = jugador2.getMazo().verPrimera();

            if (miCarta == null || cartaRival == null)
            {
                resultado = Comunicacion.ERROR;
            }
            else
            {
                jugador1.elegirCarta(miCarta);
                if (comunicacion != null)
                {
                    if (!comunicacion.enviarCarta(numero, miCarta))
                        resultado = Comunicacion.ERROR;
                    else
                        resultado = verificarCartaRival(comunicacion.recibirCarta(numero), cartaRival);
                }
                if (resultado == OK)
                    jugador2.elegirCarta(cartaRival);
            }
            return resultado;
        }

        private string verificarCartaRival(string datoRecibido, Carta cartaEsperada)
        {
            string resultado = OK;
            if (Comunicacion.esEspecial(datoRecibido))
            {
                resultado = datoRecibido;
            }
            else
            {
                string[] partes = datoRecibido.Split('|');
                int id;
                if (!int.TryParse(partes[0], out id) || id != cartaEsperada.getId())
                    resultado = Comunicacion.DESINCRONIZADO;
            }
            return resultado;
        }

        // Si a esta computadora le toca elegir, pide el atributo por teclado y lo envía.
        // Si no, espera el atributo elegido por el rival.
        // Devuelve el atributo, RENDIDO, o un resultado especial de Comunicacion.
        public string elegirAtributo()
        {
            string atributo;
            if (soySelector)
            {
                atributo = pedirAtributo();
                if (atributo != RENDIDO && comunicacion != null && !comunicacion.enviarAtributo(numero, atributo))
                    atributo = Comunicacion.ERROR;
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Esperando el atributo elegido por " + jugador2.getNombre() + "...");
                atributo = comunicacion.recibirAtributo(numero);
                if (!Comunicacion.esEspecial(atributo) && !atributosDisponibles.Contains(atributo))
                    atributo = Comunicacion.DESINCRONIZADO;
            }

            if (atributo != RENDIDO && !Comunicacion.esEspecial(atributo))
                establecerAtributo(atributo);
            return atributo;
        }

        private string pedirAtributo()
        {
            string atributo;
            if (Consola.puedeUsarMenu())
                atributo = pedirAtributoConMenu();
            else
                atributo = pedirAtributoPorTexto();
            return atributo;
        }

        // Se elige con las flechas usando la clase Menu. La última opción es rendirse.
        private string pedirAtributoConMenu()
        {
            List<string> opciones = new List<string>();
            string atributo;
            int elegida;

            Console.WriteLine();
            foreach (string disponible in atributosDisponibles)
                opciones.Add(disponible.ToUpper() + " (" + jugador1.getCartaElegida().getValor(disponible) + ")");
            opciones.Add("Rendirse (abandonar la partida)");

            elegida = Consola.elegirConMenu("Te toca elegir el atributo", opciones);
            if (elegida == atributosDisponibles.Count)
                atributo = RENDIDO;
            else
                atributo = atributosDisponibles[elegida];
            return atributo;
        }

        private string pedirAtributoPorTexto()
        {
            string atributo = null;
            string texto;

            Console.WriteLine();
            Console.WriteLine("Te toca elegir el atributo. Opciones disponibles:");
            for (int i = 0; i < atributosDisponibles.Count; i++)
            {
                Console.WriteLine("  " + (i + 1) + ". " + atributosDisponibles[i].ToUpper()
                    + " (" + jugador1.getCartaElegida().getValor(atributosDisponibles[i]) + ")");
            }
            Console.WriteLine("  (escribí \"" + PALABRA_RENDIRSE + "\" para abandonar la partida)");

            while (atributo == null)
            {
                Console.Write("Atributo (número o nombre): ");
                texto = Console.ReadLine();
                atributo = interpretarAtributo(texto);
                if (atributo == null)
                    Console.WriteLine("Opción inválida o atributo bloqueado. Intentá de nuevo.");
            }
            return atributo;
        }

        // Convierte lo que escribió el usuario en un atributo disponible. Devuelve null si no es válido.
        public string interpretarAtributo(string texto)
        {
            string atributo = null;
            int numeroOpcion;

            if (texto == null || Personaje.normalizarAtributo(texto) == PALABRA_RENDIRSE)
            {
                atributo = RENDIDO;
            }
            else if (int.TryParse(texto.Trim(), out numeroOpcion))
            {
                if (numeroOpcion >= 1 && numeroOpcion <= atributosDisponibles.Count)
                    atributo = atributosDisponibles[numeroOpcion - 1];
            }
            else
            {
                string normalizado = Personaje.normalizarAtributo(texto);
                if (atributosDisponibles.Contains(normalizado))
                    atributo = normalizado;
            }
            return atributo;
        }

        public void establecerAtributo(string atributo)
        {
            string normalizado = Personaje.normalizarAtributo(atributo);
            if (!atributosDisponibles.Contains(normalizado))
                throw new ArgumentException("El atributo " + atributo + " no está disponible en esta ronda.");
            atributoElegido = normalizado;
        }

        // Devuelve el jugador cuya carta tiene el mayor valor en el atributo elegido, o null si empatan.
        public Jugador compararCartas()
        {
            Jugador resultadoGanador = null;
            int resultado;

            if (atributoElegido == "")
                throw new InvalidOperationException("Todavía no se eligió el atributo de la ronda.");

            resultado = jugador1.getCartaElegida().compararAtributo(jugador2.getCartaElegida(), atributoElegido);
            if (resultado > 0)
                resultadoGanador = jugador1;
            else if (resultado < 0)
                resultadoGanador = jugador2;
            return resultadoGanador;
        }

        // Aplica el resultado: si hay ganador, absorbe la carta del rival y ambas van al final de su mazo;
        // si hay empate, cada carta vuelve al final del mazo de su dueño.
        public void finalizarRonda()
        {
            Carta carta1;
            Carta carta2;

            if (!finalizada)
            {
                carta1 = jugador1.getCartaElegida();
                carta2 = jugador2.getCartaElegida();
                ganador = compararCartas();
                if (ganador == null)
                {
                    jugador1.getMazo().quitarCarta(carta1);
                    jugador1.getMazo().agregarCarta(carta1);
                    jugador2.getMazo().quitarCarta(carta2);
                    jugador2.getMazo().agregarCarta(carta2);
                }
                else
                {
                    Jugador perdedor = (ganador == jugador1) ? jugador2 : jugador1;
                    Carta cartaGanadora = ganador.getCartaElegida();
                    Carta cartaPerdedora = perdedor.getCartaElegida();

                    perdedor.getMazo().quitarCarta(cartaPerdedora);
                    ganador.getMazo().quitarCarta(cartaGanadora);
                    ganador.getMazo().agregarCarta(cartaGanadora);
                    ganador.getMazo().agregarCarta(cartaPerdedora);
                    ganador.sumarPunto();
                }
                finalizada = true;
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

        public bool esEmpate()
        {
            return finalizada && ganador == null;
        }

        public bool getSoySelector()
        {
            return soySelector;
        }
    }
}
