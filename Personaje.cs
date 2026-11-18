using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabajoFinal_Fink_Szymsio_Fio
{
    public class Personaje
    {
        private string nombre;
        private int fuerza;
        private int velocidad;
        private int resistencia;
        private int inteligencia;
        private int energia;
        private int habilidad;

        public Personaje(string nombre, int fuerza, int velocidad, int resistencia, int inteligencia, int energia, int habilidad)
        {
            this.nombre = nombre;
            this.fuerza = fuerza;
            this.velocidad = velocidad;
            this.resistencia = resistencia;
            this.inteligencia = inteligencia;
            this.energia = energia;
            this.habilidad = habilidad;
        }

        public List<int> getAtributos()
        {
            List<int> atributos = new List<int>();
            // Orden: fuerza, velocidad, resistencia, inteligencia, energía, habilidad (suman 3000)
            atributos.Add(fuerza);
            atributos.Add(velocidad);
            atributos.Add(resistencia);
            atributos.Add(inteligencia);
            atributos.Add(energia);
            atributos.Add(habilidad);
            return atributos;
        }

        public string getNombre()
        {
            return nombre;
        }
    }
}
