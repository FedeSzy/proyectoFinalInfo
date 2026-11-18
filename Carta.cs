using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabajoFinal_Fink_Szymsio_Fio
{
    public class Carta
    {
        private int id;
        private Personaje personaje;

        public Carta(int id, Personaje personaje)
        {
            this.id = id;
            this.personaje = personaje;
        }

        public int compararAtributo(Carta carta, string atributo)
        {
            List<int> misAtributos = personaje.getAtributos();
            List<int> susAtributos = carta.getPersonaje().getAtributos();
            int posicion;
            int resultado;
            switch (atributo)
            {
                case "fuerza":
                    posicion = 0;
                    break;
                case "velocidad":
                    posicion = 1;
                    break;
                case "resistencia":
                    posicion = 2;
                    break;
                case "inteligencia":
                    posicion = 3;
                    break;
                case "energia":
                    posicion = 4;
                    break;
                case "habilidad":
                    posicion = 5;
                    break;
                default:
                    Console.WriteLine("Atributo inválido");
                    posicion = -1;
                    break;
            }
            if (posicion == -1)
            {
                resultado = 0;
            }
            else if (misAtributos.get(posicion) > susAtributos.get(posicion))
            {
                resultado = 1;
            }
            else if (misAtributos.get(posicion) < susAtributos.get(posicion))
            {
                resultado = -1;
            }
            else
            {
                resultado = 0;
            }
            return resultado;
        }

        public Personaje getPersonaje()
        {
            return personaje;
        }
    }
}
