using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabajoFinal_Fink_Szymsio_Fio
{
    public class Comunicacion
    {
        private string archivo;
        private string datosEnviados;

        public void enviarCarta(Carta carta)
        {
            if (hayConexion())
            {
                datosEnviados = serializar(carta);
                transmitir(datosEnviados);
            }
            else
            {
                Console.WriteLine("Se perdió la conexión con el rival");
            }
        }

        public void enviarAtributo(string atributo)
        {
            if (hayConexion())
            {
                datosEnviados = atributo;
                transmitir(datosEnviados);
            }
            else
            {
                Console.WriteLine("Se perdió la conexión con el rival");
            }
        }

        public string recibirDatos()
        {
            string datos = "";
            if (hayConexion())
            {
                datos = leerDeLaConexion();
            }
            else
            {
                Console.WriteLine("Se perdió la conexión con el rival");
                datos = "DESCONECTADO";
            }
            return datos;
        }

        public bool sincronizar()
        {
            bool sincronizado;
            string datosRecibidos;
            sincronizado = hayConexion();
            if (sincronizado)
            {
                datosEnviados = resumenDelArchivo(archivo);
                transmitir(datosEnviados);
                datosRecibidos = recibirDatos();
                sincronizado = datosRecibidos == datosEnviados;
                if (!sincronizado)
                {
                    Console.WriteLine("Los archivos de cartas de ambas computadoras no son idénticos");
                }
            }
            else
            {
                Console.WriteLine("No hay conexión con el otro jugador");
            }
            return sincronizado;
        }
    }
}
