using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabajoFinal_Fink_Szymsio_Fio
{
    // La primera carta de la lista es la que se juega; las cartas nuevas van al final
    public class Mazo
    {
        private List<Carta> cartas;

        public Mazo()
        {
            cartas = new List<Carta>();
        }

        public void agregarCarta(Carta carta)
        {
            if (carta != null && !contieneCarta(carta))
            {
                cartas.Add(carta);
            }
        }

        public void quitarCarta(Carta carta)
        {
            if (contieneCarta(carta))
            {
                cartas.Remove(carta);
            }
        }

        // Recorre desde el último índice hasta el primero intercambiando cada carta con otra al azar
        public void mezclar()
        {
            int i;
            int j;
            Carta temp;
            Random rnd = new Random();
            i = cartas.Count - 1;
            while (i > 0)
            {
                j = rnd.Next(0, i + 1);
                temp = cartas[i];
                cartas[i] = cartas[j];
                cartas[j] = temp;
                i = i - 1;
            }
        }

        // Extrae cartas desde el comienzo del mazo hasta llegar a "cantidad" o quedarse sin cartas
        public List<Carta> repartir(int cantidad)
        {
            List<Carta> repartidas = new List<Carta>();
            int i = 0;
            while (i < cantidad && !estaVacio())
            {
                repartidas.Add(cartas[0]);
                cartas.RemoveAt(0);
                i = i + 1;
            }
            return repartidas;
        }

        // Carta que participa del enfrentamiento (la primera). Devuelve null si el mazo está vacío.
        public Carta verPrimera()
        {
            Carta primera = null;
            if (!estaVacio())
                primera = cartas[0];
            return primera;
        }

        public bool contieneCarta(Carta carta)
        {
            bool encontrada = false;
            foreach (Carta c in cartas)
            {
                if (c == carta)
                {
                    encontrada = true;
                }
            }
            return encontrada;
        }

        public int cantidadCartas()
        {
            return cartas.Count;
        }

        public bool estaVacio()
        {
            return cartas.Count == 0;
        }

        public List<Carta> getCartas()
        {
            return new List<Carta>(cartas);
        }
    }
}
