using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabajoFinal_Fink_Szymsio_Fio
{
    // Pruebas de cada etapa de Taller y de los casos del plan de pruebas de TI (opción "Ejecutar pruebas")
    public class Pruebas
    {
        private static int aprobadas;
        private static int fallidas;
        private static string carpetaDatos;

        public static void ejecutarTodas()
        {
            aprobadas = 0;
            fallidas = 0;
            carpetaDatos = Path.Combine(Directory.GetCurrentDirectory(), "DatosDePrueba");

            Console.WriteLine("===== PRUEBAS =====");
            probarPersonaje();
            probarLecturaDeArchivos();
            probarCarta();
            probarMazo();
            probarReparto();
            probarRonda();
            probarPartida();
            probarArchivoCompartido();

            Console.WriteLine();
            Console.WriteLine("Resultado: " + aprobadas + " pruebas aprobadas, " + fallidas + " fallidas.");
        }

        // ------------------------------------------------------------
        // Etapa 1 - Personaje
        // ------------------------------------------------------------
        private static void probarPersonaje()
        {
            titulo("Etapa 1 - Personaje");
            Personaje p = new Personaje("Hilarius", "Repartidor", "", 650, 700, 500, 300, 400, 450);

            verificar("getNombre devuelve el nombre", p.getNombre() == "Hilarius");
            verificar("getAtributos devuelve los 6 valores en orden", textoDeLista(p.getAtributos()) == "650,700,500,300,400,450");
            verificar("getValorAtributo(\"velocidad\") = 700", p.getValorAtributo("velocidad") == 700);
            verificar("getValorAtributo de un atributo inexistente = -1", p.getValorAtributo("magia") == -1);
            verificar("Sin ASCII Art se muestra un texto por defecto", p.getAsciiArt().Contains("No hay imagen"));

            verificar("Distribución válida (Hilarius)", esValida(650, 700, 500, 300, 400, 450));
            verificar("Límites 750 y 250 son válidos", esValida(750, 250, 500, 500, 500, 500));
            verificar("Total distinto de 3000 es inválido", !esValida(650, 700, 500, 300, 400, 550));
            verificar("Atributo mayor a 750 es inválido", !esValida(800, 700, 400, 300, 400, 400));
            verificar("Atributo menor a 250 es inválido", !esValida(750, 750, 750, 200, 300, 250));
            verificar("Sin atributo de 650 o más es inválido", !esValida(600, 600, 600, 300, 450, 450));
            verificar("Sin atributo de 350 o menos es inválido", !esValida(700, 400, 400, 500, 500, 500));
            verificar("Los seis atributos en 500 es inválido", !esValida(500, 500, 500, 500, 500, 500));
        }

        private static bool esValida(int f, int v, int r, int i, int e, int h)
        {
            List<int> valores = new List<int>();
            valores.Add(f);
            valores.Add(v);
            valores.Add(r);
            valores.Add(i);
            valores.Add(e);
            valores.Add(h);
            return Personaje.validarAtributos(valores).Count == 0;
        }

        // ------------------------------------------------------------
        // Etapa 2 - Lectura de archivos de cartas
        // ------------------------------------------------------------
        private static void probarLecturaDeArchivos()
        {
            titulo("Etapa 2 - Lectura de archivos");

            GestorDeCartas gestor = cargarArchivo("valido.txt");
            verificar("Archivo válido se carga sin errores", !gestor.hayErrores() && gestor.getCartasDisponibles().Count == 1);
            if (gestor.getCartasDisponibles().Count == 1)
            {
                Personaje p = gestor.getCartasDisponibles()[0].getPersonaje();
                verificar("Se lee el nombre", p.getNombre() == "Personaje de Prueba");
                verificar("Se lee la descripción", p.getDescripcion() == "Descripción de prueba.");
                verificar("Se lee el ASCII Art", p.getAsciiArt().Contains("/|\\"));
                verificar("Se leen los atributos (en cualquier orden)", textoDeLista(p.getAtributos()) == "450,700,300,500,550,500");
            }

            verificarError("Total distinto de 3000", "total_incorrecto.txt", "3000");
            verificarError("Atributo fuera de rango", "fuera_de_rango.txt", "entre 250 y 750");
            verificarError("Sin especialización", "sin_especializacion.txt", "650 puntos o más");
            verificarError("Seis atributos en 500", "todos_500.txt", "500 puntos a los seis");
            verificarError("Archivo incompleto", "incompleto.txt", "falta el atributo HABILIDAD");
            verificarError("Atributo repetido", "atributo_repetido.txt", "repetido");
            verificarError("Valor no numérico", "valor_no_numerico.txt", "no es un número");
            verificarError("Falta FINASCIIART", "sin_fin_ascii.txt", "FINASCIIART");
            verificarError("Formato incorrecto", "formato_incorrecto.txt", "se esperaba la línea DESCRIPCION");
            verificarError("Archivo vacío", "vacio.txt", "termina antes");

            GestorDeCartas mixto = new GestorDeCartas();
            mixto.cargarCartas(Path.Combine(carpetaDatos, "Mixto"));
            verificar("Carpeta con archivos válidos e inválidos: informa los 2 errores", mixto.getErrores().Count == 2);
            verificar("El error indica el nombre del archivo", mixto.getErrores()[mixto.getErrores().Count - 1].StartsWith("Roto.txt"));

            GestorDeCartas sinCarpeta = new GestorDeCartas();
            sinCarpeta.cargarCartas(Path.Combine(carpetaDatos, "NoExiste"));
            verificar("Carpeta inexistente informa un error", sinCarpeta.hayErrores());

            GestorDeCartas cartasDelJuego = new GestorDeCartas();
            cartasDelJuego.cargarCartas(Path.Combine(Directory.GetCurrentDirectory(), "Cartas"));
            verificar("Todos los archivos de la carpeta Cartas son válidos (" + cartasDelJuego.getCartasDisponibles().Count + ")",
                !cartasDelJuego.hayErrores() && cartasDelJuego.getCartasDisponibles().Count >= 2);
        }

        // Copia un único archivo a una carpeta temporal y lo carga
        private static GestorDeCartas cargarArchivo(string archivo)
        {
            string carpeta = carpetaTemporal();
            File.Copy(Path.Combine(carpetaDatos, archivo), Path.Combine(carpeta, archivo));
            GestorDeCartas gestor = new GestorDeCartas();
            gestor.cargarCartas(carpeta);
            Directory.Delete(carpeta, true);
            return gestor;
        }

        private static void verificarError(string descripcion, string archivo, string textoEsperado)
        {
            GestorDeCartas gestor = cargarArchivo(archivo);
            string error = "(sin error)";
            if (gestor.hayErrores())
                error = gestor.getErrores()[0];
            verificar(descripcion + " -> " + error, error.Contains(textoEsperado) && gestor.getCartasDisponibles().Count == 0);
        }

        // ------------------------------------------------------------
        // Etapa 3 - Carta
        // ------------------------------------------------------------
        private static void probarCarta()
        {
            titulo("Etapa 3 - Carta");
            Carta a = crearCarta(1, "A", 650, 700, 500, 300, 400, 450);
            Carta b = crearCarta(2, "B", 750, 250, 700, 300, 500, 500);

            verificar("La carta obtiene el nombre de su personaje", a.getNombre() == "A");
            verificar("La carta obtiene los valores de su personaje", a.getValor("fuerza") == 650);
            verificar("compararAtributo: menor devuelve -1", a.compararAtributo(b, "fuerza") == -1);
            verificar("compararAtributo: mayor devuelve 1", a.compararAtributo(b, "velocidad") == 1);
            verificar("compararAtributo: empate devuelve 0", a.compararAtributo(b, "inteligencia") == 0);
        }

        // ------------------------------------------------------------
        // Etapa 4 - Mazo
        // ------------------------------------------------------------
        private static void probarMazo()
        {
            titulo("Etapa 4 - Mazo");
            List<Carta> cartas = crearCartas(10);
            Mazo mazo = new Mazo();

            verificar("Mazo nuevo vacío", mazo.estaVacio() && mazo.cantidadCartas() == 0);
            verificar("Mazo vacío: verPrimera devuelve null", mazo.verPrimera() == null);
            verificar("Mazo vacío: repartir devuelve una lista vacía", mazo.repartir(3).Count == 0);

            foreach (Carta carta in cartas)
                mazo.agregarCarta(carta);
            verificar("agregarCarta incorpora las cartas", mazo.cantidadCartas() == 10);
            mazo.agregarCarta(cartas[0]);
            verificar("No se agrega una carta repetida", mazo.cantidadCartas() == 10);
            verificar("verPrimera devuelve la primera sin sacarla", mazo.verPrimera() == cartas[0] && mazo.cantidadCartas() == 10);

            mazo.quitarCarta(cartas[0]);
            verificar("quitarCarta retira la carta", mazo.cantidadCartas() == 9 && !mazo.contieneCarta(cartas[0]));
            mazo.agregarCarta(cartas[0]);
            verificar("Una carta agregada va al final", mazo.getCartas()[9] == cartas[0]);

            mazo.mezclar();
            bool estanTodas = true;
            foreach (Carta carta in cartas)
            {
                if (!mazo.contieneCarta(carta))
                    estanTodas = false;
            }
            verificar("mezclar conserva las mismas cartas", estanTodas && mazo.cantidadCartas() == 10);

            List<Carta> repartidas = mazo.repartir(4);
            verificar("repartir(4) saca 4 cartas del mazo", repartidas.Count == 4 && mazo.cantidadCartas() == 6);
            verificar("repartir más de las que hay entrega las disponibles", mazo.repartir(50).Count == 6 && mazo.estaVacio());
        }

        private static void probarReparto()
        {
            titulo("Etapa 4 - Reparto");
            verificarReparto(16, 7);
            verificarReparto(14, 7);
            verificarReparto(9, 4);
            verificarReparto(2, 1);

            GestorDeCartas gestor = gestorConCartas(14);
            Jugador uno = new Jugador("Uno");
            Jugador dos = new Jugador("Dos");
            List<Jugador> jugadores = new List<Jugador>();
            jugadores.Add(uno);
            jugadores.Add(dos);
            gestor.repartirCartas(jugadores);
            bool repetida = false;
            foreach (Carta carta in uno.getMazo().getCartas())
            {
                if (dos.getMazo().contieneCarta(carta))
                    repetida = true;
            }
            verificar("Ninguna carta se reparte a los dos jugadores", !repetida);
        }

        private static void verificarReparto(int totalCartas, int esperado)
        {
            GestorDeCartas gestor = gestorConCartas(totalCartas);
            Jugador uno = new Jugador("Uno");
            Jugador dos = new Jugador("Dos");
            List<Jugador> jugadores = new List<Jugador>();
            jugadores.Add(uno);
            jugadores.Add(dos);
            gestor.repartirCartas(jugadores);
            verificar("Con " + totalCartas + " cartas cada jugador recibe " + esperado,
                uno.getMazo().cantidadCartas() == esperado && dos.getMazo().cantidadCartas() == esperado);
        }

        // ------------------------------------------------------------
        // Etapa 5 - Reglas
        // ------------------------------------------------------------
        private static void probarRonda()
        {
            titulo("Etapa 5 - Reglas de la ronda");
            Carta debil = crearCarta(1, "Debil", 650, 700, 500, 300, 400, 450);
            Carta fuerte = crearCarta(2, "Fuerte", 750, 250, 700, 300, 500, 500);
            Carta otra = crearCarta(3, "Otra", 650, 700, 500, 300, 400, 450);
            Jugador yo = new Jugador("Yo");
            Jugador rival = new Jugador("Rival");
            yo.getMazo().agregarCarta(debil);
            yo.getMazo().agregarCarta(otra);
            rival.getMazo().agregarCarta(fuerte);

            Ronda ronda = rondaDePrueba(1, yo, rival, todosLosAtributos());
            verificar("No se puede usar un atributo que no existe", !ronda.establecerAtributo("magia"));
            ronda.establecerAtributo("fuerza");
            verificar("Gana la carta con el valor mayor", ronda.compararCartas() == rival);
            ronda.finalizarRonda();
            verificar("El ganador absorbe la carta del perdedor", rival.getMazo().cantidadCartas() == 2 && yo.getMazo().cantidadCartas() == 1);
            verificar("Las dos cartas van al final del mazo del ganador", rival.getMazo().getCartas()[1] == debil);
            verificar("El ganador suma un punto", rival.getPuntos() == 1 && yo.getPuntos() == 0);

            // Empate: "Otra" y "Fuerte" tienen la misma inteligencia (300)
            Ronda empate = rondaDePrueba(2, yo, rival, todosLosAtributos());
            empate.establecerAtributo("inteligencia");
            empate.finalizarRonda();
            verificar("Empate: no hay ganador", empate.getGanador() == null);
            verificar("Empate: cada jugador conserva su carta", yo.getMazo().cantidadCartas() == 1 && rival.getMazo().cantidadCartas() == 2);
            verificar("Empate: la carta vuelve al final del mazo", rival.getMazo().getCartas()[1] == fuerte);

            // A "yo" solo le queda "Otra" (resistencia 500) y pierde contra resistencia 700
            Jugador rivalResistente = new Jugador("Resistente");
            rivalResistente.getMazo().agregarCarta(crearCarta(9, "Resistente", 750, 250, 700, 300, 500, 500));
            Ronda ultima = rondaDePrueba(3, yo, rivalResistente, todosLosAtributos());
            ultima.establecerAtributo("resistencia");
            ultima.finalizarRonda();
            verificar("Jugador que pierde su última carta se queda sin cartas", yo.getMazo().estaVacio());

            List<string> pocos = new List<string>();
            pocos.Add("fuerza");
            Ronda bloqueada = rondaDePrueba(4, rival, rivalResistente, pocos);
            verificar("No se puede elegir un atributo bloqueado", !bloqueada.establecerAtributo("velocidad"));
        }

        private static Ronda rondaDePrueba(int numero, Jugador j1, Jugador j2, List<string> disponibles)
        {
            j1.elegirCarta(j1.getMazo().verPrimera());
            j2.elegirCarta(j2.getMazo().verPrimera());
            return new Ronda(numero, j1, j2, null, true, disponibles);
        }

        private static List<string> todosLosAtributos()
        {
            List<string> atributos = new List<string>();
            foreach (string atributo in Personaje.ATRIBUTOS)
                atributos.Add(atributo);
            return atributos;
        }

        private static void probarPartida()
        {
            titulo("Etapa 5 - Partida");
            Jugador conCartas = new Jugador("Con cartas");
            Jugador sinCartas = new Jugador("Sin cartas");
            conCartas.getMazo().agregarCarta(crearCarta(1, "A", 650, 700, 500, 300, 400, 450));

            verificar("El jugador sin cartas pierde la partida", new Partida(sinCartas, conCartas, Partida.FINALIZADA).determinarGanador() == conCartas);
            verificar("Si el rival se desconecta, gana el que permanece", new Partida(sinCartas, conCartas, Partida.DESCONECTADO).determinarGanador() == sinCartas);
            verificar("Si el jugador se rinde, gana el rival", new Partida(conCartas, sinCartas, Partida.RENDIDO).determinarGanador() == sinCartas);

            Partida partida = new Partida(conCartas, sinCartas, Partida.EN_JUEGO);
            partida.bloquearAtributo("fuerza");
            verificar("El atributo usado queda bloqueado", !partida.getAtributosDisponibles().Contains("fuerza") && partida.getAtributosDisponibles().Count == 5);
            partida.bloquearAtributo("velocidad");
            partida.bloquearAtributo("resistencia");
            partida.bloquearAtributo("inteligencia");
            partida.bloquearAtributo("energia");
            verificar("Con cinco bloqueados queda uno disponible", partida.getAtributosDisponibles().Count == 1);
            partida.bloquearAtributo("habilidad");
            verificar("Cuando no queda ninguno, se desbloquean todos", partida.getAtributosDisponibles().Count == 6);
        }

        // ------------------------------------------------------------
        // Etapa 6 - Archivo compartido y comunicación
        // ------------------------------------------------------------
        private static void probarArchivoCompartido()
        {
            titulo("Etapa 6 - Archivo compartido");
            string carpeta = carpetaTemporal();
            ArchivoTexto archivo = new ArchivoTexto(Path.Combine(carpeta, "Guerra.txt"));

            verificar("Archivo inexistente: leer devuelve null", archivo.leer() == null);
            verificar("guardar escribe el archivo", archivo.guardar("ESTADO=EN_JUEGO\nRONDA=1\nTURNO=CLIENTE\n") && archivo.existe());
            verificar("leer devuelve el contenido", archivo.leer().Contains("RONDA=1"));

            ArchivoTexto inaccesible = new ArchivoTexto(Path.Combine(carpeta, "no_existe", "Guerra.txt"));
            verificar("Archivo compartido inaccesible: leer devuelve null", inaccesible.leer() == null);
            verificar("Archivo compartido inaccesible: guardar devuelve false", !inaccesible.guardar("X=1"));

            // El cliente puede usar una ruta completa como carpeta compartida
            Comunicacion cliente = new Comunicacion(false, carpeta);
            cliente.setEsperaMaxima(4);
            verificar("Es el turno del cliente en la ronda 1", cliente.esperarTurno(1) == Comunicacion.OK);

            Carta carta = crearCarta(1, "Mi Carta", 650, 700, 500, 300, 400, 450);
            cliente.enviarAtributo("velocidad");
            cliente.enviarCarta(carta);
            string contenido = archivo.leer();
            verificar("Se comparten la carta y el atributo elegidos", contenido.Contains("CARTA_CLIENTE=Mi Carta") && contenido.Contains("ATRIBUTO=velocidad"));
            verificar("Al enviar la carta se pasa el turno (TURNO queda al final)", contenido.EndsWith("TURNO=SERVIDOR\n"));
            verificar("Si no es su turno y el rival no responde -> DESCONECTADO", cliente.esperarTurno(1) == Comunicacion.DESCONECTADO);

            archivo.guardar("ESTADO=EN_JUEGO\nRONDA=5\nTURNO=CLIENTE\n");
            verificar("Información de una ronda que no corresponde -> DESINCRONIZADO", cliente.esperarTurno(2) == Comunicacion.DESINCRONIZADO);

            archivo.guardar("ESTADO=RENDIDO\nRINDIO=SERVIDOR\nRONDA=2\nTURNO=SERVIDOR\n");
            verificar("El rival se rinde -> DESCONECTADO (gana el que permanece)", cliente.esperarTurno(2) == Comunicacion.DESCONECTADO);

            Comunicacion lejana = new Comunicacion(false, Path.Combine(carpeta, "no_existe"));
            lejana.setEsperaMaxima(2);
            verificar("Archivo compartido inaccesible -> DESCONECTADO", lejana.esperarTurno(1) == Comunicacion.DESCONECTADO);

            Directory.Delete(carpeta, true);
        }

        // ------------------------------------------------------------
        // Auxiliares
        // ------------------------------------------------------------
        private static void titulo(string texto)
        {
            Console.WriteLine();
            Console.WriteLine("--- " + texto + " ---");
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

        private static string textoDeLista(List<int> valores)
        {
            string texto = "";
            foreach (int valor in valores)
            {
                if (texto != "")
                    texto += ",";
                texto += valor;
            }
            return texto;
        }

        private static Carta crearCarta(int id, string nombre, int f, int v, int r, int i, int e, int h)
        {
            return new Carta(id, new Personaje(nombre, "Descripción de prueba", "", f, v, r, i, e, h));
        }

        private static List<Carta> crearCartas(int cantidad)
        {
            List<Carta> cartas = new List<Carta>();
            for (int i = 1; i <= cantidad; i++)
                cartas.Add(crearCarta(i, "Carta " + i, 650, 700, 500, 300, 400, 450));
            return cartas;
        }

        private static GestorDeCartas gestorConCartas(int cantidad)
        {
            string carpeta = carpetaTemporal();
            for (int i = 1; i <= cantidad; i++)
            {
                File.WriteAllText(Path.Combine(carpeta, "Personaje " + i + ".txt"),
                    "NOMBRE\nPersonaje " + i + "\nDESCRIPCION\nDe prueba\nASCIIART\n :) \nFINASCIIART\nATRIBUTOS\n"
                    + "FUERZA;650\nVELOCIDAD;700\nRESISTENCIA;500\nINTELIGENCIA;300\nENERGIA;400\nHABILIDAD;450\n");
            }
            GestorDeCartas gestor = new GestorDeCartas();
            gestor.cargarCartas(carpeta);
            Directory.Delete(carpeta, true);
            return gestor;
        }

        private static string carpetaTemporal()
        {
            string carpeta = Path.Combine(Path.GetTempPath(), "PruebasGuerra" + new Random().Next(1000000));
            Directory.CreateDirectory(carpeta);
            return carpeta;
        }
    }
}
