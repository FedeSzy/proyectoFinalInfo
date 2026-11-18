using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabajoFinal_Fink_Szymsio_Fio
{
    // Intercambio de datos entre las dos computadoras con el archivo compartido Guerra.txt,
    // con la misma idea que el TP ClienteServidor: el archivo tiene una línea CLAVE=VALOR por dato
    // y la clave TURNO indica quién tiene que actuar. Solo escribe el que tiene el turno, y al
    // terminar le pasa el turno al otro, así nunca escriben los dos a la vez.
    //
    // Lo que se comparte en cada ronda es lo que pide la Actividad 2: la carta elegida
    // (CARTA_SERVIDOR / CARTA_CLIENTE) y el atributo elegido (ATRIBUTO).
    public class Comunicacion
    {
        public const string SERVIDOR = "SERVIDOR";
        public const string CLIENTE = "CLIENTE";

        // Resultados de esperarTurno
        public const string OK = "OK";
        public const string DESCONECTADO = "DESCONECTADO";
        public const string DESINCRONIZADO = "DESINCRONIZADO";
        public const string ERROR = "ERROR";

        private const int ESPERA_MAXIMA = 600;   // 600 intentos de medio segundo = 5 minutos

        private string participante;
        private string otroParticipante;
        private PartidaCompartida partida;
        private int esperaMaxima;

        public Comunicacion(bool esServidor, string nombreServidor)
        {
            if (esServidor)
            {
                participante = SERVIDOR;
                otroParticipante = CLIENTE;
            }
            else
            {
                participante = CLIENTE;
                otroParticipante = SERVIDOR;
            }
            partida = new PartidaCompartida(esServidor, nombreServidor);
            esperaMaxima = ESPERA_MAXIMA;
        }

        // Se usa en las pruebas para no esperar 5 minutos
        public void setEsperaMaxima(int intentos)
        {
            esperaMaxima = intentos;
        }

        public bool esServidor()
        {
            return participante == SERVIDOR;
        }

        public string getRol()
        {
            return participante;
        }

        public string getRolRival()
        {
            return otroParticipante;
        }

        public string getRutaArchivo()
        {
            return partida.obtenerRutaArchivo();
        }

        // ============================================================
        // INICIO DE LA PARTIDA
        // ============================================================

        // Devuelve el nombre del rival, o null si no se pudo conectar
        public string conectar(string nombreJugador)
        {
            string nombreRival = null;

            if (esServidor())
            {
                partida.prepararServidor();
                Console.WriteLine("Nombre de esta computadora: " + Environment.MachineName);
                string contenido = "ESTADO=ESPERANDO\nNOMBRE_SERVIDOR=" + nombreJugador + "\nTURNO=" + CLIENTE + "\n";
                if (!partida.guardar(contenido))
                {
                    Console.WriteLine("No se pudo crear el archivo compartido " + partida.obtenerRutaArchivo());
                }
                else
                {
                    Console.WriteLine("Esperando que el rival se conecte...");
                    if (esperarEstado("CONECTADO"))
                        nombreRival = recibirDatos("NOMBRE_CLIENTE");
                }
            }
            else
            {
                Console.WriteLine("Buscando la partida en " + partida.obtenerRutaArchivo() + " ...");
                if (esperarEstado("ESPERANDO"))
                {
                    List<string> claves = new List<string>();
                    List<string> valores = new List<string>();
                    claves.Add("NOMBRE_CLIENTE");
                    valores.Add(nombreJugador);
                    claves.Add("ESTADO");
                    valores.Add("CONECTADO");
                    claves.Add("TURNO");
                    valores.Add(SERVIDOR);
                    if (guardarDatos(claves, valores))
                        nombreRival = recibirDatos("NOMBRE_SERVIDOR");
                }
            }

            if (nombreRival == null)
                Console.WriteLine("No se pudo establecer la conexión con el rival.");
            return nombreRival;
        }

        // El servidor guarda el reparto (las cartas de cada jugador) y quién elige en la primera ronda
        public bool publicarReparto(string cartasServidor, string cartasCliente, string selector)
        {
            List<string> claves = new List<string>();
            List<string> valores = new List<string>();
            claves.Add("CARTAS_SERVIDOR");
            valores.Add(cartasServidor);
            claves.Add("CARTAS_CLIENTE");
            valores.Add(cartasCliente);
            claves.Add("SELECTOR_INICIAL");
            valores.Add(selector);
            claves.Add("RONDA");
            valores.Add("1");
            claves.Add("ESTADO");
            valores.Add("EN_JUEGO");
            claves.Add("TURNO");
            valores.Add(selector);
            return guardarDatos(claves, valores);
        }

        // El cliente espera a que el servidor haya repartido las cartas
        public bool esperarReparto()
        {
            return esperarEstado("EN_JUEGO");
        }

        // ============================================================
        // DATOS DE CADA RONDA
        // ============================================================

        // El selector guarda el atributo elegido (todavía no pasa el turno)
        public bool enviarAtributo(string atributo)
        {
            List<string> claves = new List<string>();
            List<string> valores = new List<string>();
            claves.Add("ATRIBUTO");
            valores.Add(atributo);
            return guardarDatos(claves, valores);
        }

        // Guarda la carta elegida y le pasa el turno al rival
        public bool enviarCarta(Carta carta)
        {
            List<string> claves = new List<string>();
            List<string> valores = new List<string>();
            claves.Add("CARTA_" + participante);
            valores.Add(carta.getNombre());
            claves.Add("TURNO");
            valores.Add(otroParticipante);
            return guardarDatos(claves, valores);
        }

        public string recibirCarta()
        {
            return recibirDatos("CARTA_" + otroParticipante);
        }

        public string recibirAtributo()
        {
            return recibirDatos("ATRIBUTO");
        }

        // Al terminar la ronda, el selector guarda el resultado y le da el turno al próximo selector
        public bool pasarRonda(int nuevaRonda, string selector, string ganadorRonda, int puntosServidor, int puntosCliente)
        {
            List<string> claves = new List<string>();
            List<string> valores = new List<string>();
            claves.Add("GANADOR_RONDA_" + (nuevaRonda - 1));
            valores.Add(ganadorRonda);
            claves.Add("PUNTOS_SERVIDOR");
            valores.Add(puntosServidor.ToString());
            claves.Add("PUNTOS_CLIENTE");
            valores.Add(puntosCliente.ToString());
            claves.Add("RONDA");
            valores.Add(nuevaRonda.ToString());
            claves.Add("TURNO");
            valores.Add(selector);
            return guardarDatos(claves, valores);
        }

        public void finalizar(string ganador)
        {
            List<string> claves = new List<string>();
            List<string> valores = new List<string>();
            claves.Add("ESTADO");
            valores.Add("FINALIZADO");
            claves.Add("GANADOR");
            valores.Add(ganador);
            guardarDatos(claves, valores);
        }

        // El jugador abandona: el rival gana
        public void rendirse()
        {
            List<string> claves = new List<string>();
            List<string> valores = new List<string>();
            claves.Add("ESTADO");
            valores.Add("RENDIDO");
            claves.Add("RINDIO");
            valores.Add(participante);
            guardarDatos(claves, valores);
        }

        // Avisa al rival que la partida no puede seguir
        public void informarError()
        {
            List<string> claves = new List<string>();
            List<string> valores = new List<string>();
            claves.Add("ESTADO");
            valores.Add("ERROR");
            guardarDatos(claves, valores);
        }

        // ============================================================
        // ESPERA
        // ============================================================

        // Espera hasta que sea el turno de esta computadora en la ronda indicada.
        // Devuelve OK, DESCONECTADO (el rival se rindió o no responde), DESINCRONIZADO
        // (el archivo indica otra ronda) o ERROR (la otra computadora informó un error).
        public string esperarTurno(int ronda)
        {
            string resultado = "";
            int intentos = 0;

            while (resultado == "")
            {
                string contenido = partida.leer();
                if (contenido != null)
                {
                    string estado = obtenerValor(contenido, "ESTADO");
                    if (estado == "RENDIDO" && obtenerValor(contenido, "RINDIO") == otroParticipante)
                    {
                        resultado = DESCONECTADO;
                    }
                    else if (estado == "ERROR")
                    {
                        resultado = ERROR;
                    }
                    else if (obtenerValor(contenido, "TURNO") == participante)
                    {
                        int rondaArchivo = convertirNumero(obtenerValor(contenido, "RONDA"));
                        if (rondaArchivo == ronda)
                            resultado = OK;
                        else if (rondaArchivo > ronda)
                            resultado = DESINCRONIZADO;
                    }
                }

                if (resultado == "")
                {
                    intentos++;
                    if (intentos >= esperaMaxima)
                        resultado = DESCONECTADO;
                    else
                        Thread.Sleep(500);
                }
            }
            return resultado;
        }

        // Espera hasta que el ESTADO del archivo sea el indicado. Devuelve false si se agotó la espera.
        private bool esperarEstado(string estadoEsperado)
        {
            bool encontrado = false;
            int intentos = 0;

            while (!encontrado && intentos < esperaMaxima)
            {
                string contenido = partida.leer();
                if (contenido != null && obtenerValor(contenido, "ESTADO") == estadoEsperado)
                {
                    encontrado = true;
                }
                else
                {
                    intentos++;
                    Thread.Sleep(500);
                }
            }
            return encontrado;
        }

        // ============================================================
        // ARCHIVO COMPARTIDO
        // ============================================================

        // Devuelve el valor de una clave del archivo ("" si no está o no se pudo leer)
        public string recibirDatos(string clave)
        {
            string valor = "";
            string contenido = partida.leer();
            if (contenido != null)
                valor = obtenerValor(contenido, clave);
            return valor;
        }

        // Modifica (o agrega) las claves indicadas y conserva el resto del archivo.
        // La línea TURNO se escribe siempre al final: si el otro programa lee el archivo mientras
        // se está copiando y ya ve el TURNO, quiere decir que todo lo anterior ya está escrito.
        private bool guardarDatos(List<string> claves, List<string> valores)
        {
            bool guardado = false;
            string contenido = partida.leer();

            if (contenido != null)
            {
                List<string> lineas = new List<string>();
                foreach (string linea in contenido.Replace("\r", "").Split('\n'))
                {
                    if (linea.Trim() != "")
                        lineas.Add(linea);
                }

                for (int i = 0; i < claves.Count; i++)
                {
                    reemplazarValor(lineas, claves[i], valores[i]);
                }

                string turno = "";
                string nuevoContenido = "";
                foreach (string linea in lineas)
                {
                    if (linea.StartsWith("TURNO="))
                        turno = linea;
                    else
                        nuevoContenido += linea + "\n";
                }
                nuevoContenido += turno + "\n";

                guardado = partida.guardar(nuevoContenido);
            }
            return guardado;
        }

        private void reemplazarValor(List<string> lineas, string clave, string valor)
        {
            bool reemplazado = false;
            for (int i = 0; i < lineas.Count; i++)
            {
                if (lineas[i].StartsWith(clave + "="))
                {
                    lineas[i] = clave + "=" + valor;
                    reemplazado = true;
                }
            }
            if (!reemplazado)
                lineas.Add(clave + "=" + valor);
        }

        private string obtenerValor(string contenido, string clave)
        {
            string valor = "";
            foreach (string linea in contenido.Replace("\r", "").Split('\n'))
            {
                if (linea.StartsWith(clave + "="))
                    valor = linea.Substring(clave.Length + 1).Trim();
            }
            return valor;
        }

        private int convertirNumero(string texto)
        {
            int numero = 0;
            try
            {
                numero = int.Parse(texto);
            }
            catch (FormatException)
            {
                numero = 0;
            }
            return numero;
        }
    }
}
