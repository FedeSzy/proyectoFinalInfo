namespace TrabajoFinal_Fink_Szymsio_Fio
{
    internal class Program
    {
        private const string CREAR = "Crear partida (esta PC es el SERVIDOR)";
        private const string UNIRSE = "Unirse a una partida (CLIENTE)";
        private const string VER_CARTAS = "Ver cartas";
        private const string PRUEBAS = "Ejecutar pruebas";
        private const string SALIR = "Salir";

        static void Main(string[] args)
        {
            GestorDeCartas gestor = new GestorDeCartas();
            Menu menu = new Menu();
            string opcion;

            gestor.cargarCartas(Path.Combine(Directory.GetCurrentDirectory(), "Cartas"));

            menu.agregarOpcion(CREAR);
            menu.agregarOpcion(UNIRSE);
            menu.agregarOpcion(VER_CARTAS);
            menu.agregarOpcion(PRUEBAS);
            menu.agregarOpcion(SALIR);

            do
            {
                Console.Clear();
                opcion = menu.menuInteractuar("===== JUEGO GUERRA =====");
                Console.CursorVisible = true;
                Console.Clear();

                if (opcion == CREAR)
                    jugar(gestor, true);
                else if (opcion == UNIRSE)
                    jugar(gestor, false);
                else if (opcion == VER_CARTAS)
                    mostrarCartas(gestor);
                else if (opcion == PRUEBAS)
                    Pruebas.ejecutarTodas();

                if (opcion != SALIR)
                    pausa();
            } while (opcion != SALIR);

            Console.WriteLine("Fin del programa.");
        }

        static void jugar(GestorDeCartas gestor, bool esServidor)
        {
            string nombre = leerTextoObligatorio("Tu nombre: ");
            string servidor = "";

            if (!esServidor)
                servidor = leerTextoObligatorio("Nombre de la PC servidor: ");

            Partida partida = new Partida(nombre, gestor, new Comunicacion(esServidor, servidor));
            partida.iniciar();
        }

        static void mostrarCartas(GestorDeCartas gestor)
        {
            List<Carta> cartas = gestor.getCartasDisponibles();

            if (gestor.hayErrores())
                gestor.mostrarErrores();

            if (cartas.Count == 0)
            {
                Console.WriteLine("No hay cartas disponibles.");
            }
            else
            {
                for (int i = 0; i < cartas.Count; i++)
                {
                    cartas[i].mostrar();
                    pausa();
                }
                Console.WriteLine("\nCantidad de cartas: " + cartas.Count);
            }
        }

        static string leerTextoObligatorio(string mensaje)
        {
            Console.Write(mensaje);
            string texto = Console.ReadLine();
            while (texto == null || texto.Trim() == "")
            {
                Console.Write("El dato no puede estar vacio. Ingrese nuevamente: ");
                texto = Console.ReadLine();
            }
            return texto.Trim();
        }

        private static void pausa()
        {
            Console.WriteLine("\n\nPresione Enter para continuar: ");
            Console.ReadLine();
            Console.Clear();
        }
    }
}
