using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TrabajoFinal_Fink_Szymsio_Fio
{
    public class Mazo
    {
        private List<Carta> cartas;

        public Mazo(List<Carta> cartas)
        {
            this.cartas = cartas;
        }

        public void agregarCarta(Carta carta)
        {
            if (!contieneCarta(carta))
            {
                cartas.add(carta);
            }
        }

        public void quitarCarta(Carta carta)
        {
            if (contieneCarta(carta))
            {
                cartas.Remove(carta);
            }
        }

        public void mezclar()
        {
            int i;
            int j;
            Carta temp;
            Random rnd = new Random();
            i = cartas.Count() - 1;
            while (i > 0)
            {
                j = rnd.Next(0, i);
                temp = cartas.get(i);
                cartas.set(i, cartas.get(j));
                cartas.set(j, temp);
                i = i - 1;
            }
        }

        public List<Carta> repartir(int cantidad)
        {
            List<Carta> repartidas = new List<Carta>();
            int i = 0;
            while (i < cantidad && !cartas.isEmpty())
            {
                repartidas.Add(cartas.get(0));
                cartas.Remove(0);
                i = i + 1;
            }
            return repartidas;
        }

        public bool contieneCarta(Carta carta)
        {
            bool encontrada = false;
            foreach (Carta c in cartas)
            {
                if (c.equals(carta))
                {
                    encontrada = true;
                }
            }
            return encontrada;
        }

        public List<Carta> getCartas()
        {
            return cartas;
        }
    }

}
