using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabajoFinal_Fink_Szymsio_Fio
{
    // Controla el juego completo: conexión, reparto, rondas, atributos bloqueados, turno y resultado.
    // jugador1 es el jugador de ESTA computadora y jugador2 el rival.
    // turnoActual indica quién elige el atributo en la próxima ronda (1 = yo, 2 = rival).
    public class Partida
    {
        public const string INICIANDO = "INICIANDO";
        public const string EN_JUEGO = "EN_JUEGO";
        public const string FINALIZADA = "FINALIZADA";
        public const string DESCONECTADO = "DESCONECTADO";   // el rival abandonó: gana jugador1
        public const string RENDIDO = "RENDIDO";             // este jugador abandonó: gana jugador2
        public const string ERROR = "ERROR";

        private Jugador jugador1;
        private Jugador jugador2;
        private int turnoActual;
        private List<Ronda> rondas;
        private string estado;
        private GestorDeCartas gestorDeCartas;
        private Comunicacion comunicacion;
        private List<string> atributosBloqueados;
        private Random azar;

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
            this.azar = new Random();
        }

        // Constructor para las pruebas: permite armar una partida sin conexión.
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
            this.azar = new Random();
        }

        // Ejecuta el flujo completo de la partida.
        public void iniciar()
        {
            estado = INICIANDO;
            if (prepararPartida())
            {
                repartirCartas();
                turnoActual = sortearSelector();
                estado = EN_JUEGO;
                mostrarInicio();
                while (estado == EN_JUEGO)
                {
                    iniciarRonda();
                }
            }
            finalizar();
        }

        // Conecta con el rival, verifica que ambos tengan las mismas cartas y acuerda la semilla.
        private bool prepararPartida()
        {
            string nombreRival;
            int semilla;

            if (!gestorDeCartas.hayCartasSuficientes())
            {
                Console.WriteLine("No hay suficientes personajes válidos para jugar (mínimo " + GestorDeCartas.MINIMO_DE_CARTAS + ").");
                estado = ERROR;
            }
            else
            {
                Console.WriteLine("Archivo compartido: " + comunicacion.getRutaArchivo());
                nombreRival = comunicacion.conectar(jugador1.getNombre());
                if (nombreRival == null)
                {
                    estado = ERROR;
                }
                else
                {
                    jugador2 = new Jugador(nombreRival);
                    Console.WriteLine("Conectado con " + jugador2.getNombre() + ". Verificando las cartas...");
                    if (!comunicacion.sincronizar(gestorDeCartas.calcularResumen()))
                    {
                        Console.WriteLine("No se pudo sincronizar con el otro jugador: ambas computadoras deben tener");
                        Console.WriteLine("exactamente los mismos archivos de personajes en la carpeta Personajes.");
                        estado = ERROR;
                    }
                    else
                    {
                        semilla = comunicacion.acordarSemilla();
                        if (semilla == -1)
                            estado = ERROR;
                        else
                            azar = new Random(semilla);
                    }
                }
            }
            return estado == INICIANDO;
        }

        // Reparte las cartas. Se arma la lista siempre en el orden SERVIDOR, CLIENTE para que en
        // las dos computadoras cada jugador reciba exactamente las mismas cartas.
        public void repartirCartas()
        {
            List<Jugador> jugadores = new List<Jugador>();
            jugadores.Add(getJugadorServidor());
            jugadores.Add(getJugadorCliente());
            gestorDeCartas.repartirCartas(jugadores, azar);
        }

        // Sortea quién elige el atributo en la primera ronda. Devuelve 1 (yo) o 2 (rival).
        public int sortearSelector()
        {
            string rolSorteado = azar.Next(2) == 0 ? Comunicacion.SERVIDOR : Comunicacion.CLIENTE;
            return rolSorteado == comunicacion.getRol() ? 1 : 2;
        }

        // Crea y ejecuta una ronda, aplica el resultado y actualiza el turno y el estado.
        public void iniciarRonda()
        {
            Ronda ronda = new Ronda(rondas.Count + 1, jugador1, jugador2, comunicacion, turnoActual == 1, getAtributosDisponibles());
            string resultado;
            string atributo;
            Jugador ganador;

            rondas.Add(ronda);
            mostrarEncabezadoRonda(ronda);

            resultado = ronda.prepararCartas();
            if (resultado != Ronda.OK)
            {
                manejarProblema(resultado);
            }
            else
            {
                jugador1.getCartaElegida().mostrar(atributosBloqueados);
                atributo = ronda.elegirAtributo();
                if (atributo == Ronda.RENDIDO || Comunicacion.esEspecial(atributo))
                {
                    manejarProblema(atributo);
                }
                else
                {
                    ganador = ronda.compararCartas();
                    ronda.finalizarRonda();
                    mostrarResultado(ronda, ganador);
                    bloquearAtributo(atributo);
                    actualizarTurno(ganador);
                    registrarEnArchivo(ronda, ganador);
                    if (jugador1.sinCartas() || jugador2.sinCartas())
                        estado = FINALIZADA;
                    else
                        Consola.pausa();
                }
            }
        }

        private void manejarProblema(string resultado)
        {
            if (resultado == Ronda.RENDIDO)
            {
                estado = RENDIDO;
                comunicacion.rendirse();
                Console.WriteLine("Abandonaste la partida.");
            }
            else if (resultado == Comunicacion.DESCONECTADO)
            {
                estado = DESCONECTADO;
                Console.WriteLine("El rival se desconectó o abandonó la partida.");
            }
            else if (resultado == Comunicacion.DESINCRONIZADO)
            {
                estado = ERROR;
                Console.WriteLine("La información recibida no corresponde al turno esperado. Se cancela la partida.");
            }
            else
            {
                estado = ERROR;
                Console.WriteLine("Se produjo un error de comunicación. Se cancela la partida.");
            }
        }

        // El ganador de la ronda elige en la siguiente. Si hay empate, se sortea (igual en ambas computadoras).
        private void actualizarTurno(Jugador ganador)
        {
            if (ganador == jugador1)
                turnoActual = 1;
            else if (ganador == jugador2)
                turnoActual = 2;
            else
                turnoActual = sortearSelector();
        }

        // El atributo usado queda bloqueado. Cuando se usaron los seis, se desbloquean todos.
        public void bloquearAtributo(string atributo)
        {
            if (!atributosBloqueados.Contains(atributo))
                atributosBloqueados.Add(atributo);
            if (atributosBloqueados.Count >= Personaje.ATRIBUTOS.Length)
            {
                atributosBloqueados.Clear();
                Console.WriteLine("Se usaron todos los atributos: vuelven a estar disponibles.");
            }
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

        // Solo el servidor escribe el resumen de la ronda, para que haya una única versión del estado.
        private void registrarEnArchivo(Ronda ronda, Jugador ganador)
        {
            if (comunicacion.esServidor())
            {
                string nombreGanador = ganador == null ? "EMPATE" : ganador.getNombre();
                string selector = turnoActual == 1 ? comunicacion.getRol() : comunicacion.getRolRival();
                comunicacion.registrarRonda(ronda.getNumero(), selector,
                    getJugadorServidor().cantidadCartas(), getJugadorCliente().cantidadCartas(),
                    getJugadorServidor().getPuntos(), getJugadorCliente().getPuntos(), nombreGanador);
            }
        }

        // Devuelve el ganador de la partida, o null si terminó por un error.
        public Jugador determinarGanador()
        {
            Jugador ganador = null;
            if (estado == DESCONECTADO)
                ganador = jugador1;
            else if (estado == RENDIDO)
                ganador = jugador2;
            else if (estado == FINALIZADA)
            {
                if (jugador1.sinCartas())
                    ganador = jugador2;
                else if (jugador2.sinCartas())
                    ganador = jugador1;
            }
            return ganador;
        }

        public void finalizar()
        {
            Jugador ganador = determinarGanador();

            Consola.titulo("FIN DE LA PARTIDA");
            if (ganador == null)
            {
                Console.WriteLine("La partida terminó sin ganador.");
            }
            else
            {
                Console.WriteLine("Ganador: " + ganador.getNombre());
                Console.WriteLine(ganador == jugador1 ? "¡FELICITACIONES, GANASTE!" : "Perdiste. ¡Suerte la próxima!");
            }
            if (jugador2 != null)
            {
                Console.WriteLine("Rondas jugadas: " + rondas.Count);
                Console.WriteLine(jugador1.getNombre() + ": " + jugador1.getPuntos() + " rondas ganadas, " + jugador1.cantidadCartas() + " cartas");
                Console.WriteLine(jugador2.getNombre() + ": " + jugador2.getPuntos() + " rondas ganadas, " + jugador2.cantidadCartas() + " cartas");
            }

            // Si este jugador se rindió, rendirse() ya avisó. Si el cliente nunca llegó a conectarse,
            // no hay nada que avisar (y el archivo puede ni siquiera estar accesible).
            if (comunicacion != null && estado != RENDIDO && (jugador2 != null || comunicacion.esServidor()))
            {
                string estadoArchivo = estado == ERROR ? Comunicacion.ESTADO_ERROR : Comunicacion.ESTADO_FINALIZADA;
                comunicacion.finalizarPartida(estadoArchivo, ganador == null ? "" : ganador.getNombre());
            }
            if (estado != ERROR)
                estado = FINALIZADA;
        }

        // Se usa si el usuario cierra el programa con Ctrl+C en medio de la partida.
        public void abandonar()
        {
            if (comunicacion != null && (estado == EN_JUEGO || estado == INICIANDO))
            {
                estado = RENDIDO;
                comunicacion.rendirse();
            }
        }

        // ============================================================
        // Pantallas
        // ============================================================

        private void mostrarInicio()
        {
            Consola.titulo("COMIENZA LA PARTIDA");
            Console.WriteLine(jugador1.getNombre() + " vs " + jugador2.getNombre());
            Console.WriteLine("Cada jugador recibió " + jugador1.cantidadCartas() + " cartas.");
            Console.WriteLine("Sorteo del primer turno: elige " + (turnoActual == 1 ? "VOS" : jugador2.getNombre()) + ".");
            Consola.pausa();
        }

        private void mostrarEncabezadoRonda(Ronda ronda)
        {
            Consola.limpiar();
            Consola.titulo("RONDA " + ronda.getNumero());
            Console.WriteLine("Tus cartas: " + jugador1.cantidadCartas() + "   |   Cartas de " + jugador2.getNombre() + ": " + jugador2.cantidadCartas());
            Console.WriteLine("Rondas ganadas: vos " + jugador1.getPuntos() + " - " + jugador2.getPuntos() + " " + jugador2.getNombre());
            Console.WriteLine("Elige el atributo: " + (ronda.getSoySelector() ? "VOS" : jugador2.getNombre()));
            Console.WriteLine();
            Console.WriteLine("Tu carta para esta ronda:");
        }

        private void mostrarResultado(Ronda ronda, Jugador ganador)
        {
            string atributo = ronda.getAtributoElegido();
            Carta miCarta = jugador1.getCartaElegida();
            Carta cartaRival = jugador2.getCartaElegida();

            Console.WriteLine();
            Console.WriteLine("Atributo elegido: " + atributo.ToUpper() + "  (tu valor: " + miCarta.getValor(atributo) + ")");
            if (ganador == null)
            {
                Console.WriteLine("¡EMPATE! Cada jugador conserva su carta. El próximo selector se sortea.");
            }
            else if (ganador == jugador1)
            {
                Console.WriteLine("¡GANASTE LA RONDA! Absorbiste la carta del rival:");
                cartaRival.mostrar();
            }
            else
            {
                Console.WriteLine("Perdiste la ronda. " + jugador2.getNombre() + " se lleva tu carta " + miCarta.getNombre() + ".");
            }
        }

        // ============================================================
        // Consultas
        // ============================================================

        private Jugador getJugadorServidor()
        {
            return comunicacion.esServidor() ? jugador1 : jugador2;
        }

        private Jugador getJugadorCliente()
        {
            return comunicacion.esServidor() ? jugador2 : jugador1;
        }

        public string getEstado()
        {
            return estado;
        }

        public Jugador getJugador1()
        {
            return jugador1;
        }

        public Jugador getJugador2()
        {
            return jugador2;
        }

        public int getTurnoActual()
        {
            return turnoActual;
        }

        public List<Ronda> getRondas()
        {
            return new List<Ronda>(rondas);
        }
    }
}
