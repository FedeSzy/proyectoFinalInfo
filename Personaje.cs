using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabajoFinal_Fink_Szymsio_Fio
{
    public class Personaje
    {
        // Orden de los atributos: es el mismo orden del archivo y de getAtributos()
        public static readonly string[] ATRIBUTOS = { "fuerza", "velocidad", "resistencia", "inteligencia", "energia", "habilidad" };

        private string nombre;
        private string descripcion;
        private string asciiArt;
        private int fuerza;
        private int velocidad;
        private int resistencia;
        private int inteligencia;
        private int energia;
        private int habilidad;

        public Personaje(string nombre, string descripcion, string asciiArt, int fuerza, int velocidad, int resistencia, int inteligencia, int energia, int habilidad)
        {
            this.nombre = nombre;
            this.descripcion = descripcion;
            if (asciiArt.Trim() == "")
                this.asciiArt = "\nNo hay imagen para mostrar!!\n";
            else
                this.asciiArt = asciiArt;
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

        // Devuelve el valor del atributo indicado, o -1 si el atributo no existe
        public int getValorAtributo(string atributo)
        {
            int valor = -1;
            int posicion = posicionDeAtributo(atributo);
            if (posicion != -1)
                valor = getAtributos()[posicion];
            return valor;
        }

        public string getNombre()
        {
            return nombre;
        }

        public string getDescripcion()
        {
            return descripcion;
        }

        public string getAsciiArt()
        {
            return asciiArt;
        }

        // Devuelve la posición del atributo dentro de ATRIBUTOS, o -1 si no existe
        public static int posicionDeAtributo(string atributo)
        {
            int i = 0;
            int posicion = -1;
            while (i < ATRIBUTOS.Length && posicion == -1)
            {
                if (ATRIBUTOS[i] == atributo.Trim().ToLower())
                    posicion = i;
                i++;
            }
            return posicion;
        }

        // Reglas de TI para la distribución de puntos. Devuelve la lista de errores (vacía si es válida).
        public static List<string> validarAtributos(List<int> valores)
        {
            List<string> errores = new List<string>();
            int total = 0;
            bool hayAlto = false;
            bool hayBajo = false;
            bool todosEn500 = true;

            for (int i = 0; i < valores.Count; i++)
            {
                total = total + valores[i];
                if (valores[i] < 250 || valores[i] > 750)
                    errores.Add(ATRIBUTOS[i].ToUpper() + " vale " + valores[i] + " y debe estar entre 250 y 750.");
                if (valores[i] >= 650)
                    hayAlto = true;
                if (valores[i] <= 350)
                    hayBajo = true;
                if (valores[i] != 500)
                    todosEn500 = false;
            }

            if (total != 3000)
                errores.Add("La suma de los atributos es " + total + " y debe ser exactamente 3000.");
            if (todosEn500)
                errores.Add("No se permite asignar 500 puntos a los seis atributos.");
            if (!hayAlto)
                errores.Add("Al menos un atributo debe tener 650 puntos o más.");
            if (!hayBajo)
                errores.Add("Al menos un atributo debe tener 350 puntos o menos.");
            return errores;
        }
    }
}
