# Juego Guerra — Proyecto Integrador (Taller de Programación, C#)

Fink · Szymsiowicz · Fio — 3.º año Informática

Juego de cartas por consola para **dos computadoras en red local**. Cada carta es un personaje
con seis atributos; en cada ronda el jugador selector elige un atributo y gana la carta con el
valor más alto, que absorbe la carta del rival. Pierde quien se queda sin cartas.

---

## Cómo ejecutarlo

Requisitos: .NET 8 (Visual Studio 2022 o `dotnet` CLI).

```bash
dotnet run                 # abre el menú del juego
dotnet run -- --pruebas    # ejecuta las pruebas automáticas (también está en el menú, opción 4)
```

En Visual Studio: abrir `TrabajoFinal_Fink_Szymsio_Fio.sln` y ejecutar (F5).

### Jugar entre dos computadoras

1. **Las dos PCs deben tener exactamente los mismos archivos en la carpeta `Personajes`.**
   El programa lo verifica al conectarse (compara un resumen de las cartas) y no deja jugar si difieren.
2. **PC servidor** → opción `1. Crear partida`.
   - Usa la carpeta `C:\JuegoGuerra\Guerra` (Enter acepta ese valor).
   - Si la carpeta no está compartida, intenta compartirla como `Guerra` pidiendo permiso de
     administrador (`net share Guerra="C:\JuegoGuerra\Guerra" /grant:Todos,FULL`). Si no puede,
     muestra los pasos para hacerlo a mano.
   - Muestra el nombre de la PC para dárselo al rival.
3. **PC cliente** → opción `2. Unirse a una partida` e ingresar el nombre de la PC servidor
   (se arma la ruta `\\NOMBRE-PC\Guerra`) o una ruta completa a la carpeta compartida.
4. Se sortea quién elige primero y comienza la partida. Para abandonar, escribir `rendirse`
   en el menú de atributos (o cerrar con Ctrl+C): el rival gana automáticamente.
   Los menús se manejan con las flechas ↑ ↓ y Enter.

> Para probar en **una sola PC**, abrir dos consolas: en la primera crear la partida y en la
> segunda unirse escribiendo la ruta de la carpeta (por ejemplo `C:\JuegoGuerra\Guerra`).

---

## Organización del código

| Archivo | Clase | Responsabilidad | Etapa Taller |
|---|---|---|---|
| `Personaje.cs` | `Personaje` | Nombre, descripción, ASCII Art y los 6 atributos. El constructor valida, nunca existe un personaje inválido. | 1 |
| `ValidadorDeAtributos.cs` | `ValidadorDeAtributos` | Reglas de puntos de TI (3000, 250–750, ≥650, ≤350, no todos 500). Devuelve la lista de errores. | 1 / 2 |
| `LectorDePersonajes.cs` | `LectorDePersonajes`, `ArchivoPersonajeException` | Lee **un** archivo de personaje, un método por sección (nombre, descripción, ASCII, atributos). | 2 |
| `Carta.cs` | `Carta` | Forma jugable de un personaje. Compone un `Personaje` y **delega** en él (no duplica datos). Compara atributos y se muestra. | 3 |
| `Mazo.cs` | `Mazo` | Agregar, quitar, mezclar (Fisher-Yates), repartir, ver la primera carta, cantidad, vacío. | 4 |
| `GestorDeCartas.cs` | `GestorDeCartas` | Carga la carpeta `Personajes`, descarta los inválidos informando el error, mezcla y reparte (7 por jugador o la mitad si no alcanzan). | 2 / 4 |
| `Jugador.cs` | `Jugador` | Nombre, mazo, carta elegida y rondas ganadas. | 5 |
| `Ronda.cs` | `Ronda` | Un enfrentamiento: intercambia las cartas, elige/recibe el atributo, compara y aplica el resultado. | 5 |
| `Partida.cs` | `Partida` | Flujo completo: conexión, sincronización, reparto, sorteo, rondas, atributos bloqueados, turno y ganador. | 5 / 7 |
| `Comunicacion.cs` | `Comunicacion` | Protocolo entre las dos PCs sobre el archivo compartido (envía/recibe carta y atributo, sincroniza, detecta desconexión). | 6 |
| `ArchivoCompartido.cs` | `ArchivoCompartido` | Lectura/escritura del archivo `Guerra.txt` con bloqueo y reintentos. | 6 |
| `CarpetaCompartida.cs` | `CarpetaCompartida` | Rutas local/red y creación/compartido de la carpeta (SSI). | 6 |
| `Menu.cs` | `Menu` | Menú con flechas y Enter (de Menu_Fede_TP03). Se usa en el menú principal y para elegir el atributo. | — |
| `Consola.cs` | `Consola` | Lectura segura del teclado y uso de `Menu`. Si la consola está redirigida (pruebas) se ingresa por número. | — |
| `Pruebas.cs` | `Pruebas` | 113 pruebas automáticas de todas las etapas y del plan de pruebas de TI. | todas |
| `Program.cs` | `Program` | Menú principal. | 7 |

Carpetas de datos (se copian junto al ejecutable al compilar):

- `Personajes/` — un archivo `.txt` por personaje. **Acá van los personajes de cada alumno.**
  Los incluidos (Hilarius Marianus II y siete personajes de ejemplo) sirven para probar el juego.
- `DatosDePrueba/` — archivos válidos e inválidos que usan las pruebas automáticas.

---

## Reglas implementadas

- Cada jugador recibe 7 cartas mezcladas al azar. Si no alcanzan para 14, cada uno recibe la mitad
  (parte entera); si el total es impar, sobra una. Hacen falta al menos 2 personajes válidos.
- Las cartas se juegan en el orden en que fueron repartidas (primera carta del mazo).
- En la primera ronda el selector se sortea; después elige quien ganó la ronda anterior.
- Gana la ronda el valor mayor en el atributo elegido: el ganador se lleva la carta del rival y
  ambas van al final de su mazo, y suma un punto.
- **Empate:** cada carta vuelve al final del mazo de su dueño y el próximo selector se sortea.
- **Atributos bloqueados:** el atributo usado no puede volver a elegirse hasta que se usaron los
  seis; en ese momento se desbloquean todos.
- El jugador solo ve su propia carta (y la del rival únicamente si la absorbe).
- Pierde la partida quien se queda sin cartas. Si un jugador abandona o se desconecta, gana el otro.

---

## Formato del archivo de personaje

Nombre del archivo: `{nombreDelPersonaje}.txt` dentro de `Personajes/`.

```
NOMBRE
Hilarius Marianus II
DESCRIPCION
Repartidor intergaláctico humano ... (puede ocupar varias líneas)
ASCIIART
   (dibujo, tantas líneas como haga falta)
FINASCIIART
ATRIBUTOS
FUERZA;650
VELOCIDAD;700
RESISTENCIA;500
INTELIGENCIA;300
ENERGIA;400
HABILIDAD;450
```

- Las marcas deben estar en mayúsculas y en ese orden. Los atributos pueden estar en cualquier orden,
  separados por `;`. Se toleran líneas vacías entre secciones.
- Se rechaza el archivo si: falta una sección, nombre o descripción; falta `FINASCIIART`; un atributo
  falta, está repetido, no existe o su valor no es entero; o los valores no cumplen las reglas de TI.
- Si dos archivos tienen el mismo nombre de personaje, se usa el primero y se informa el duplicado.
- Al iniciar, el programa informa **cada archivo con error** (nombre y motivo) y muestra las reglas de
  estructura. Esos personajes no se usan; si quedan menos de 2 válidos no se puede jugar.

---

## Archivo de partida compartida (`Guerra.txt`)

Ubicación: `C:\JuegoGuerra\Guerra\Guerra.txt` en el servidor, `\\PC-SERVIDOR\Guerra\Guerra.txt`
desde el cliente. Formato `CLAVE=VALOR`, una por línea. Ejemplo durante una partida:

```
ESTADO=EN_JUEGO
NOMBRE_SERVIDOR=Ana
NOMBRE_CLIENTE=Beto
RESUMEN_SERVIDOR=F2CF805809EEC94F
RESUMEN_CLIENTE=F2CF805809EEC94F
SEMILLA=1775026005
LATIDO_SERVIDOR=57
LATIDO_CLIENTE=55
RONDA_SERVIDOR=3
RONDA_CLIENTE=3
CARTA_SERVIDOR_3=3|7|Nimbo el Zorro Ninja
CARTA_CLIENTE_3=3|8|Robot K-9000
ATRIBUTO_CLIENTE_3=3|velocidad
RONDA=2
SELECTOR=CLIENTE
CARTAS_SERVIDOR=3
CARTAS_CLIENTE=5
PUNTOS_SERVIDOR=0
PUNTOS_CLIENTE=2
ULTIMO_GANADOR=Beto
```

| Clave | Quién la escribe | Cuándo |
|---|---|---|
| `ESTADO` | ambos | `ESPERANDO_RIVAL` (servidor crea la partida) → `CONECTADO` (cliente se une) → `EN_JUEGO` → `FINALIZADA` / `RENDIDO` / `ERROR` |
| `NOMBRE_SERVIDOR`, `NOMBRE_CLIENTE` | cada uno el suyo | al conectarse |
| `RESUMEN_<ROL>` | cada uno el suyo | al conectarse: huella de las cartas cargadas; deben coincidir |
| `SEMILLA` | servidor | al iniciar: ambas PCs mezclan, reparten y sortean con la misma semilla, por eso obtienen el mismo resultado |
| `LATIDO_<ROL>` | cada uno el suyo | cada 2 s: si el contador del rival no cambia en 30 s se lo considera desconectado |
| `RONDA_<ROL>` | cada uno el suyo | ronda en la que está cada jugador |
| `CARTA_<ROL>_<n>` | cada uno el suyo | al empezar la ronda `n`: `n|id|nombre` de la carta jugada |
| `ATRIBUTO_<ROL>_<n>` | el selector | cuando elige el atributo de la ronda `n` |
| `RONDA`, `SELECTOR`, `CARTAS_*`, `PUNTOS_*`, `ULTIMO_GANADOR` | servidor | al terminar cada ronda (estado general de la partida) |
| `GANADOR`, `RINDIO` | ambos | al terminar / al abandonar |

**Consultas:** cada computadora lee el archivo cada 0,3 s mientras espera un dato del rival.

**Turno esperado:** cada dato lleva el número de ronda. Si llega un dato con otra ronda, o el rival
figura más de una ronda adelante, la información no corresponde al turno esperado: se informa y la
partida se cancela con `ESTADO=ERROR`. La carta recibida también se compara con la que corresponde
según el reparto.

**Acceso simultáneo:** para escribir, el archivo se abre en modo exclusivo (`FileShare.None`) y se
lee, modifica y guarda sin soltarlo, así no se pierden datos del otro jugador. Para leer se usa
`FileShare.Read`, por lo que nunca se lee un archivo a medio escribir. Cada PC escribe solo sus
propias claves. Si el archivo está ocupado, se reintenta.

**Archivo no disponible:** la partida queda en pausa mostrando un aviso y se reintenta; si se
recupera el acceso, sigue. Si pasan 2 minutos sin acceso, la partida termina por error. Las
operaciones de escritura informan si fallaron (no se asume que siempre terminan bien).

---

## Pruebas

`dotnet run -- --pruebas` ejecuta 113 pruebas agrupadas por etapa. Cubren los casos del plan de
pruebas de TI:

| Caso de TI | Prueba |
|---|---|
| Personaje con todos sus datos válidos | Etapa 1 y 2: se crea / se lee el archivo |
| Total distinto de 3000 | `total_incorrecto.txt` y validador |
| Atributo fuera de rango | `fuera_de_rango.txt`, límites 250 y 750 |
| Sin especialización | `sin_especializacion.txt`, `todos_500.txt` |
| Archivo incompleto | `incompleto.txt`, `sin_fin_ascii.txt`, `vacio.txt`, `formato_incorrecto.txt` |
| Mazo sin cartas | operaciones de `Mazo` vacío y ronda sin cartas |
| Empate | ronda con empate: cada uno conserva su carta |
| Jugador que se queda sin cartas | pierde la última carta / `determinarGanador` |
| Archivo compartido inaccesible | `ArchivoCompartido` y `Comunicacion` → `ERROR` |
| Información que no corresponde al turno | `Comunicacion` → `DESINCRONIZADO` |

Además se probó una partida completa entre dos procesos (normal, con abandono, con desconexión
y con carpetas de personajes distintas).

---

## Decisiones de implementación a comunicar a los Analistas

Durante la implementación se ajustaron algunos puntos de la especificación de TI:

1. **Archivo de cartas:** en lugar de un único `cartas.txt` se usa la carpeta `Personajes` con un
   archivo por personaje, que es el formato definido en la Actividad 5.
2. **Archivo con errores:** los requerimientos dicen terminar la partida, pero el plan de pruebas dice
   descartar ese personaje y jugar si quedan al menos 2. Se implementó el plan de pruebas (informando
   el archivo y las reglas de estructura).
3. **Mezcla compartida:** para que las dos PCs tengan el mismo reparto sin enviar todo el mazo, el
   servidor publica una semilla y ambas usan `new Random(semilla)`. `Mazo.mezclar` recibe el generador.
   También se corrigió el algoritmo (`Next(0, i + 1)` para Fisher-Yates).
4. **Ronda:** recibe en el constructor los jugadores, la comunicación, si este jugador es el selector y
   los atributos disponibles. `elegirAtributo()` se separó en `prepararCartas()` (intercambio de
   cartas) y `elegirAtributo()`.
5. **Partida:** se agregó el estado `RENDIDO` (este jugador abandona) además de `DESCONECTADO`
   (abandona el rival). `determinarGanador()` devuelve `null` si la partida terminó por error.
6. **Comunicación:** los datos de cada ronda usan claves propias (`CARTA_CLIENTE_3`) para que un jugador
   que avanza más rápido no pise un dato que el otro todavía no leyó. Se agregó el "latido" para
   detectar desconexiones.
7. **Carta.compararAtributo** lanza una excepción si el atributo no existe (antes devolvía 0, lo que
   se confundía con un empate). La ronda solo permite elegir atributos válidos y disponibles.
