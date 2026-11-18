using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabajoFinal_Fink_Szymsio_Fio
{
    // Guarda los datos de la ficha del personaje: nombre, descripción, ASCII Art y los seis atributos.
    // El constructor valida los datos, por lo que nunca puede existir un Personaje inválido.
    public class Personaje
    {
        // Orden oficial de los atributos (es el mismo orden del archivo y de getAtributos()).
        public static readonly string[] ATRIBUTOS = { "fuerza", "velocidad", "resistencia", "inteligencia", "energia", "habilidad" };

        private const string SIN_IMAGEN = "\n   (No hay imagen para mostrar)\n";

        private string nombre;
        private string descripcion;
        private string asciiArt;
        private int fuerza;
        private int velocidad;
        private int resistencia;
        private int inteligencia;
        private int energia;
        private int habilidad;

        public Personaje(string nombre, string descripcion, string asciiArt,
                         int fuerza, int velocidad, int resistencia, int inteligencia, int energia, int habilidad)
        {
            List<string> errores;

            if (nombre == null || nombre.Trim() == "")
                throw new ArgumentException("El nombre del personaje no puede estar vacío.");

            errores = ValidadorDeAtributos.validar(new int[] { fuerza, velocidad, resistencia, inteligencia, energia, habilidad });
            if (errores.Count > 0)
                throw new ArgumentException(string.Join(" ", errores));

            this.nombre = nombre.Trim();
            this.descripcion = descripcion == null ? "" : descripcion.Trim();
            if (asciiArt == null || asciiArt.Trim() == "")
                this.asciiArt = SIN_IMAGEN;
            else
                this.asciiArt = asciiArt;
            this.fuerza = fuerza;
            this.velocidad = velocidad;
            this.resistencia = resistencia;
            this.inteligencia = inteligencia;
            this.energia = energia;
            this.habilidad = habilidad;
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

        // Orden: fuerza, velocidad, resistencia, inteligencia, energía, habilidad (suman 3000).
        // Devuelve una lista nueva para no exponer el estado interno.
        public List<int> getAtributos()
        {
            List<int> atributos = new List<int>();
            atributos.Add(fuerza);
            atributos.Add(velocidad);
            atributos.Add(resistencia);
            atributos.Add(inteligencia);
            atributos.Add(energia);
            atributos.Add(habilidad);
            return atributos;
        }

        // Devuelve el valor del atributo indicado por nombre (por ejemplo "fuerza").
        public int getValorAtributo(string atributo)
        {
            int posicion = posicionDeAtributo(atributo);
            if (posicion == -1)
                throw new ArgumentException("Atributo inválido: " + atributo);
            return getAtributos()[posicion];
        }

        public string getAtributoPrincipal()
        {
            List<int> valores = getAtributos();
            int mayor = 0;
            for (int i = 1; i < valores.Count; i++)
            {
                if (valores[i] > valores[mayor])
                    mayor = i;
            }
            return ATRIBUTOS[mayor];
        }

        public string getAtributoMasBajo()
        {
            List<int> valores = getAtributos();
            int menor = 0;
            for (int i = 1; i < valores.Count; i++)
            {
                if (valores[i] < valores[menor])
                    menor = i;
            }
            return ATRIBUTOS[menor];
        }

        // Devuelve la posición del atributo en ATRIBUTOS, o -1 si no existe.
        public static int posicionDeAtributo(string atributo)
        {
            int posicion = -1;
            string buscado = normalizarAtributo(atributo);
            int i = 0;
            while (i < ATRIBUTOS.Length && posicion == -1)
            {
                if (ATRIBUTOS[i] == buscado)
                    posicion = i;
                i++;
            }
            return posicion;
        }

        public static bool esAtributoValido(string atributo)
        {
            return posicionDeAtributo(atributo) != -1;
        }

        // Pasa a minúsculas, quita espacios y acentos para aceptar "Energía", " FUERZA ", etc.
        public static string normalizarAtributo(string atributo)
        {
            string resultado = "";
            if (atributo != null)
            {
                resultado = atributo.Trim().ToLower()
                    .Replace("á", "a").Replace("é", "e").Replace("í", "i")
                    .Replace("ó", "o").Replace("ú", "u");
            }
            return resultado;
        }
    }
}
