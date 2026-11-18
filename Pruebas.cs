using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabajoFinal_Fink_Szymsio_Fio
{
    // Pruebas automáticas de cada etapa de Taller y de los casos del plan de pruebas de TI.
    // Se ejecutan desde el menú (opción 4) o con:  dotnet run -- --pruebas
    public class Pruebas
    {
        private static int aprobadas;
        private static int fallidas;
        private static string carpetaDatos;

        public static bool ejecutarTodas()
        {
            aprobadas = 0;
            fallidas = 0;
            carpetaDatos = Path.Combine(AppContext.BaseDirectory, "DatosDePrueba");

            Consola.titulo("PRUEBAS AUTOMÁTICAS");
            probarPersonaje();
            probarLecturaDeArchivos();
            probarCarta();
            probarMazo();
            probarGestorDeCartas();
            probarRonda();
            probarPartida();
            probarArchivoCompartido();
            probarComunicacion();

            Console.WriteLine();
            Console.WriteLine("Resultado: " + aprobadas + " pruebas aprobadas, " + fallidas + " fallidas.");
            return fallidas == 0;
        }

        // ------------------------------------------------------------
        // Etapa 1 - Personaje
        // ------------------------------------------------------------
        private static void probarPersonaje()
        {
            seccion("Etapa 1 - Personaje");

            Personaje hilarius = crearPersonaje("Hilarius", 650, 700, 500, 300, 400, 450);
            verificar("Personaje con todos sus datos válidos se crea", hilarius != null);
            verificar("getNombre devuelve el nombre", hilarius.getNombre() == "Hilarius");
            verificar("getAtributos respeta el orden y los valores",
                string.Join(",", hilarius.getAtributos()) == "650,700,500,300,400,450");
            verificar("getValorAtributo(\"velocidad\") = 700", hilarius.getValorAtributo("velocidad") == 700);
            verificar("getValorAtributo acepta mayúsculas y acentos (\"Energía\")", hilarius.getValorAtributo("Energía") == 400);
            verificar("Atributo principal = velocidad", hilarius.getAtributoPrincipal() == "velocidad");
            verificar("Atributo más bajo = inteligencia", hilarius.getAtributoMasBajo() == "inteligencia");
            verificar("Modificar la lista devuelta no cambia al personaje (encapsulamiento)", encapsulamientoPersonaje(hilarius));
            verificar("ASCII Art vacío muestra un texto por defecto", hilarius.getAsciiArt().Contains("No hay imagen"));

            verificar("Límites: 750 y 250 son válidos", crearPersonaje("Limites", 750, 250, 500, 500, 500, 500) != null);
            verificar("Total distinto de 3000 es inválido", crearPersonaje("Total", 650, 700, 500, 300, 400, 550) == null);
            verificar("Atributo mayor a 750 es inválido", crearPersonaje("Rango", 800, 700, 400, 300, 400, 400) == null);
            verificar("Atributo menor a 250 es inválido", crearPersonaje("Rango", 750, 750, 750, 200, 300, 250) == null);
            verificar("Sin atributo >= 650 es inválido", crearPersonaje("Esp", 600, 600, 600, 300, 450, 450) == null);
            verificar("Sin atributo <= 350 es inválido", crearPersonaje("Esp", 700, 400, 400, 500, 500, 500) == null);
            verificar("Los seis atributos en 500 es inválido", crearPersonaje("Todo500", 500, 500, 500, 500, 500, 500) == null);
            verificar("Nombre vacío es inválido", crearPersonaje("  ", 650, 700, 500, 300, 400, 450) == null);
            verificar("El validador informa todos los errores juntos",
                ValidadorDeAtributos.validar(new int[] { 900, 100, 500, 500, 500, 500 }).Count >= 2);
        }

        private static bool encapsulamientoPersonaje(Personaje personaje)
        {
            List<int> lista = personaje.getAtributos();
            lista[0] = 9999;
            return personaje.getAtributos()[0] == 650;
        }

        // ------------------------------------------------------------
        // Etapa 2 - Lectura de archivos de personajes
        // ------------------------------------------------------------
        private static void probarLecturaDeArchivos()
        {
            seccion("Etapa 2 - Lectura de archivos");

            Personaje leido = leerArchivo("valido.txt");
            verificar("Archivo válido se lee correctamente", leido != null);
            if (leido != null)
            {
                verificar("Se lee el nombre", leido.getNombre() == "Personaje de Prueba");
                verificar("Se lee la descripción de varias líneas", leido.getDescripcion() == "Primera línea de la descripción. Segunda línea.");
                verificar("Se lee el ASCII Art completo (3 líneas)", leido.getAsciiArt().Replace("\r", "").TrimEnd('\n').Split('\n').Length == 3);
                verificar("Se leen los atributos en cualquier orden", string.Join(",", leido.getAtributos()) == "450,700,300,500,550,500");
            }

            verificarError("Archivo inexistente", "no_existe.txt", "no existe");
            verificarError("Total distinto de 3000", "total_incorrecto.txt", "3000");
            verificarError("Atributo fuera de rango", "fuera_de_rango.txt", "entre 250 y 750");
            verificarError("Sin especialización", "sin_especializacion.txt", "650 puntos o más");
            verificarError("Seis atributos en 500", "todos_500.txt", "500 puntos a los seis");
            verificarError("Archivo incompleto (falta HABILIDAD)", "incompleto.txt", "Falta el atributo HABILIDAD");
            verificarError("Atributo repetido", "atributo_repetido.txt", "repetido");
            verificarError("Valor no numérico", "valor_no_numerico.txt", "no es un número");
            verificarError("Falta FINASCIIART", "sin_fin_ascii.txt", "FINASCIIART");
            verificarError("Formato incorrecto (Desc:, ASCII:, FUERZA: 999)", "formato_incorrecto.txt", "Se esperaba");
            verificarError("Archivo vacío", "vacio.txt", "termina antes");
        }

        private static void verificarError(string descripcion, string archivo, string textoEsperado)
        {
            string mensaje = "";
            try
            {
                new LectorDePersonajes().leer(Path.Combine(carpetaDatos, archivo));
                mensaje = "(se leyó sin error)";
            }
            catch (ArchivoPersonajeException e)
            {
                mensaje = e.Message;
            }
            verificar(descripcion + " -> error: " + mensaje, mensaje.Contains(textoEsperado));
        }

        // ------------------------------------------------------------
        // Etapa 3 - Carta
        // ------------------------------------------------------------
        private static void probarCarta()
        {
            seccion("Etapa 3 - Carta");

            Carta a = new Carta(1, crearPersonaje("A", 650, 700, 500, 300, 400, 450));
            Carta b = new Carta(2, crearPersonaje("B", 750, 250, 700, 300, 500, 500));

            verificar("La carta obtiene el nombre desde su Personaje (delegación)", a.getNombre() == "A");
            verificar("La carta obtiene los valores desde su Personaje", a.getValor("fuerza") == 650);
            verificar("compararAtributo: menor devuelve -1 (fuerza 650 vs 750)", a.compararAtributo(b, "fuerza") == -1);
            verificar("compararAtributo: mayor devuelve 1 (velocidad 700 vs 250)", a.compararAtributo(b, "velocidad") == 1);
            verificar("compararAtributo: empate devuelve 0 (inteligencia 300 vs 300)", a.compararAtributo(b, "inteligencia") == 0);
            verificar("compararAtributo con atributo inválido lanza excepción", lanzaExcepcion(() => a.compararAtributo(b, "magia")));
            verificar("compararAtributo con carta nula lanza excepción", lanzaExcepcion(() => a.compararAtributo(null, "fuerza")));
            verificar("No se puede crear una carta sin personaje", lanzaExcepcion(() => new Carta(3, null)));
        }

        // ------------------------------------------------------------
        // Etapa 4 - Mazo
        // ------------------------------------------------------------
        private static void probarMazo()
        {
            seccion("Etapa 4 - Mazo");

            List<Carta> cartas = crearCartas(10);
            Mazo mazo = new Mazo();
            Mazo vacio = new Mazo();

            verificar("Mazo nuevo está vacío", mazo.estaVacio() && mazo.cantidadCartas() == 0);
            verificar("Mazo vacío: verPrimera devuelve null", vacio.verPrimera() == null);
            verificar("Mazo vacío: repartir devuelve lista vacía", vacio.repartir(3).Count == 0);
            verificar("Mazo vacío: quitar una carta devuelve false", !vacio.quitarCarta(cartas[0]));

            foreach (Carta carta in cartas)
                mazo.agregarCarta(carta);
            verificar("agregarCarta incorpora cartas", mazo.cantidadCartas() == 10);
            verificar("No se agrega una carta repetida", !mazo.agregarCarta(cartas[0]) && mazo.cantidadCartas() == 10);
            verificar("No se agrega una carta nula", !mazo.agregarCarta(null));
            verificar("verPrimera devuelve la primera sin quitarla", mazo.verPrimera() == cartas[0] && mazo.cantidadCartas() == 10);

            mazo.quitarCarta(cartas[0]);
            mazo.agregarCarta(cartas[0]);
            verificar("Una carta retirada y vuelta a agregar queda al final", mazo.getCartas()[9] == cartas[0] && mazo.verPrimera() == cartas[1]);

            Mazo mezclado = new Mazo(cartas);
            mezclado.mezclar(new Random(1234));
            verificar("mezclar conserva la cantidad de cartas", mezclado.cantidadCartas() == 10);
            verificar("mezclar conserva las mismas cartas", cartas.All(c => mezclado.contieneCarta(c)));
            verificar("mezclar cambia el orden", !mezclado.getCartas().SequenceEqual(cartas));
            Mazo mezclado2 = new Mazo(cartas);
            mezclado2.mezclar(new Random(1234));
            verificar("Con la misma semilla se mezcla igual (sincronización)", mezclado.getCartas().SequenceEqual(mezclado2.getCartas()));

            Mazo paraRepartir = new Mazo(cartas);
            List<Carta> repartidas = paraRepartir.repartir(4);
            verificar("repartir(4) extrae 4 cartas en orden", repartidas.Count == 4 && repartidas[0] == cartas[0] && repartidas[3] == cartas[3]);
            verificar("repartir quita las cartas del mazo", paraRepartir.cantidadCartas() == 6);
            verificar("repartir más cartas de las que hay entrega las disponibles", paraRepartir.repartir(50).Count == 6 && paraRepartir.estaVacio());
            verificar("repartir una cantidad negativa lanza excepción", lanzaExcepcion(() => paraRepartir.repartir(-1)));
            verificar("getCartas devuelve una copia (encapsulamiento)", encapsulamientoMazo(cartas));
        }

        private static bool encapsulamientoMazo(List<Carta> cartas)
        {
            Mazo mazo = new Mazo(cartas);
            mazo.getCartas().Clear();
            return mazo.cantidadCartas() == cartas.Count;
        }

        // ------------------------------------------------------------
        // GestorDeCartas (carga, mezcla y reparto)
        // ------------------------------------------------------------
        private static void probarGestorDeCartas()
        {
            seccion("GestorDeCartas - carga y reparto");

            GestorDeCartas gestor = new GestorDeCartas();
            int cantidad = gestor.cargarCartas(Path.Combine(carpetaDatos, "Mixto"));
            verificar("Carga solo los archivos válidos de la carpeta (3 de 5)", cantidad == 3);
            verificar("Informa los archivos inválidos y el duplicado (2 errores)", gestor.getErrores().Count == 2);
            verificar("Las cartas se numeran en orden de archivo (ids 1, 2, 3)",
                string.Join(",", gestor.getCartasDisponibles().Select(c => c.getId())) == "1,2,3");

            GestorDeCartas sinCarpeta = new GestorDeCartas();
            verificar("Carpeta inexistente: 0 cartas y un error", sinCarpeta.cargarCartas(Path.Combine(carpetaDatos, "NoExiste")) == 0 && sinCarpeta.getErrores().Count == 1);
            verificar("Con menos de 2 personajes no se puede jugar", !sinCarpeta.hayCartasSuficientes());

            GestorDeCartas personajes = new GestorDeCartas();
            int cantidadPersonajes = personajes.cargarCartas(Path.Combine(AppContext.BaseDirectory, "Personajes"));
            verificar("Todos los archivos de la carpeta Personajes son válidos (" + cantidadPersonajes + " cargados)",
                personajes.getErrores().Count == 0 && personajes.hayCartasSuficientes());

            verificarReparto(16, 7);
            verificarReparto(14, 7);
            verificarReparto(9, 4);
            verificarReparto(2, 1);

            GestorDeCartas g1 = gestorConCartas(14);
            GestorDeCartas g2 = gestorConCartas(14);
            verificar("Mismas cartas -> mismo resumen (sincronización)", g1.calcularResumen() == g2.calcularResumen());
            verificar("Cartas distintas -> resumen distinto", g1.calcularResumen() != gestorConCartas(13).calcularResumen());

            Jugador s1 = new Jugador("S"), c1 = new Jugador("C"), s2 = new Jugador("S"), c2 = new Jugador("C");
            g1.repartirCartas(new List<Jugador> { s1, c1 }, new Random(99));
            g2.repartirCartas(new List<Jugador> { s2, c2 }, new Random(99));
            verificar("Con la misma semilla ambas computadoras reparten igual",
                idsDe(s1).SequenceEqual(idsDe(s2)) && idsDe(c1).SequenceEqual(idsDe(c2)));
            verificar("Nunca se reparte la misma carta a los dos jugadores", !idsDe(s1).Intersect(idsDe(c1)).Any());
        }

        private static void verificarReparto(int totalCartas, int esperadoPorJugador)
        {
            GestorDeCartas gestor = gestorConCartas(totalCartas);
            Jugador j1 = new Jugador("Uno");
            Jugador j2 = new Jugador("Dos");
            gestor.repartirCartas(new List<Jugador> { j1, j2 }, new Random(5));
            verificar("Con " + totalCartas + " cartas cada jugador recibe " + esperadoPorJugador,
                j1.cantidadCartas() == esperadoPorJugador && j2.cantidadCartas() == esperadoPorJugador);
        }

        // ------------------------------------------------------------
        // Etapa 5 - Reglas (Ronda)
        // ------------------------------------------------------------
        private static void probarRonda()
        {
            seccion("Etapa 5 - Reglas de la ronda");

            List<Carta> cartas = crearCartas(4);
            Jugador yo = new Jugador("Yo");
            Jugador rival = new Jugador("Rival");
            yo.getMazo().agregarCarta(cartas[0]);
            yo.getMazo().agregarCarta(cartas[1]);
            rival.getMazo().agregarCarta(cartas[2]);
            rival.getMazo().agregarCarta(cartas[3]);

            // cartas[0] tiene fuerza 650 y cartas[2] tiene fuerza 750 (ver crearCartas)
            Ronda ronda = new Ronda(1, yo, rival, null, true, new List<string>(Personaje.ATRIBUTOS));
            verificar("prepararCartas elige la primera carta de cada mazo", ronda.prepararCartas() == Ronda.OK
                && yo.getCartaElegida() == cartas[0] && rival.getCartaElegida() == cartas[2]);
            verificar("No se puede comparar sin atributo elegido", lanzaExcepcion(() => ronda.compararCartas()));
            ronda.establecerAtributo("fuerza");
            verificar("Gana la carta con el atributo mayor", ronda.compararCartas() == rival);
            ronda.finalizarRonda();
            verificar("El ganador absorbe la carta del perdedor", rival.cantidadCartas() == 3 && yo.cantidadCartas() == 1);
            verificar("Ambas cartas van al final del mazo del ganador",
                rival.getMazo().getCartas()[1] == cartas[2] && rival.getMazo().getCartas()[2] == cartas[0]);
            verificar("El ganador suma un punto", rival.getPuntos() == 1 && yo.getPuntos() == 0);

            // Empate: cartas[1] y cartas[3] tienen la misma inteligencia
            Ronda empate = new Ronda(2, yo, rival, null, false, new List<string>(Personaje.ATRIBUTOS));
            empate.prepararCartas();
            empate.establecerAtributo("inteligencia");
            empate.finalizarRonda();
            verificar("Empate: nadie gana", empate.esEmpate() && empate.getGanador() == null);
            verificar("Empate: cada jugador conserva su carta", yo.cantidadCartas() == 1 && rival.cantidadCartas() == 3);
            verificar("Empate: la carta del rival vuelve al final de su mazo", rival.getMazo().getCartas()[2] == cartas[3]);

            // Jugador que se queda sin cartas: a "yo" le queda cartas[1] (velocidad 250)
            Jugador rivalVeloz = new Jugador("Veloz");
            rivalVeloz.getMazo().agregarCarta(new Carta(9, crearPersonaje("Veloz", 650, 700, 500, 300, 400, 450)));
            Ronda ultima = new Ronda(3, yo, rivalVeloz, null, true, new List<string>(Personaje.ATRIBUTOS));
            ultima.prepararCartas();
            ultima.establecerAtributo("velocidad");
            ultima.finalizarRonda();
            verificar("Jugador que pierde su última carta se queda sin cartas", yo.sinCartas() && rivalVeloz.cantidadCartas() == 2);

            List<string> disponibles = new List<string> { "fuerza", "habilidad" };
            Ronda bloqueos = new Ronda(4, yo, rival, null, true, disponibles);
            verificar("No se puede elegir un atributo bloqueado", lanzaExcepcion(() => bloqueos.establecerAtributo("velocidad")));
            verificar("interpretarAtributo acepta el número de opción", bloqueos.interpretarAtributo("2") == "habilidad");
            verificar("interpretarAtributo acepta el nombre", bloqueos.interpretarAtributo(" FUERZA ") == "fuerza");
            verificar("interpretarAtributo rechaza opciones inválidas", bloqueos.interpretarAtributo("7") == null && bloqueos.interpretarAtributo("velocidad") == null);
            verificar("Escribir \"rendirse\" abandona la partida", bloqueos.interpretarAtributo("rendirse") == Ronda.RENDIDO);

            Jugador sinCartas = new Jugador("Vacío");
            Ronda sinMazo = new Ronda(5, sinCartas, rival, null, true, new List<string>(Personaje.ATRIBUTOS));
            verificar("Mazo sin cartas: la ronda no puede jugarse", sinMazo.prepararCartas() == Comunicacion.ERROR);
        }

        // ------------------------------------------------------------
        // Partida (ganador y atributos bloqueados)
        // ------------------------------------------------------------
        private static void probarPartida()
        {
            seccion("Partida - ganador y atributos bloqueados");

            List<Carta> cartas = crearCartas(2);
            Jugador conCartas = new Jugador("Con cartas");
            Jugador sinCartas = new Jugador("Sin cartas");
            conCartas.getMazo().agregarCarta(cartas[0]);

            verificar("Jugador sin cartas pierde la partida",
                new Partida(sinCartas, conCartas, Partida.FINALIZADA).determinarGanador() == conCartas);
            verificar("Si el rival se desconecta, gana el jugador que permanece",
                new Partida(sinCartas, conCartas, Partida.DESCONECTADO).determinarGanador() == sinCartas);
            verificar("Si el jugador se rinde, gana el rival",
                new Partida(conCartas, sinCartas, Partida.RENDIDO).determinarGanador() == sinCartas);
            verificar("Partida con error no tiene ganador",
                new Partida(conCartas, sinCartas, Partida.ERROR).determinarGanador() == null);

            Partida partida = new Partida(conCartas, sinCartas, Partida.EN_JUEGO);
            TextWriter salida = Console.Out;
            Console.SetOut(TextWriter.Null);
            partida.bloquearAtributo("fuerza");
            bool bloqueado = !partida.getAtributosDisponibles().Contains("fuerza") && partida.getAtributosDisponibles().Count == 5;
            foreach (string atributo in new string[] { "velocidad", "resistencia", "inteligencia", "energia" })
                partida.bloquearAtributo(atributo);
            bool quedaUno = partida.getAtributosDisponibles().Count == 1;
            partida.bloquearAtributo("habilidad");
            bool reinicio = partida.getAtributosDisponibles().Count == 6;
            Console.SetOut(salida);
            verificar("Un atributo usado queda bloqueado", bloqueado);
            verificar("Con cinco bloqueados queda uno disponible", quedaUno);
            verificar("Al usar los seis atributos se desbloquean todos", reinicio);
        }

        // ------------------------------------------------------------
        // Etapa 6 - Archivo compartido y comunicación
        // ------------------------------------------------------------
        private static void probarArchivoCompartido()
        {
            seccion("Etapa 6 - Archivo compartido");

            string carpeta = carpetaTemporal();
            ArchivoCompartido archivo = new ArchivoCompartido(Path.Combine(carpeta, "Guerra.txt"));
            Dictionary<string, string> datos = new Dictionary<string, string>();
            datos["ESTADO"] = "ESPERANDO_RIVAL";
            datos["RONDA"] = "0";

            verificar("Archivo que todavía no existe: leer devuelve null", archivo.leer() == null);
            verificar("crear escribe el archivo", archivo.crear(datos, 2) && archivo.existe());
            verificar("leer recupera los datos", archivo.leer()["ESTADO"] == "ESPERANDO_RIVAL");

            Dictionary<string, string> cambios = new Dictionary<string, string>();
            cambios["RONDA"] = "1";
            cambios["MENSAJE"] = "con\nsalto de línea";
            archivo.actualizar(cambios, 2);
            Dictionary<string, string> leidos = archivo.leer();
            verificar("actualizar modifica una clave y conserva las demás", leidos["RONDA"] == "1" && leidos["ESTADO"] == "ESPERANDO_RIVAL");
            verificar("Los saltos de línea se eliminan para no romper el formato", leidos["MENSAJE"] == "con salto de línea");

            bool bloqueoRespetado;
            using (FileStream ocupado = new FileStream(archivo.getRuta(), FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                DateTime inicio = DateTime.Now;
                bloqueoRespetado = !archivo.actualizar(cambios, 1) && (DateTime.Now - inicio).TotalSeconds >= 1;
            }
            verificar("Archivo ocupado por otro proceso: se reintenta y luego informa el fallo", bloqueoRespetado);
            verificar("Al liberarse el archivo se puede volver a escribir", archivo.actualizar(cambios, 2));

            ArchivoCompartido inaccesible = new ArchivoCompartido(Path.Combine(carpeta, "carpeta_que_no_existe", "Guerra.txt"));
            verificar("Archivo compartido inaccesible: leer devuelve null", inaccesible.leer() == null);
            verificar("Archivo compartido inaccesible: escribir devuelve false", !inaccesible.actualizar(cambios, 1));

            borrarCarpeta(carpeta);
        }

        private static void probarComunicacion()
        {
            seccion("Etapa 6 - Comunicación (protocolo)");

            string carpeta = carpetaTemporal();
            Comunicacion servidor = new Comunicacion(true, carpeta);
            Comunicacion cliente = new Comunicacion(false, carpeta);
            servidor.configurarTiempos(2, 2, 2, 2);
            cliente.configurarTiempos(2, 2, 2, 2);
            Carta carta = crearCartas(1)[0];
            TextWriter salida = Console.Out;
            Console.SetOut(TextWriter.Null);

            ArchivoCompartido archivo = new ArchivoCompartido(servidor.getRutaArchivo());
            archivo.crear(new Dictionary<string, string> { { "ESTADO", Comunicacion.ESTADO_EN_JUEGO } }, 2);

            cliente.enviarCarta(3, carta);
            string recibida = servidor.recibirCarta(3);
            cliente.enviarAtributo(3, "fuerza");
            string atributo = servidor.recibirAtributo(3);
            string adelantada = servidor.recibirCarta(1);
            string sinRespuesta = cliente.recibirAtributo(3);
            servidor.rendirse();
            string rendido = cliente.recibirCarta(4);

            Comunicacion lejana = new Comunicacion(true, Path.Combine(carpeta, "no_existe"));
            lejana.configurarTiempos(5, 1, 5, 1);
            string inaccesible = lejana.recibirCarta(1);

            Console.SetOut(salida);
            verificar("Se recibe la carta enviada por el rival (id|nombre)", recibida == carta.getId() + "|" + carta.getNombre());
            verificar("Se recibe el atributo enviado por el rival", atributo == "fuerza");
            verificar("Información de un turno que no corresponde -> DESINCRONIZADO", adelantada == Comunicacion.DESINCRONIZADO);
            verificar("Rival que no responde -> DESCONECTADO (al vencer la espera)", sinRespuesta == Comunicacion.DESCONECTADO);
            verificar("Rival que se rinde -> DESCONECTADO (gana el que permanece)", rendido == Comunicacion.DESCONECTADO);
            verificar("Archivo compartido inaccesible por mucho tiempo -> ERROR", inaccesible == Comunicacion.ERROR);
            verificar("Ruta de red armada a partir del nombre de la PC",
                CarpetaCompartida.rutaDelServidor("PC-AULA-01") == @"\\PC-AULA-01\Guerra");

            borrarCarpeta(carpeta);
        }

        // ------------------------------------------------------------
        // Auxiliares
        // ------------------------------------------------------------
        private static void seccion(string titulo)
        {
            Console.WriteLine();
            Console.WriteLine("--- " + titulo + " ---");
        }

        private static void verificar(string descripcion, bool condicion)
        {
            if (condicion)
            {
                aprobadas++;
                Console.WriteLine("  [OK]    " + descripcion);
            }
            else
            {
                fallidas++;
                Console.WriteLine("  [FALLO] " + descripcion);
            }
        }

        private static bool lanzaExcepcion(Action accion)
        {
            bool lanzo = false;
            try
            {
                accion();
            }
            catch (Exception)
            {
                lanzo = true;
            }
            return lanzo;
        }

        private static Personaje crearPersonaje(string nombre, int f, int v, int r, int i, int e, int h)
        {
            Personaje personaje = null;
            try
            {
                personaje = new Personaje(nombre, "Descripción de prueba", "", f, v, r, i, e, h);
            }
            catch (ArgumentException)
            {
                personaje = null;
            }
            return personaje;
        }

        private static Personaje leerArchivo(string archivo)
        {
            Personaje personaje = null;
            try
            {
                personaje = new LectorDePersonajes().leer(Path.Combine(carpetaDatos, archivo));
            }
            catch (ArchivoPersonajeException e)
            {
                Console.WriteLine("  (error inesperado: " + e.Message + ")");
            }
            return personaje;
        }

        // Crea cartas alternando dos distribuciones conocidas:
        //  pares:   F650 V700 R500 I300 E400 H450
        //  impares: F750 V250 R700 I300 E500 H500
        private static List<Carta> crearCartas(int cantidad)
        {
            List<Carta> cartas = new List<Carta>();
            for (int i = 0; i < cantidad; i++)
            {
                Personaje personaje;
                if (i % 2 == 0)
                    personaje = crearPersonaje("Carta " + (i + 1), 650, 700, 500, 300, 400, 450);
                else
                    personaje = crearPersonaje("Carta " + (i + 1), 750, 250, 700, 300, 500, 500);
                cartas.Add(new Carta(i + 1, personaje));
            }
            // Para que las pruebas de ronda sean claras, la carta 3 (índice 2) es la "fuerte".
            if (cantidad >= 3)
                cartas[2] = new Carta(3, crearPersonaje("Carta 3", 750, 250, 700, 300, 500, 500));
            return cartas;
        }

        private static GestorDeCartas gestorConCartas(int cantidad)
        {
            string carpeta = carpetaTemporal();
            for (int i = 1; i <= cantidad; i++)
            {
                File.WriteAllText(Path.Combine(carpeta, "Personaje " + i.ToString("00") + ".txt"),
                    "NOMBRE\nPersonaje " + i.ToString("00") + "\nDESCRIPCION\nDe prueba\nASCIIART\n :) \nFINASCIIART\nATRIBUTOS\n"
                    + "FUERZA;650\nVELOCIDAD;700\nRESISTENCIA;500\nINTELIGENCIA;300\nENERGIA;400\nHABILIDAD;450\n");
            }
            GestorDeCartas gestor = new GestorDeCartas();
            gestor.cargarCartas(carpeta);
            borrarCarpeta(carpeta);
            return gestor;
        }

        private static List<int> idsDe(Jugador jugador)
        {
            return jugador.getMazo().getCartas().Select(c => c.getId()).ToList();
        }

        private static string carpetaTemporal()
        {
            string carpeta = Path.Combine(Path.GetTempPath(), "JuegoGuerraPruebas_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(carpeta);
            return carpeta;
        }

        private static void borrarCarpeta(string carpeta)
        {
            try
            {
                Directory.Delete(carpeta, true);
            }
            catch (Exception)
            {
                // no es grave si queda una carpeta temporal
            }
        }
    }
}
