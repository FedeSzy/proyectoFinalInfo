using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabajoFinal_Fink_Szymsio_Fio
{
    // Contiene las reglas de distribución de puntos definidas en TI (Actividad 1).
    // Se separa de Personaje para que la regla de negocio quede en un solo lugar
    // y pueda probarse sin necesidad de leer archivos.
    public class ValidadorDeAtributos
    {
        public const int CANTIDAD_ATRIBUTOS = 6;
        public const int TOTAL_PUNTOS = 3000;
        public const int VALOR_MINIMO = 250;
        public const int VALOR_MAXIMO = 750;
        public const int ESPECIALIZACION_ALTA = 650;
        public const int ESPECIALIZACION_BAJA = 350;
        public const int VALOR_EQUILIBRADO = 500;

        // Devuelve la lista de errores encontrados. Si la lista está vacía, la distribución es válida.
        public static List<string> validar(int[] valores)
        {
            List<string> errores = new List<string>();

            if (valores == null || valores.Length != CANTIDAD_ATRIBUTOS)
            {
                errores.Add("Se necesitan exactamente " + CANTIDAD_ATRIBUTOS + " atributos.");
            }
            else
            {
                validarRangos(valores, errores);
                validarTotal(valores, errores);
                validarEspecializacion(valores, errores);
            }
            return errores;
        }

        public static bool esValido(int[] valores)
        {
            return validar(valores).Count == 0;
        }

        private static void validarRangos(int[] valores, List<string> errores)
        {
            for (int i = 0; i < valores.Length; i++)
            {
                if (valores[i] < VALOR_MINIMO || valores[i] > VALOR_MAXIMO)
                {
                    errores.Add("El atributo " + Personaje.ATRIBUTOS[i].ToUpper() + " vale " + valores[i]
                        + " y debe estar entre " + VALOR_MINIMO + " y " + VALOR_MAXIMO + ".");
                }
            }
        }

        private static void validarTotal(int[] valores, List<string> errores)
        {
            int total = 0;
            for (int i = 0; i < valores.Length; i++)
            {
                total = total + valores[i];
            }
            if (total != TOTAL_PUNTOS)
            {
                errores.Add("La suma de los atributos es " + total + " y debe ser exactamente " + TOTAL_PUNTOS + ".");
            }
        }

        private static void validarEspecializacion(int[] valores, List<string> errores)
        {
            bool hayAlto = false;
            bool hayBajo = false;
            bool todosEquilibrados = true;

            for (int i = 0; i < valores.Length; i++)
            {
                if (valores[i] >= ESPECIALIZACION_ALTA)
                    hayAlto = true;
                if (valores[i] <= ESPECIALIZACION_BAJA)
                    hayBajo = true;
                if (valores[i] != VALOR_EQUILIBRADO)
                    todosEquilibrados = false;
            }

            if (todosEquilibrados)
                errores.Add("No se permite asignar " + VALOR_EQUILIBRADO + " puntos a los seis atributos.");
            if (!hayAlto)
                errores.Add("Al menos un atributo debe tener " + ESPECIALIZACION_ALTA + " puntos o más.");
            if (!hayBajo)
                errores.Add("Al menos un atributo debe tener " + ESPECIALIZACION_BAJA + " puntos o menos.");
        }
    }
}
