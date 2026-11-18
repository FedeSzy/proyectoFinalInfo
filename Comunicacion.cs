using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TrabajoFinal_Fink_Szymsio_Fio
{
    // Intercambia la información de la partida entre las dos computadoras usando el archivo
    // compartido Guerra.txt (ver README, "Archivo de partida compartida").
    //
    // Una computadora tiene el rol SERVIDOR (crea la partida y comparte la carpeta) y la otra
    // el rol CLIENTE (se une a través de la ruta de red). Cada una escribe solamente sus propias
    // claves (CARTA_SERVIDOR_3, ATRIBUTO_CLIENTE_3, RONDA_CLIENTE, ...). Cada dato de una ronda se
    // guarda como "numeroDeRonda|dato" para poder detectar información que no corresponde al turno esperado.
    public class Comunicacion
    {
        public const string NOMBRE_ARCHIVO = "Guerra.txt";
        public const string SERVIDOR = "SERVIDOR";
        public const string CLIENTE = "CLIENTE";

        // Resultados especiales que puede devolver recibirDatos()
        public const string DESCONECTADO = "#DESCONECTADO";
        public const string ERROR = "#ERROR";
        public const string DESINCRONIZADO = "#DESINCRONIZADO";

        // Estados que se guardan en la clave ESTADO del archivo
        public const string ESTADO_ESPERANDO_RIVAL = "ESPERANDO_RIVAL";
        public const string ESTADO_CONECTADO = "CONECTADO";
        public const string ESTADO_EN_JUEGO = "EN_JUEGO";
        public const string ESTADO_FINALIZADA = "FINALIZADA";
        public const string ESTADO_RENDIDO = "RENDIDO";
        public const string ESTADO_ERROR = "ERROR";

        private const char SEPARADOR = '|';
        private const int MILISEGUNDOS_ENTRE_LECTURAS = 300;
        private const int MILISEGUNDOS_ENTRE_LATIDOS = 2000;

        private ArchivoCompartido archivo;
        private string rol;
        private string rolRival;

        // Tiempos de espera (en segundos)
        private int segundosEsperaConexion;
        private int segundosArchivoInaccesible;
        private int segundosSinLatido;
        private int segundosEscritura;

        // "Latido": cada 2 segundos se incrementa un contador en el archivo para avisar que el programa
        // sigue abierto. Si el contador del rival deja de cambiar, se considera que se desconectó.
        private Timer temporizadorLatido;
        private int latido;
        private bool escribiendoLatido;
        private bool controlarLatido;

        public Comunicacion(bool esServidor, string carpetaCompartida)
        {
            if (esServidor)
            {
                rol = SERVIDOR;
                rolRival = CLIENTE;
            }
            else
            {
                rol = CLIENTE;
                rolRival = SERVIDOR;
            }
            archivo = new ArchivoCompartido(Path.Combine(carpetaCompartida, NOMBRE_ARCHIVO));
            segundosEsperaConexion = 600;
            segundosArchivoInaccesible = 120;
            segundosSinLatido = 30;
            segundosEscritura = 30;
            latido = 0;
            escribiendoLatido = false;
            controlarLatido = false;
        }

        // Permite acortar las esperas (se usa en las pruebas).
        public void configurarTiempos(int esperaConexion, int archivoInaccesible, int sinLatido, int escritura)
        {
            segundosEsperaConexion = esperaConexion;
            segundosArchivoInaccesible = archivoInaccesible;
            segundosSinLatido = sinLatido;
            segundosEscritura = escritura;
        }

        public bool esServidor()
        {
            return rol == SERVIDOR;
        }

        public string getRol()
        {
            return rol;
        }

        public string getRolRival()
        {
            return rolRival;
        }

        public string getRutaArchivo()
        {
            return archivo.getRuta();
        }

        // ============================================================
        // INICIO DE LA PARTIDA
        // ============================================================

        // Establece la conexión con el rival. Devuelve el nombre del rival o null si no se pudo.
        public string conectar(string nombreJugador)
        {
            string nombreRival;
            if (esServidor())
                nombreRival = conectarComoServidor(nombreJugador);
            else
                nombreRival = conectarComoCliente(nombreJugador);

            if (nombreRival != null)
                iniciarLatidos();
            return nombreRival;
        }

        private string conectarComoServidor(string nombreJugador)
        {
            string nombreRival = null;
            Dictionary<string, string> datos = new Dictionary<string, string>();
            datos["ESTADO"] = ESTADO_ESPERANDO_RIVAL;
            datos["NOMBRE_SERVIDOR"] = nombreJugador;
            datos["NOMBRE_CLIENTE"] = "";
            datos["RONDA"] = "0";

            if (!archivo.crear(datos, segundosEscritura))
            {
                Console.WriteLine("No se pudo crear el archivo compartido: " + archivo.getRuta());
            }
            else
            {
                Console.WriteLine("Esperando que el rival se una a la partida...");
                nombreRival = recibirDatos("NOMBRE_CLIENTE", -1, segundosEsperaConexion);
                if (esEspecial(nombreRival))
                {
                    Console.WriteLine("El rival no se conectó.");
                    nombreRival = null;
                }
            }
            return nombreRival;
        }

        private string conectarComoCliente(string nombreJugador)
        {
            string nombreRival = null;
            Dictionary<string, string> datos = esperarPartidaDisponible();

            if (datos != null)
            {
                Dictionary<string, string> cambios = new Dictionary<string, string>();
                cambios["NOMBRE_CLIENTE"] = nombreJugador;
                cambios["ESTADO"] = ESTADO_CONECTADO;
                if (archivo.actualizar(cambios, segundosEscritura))
                    nombreRival = obtener(datos, "NOMBRE_SERVIDOR");
                else
                    Console.WriteLine("No se pudo escribir en el archivo compartido. Revisen los permisos de la carpeta.");
            }
            return nombreRival;
        }

        // El cliente espera hasta que el archivo exista y el servidor haya creado una partida nueva.
        private Dictionary<string, string> esperarPartidaDisponible()
        {
            Dictionary<string, string> encontrada = null;
            DateTime limite = DateTime.Now.AddSeconds(segundosEsperaConexion);
            bool avisado = false;

            while (encontrada == null && DateTime.Now < limite)
            {
                Dictionary<string, string> datos = archivo.leer();
                if (datos != null && obtener(datos, "ESTADO") == ESTADO_ESPERANDO_RIVAL)
                {
                    encontrada = datos;
                }
                else
                {
                    if (!avisado)
                    {
                        if (datos == null)
                            Console.WriteLine("No se encuentra " + archivo.getRuta() + ". Reintentando...");
                        else
                            Console.WriteLine("Todavía no hay una partida nueva esperando rival. Reintentando...");
                        Console.WriteLine("(Verifiquen que el servidor haya creado la partida y que la carpeta esté compartida)");
                        avisado = true;
                    }
                    Thread.Sleep(MILISEGUNDOS_ENTRE_LECTURAS);
                }
            }
            if (encontrada == null)
                Console.WriteLine("No se encontró ninguna partida disponible.");
            return encontrada;
        }

        // Compara el resumen de las cartas de ambas computadoras. Deben ser idénticos para jugar.
        public bool sincronizar(string resumenLocal)
        {
            bool sincronizado = false;
            string resumenRival;
            Dictionary<string, string> cambios = new Dictionary<string, string>();
            cambios["RESUMEN_" + rol] = resumenLocal;

            if (archivo.actualizar(cambios, segundosEscritura))
            {
                resumenRival = recibirDatos("RESUMEN_" + rolRival, -1, 0);
                if (!esEspecial(resumenRival))
                {
                    sincronizado = resumenRival == resumenLocal;
                    if (!sincronizado)
                        Console.WriteLine("Los archivos de personajes de ambas computadoras NO son idénticos.");
                }
            }
            return sincronizado;
        }

        // El servidor sortea una semilla y la publica; el cliente la lee. Con la misma semilla ambas
        // computadoras mezclan y sortean exactamente igual. Devuelve -1 si hubo un problema.
        public int acordarSemilla()
        {
            int semilla = -1;
            if (esServidor())
            {
                int sorteada = new Random().Next(1, int.MaxValue);
                Dictionary<string, string> cambios = new Dictionary<string, string>();
                cambios["SEMILLA"] = sorteada.ToString();
                cambios["ESTADO"] = ESTADO_EN_JUEGO;
                if (archivo.actualizar(cambios, segundosEscritura))
                    semilla = sorteada;
            }
            else
            {
                string dato = recibirDatos("SEMILLA", -1, 0);
                if (esEspecial(dato) || !int.TryParse(dato, out semilla))
                    semilla = -1;
            }
            return semilla;
        }

        // ============================================================
        // DATOS DE CADA RONDA
        // ============================================================

        public bool enviarCarta(int ronda, Carta carta)
        {
            return enviarDato("CARTA", ronda, carta.getId() + SEPARADOR.ToString() + carta.getNombre());
        }

        // Devuelve "id|nombre" de la carta del rival, o un resultado especial (DESCONECTADO, ERROR, DESINCRONIZADO).
        public string recibirCarta(int ronda)
        {
            return recibirDatos("CARTA_" + rolRival + "_" + ronda, ronda, 0);
        }

        public bool enviarAtributo(int ronda, string atributo)
        {
            return enviarDato("ATRIBUTO", ronda, atributo);
        }

        public string recibirAtributo(int ronda)
        {
            return recibirDatos("ATRIBUTO_" + rolRival + "_" + ronda, ronda, 0);
        }

        // Cada dato se guarda con una clave propia de la ronda (por ejemplo CARTA_CLIENTE_3), así un
        // jugador que avanza a la ronda siguiente no borra un dato que el rival todavía no leyó.
        // Junto con el dato se actualiza RONDA_<rol>, que indica en qué ronda está cada jugador.
        private bool enviarDato(string tipo, int ronda, string dato)
        {
            Dictionary<string, string> cambios = new Dictionary<string, string>();
            cambios["RONDA_" + rol] = ronda.ToString();
            cambios[tipo + "_" + rol + "_" + ronda] = ronda + SEPARADOR.ToString() + dato;
            return archivo.actualizar(cambios, segundosEscritura);
        }

        // Guarda en el archivo el estado general de la partida después de cada ronda.
        public bool registrarRonda(int ronda, string selector, int cartasServidor, int cartasCliente,
                                   int puntosServidor, int puntosCliente, string ultimoGanador)
        {
            Dictionary<string, string> cambios = new Dictionary<string, string>();
            cambios["RONDA"] = ronda.ToString();
            cambios["SELECTOR"] = selector;
            cambios["CARTAS_SERVIDOR"] = cartasServidor.ToString();
            cambios["CARTAS_CLIENTE"] = cartasCliente.ToString();
            cambios["PUNTOS_SERVIDOR"] = puntosServidor.ToString();
            cambios["PUNTOS_CLIENTE"] = puntosCliente.ToString();
            cambios["ULTIMO_GANADOR"] = ultimoGanador;
            return archivo.actualizar(cambios, segundosEscritura);
        }

        // ============================================================
        // FIN DE LA PARTIDA
        // ============================================================

        public void finalizarPartida(string estado, string ganador)
        {
            Dictionary<string, string> cambios = new Dictionary<string, string>();
            detenerLatidos();
            cambios["ESTADO"] = estado;
            cambios["GANADOR"] = ganador;
            archivo.actualizar(cambios, segundosEscritura);
        }

        // El jugador abandona: el rival gana automáticamente.
        public void rendirse()
        {
            Dictionary<string, string> cambios = new Dictionary<string, string>();
            detenerLatidos();
            cambios["ESTADO"] = ESTADO_RENDIDO;
            cambios["RINDIO"] = rol;
            archivo.actualizar(cambios, 5);
        }

        // ============================================================
        // RECEPCIÓN
        // ============================================================

        // Espera a que el rival escriba la clave indicada y devuelve el dato.
        //  - ronda >= 0: el dato debe tener el formato "ronda|dato". Si es de una ronda anterior se
        //    sigue esperando; si es de una ronda posterior se devuelve DESINCRONIZADO.
        //  - ronda < 0: se devuelve el valor tal cual, apenas no esté vacío.
        //  - segundosMaximos = 0: sin límite de tiempo (mientras el rival siga conectado).
        // Devuelve DESCONECTADO si el rival se rindió o dejó de dar señales, y ERROR si el archivo
        // estuvo inaccesible demasiado tiempo o la otra computadora informó un error.
        public string recibirDatos(string clave, int ronda, int segundosMaximos)
        {
            string resultado = null;
            DateTime inicio = DateTime.Now;
            DateTime ultimoAcceso = DateTime.Now;
            DateTime ultimoCambioLatido = DateTime.Now;
            string ultimoLatido = null;
            bool avisado = false;

            // Sin latidos activos no hay forma de saber si el rival sigue conectado: se pone un límite.
            if (segundosMaximos <= 0 && !controlarLatido)
                segundosMaximos = segundosEsperaConexion;

            while (resultado == null)
            {
                Dictionary<string, string> datos = archivo.leer();
                if (datos == null)
                {
                    if (!avisado)
                    {
                        Console.WriteLine("No se puede acceder al archivo compartido (" + archivo.getRuta() + ").");
                        Console.WriteLine("La partida queda en pausa hasta que se recupere el acceso...");
                        avisado = true;
                    }
                    if ((DateTime.Now - ultimoAcceso).TotalSeconds > segundosArchivoInaccesible)
                        resultado = ERROR;
                }
                else
                {
                    if (avisado)
                    {
                        Console.WriteLine("Se recuperó el acceso al archivo compartido.");
                        avisado = false;
                        ultimoCambioLatido = DateTime.Now;
                    }
                    ultimoAcceso = DateTime.Now;
                    resultado = analizarDatos(datos, clave, ronda);

                    if (resultado == null && controlarLatido)
                    {
                        string latidoRival = obtener(datos, "LATIDO_" + rolRival);
                        if (latidoRival != ultimoLatido)
                        {
                            ultimoLatido = latidoRival;
                            ultimoCambioLatido = DateTime.Now;
                        }
                        else if ((DateTime.Now - ultimoCambioLatido).TotalSeconds > segundosSinLatido)
                        {
                            resultado = DESCONECTADO;
                        }
                    }
                }

                if (resultado == null && segundosMaximos > 0 && (DateTime.Now - inicio).TotalSeconds > segundosMaximos)
                    resultado = DESCONECTADO;
                if (resultado == null)
                    Thread.Sleep(MILISEGUNDOS_ENTRE_LECTURAS);
            }
            return resultado;
        }

        // Devuelve el dato buscado, un resultado especial, o null si hay que seguir esperando.
        private string analizarDatos(Dictionary<string, string> datos, string clave, int ronda)
        {
            string resultado = null;
            string estado = obtener(datos, "ESTADO");
            string valor = obtener(datos, clave);

            if (estado == ESTADO_RENDIDO && obtener(datos, "RINDIO") == rolRival)
            {
                resultado = DESCONECTADO;
            }
            else if (estado == ESTADO_ERROR)
            {
                resultado = ERROR;
            }
            else if (ronda >= 0 && rivalDemasiadoAdelantado(datos, ronda))
            {
                resultado = DESINCRONIZADO;
            }
            else if (valor != "")
            {
                if (ronda < 0)
                {
                    resultado = valor;
                }
                else
                {
                    string[] partes = valor.Split(SEPARADOR, 2);
                    int rondaDelDato;
                    if (partes.Length == 2 && int.TryParse(partes[0], out rondaDelDato) && rondaDelDato == ronda)
                        resultado = partes[1];
                    else
                        resultado = DESINCRONIZADO;
                }
            }
            return resultado;
        }

        // El rival puede estar, como mucho, una ronda adelante (ya leyó mis datos y siguió jugando).
        // Si figura más adelante, la información no corresponde al turno esperado.
        private bool rivalDemasiadoAdelantado(Dictionary<string, string> datos, int ronda)
        {
            int rondaRival;
            return int.TryParse(obtener(datos, "RONDA_" + rolRival), out rondaRival) && rondaRival > ronda + 1;
        }

        public static bool esEspecial(string dato)
        {
            return dato == null || dato == DESCONECTADO || dato == ERROR || dato == DESINCRONIZADO;
        }

        private static string obtener(Dictionary<string, string> datos, string clave)
        {
            string valor = "";
            if (datos.ContainsKey(clave))
                valor = datos[clave];
            return valor;
        }

        // ============================================================
        // LATIDOS (detección de desconexión)
        // ============================================================

        private void iniciarLatidos()
        {
            controlarLatido = true;
            temporizadorLatido = new Timer(enviarLatido, null, 0, MILISEGUNDOS_ENTRE_LATIDOS);
        }

        private void enviarLatido(object estado)
        {
            if (!escribiendoLatido)
            {
                escribiendoLatido = true;
                Dictionary<string, string> cambios = new Dictionary<string, string>();
                latido = latido + 1;
                cambios["LATIDO_" + rol] = latido.ToString();
                archivo.actualizar(cambios, 1);
                escribiendoLatido = false;
            }
        }

        public void detenerLatidos()
        {
            controlarLatido = false;
            if (temporizadorLatido != null)
            {
                temporizadorLatido.Dispose();
                temporizadorLatido = null;
            }
        }
    }
}
