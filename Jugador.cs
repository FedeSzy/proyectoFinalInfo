using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabajoFinal_Fink_Szymsio_Fio
{
    // Representa a cada participante: su nombre, su mazo, la carta que juega en la ronda y las rondas ganadas.
    public class Jugador
    {
        private string nombre;
        private Mazo mazo;
        private Carta cartaElegida;
        private int puntos;

        public Jugador(string nombre)
        {
            if (nombre == null || nombre.Trim() == "")
                nombre = "Jugador";
            this.nombre = nombre.Trim();
            this.mazo = new Mazo();
            this.cartaElegida = null;
            this.puntos = 0;
        }

        // Elige la carta para la ronda. Solo puede elegirse una carta que esté en el propio mazo.
        public bool elegirCarta(Carta carta)
        {
            bool elegida = false;
            if (carta != null && mazo.contieneCarta(carta))
            {
                cartaElegida = carta;
                elegida = true;
            }
            return elegida;
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

        public int cantidadCartas()
        {
            return mazo.cantidadCartas();
        }

        public bool sinCartas()
        {
            return mazo.estaVacio();
        }
    }
}
