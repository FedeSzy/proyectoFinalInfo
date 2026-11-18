using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabajoFinal_Fink_Szymsio_Fio
{
    internal class Menu
    {
        private List<String> opciones;
        private int posicionActual;
        public Menu()
        {
            opciones = new List<String>();
            this.posicionActual = 0;
        }

        public void agregarOpcion(string opcion)
        {
            opciones.Add(opcion);
        }

        public bool borrarOpcion(string opcion)
        {
            string opcionEliminada = buscarOpcion(opcion);
            bool pude = false;
            if (opcionEliminada != null)
            {
                opciones.Remove(opcionEliminada);
                pude = true;
            }
            return pude;
        }

        public string buscarOpcion(string opcion)
        {
            string opcionEncontrada = null;
            int i = 0;
            while ((i < this.opciones.Count) && !this.opciones[i].Equals(opcion))
                i++;
            if (i < this.opciones.Count)
                opcionEncontrada = this.opciones[i];
            return opcionEncontrada;
        }

        public string menuInteractuar(string titulo)
        {
            int inicioMenu = mostrarMenu(titulo);
            int opcionElegida = seleccionar(inicioMenu, ConsoleColor.Cyan, ConsoleColor.Black);

            return (opciones[opcionElegida]);
        }

        // Agregado para el Juego Guerra: igual que menuInteractuar, pero devuelve la posición de la
        // opción elegida y deja el cursor visible debajo del menú para seguir escribiendo.
        public int elegirOpcion(string titulo)
        {
            int inicioMenu = mostrarMenu(titulo);
            int opcionElegida = seleccionar(inicioMenu, ConsoleColor.Cyan, ConsoleColor.Black);
            Console.SetCursorPosition(0, inicioMenu + opciones.Count);
            Console.CursorVisible = true;
            Console.WriteLine();
            return opcionElegida;
        }

        public int mostrarMenu(string titulo)
        {
            int lineaNumero = Console.CursorTop + 3;

            Console.WriteLine("<=============>");
            Console.WriteLine(titulo);
            Console.WriteLine("<=============>");
            if (opciones.Count() == 0)
                Console.WriteLine("No hay opciones.");
            else
                foreach (string opcion in opciones)
                {
                    Console.WriteLine("  " + opcion);
                }
            return lineaNumero; // numero de linea donde arranca el menu
        }

        public int seleccionar(int inicioMenu, ConsoleColor colorFondo, ConsoleColor colorLetra)
        {
            int opcionElegida;
            this.posicionActual = 0;

            Console.CursorVisible = false;

            // Resaltar la primera opcion antes de esperar cualquier tecla
            Console.SetCursorPosition(0, inicioMenu + this.posicionActual);
            Console.ForegroundColor = colorLetra;
            Console.BackgroundColor = colorFondo;
            Console.Write("> " + opciones[this.posicionActual]);
            Console.ResetColor();

            ConsoleKeyInfo tecla;
            tecla = Console.ReadKey(true);
            while (!tecla.Key.Equals(ConsoleKey.Enter))
            {
                switch (tecla.Key)
                {
                    case ConsoleKey.UpArrow:
                        Console.SetCursorPosition(0, inicioMenu + this.posicionActual);
                        Console.Write("  " + opciones[this.posicionActual]);

                        this.posicionActual--;
                        if (this.posicionActual < 0)
                            this.posicionActual = opciones.Count - 1;

                        break;

                    case ConsoleKey.DownArrow:
                        Console.SetCursorPosition(0, inicioMenu + this.posicionActual);
                        Console.Write("  " + opciones[this.posicionActual]);

                        this.posicionActual++;
                        if (this.posicionActual >= opciones.Count)
                            this.posicionActual = 0;

                        break;
                }

                Console.SetCursorPosition(0, inicioMenu + this.posicionActual);
                Console.ForegroundColor = colorLetra;
                Console.BackgroundColor = colorFondo;
                Console.Write("> " + opciones[this.posicionActual]);
                Console.ResetColor();

                tecla = Console.ReadKey(true);
            }

            opcionElegida = this.posicionActual;
            return opcionElegida;
        }
    }
}