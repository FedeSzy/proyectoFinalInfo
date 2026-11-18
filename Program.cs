using System.Text;

namespace TrabajoFinal_Fink_Szymsio_Fio
{
    internal class Program
    {
        private const string CARPETA_PERSONAJES = "Personajes";

        private static Partida partidaActual = null;

        static void Main(string[] args)
        {
            int opcion;
            GestorDeCartas gestor;

            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            // dotnet run -- --pruebas  ejecuta las pruebas automáticas y termina
            if (args.Length > 0 && args[0] == "--pruebas")
            {
                Environment.Exit(Pruebas.ejecutarTodas() ? 0 : 1);
            }

            Console.CancelKeyPress += alCancelar;
            mostrarTitulo();
            gestor = cargarPersonajes();

            do
            {
                opcion = elegirOpcionPrincipal();
                switch (opcion)
                {
                    case 1:
                        jugar(gestor, true);
                        break;
                    case 2:
                        jugar(gestor, false);
                        break;
                    case 3:
                        mostrarCartas(gestor);
                        break;
                    case 4:
                        Pruebas.ejecutarTodas();
                        Consola.pausa();
                        break;
                }
            } while (opcion != 0);

            Console.WriteLine("¡Hasta la próxima!");
        }

        static void mostrarTitulo()
        {
            Console.WriteLine("""
             ==============================================
                  _ _   _ _____ ____  ___
                 | | | | | ____/ ___|/ _ \
              _  | | | | |  _|| |  _| | | |
             | |_| | |_| | |__| |_| | |_| |
              \___/ \___/|_____\____|\___/
                   G   U   E   R   R   A
             ==============================================
            """);
        }

        // Devuelve 1 a 4 según la opción, o 0 para salir.
        static int elegirOpcionPrincipal()
        {
            int opcion;
            if (Consola.puedeUsarMenu())
            {
                List<string> opciones = new List<string>();
                opciones.Add("Crear partida (esta PC comparte la carpeta - SERVIDOR)");
                opciones.Add("Unirse a una partida (CLIENTE)");
                opciones.Add("Ver cartas disponibles");
                opciones.Add("Ejecutar pruebas");
                opciones.Add("Salir");
                Console.WriteLine();
                opcion = Consola.elegirConMenu("JUEGO GUERRA", opciones) + 1;
                if (opcion == opciones.Count)
                    opcion = 0;
            }
            else
            {
                mostrarMenu();
                opcion = Consola.leerOpcion("Opción: ", 0, 4, 0);
            }
            return opcion;
        }

        static void mostrarMenu()
        {
            Console.WriteLine();
            Console.WriteLine("===== JUEGO GUERRA =====");
            Console.WriteLine("1. Crear partida (esta PC comparte la carpeta - SERVIDOR)");
            Console.WriteLine("2. Unirse a una partida (CLIENTE)");
            Console.WriteLine("3. Ver cartas disponibles");
            Console.WriteLine("4. Ejecutar pruebas");
            Console.WriteLine("0. Salir");
        }

        static GestorDeCartas cargarPersonajes()
        {
            GestorDeCartas gestor = new GestorDeCartas();
            string carpeta = Path.Combine(AppContext.BaseDirectory, CARPETA_PERSONAJES);
            int cantidad = gestor.cargarCartas(carpeta);

            Console.WriteLine("Carpeta de personajes: " + carpeta);
            gestor.mostrarErrores();
            Console.WriteLine("Personajes válidos cargados: " + cantidad);
            if (!gestor.hayCartasSuficientes())
                Console.WriteLine("ATENCIÓN: se necesitan al menos " + GestorDeCartas.MINIMO_DE_CARTAS + " personajes válidos para jugar.");
            return gestor;
        }

        static void jugar(GestorDeCartas gestor, bool esServidor)
        {
            string nombre;
            string carpeta;

            if (!gestor.hayCartasSuficientes())
            {
                Console.WriteLine("No se puede jugar: no hay suficientes personajes válidos.");
            }
            else
            {
                nombre = Consola.leerTexto("Tu nombre: ", null);
                if (esServidor)
                    carpeta = pedirCarpetaServidor();
                else
                    carpeta = pedirCarpetaCliente();

                if (carpeta != null)
                {
                    partidaActual = new Partida(nombre, gestor, new Comunicacion(esServidor, carpeta));
                    partidaActual.iniciar();
                    partidaActual = null;
                }
            }
            Consola.pausa();
        }

        static string pedirCarpetaServidor()
        {
            string porDefecto = CarpetaCompartida.carpetaLocalPorDefecto();
            string carpeta = Consola.leerTexto("Carpeta a compartir [Enter = " + porDefecto + "]: ", porDefecto);

            if (!CarpetaCompartida.preparar(carpeta))
                carpeta = null;
            else
                Console.WriteLine("Decile a tu rival que se conecte a la PC: " + Environment.MachineName);
            return carpeta;
        }

        static string pedirCarpetaCliente()
        {
            string dato = Consola.leerTexto("Nombre de la PC servidor (o ruta completa de la carpeta compartida): ", null);
            string carpeta = null;
            if (dato != null)
                carpeta = CarpetaCompartida.rutaDelServidor(dato);
            return carpeta;
        }

        static void mostrarCartas(GestorDeCartas gestor)
        {
            List<Carta> cartas = gestor.getCartasDisponibles();
            if (cartas.Count == 0)
            {
                Console.WriteLine("No hay cartas disponibles.");
            }
            else
            {
                foreach (Carta carta in cartas)
                {
                    carta.mostrar();
                    Console.WriteLine("Atributo principal: " + carta.getPersonaje().getAtributoPrincipal().ToUpper()
                        + "  |  Atributo más bajo: " + carta.getPersonaje().getAtributoMasBajo().ToUpper());
                    Consola.pausa();
                }
                Console.WriteLine("Cantidad de cartas: " + cartas.Count);
            }
        }

        // Si el jugador cierra con Ctrl+C durante una partida, se avisa al rival que abandonó.
        static void alCancelar(object sender, ConsoleCancelEventArgs e)
        {
            if (partidaActual != null)
                partidaActual.abandonar();
        }
    }
}
