using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabajoFinal_Fink_Szymsio_Fio
{
    // La carta no repite los datos del personaje: se los pide a su Personaje (delegación)
    public class Carta
    {
        private int id;
        private Personaje personaje;

        public Carta(int id, Personaje personaje)
        {
            this.id = id;
            this.personaje = personaje;
        }

        // Devuelve 1 si esta carta tiene el valor mayor, -1 si tiene el menor y 0 si empatan
        public int compararAtributo(Carta carta, string atributo)
        {
            int miValor = getValor(atributo);
            int suValor = carta.getValor(atributo);
            int resultado;

            if (miValor == -1)
            {
                Console.WriteLine("Atributo inválido");
                resultado = 0;
            }
            else if (miValor > suValor)
            {
                resultado = 1;
            }
            else if (miValor < suValor)
            {
                resultado = -1;
            }
            else
            {
                resultado = 0;
            }
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

        public void mostrar()
        {
            List<int> valores = personaje.getAtributos();

            Console.WriteLine();
            Console.WriteLine("================================");
            Console.WriteLine(personaje.getNombre());
            Console.WriteLine("================================");
            Console.WriteLine(personaje.getAsciiArt());
            Console.WriteLine(personaje.getDescripcion());
            Console.WriteLine("--------------------------------");
            for (int i = 0; i < valores.Count; i++)
            {
                Console.WriteLine(Personaje.ATRIBUTOS[i].ToUpper() + ": " + valores[i]);
            }
            Console.WriteLine("================================");
        }
    }
}
