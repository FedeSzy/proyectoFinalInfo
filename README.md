# Juego Guerra — Proyecto Integrador (Taller de Programación, C#)

Fink · Szymsiowicz · Fio — 3.º año Informática

Juego de cartas por consola para dos computadoras conectadas en red local, según las reglas de la
Actividad 2 de TI.

## Cómo ejecutarlo

Abrir `TrabajoFinal_Fink_Szymsio_Fio.sln` en Visual Studio y ejecutar (o `dotnet run`).
Los menús se manejan con las flechas ↑ ↓ y Enter.

1. Las dos computadoras tienen que tener **los mismos archivos** en la carpeta `Cartas`.
2. **PC servidor:** `Crear partida`. Crea `C:\JuegoGuerra\Guerra` y la comparte con el nombre `Guerra`
   (pide permiso de administrador). Muestra el nombre de la PC para dárselo al rival.
3. **PC cliente:** `Unirse a una partida` y escribir el nombre de la PC servidor.
   Para probar en una sola PC, en lugar del nombre se puede escribir la ruta de la carpeta
   (`C:\JuegoGuerra\Guerra`).

## Reglas (Actividad 2)

- Se juntan todas las cartas, se mezclan y se reparten 7 a cada jugador. Si no alcanzan, cada uno
  recibe la misma cantidad (la mitad); si la cantidad es impar, la última no se reparte.
- Las cartas se juegan en el orden en que fueron repartidas.
- En la primera ronda el selector se sortea; después elige el ganador de la ronda anterior.
- El selector ve su carta y elige el atributo; el otro jugador confía en ese atributo.
- Gana la ronda el valor mayor: se informa el ganador, que absorbe la carta del rival y las dos van al
  final de su mazo.
- El atributo usado queda bloqueado hasta que no quede ninguno disponible; ahí se desbloquean todos.
- No se ven las cartas del rival, salvo la que se absorbe.
- Pierde el que se queda sin cartas.
- Si un archivo de cartas tiene errores, al iniciar la partida se muestra el nombre del archivo y las
  reglas de estructura, y la partida se da por terminada.
- Si un jugador se desconecta (o elige `Rendirse`), el otro gana automáticamente.
- Empate (no está en la Actividad 2): cada carta vuelve al final de su mazo y sigue eligiendo el mismo jugador.

## Clases

| Clase | Responsabilidad |
|---|---|
| `Personaje` | Nombre, descripción, ASCII Art y los seis atributos. Valida las reglas de puntos. |
| `Carta` | Usa un `Personaje` (delegación). Compara un atributo con otra carta y se muestra. |
| `Mazo` | Agregar, quitar, mezclar, repartir, ver la primera carta y cantidad de cartas. |
| `Jugador` | Nombre, mazo, carta elegida y rondas ganadas. |
| `Ronda` | Un enfrentamiento: elegir/recibir el atributo, comparar cartas y aplicar el resultado. |
| `Partida` | Conexión, reparto, sorteo, rondas, atributos bloqueados, turno y ganador. |
| `GestorDeCartas` | Lee la carpeta `Cartas` (un método por sección del archivo), informa errores, mezcla y reparte. |
| `Comunicacion` | Intercambio de la carta y el atributo elegidos a través del archivo compartido. |
| `ArchivoTexto`, `PartidaCompartida` | Tomadas del TP ClienteServidor: lectura/escritura del archivo y carpeta compartida. |
| `Menu` | El menú del TP Menu_Fede_TP03 (se le agregó `elegirOpcion`). |
| `Pruebas` | Pruebas de cada etapa (opción `Ejecutar pruebas` del menú). |

## Archivo de cada carta (`Cartas/{nombreDelPersonaje}.txt`)

```
NOMBRE
Hilarius Marianus II
DESCRIPCION
Repartidor intergaláctico humano...
ASCIIART
(dibujo)
FINASCIIART
ATRIBUTOS
FUERZA;650
VELOCIDAD;700
RESISTENCIA;500
INTELIGENCIA;300
ENERGIA;400
HABILIDAD;450
```

La suma debe ser 3000, cada atributo entre 250 y 750, al menos uno con 650 o más, al menos uno con
350 o menos, y no pueden ser los seis 500.

## Archivo de partida compartida (`Guerra.txt`)

Como en el TP ClienteServidor, tiene una línea `CLAVE=VALOR` por dato y `TURNO` indica qué
computadora tiene que actuar. Solo escribe la que tiene el turno, y al terminar se lo pasa a la otra.

```
ESTADO=EN_JUEGO
NOMBRE_SERVIDOR=Ana
NOMBRE_CLIENTE=Beto
CARTAS_SERVIDOR=Golem de Basalto;Lira la Hechicera;...
CARTAS_CLIENTE=Robot K-9000;Nimbo el Zorro Ninja;...
SELECTOR_INICIAL=CLIENTE
RONDA=3
ATRIBUTO=velocidad
CARTA_CLIENTE=Nimbo el Zorro Ninja
CARTA_SERVIDOR=Robot K-9000
GANADOR_RONDA_1=Ana
GANADOR_RONDA_2=Beto
PUNTOS_SERVIDOR=1
PUNTOS_CLIENTE=1
TURNO=SERVIDOR
```

Una ronda:
1. El selector elige el atributo y guarda `ATRIBUTO` y su `CARTA_...`; pasa el `TURNO` al otro.
2. El otro lee el atributo y la carta, resuelve la ronda, guarda su `CARTA_...` y devuelve el `TURNO`.
3. El selector lee esa carta, resuelve la ronda, guarda el ganador y los puntos, aumenta `RONDA` y le
   da el `TURNO` al próximo selector (o guarda `ESTADO=FINALIZADO` si alguien se quedó sin cartas).

Si `RONDA` o la carta recibida no corresponden a lo esperado, la partida se cancela. Si el rival no
responde en 5 minutos, o el archivo no se puede leer durante ese tiempo, se lo considera desconectado.
