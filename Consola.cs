using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabajoFinal_Fink_Szymsio_Fio
{
    // Funciones auxiliares para leer datos del teclado sin que el programa se rompa
    // si el usuario escribe algo incorrecto.
    public class Consola
    {
        // Lee un texto no vacío. Si la entrada se cerró devuelve el valor por defecto.
        public static string leerTexto(string mensaje, string porDefecto)
        {
            string texto;
            Console.Write(mensaje);
            texto = Console.ReadLine();
            while (texto != null && texto.Trim() == "" && porDefecto == null)
            {
                Console.Write("El dato no puede estar vacío. Ingrese nuevamente: ");
                texto = Console.ReadLine();
            }
            if (texto == null || texto.Trim() == "")
                texto = porDefecto;
            else
                texto = texto.Trim();
            return texto;
        }

        // Lee un número entero entre minimo y maximo. Si la entrada se cerró devuelve "siCierra".
        public static int leerOpcion(string mensaje, int minimo, int maximo, int siCierra)
        {
            int opcion = 0;
            bool valida = false;
            string texto;

            while (!valida)
            {
                Console.Write(mensaje);
                texto = Console.ReadLine();
                if (texto == null)
                {
                    opcion = siCierra;
                    valida = true;
                }
                else if (int.TryParse(texto.Trim(), out opcion) && opcion >= minimo && opcion <= maximo)
                {
                    valida = true;
                }
                else
                {
                    Console.WriteLine("Opción inválida. Debe ser un número entre " + minimo + " y " + maximo + ".");
                }
            }
            return opcion;
        }

        // El menú con flechas (clase Menu) necesita una consola real. Si la entrada o la salida están
        // redirigidas (por ejemplo, pruebas automáticas) se usa el ingreso por número.
        public static bool puedeUsarMenu()
        {
            return !Console.IsInputRedirected && !Console.IsOutputRedirected;
        }

        // Muestra las opciones con la clase Menu (se elige con las flechas y Enter).
        // Devuelve la posición de la opción elegida.
        public static int elegirConMenu(string titulo, List<string> opciones)
        {
            Menu menu = new Menu();
            int inicio;
            int elegida;

            foreach (string opcion in opciones)
                menu.agregarOpcion(opcion);
            inicio = menu.mostrarMenu(titulo);
            elegida = menu.seleccionar(inicio, ConsoleColor.Cyan, ConsoleColor.Black);

            // Se deja el cursor debajo del menú y visible para lo que siga.
            Console.SetCursorPosition(0, inicio + opciones.Count);
            Console.CursorVisible = true;
            Console.WriteLine();
            return elegida;
        }

        public static void pausa()
        {
            Console.WriteLine();
            Console.Write("Presione Enter para continuar...");
            Console.ReadLine();
        }

        public static void limpiar()
        {
            try
            {
                if (!Console.IsOutputRedirected)
                    Console.Clear();
            }
            catch (IOException)
            {
                // Algunas terminales no permiten borrar la pantalla: se continúa igual.
            }
        }

        public static void titulo(string texto)
        {
            Console.WriteLine();
            Console.WriteLine("==============================================");
            Console.WriteLine("  " + texto);
            Console.WriteLine("==============================================");
        }
    }
}
