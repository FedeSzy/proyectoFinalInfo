using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabajoFinal_Fink_Szymsio_Fio
{
    // Forma jugable de un personaje. No duplica sus datos: los obtiene delegando en Personaje (composición).
    public class Carta
    {
        private int id;
        private Personaje personaje;

        public Carta(int id, Personaje personaje)
        {
            if (personaje == null)
                throw new ArgumentException("Una carta necesita un personaje.");
            this.id = id;
            this.personaje = personaje;
        }

        // Devuelve 1 si esta carta tiene el valor mayor, -1 si tiene el menor y 0 si empatan.
        public int compararAtributo(Carta carta, string atributo)
        {
            int miValor;
            int suValor;
            int resultado;

            if (carta == null)
                throw new ArgumentException("No hay carta rival para comparar.");

            miValor = getValor(atributo);
            suValor = carta.getValor(atributo);
            if (miValor > suValor)
                resultado = 1;
            else if (miValor < suValor)
                resultado = -1;
            else
                resultado = 0;
            return resultado;
        }

        public int getValor(string atributo)
        {
            return personaje.getValorAtributo(atributo);
        }

        public int getId()
        {
            return id;
        }

        public string getNombre()
        {
            return personaje.getNombre();
        }

        public Personaje getPersonaje()
        {
            return personaje;
        }

        // Muestra la carta completa. Los atributos de la lista "bloqueados" se marcan como no disponibles.
        public void mostrar(List<string> bloqueados)
        {
            List<int> valores = personaje.getAtributos();

            Console.WriteLine("==============================================");
            Console.WriteLine("  " + personaje.getNombre().ToUpper());
            Console.WriteLine("==============================================");
            Console.WriteLine(personaje.getAsciiArt().TrimEnd());
            Console.WriteLine("----------------------------------------------");
            Console.WriteLine(personaje.getDescripcion());
            Console.WriteLine("----------------------------------------------");
            for (int i = 0; i < valores.Count; i++)
            {
                string nombre = Personaje.ATRIBUTOS[i].ToUpper().PadRight(13);
                string barra = new string('#', valores[i] / 50).PadRight(15);
                string marca = "";
                if (bloqueados != null && bloqueados.Contains(Personaje.ATRIBUTOS[i]))
                    marca = "  [BLOQUEADO]";
                Console.WriteLine("  " + nombre + valores[i].ToString().PadLeft(4) + "  " + barra + marca);
            }
            Console.WriteLine("==============================================");
        }

        public void mostrar()
        {
            mostrar(null);
        }
    }
}
