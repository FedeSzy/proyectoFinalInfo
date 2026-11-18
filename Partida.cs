using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabajoFinal_Fink_Szymsio_Fio
{
    // Controla la partida completa. jugador1 es el jugador de ESTA computadora y jugador2 el rival.
    // turnoActual indica quién elige el atributo en la próxima ronda: 1 = yo, 2 = el rival.
    public class Partida
    {
        public const string INICIANDO = "INICIANDO";
        public const string EN_JUEGO = "EN_JUEGO";
        public const string FINALIZADA = "FINALIZADA";
        public const string DESCONECTADO = "DESCONECTADO";   // el rival se fue: gana jugador1
        public const string RENDIDO = "RENDIDO";             // este jugador se rindió: gana jugador2
        public const string ERROR = "ERROR";

        private Jugador jugador1;
        private Jugador jugador2;
        private int turnoActual;
        private List<Ronda> rondas;
        private string estado;
        private GestorDeCartas gestorDeCartas;
        private Comunicacion comunicacion;
        private List<string> atributosBloqueados;

        public Partida(string nombreJugador, GestorDeCartas gestorDeCartas, Comunicacion comunicacion)
        {
            this.jugador1 = new Jugador(nombreJugador);
            this.jugador2 = null;
            this.turnoActual = 1;
            this.rondas = new List<Ronda>();
            this.estado = INICIANDO;
            this.gestorDeCartas = gestorDeCartas;
            this.comunicacion = comunicacion;
            this.atributosBloqueados = new List<string>();
        }

        // Constructor para las pruebas (sin conexión)
        public Partida(Jugador jugador1, Jugador jugador2, string estado)
        {
            this.jugador1 = jugador1;
            this.jugador2 = jugador2;
            this.turnoActual = 1;
            this.rondas = new List<Ronda>();
            this.estado = estado;
            this.gestorDeCartas = null;
            this.comunicacion = null;
            this.atributosBloqueados = new List<string>();
        }

        public void iniciar()
        {
            string nombreRival;
            estado = INICIANDO;

            // Si algún archivo de cartas tiene errores, se informa y la partida se da por terminada
            if (gestorDeCartas.hayErrores())
            {
                gestorDeCartas.mostrarErrores();
                Console.WriteLine("La partida se da por terminada. Corregí los archivos y volvé a intentar.");
                estado = ERROR;
            }
            else if (gestorDeCartas.getCartasDisponibles().Count < 2)
            {
                Console.WriteLine("Se necesitan al menos 2 cartas para jugar. La partida se da por terminada.");
                estado = ERROR;
            }
            else
            {
                nombreRival = comunicacion.conectar(jugador1.getNombre());
                if (nombreRival == null)
                {
                    estado = ERROR;
                }
                else
                {
                    jugador2 = new Jugador(nombreRival);
                    Console.WriteLine("Conectado con " + jugador2.getNombre() + ".");
                    if (repartirCartas())
                    {
                        estado = EN_JUEGO;
                        mostrarInicio();
                        while (estado == EN_JUEGO)
                        {
                            iniciarRonda();
                        }
                        finalizar();
                    }
                    else
                    {
                        estado = ERROR;
                        comunicacion.informarError();
                    }
                }
            }
        }

        // El servidor mezcla, reparte y sortea quién empieza; después guarda todo en el archivo compartido.
        // El cliente lee ese reparto y arma los mismos mazos con sus archivos de cartas.
        public bool repartirCartas()
        {
            bool repartido;
            string selector;

            if (comunicacion.esServidor())
            {
                List<Jugador> jugadores = new List<Jugador>();
                jugadores.Add(jugador1);
                jugadores.Add(jugador2);
                gestorDeCartas.repartirCartas(jugadores);

                if (new Random().Next(2) == 0)
                    selector = Comunicacion.SERVIDOR;
                else
                    selector = Comunicacion.CLIENTE;

                repartido = comunicacion.publicarReparto(nombresDeCartas(jugador1), nombresDeCartas(jugador2), selector);
            }
            else
            {
                Console.WriteLine("Esperando el reparto de cartas...");
                repartido = comunicacion.esperarReparto();
                if (repartido)
                {
                    // El servidor es el rival (jugador2) y el cliente soy yo (jugador1)
                    repartido = armarMazo(jugador2, comunicacion.recibirDatos("CARTAS_SERVIDOR"))
                             && armarMazo(jugador1, comunicacion.recibirDatos("CARTAS_CLIENTE"));
                    if (!repartido)
                        Console.WriteLine("Los archivos de cartas de las dos computadoras no son idénticos.");
                }
                selector = comunicacion.recibirDatos("SELECTOR_INICIAL");
            }

            if (selector == comunicacion.getRol())
                turnoActual = 1;
            else
                turnoActual = 2;
            return repartido;
        }

        private string nombresDeCartas(Jugador jugador)
        {
            string nombres = "";
            foreach (Carta carta in jugador.getMazo().getCartas())
            {
                if (nombres != "")
                    nombres += ";";
                nombres += carta.getNombre();
            }
            return nombres;
        }

        private bool armarMazo(Jugador jugador, string nombres)
        {
            bool correcto = nombres != "";
            foreach (string nombre in nombres.Split(';'))
            {
                Carta carta = gestorDeCartas.buscarCarta(nombre);
                if (carta == null)
                    correcto = false;
                else
                    jugador.getMazo().agregarCarta(carta);
            }
            return correcto;
        }

        public void iniciarRonda()
        {
            Ronda ronda = new Ronda(rondas.Count + 1, jugador1, jugador2, comunicacion, turnoActual == 1, getAtributosDisponibles());
            string resultado;

            rondas.Add(ronda);
            mostrarEncabezado(ronda);
            resultado = ronda.jugar();

            if (resultado == Comunicacion.OK)
            {
                Jugador ganador = ronda.getGanador();
                mostrarResultado(ronda);
                bloquearAtributo(ronda.getAtributoElegido());

                // El ganador elige en la próxima ronda. Si hay empate, sigue eligiendo el mismo jugador.
                if (ganador == jugador1)
                    turnoActual = 1;
                else if (ganador == jugador2)
                    turnoActual = 2;

                if (jugador1.getMazo().estaVacio() || jugador2.getMazo().estaVacio())
                {
                    estado = FINALIZADA;
                }
                else
                {
                    if (ronda.getSoySelector() && !pasarRonda(ronda, ganador))
                    {
                        Console.WriteLine("No se pudo escribir en el archivo compartido. Se cancela la partida.");
                        estado = ERROR;
                    }
                    pausa();
                }
            }
            else
            {
                manejarProblema(resultado);
            }
        }

        // El selector guarda el resultado y le pasa el turno al próximo selector
        private bool pasarRonda(Ronda ronda, Jugador ganador)
        {
            string proximoSelector;
            string nombreGanador = "EMPATE";
            int puntosServidor;
            int puntosCliente;

            if (turnoActual == 1)
                proximoSelector = comunicacion.getRol();
            else
                proximoSelector = comunicacion.getRolRival();
            if (ganador != null)
                nombreGanador = ganador.getNombre();

            if (comunicacion.esServidor())
            {
                puntosServidor = jugador1.getPuntos();
                puntosCliente = jugador2.getPuntos();
            }
            else
            {
                puntosServidor = jugador2.getPuntos();
                puntosCliente = jugador1.getPuntos();
            }
            return comunicacion.pasarRonda(ronda.getNumero() + 1, proximoSelector, nombreGanador, puntosServidor, puntosCliente);
        }

        private void manejarProblema(string resultado)
        {
            if (resultado == Ronda.RENDIDO)
            {
                estado = RENDIDO;
                comunicacion.rendirse();
                Console.WriteLine("Te rendiste.");
            }
            else if (resultado == Comunicacion.DESCONECTADO)
            {
                estado = DESCONECTADO;
                Console.WriteLine(jugador2.getNombre() + " se desconectó: se da por rendido.");
            }
            else if (resultado == Comunicacion.DESINCRONIZADO)
            {
                estado = ERROR;
                comunicacion.informarError();
                Console.WriteLine("La información recibida no corresponde al turno esperado. Se cancela la partida.");
            }
            else
            {
                estado = ERROR;
                Console.WriteLine("Error de comunicación. Se cancela la partida.");
            }
        }

        // Después de cada ronda el atributo elegido queda bloqueado.
        // Cuando ya no queda ninguno disponible, se desbloquean todos.
        public void bloquearAtributo(string atributo)
        {
            atributosBloqueados.Add(atributo);
            if (atributosBloqueados.Count == Personaje.ATRIBUTOS.Length)
                atributosBloqueados.Clear();
        }

        public List<string> getAtributosDisponibles()
        {
            List<string> disponibles = new List<string>();
            foreach (string atributo in Personaje.ATRIBUTOS)
            {
                if (!atributosBloqueados.Contains(atributo))
                    disponibles.Add(atributo);
            }
            return disponibles;
        }

        // Pierde el que se queda sin cartas. Si el rival se desconecta, gana el que permanece.
        // Devuelve null si la partida terminó por un error.
        public Jugador determinarGanador()
        {
            Jugador ganador = null;
            if (estado == DESCONECTADO)
                ganador = jugador1;
            else if (estado == RENDIDO)
                ganador = jugador2;
            else if (jugador1.getMazo().estaVacio())
                ganador = jugador2;
            else if (jugador2.getMazo().estaVacio())
                ganador = jugador1;
            return ganador;
        }

        public void finalizar()
        {
            Jugador ganador = determinarGanador();

            Console.WriteLine();
            Console.WriteLine("================================");
            Console.WriteLine("       FIN DE LA PARTIDA");
            Console.WriteLine("================================");
            if (ganador == null)
            {
                Console.WriteLine("La partida terminó sin ganador.");
            }
            else
            {
                Console.WriteLine("Ganador: " + ganador.getNombre());
                // El que eligió en la última ronda guarda el final de la partida en el archivo
                if (estado == FINALIZADA && rondas[rondas.Count - 1].getSoySelector())
                    comunicacion.finalizar(ganador.getNombre());
            }
            Console.WriteLine("Rondas jugadas: " + rondas.Count);
            Console.WriteLine(jugador1.getNombre() + ": " + jugador1.getPuntos() + " rondas ganadas");
            Console.WriteLine(jugador2.getNombre() + ": " + jugador2.getPuntos() + " rondas ganadas");
        }

        private void mostrarInicio()
        {
            Console.WriteLine();
            Console.WriteLine("Cada jugador recibió " + jugador1.getMazo().cantidadCartas() + " cartas.");
            if (turnoActual == 1)
                Console.WriteLine("Por sorteo, empezás eligiendo vos.");
            else
                Console.WriteLine("Por sorteo, empieza eligiendo " + jugador2.getNombre() + ".");
            pausa();
        }

        private void mostrarEncabezado(Ronda ronda)
        {
            Console.WriteLine("===== RONDA " + ronda.getNumero() + " =====");
            Console.WriteLine("Tus cartas: " + jugador1.getMazo().cantidadCartas() + " | Cartas de " + jugador2.getNombre() + ": " + jugador2.getMazo().cantidadCartas());
            if (ronda.getSoySelector())
                Console.WriteLine("Elegís el atributo vos.");
            else
                Console.WriteLine("Elige el atributo " + jugador2.getNombre() + ".");
        }

        // Se informa el ganador. La carta del rival solo se muestra si se la absorbe.
        private void mostrarResultado(Ronda ronda)
        {
            Jugador ganador = ronda.getGanador();
            string atributo = ronda.getAtributoElegido();

            Console.WriteLine();
            Console.WriteLine("Atributo: " + atributo.ToUpper() + " (tu carta: " + jugador1.getCartaElegida().getValor(atributo) + ")");
            if (ganador == null)
            {
                Console.WriteLine("Empate: cada jugador conserva su carta.");
            }
            else
            {
                Console.WriteLine("Ganador de la ronda " + ronda.getNumero() + ": " + ganador.getNombre());
                if (ganador == jugador1)
                {
                    Console.WriteLine("Absorbiste la carta de " + jugador2.getNombre() + ":");
                    jugador2.getCartaElegida().mostrar();
                }
                else
                {
                    Console.WriteLine(jugador2.getNombre() + " se lleva tu carta " + jugador1.getCartaElegida().getNombre() + ".");
                }
            }
        }

        private void pausa()
        {
            Console.WriteLine("\n\nPresione Enter para continuar: ");
            Console.ReadLine();
            Console.Clear();
        }

        public string getEstado()
        {
            return estado;
        }
    }
}
