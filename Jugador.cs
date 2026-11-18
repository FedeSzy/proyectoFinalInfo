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

        public Jugador(string nombre, Mazo mazo, Carta cartaElegida, int puntos)
        {
            this.nombre = nombre;
            this.mazo = mazo;
            this.cartaElegida = cartaElegida;
            this.puntos = puntos;
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
            return cartaElegida ;
        }

        public Mazo getMazo() {return mazo;}
    }
}
