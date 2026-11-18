using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabajoFinal_Fink_Szymsio_Fio
{
    public class Jugador
    {
        private string nombre;
        private Mazo mazo;
        private Carta cartaElegida;
        private int puntos;

        public Jugador(string nombre)
        {
            this.nombre = nombre;
            this.mazo = new Mazo();
            this.cartaElegida = null;
            this.puntos = 0;
        }

        public void elegirCarta(Carta carta)
        {
            cartaElegida = carta;
        }

        public void sumarPunto()
        {
            puntos = puntos + 1;
        }

        public int getPuntos()
        {
            return puntos;
        }

        public Carta getCartaElegida()
        {
            return cartaElegida;
        }

        public Mazo getMazo()
        {
            return mazo;
        }

        public string getNombre()
        {
            return nombre;
        }
    }
}
