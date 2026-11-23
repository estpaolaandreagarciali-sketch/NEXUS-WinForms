# NEXUS v1.0 — Sistema de Exploración Interdimensional

**Tipo:** videojuego de exploración, supervivencia y combate en consola
**Lenguaje:** C# (.NET 10) · aplicación de consola
**Año de la historia:** 2297

> El Cadete Explorer NEXUS recibe una misión: investigar una anomalía detectada en una realidad desconocida (R-404). Debe explorar, recolectar recursos, administrar su inventario, enfrentarse a entidades, recuperar el núcleo energético y **regresar con vida**.

---

## 1. Cómo ejecutar el juego

### Opción A: Visual Studio
1. Abre la carpeta del proyecto y haz doble clic en `NEXUS.csproj` (o abre la solución).
2. Presiona **F5** (o el botón verde ▶ NEXUS).
3. Se abre una consola negra con la animación de inicio de NEXUS.

### Opción B: terminal
```bash
cd NEXUS_WinForms
dotnet run
```

**Requisito:** tener instalado el **SDK de .NET 10**.

> **Consejo:** maximiza la ventana de la consola para que el mapa y los paneles se vean completos.

---

## 2. Cómo se juega, paso a paso

### Paso 1. Pantalla de inicio
- Aparece el logo de NEXUS con un efecto de "señal con interferencia" y las barras de carga de los sistemas.
- Presiona **cualquier tecla** para continuar.

### Paso 2. Menú principal
Escribe el número de la opción y presiona **Enter**:

| Opción | Qué hace |
|---|---|
| `1` | Nueva partida |
| `2` | Cómo jugar (resumen de controles) |
| `3` | Abrir la consola de **IRIS** en otra ventana |
| `4` | Activar o desactivar las animaciones |
| `5` | Créditos |
| `0` | Salir del sistema (pide confirmación con S/N) |

Si escribes algo que no es una opción (por ejemplo `9` o `hola`), NEXUS muestra **"Opción inválida"** y vuelve a preguntar.

### Paso 3. Identificarte como cadete
- Escribe tu nombre (máximo 14 letras) y presiona **Enter**.
- Si lo dejas vacío, el sistema te lo vuelve a pedir.

### Paso 4. Narrativa y misión
- Lee la transmisión de la central NEXUS (si presionas una tecla, el texto aparece de golpe).
- Se muestran los 4 objetivos de la misión **"La anomalía perdida"**.
- El juego pregunta si quieres abrir **IRIS** en otra ventana. Responde **S** (recomendado) o **N**.

### Paso 5. Elegir una realidad
| N.º | Realidad | Riesgo | Para qué sirve |
|---|---|---|---|
| 1 | R-17 · Ciudad de los Ecos | ■□□ bajo | Conseguir objetos con poco peligro |
| 2 | R-18 · Sector de las Sombras | ■■□ medio | Más enemigos y más objetos |
| 3 | R-404 · Realidad Desconocida | ■■■ alto | **Aquí está la misión** |

Escribe el número y presiona **Enter**. Verás la animación del **salto entre realidades**.

### Paso 6. Explorar el mapa
La pantalla de exploración muestra:
- **Arriba:** tus barras de Salud, Energía, Estabilidad y Escudo, tu XP y el inventario.
- **Izquierda:** el mapa. Las zonas `░░` todavía no se exploran (se revelan al acercarte).
- **Derecha:** los objetivos de la misión y la leyenda.
- **Abajo:** la bitácora con lo último que pasó.

**Controles (una tecla, sin Enter):**

| Tecla | Acción |
|---|---|
| `W` `A` `S` `D` o flechas | Moverse (cada paso cuesta 1 de energía) |
| `I` | Abrir el inventario |
| `F` | Atacar (fuera de un encuentro avisa que no hay enemigo) |
| `C` | Pedir un consejo a IRIS |
| `R` | Cambiar de realidad (solo parado sobre `E`) |
| `M` | Ver el detalle de la misión |
| `H` | Ayuda |
| `Q` o `Esc` | Abandonar la misión y volver al menú |

**Leyenda del mapa:**

| Símbolo | Significado |
|---|---|
| `@` | Tú, el cadete |
| `E` | Punto de extracción (recupera +10 de estabilidad) |
| `+` | Objeto |
| `R` | Robot |
| `X` | Enemigo |
| `A` | Anomalía |
| `N` | Núcleo energético (protegido por el Guardián) |
| `i` | Registro de datos (+10 XP) |
| `~` | Zona inestable (-4 de estabilidad) |
| `██` | Muro |

Al pisar una zona nueva `·` puede pasar algo al azar: **objeto encontrado**, **entidad detectada**, **anomalía**, **pérdida de energía**, **sistema dañado**, **información** o nada.

### Paso 7. Encuentros con entidades
Cuando aparece un robot o un enemigo, la pantalla parpadea y empieza el encuentro. Escribe el número y presiona **Enter**:

| Opción | Acción |
|---|---|
| `1` | **Atacar**: 15–25 de daño (cuesta 5 de energía; sin energía atacas cuerpo a cuerpo por 8) |
| `2` | **Escapar**: vuelves a la casilla anterior si lo logras |
| `3` | **Usar objeto**: abre el inventario (usar un objeto gasta el turno) |
| `4` | **Analizar**: necesita el *Analizador* y 5 de energía |

**Cada entidad se comporta distinto:**
- **Robot:** no ataca si tú no lo atacas. Si lo atacas, activa su **sistema defensivo** (resiste más y lanza descargas). Si lo **analizas antes de atacarlo, lo hackeas**: te regala un objeto y +20 XP.
- **Enemigo:** siempre ataca. Si lo analizas, detectas su **debilidad** y tus ataques hacen +10 de daño. Escapar funciona el 60 % de las veces.
- **Guardián de la Anomalía (jefe):** 110 de salud, golpea fuerte y puede bajar tu estabilidad. Escapar funciona el 40 % de las veces.

### Paso 8. Inventario (agregar → consultar → utilizar → eliminar)
Presiona `I`. Verás 8 ranuras.
1. Escribe el número del objeto y presiona **Enter**.
2. Elige `1` **Utilizar** o `2` **Eliminar** (pide confirmación S/N).

| Objeto | Efecto |
|---|---|
| ♦ Célula de energía | +25 de energía (ejemplo: `Energía: 35 → 60`) |
| ■ Escudo | +30 de escudo |
| + Kit médico | +30 de salud |
| ► Módulo de ataque | El próximo ataque hace +15 |
| Θ Analizador | Se usa con la opción **Analizar** en un encuentro |
| ☼ Núcleo energético | Objetivo de la misión (no se puede eliminar) |
| ? Módulo desconocido | Recompensa final: +20 de salud, energía y estabilidad |

Empiezas con: célula de energía, escudo, analizador y kit médico.

### Paso 9. Completar la misión "La anomalía perdida"
1. Desde el punto `E` de cualquier realidad pulsa `R` y elige **3 (R-404)**.
2. **Llega a la anomalía `A`.** Camino más corto desde `E`: `D D S S D D S S D D`.
   Se revela en tu mapa la posición del núcleo `N`.
3. **Ve al núcleo `N`.** Camino desde la anomalía: `D D D S S D D D`.
   Aparece el **Guardián de la Anomalía**. Consejo: primero `4` Analizar y luego `1` Atacar.
4. **Vence al Guardián** y el núcleo pasa a tu inventario.
5. **Regresa al punto de extracción `E`** (el mismo camino al revés).
6. Aparece **🟢 MISIÓN COMPLETADA: +100 XP y +1 módulo desconocido.**

> Si la **salud** o la **estabilidad** llegan a 0 → **MISIÓN FALLIDA** y vuelves al menú.

### Paso 10. Salir del juego
En el menú principal elige `0` y confirma con `S`. NEXUS guarda la bitácora, **cierra la consola de IRIS** y se apaga con su animación de despedida.

---

## 3. IRIS: comunicación entre consolas
IRIS es la inteligencia que acompaña al cadete y funciona en una **segunda consola**:

1. El juego abre otra ventana del mismo programa con el argumento `--iris`.
2. La consola del juego **escribe** cada mensaje en el archivo `iris_canal.txt` (una línea: `hora|mensaje`).
3. La consola de IRIS **lee** ese archivo cada 0,3 s y muestra los mensajes nuevos con colores (rojo para alertas, verde para la misión, magenta para anomalías).
4. Al salir del juego se envía `##FIN##` y la consola de IRIS se cierra sola.

Si IRIS no se puede abrir, sus mensajes siguen apareciendo en la bitácora del juego.

---

## 4. Situaciones controladas (NEXUS no se rompe)

| Situación | Respuesta de NEXUS |
|---|---|
| Escribir una opción inválida en un menú | "Opción inválida. Escribe un número del X al Y" y vuelve a preguntar |
| Presionar una tecla que no es un control | "Opción inválida. Usa W A S D..." |
| Moverse fuera del mapa | "Límite de la realidad: no puedes salir del mapa" |
| Chocar con un muro | "Un muro de energía bloquea el paso" |
| Usar una ranura vacía del inventario | "No existe ningún objeto en esa ranura" |
| Analizar sin tener analizador | "No posees un analizador" |
| Atacar cuando no hay enemigo (`F`) | "No hay ninguna entidad a la que atacar" |
| Usar una célula con la energía llena | "La energía ya está al máximo. No se gastó la célula" |
| Inventario lleno | Avisa y deja el objeto en el mapa |
| Eliminar el núcleo | "No puedes abandonar el núcleo" |
| Cambiar de realidad fuera de `E` | "Solo puedes cambiar de realidad desde el punto de extracción" |
| Tocar el núcleo antes de la anomalía | "Un campo de fuerza protege este lugar" |
| Nombre vacío o muy largo | Lo vuelve a pedir |

---

## 5. Dónde está cada tema de programación

| Tema | Dónde se usa |
|---|---|
| **Variables** | Salud, energía, posición `fila`/`columna`, turnos (`Cadete.cs`, `Juego.cs`) |
| **Condiciones** | `if/else` y `switch` en movimiento, eventos, encuentros y menús |
| **Ciclos** | `while` del menú principal, del bucle de exploración y de los combates; `for` del inventario |
| **Ciclos anidados** | Dibujar el mapa, `Realidad.Descubrir()`, `PorcentajeExplorado()`, cargar el plano en la matriz |
| **Arreglos** | Inventario (`Objeto[8]`), objetivos de misión (`string[]` + `bool[]`), realidades, textos de registros |
| **Matrices** | Mapa `char[,]`, niebla de guerra `bool[,] descubierto`, `bool[,] visitado` |
| **Métodos** | Toda la lógica está dividida en métodos (`Mover`, `Encuentro`, `UsarObjeto`, …) |
| **Herencia** | `Personaje` → `Cadete`, `Robot`, `Enemigo` (cada uno con su `RecibirDanio` y `Describir`) |

---

## 6. Estructura del proyecto

```
NEXUS_WinForms/
├── Program.cs              Punto de entrada (juego normal o consola IRIS)
├── NEXUS.csproj
├── Models/
│   ├── Personaje.cs        Personaje, Cadete, Robot, Enemigo, ResultadoEncuentro
│   ├── Objeto.cs           Tipos de objeto y sus efectos
│   ├── Inventario.cs       Arreglo de 8 ranuras
│   ├── Mision.cs           "La anomalía perdida" + MensajeBitacora
│   └── Realidad.cs         Mapa en matriz, niebla de guerra
├── Services/
│   ├── Juego.cs            Lógica del juego (menús, exploración, combate)
│   ├── Pantalla.cs         Colores, cajas, barras y animaciones
│   └── Iris.cs             Comunicación entre consolas
└── Respaldo_WinForms/      Versión anterior con ventanas (no se compila)
```

---

**NEXUS — Sistema de Exploración**
*El futuro de la humanidad depende de lo que descubras en cada realidad.*
