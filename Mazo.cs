using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabajoFinal_Fink_Szymsio_Fio
{
    // Administra una lista ordenada de cartas. La primera carta es la que se juega en la próxima ronda
    // y las cartas que se agregan van al final.
    public class Mazo
    {
        private List<Carta> cartas;

        public Mazo()
        {
            cartas = new List<Carta>();
        }

        public Mazo(List<Carta> cartasIniciales)
        {
            cartas = new List<Carta>();
            if (cartasIniciales != null)
            {
                foreach (Carta carta in cartasIniciales)
                    agregarCarta(carta);
            }
        }

        // Incorpora una carta al final del mazo. No se permiten cartas nulas ni repetidas.
        public bool agregarCarta(Carta carta)
        {
            bool agregada = false;
            if (carta != null && !contieneCarta(carta))
            {
                cartas.Add(carta);
                agregada = true;
            }
            return agregada;
        }

        // Retira una carta del mazo. Devuelve false si la carta no estaba.
        public bool quitarCarta(Carta carta)
        {
            bool quitada = false;
            if (contieneCarta(carta))
            {
                cartas.Remove(carta);
                quitada = true;
            }
            return quitada;
        }

        // Mezcla con el algoritmo de Fisher-Yates: recorre desde el último índice hasta el primero
        // e intercambia cada carta con otra elegida al azar entre 0 e i (inclusive).
        // Recibe el generador para que las dos computadoras puedan mezclar igual usando la misma semilla.
        public void mezclar(Random azar)
        {
            int i;
            int j;
            Carta temporal;

            if (azar == null)
                azar = new Random();
            i = cartas.Count - 1;
            while (i > 0)
            {
                j = azar.Next(0, i + 1);
                temporal = cartas[i];
                cartas[i] = cartas[j];
                cartas[j] = temporal;
                i = i - 1;
            }
        }

        public void mezclar()
        {
            mezclar(new Random());
        }

        // Extrae hasta "cantidad" cartas desde el comienzo del mazo, en su orden original.
        public List<Carta> repartir(int cantidad)
        {
            List<Carta> repartidas = new List<Carta>();
            int i = 0;

            if (cantidad < 0)
                throw new ArgumentException("La cantidad de cartas a repartir no puede ser negativa.");

            while (i < cantidad && !estaVacio())
            {
                repartidas.Add(cartas[0]);
                cartas.RemoveAt(0);
                i = i + 1;
            }
            return repartidas;
        }

        // Carta que participa del próximo enfrentamiento (no la retira). Devuelve null si el mazo está vacío.
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
            int i = 0;
            while (i < cartas.Count && !encontrada)
            {
                if (cartas[i] == carta)
                    encontrada = true;
                i++;
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

        // Devuelve una copia para que nadie pueda modificar el mazo sin pasar por sus métodos.
        public List<Carta> getCartas()
        {
            return new List<Carta>(cartas);
        }
    }
}
