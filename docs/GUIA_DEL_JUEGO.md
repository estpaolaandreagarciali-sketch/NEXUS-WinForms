# NEXUS v1.0 — Sistema de Exploración Interdimensional

**Tipo:** videojuego de exploración, supervivencia y combate
**Tecnología:** C# · .NET 10 · **Windows Forms**
**Año de la historia:** 2297

> El Cadete Explorer NEXUS recibe una misión: investigar una anomalía detectada en una realidad desconocida (R-404). Debe explorar, recolectar recursos, administrar su inventario, enfrentarse a entidades, recuperar el núcleo energético y **regresar con vida**.

---

## 1. Cómo ejecutar el juego

1. Abre `NEXUS.csproj` en **Visual Studio**.
2. Presiona **F5** (o el botón verde ▶ NEXUS).
3. Se abre la ventana del juego, maximizada, con el fondo de estrellas animado.

**Requisito:** SDK de **.NET 10** con soporte para escritorio (Windows Forms).

---

## 2. Cómo se juega, paso a paso

### Paso 1. Menú principal
Verás el logo **N E X U S** brillando sobre un campo de estrellas en movimiento. Haz clic en uno de los botones:

| Botón | Qué hace |
|---|---|
| **NUEVA PARTIDA** | Empieza una misión nueva |
| **CÓMO JUGAR** | Muestra un resumen de los controles |
| **ABRIR CONSOLA IRIS** | Abre la consola de IRIS en otra ventana |
| **SALIR DEL SISTEMA** | Pide confirmación, cierra IRIS y la ventana se desvanece |

### Paso 2. Identificarte como cadete
1. Escribe tu nombre en el cuadro (máximo 14 letras) y pulsa **Enter** o **CONFIRMAR IDENTIDAD**.
2. Si lo dejas vacío, aparece en rojo: *"El nombre no puede estar vacío"*.
3. Deja marcada la casilla **"Abrir la consola de IRIS"** (recomendado).

### Paso 3. Leer la misión
- La transmisión de la central NEXUS se escribe letra por letra, como una máquina de escribir.
- Muestra los 4 objetivos de **"La anomalía perdida"**.
- Pulsa **CONTINUAR ►**. Si lo pulsas mientras el texto se escribe, el texto aparece completo; púlsalo otra vez para seguir.

### Paso 4. Elegir una realidad
Aparecen 3 tarjetas. Pulsa **SALTAR A ESTA REALIDAD ►** en la que quieras; las estrellas se aceleran (salto) y entras al mapa.

| Realidad | Riesgo | Para qué sirve |
|---|---|---|
| R-17 · Ciudad de los Ecos | ■□□ bajo | Conseguir objetos con poco peligro |
| R-18 · Sector de las Sombras | ■■□ medio | Más enemigos y más objetos |
| R-404 · Realidad Desconocida | ■■■ alto | **Aquí está la misión** |

### Paso 5. La pantalla de juego
| Zona | Qué muestra |
|---|---|
| **Arriba** | Nombre del cadete, realidad, XP, turno, inventario y las barras de **Salud, Energía, Estabilidad y Escudo** (se deslizan al cambiar y se ponen ámbar bajo 50 % y rojas bajo 25 %) |
| **Centro** | El **mapa**. Las casillas oscuras aún no se exploran: se revelan al acercarte |
| **Derecha** | Misión (objetivos ■ cumplidos / □ pendientes), Centro de operaciones, cruceta de movimiento y leyenda |
| **Abajo** | **Bitácora** con la hora y un color por tipo de evento |

### Paso 6. Moverse y explorar
| Tecla / botón | Acción |
|---|---|
| `W` `A` `S` `D`, flechas o la cruceta ▲ ◄ ▼ ► | Moverse (cada paso cuesta 1 de energía) |
| `I` · **INVENTARIO** | Abrir el inventario |
| `C` · **CONSULTAR IRIS** | Consejo según tu situación |
| `R` · **CAMBIAR REALIDAD** | Solo funciona parado sobre `E` |
| `F` · **ATACAR** | Fuera de un encuentro avisa que no hay enemigo |
| `H` · **AYUDA** | Resumen de controles |
| `Q` / `Esc` · **ABANDONAR** | Volver al menú (pide confirmación) |

**Leyenda del mapa:**

| Símbolo | Significado |
|---|---|
| ● amarillo (con anillo) | Tú, el cadete |
| `E` verde | Punto de extracción (recupera +10 de estabilidad) |
| ◆ amarillo | Objeto |
| `R` celeste | Robot |
| `X` rojo | Enemigo |
| Anillos morados | Anomalía `A` |
| `N` blanco | Núcleo energético (lo protege el Guardián) |
| `i` azul | Registro de datos (+10 XP) |
| Ondas naranjas | Zona inestable (-4 de estabilidad) |
| Bloque azul | Muro |

Al pisar una casilla nueva puede pasar algo al azar: **objeto encontrado**, **entidad detectada**, **anomalía**, **pérdida de energía**, **sistema dañado** (el mapa destella en rojo), **información** o nada.

### Paso 7. Encuentros con entidades
Cuando aparece un robot o un enemigo, el mapa destella y se abre la ventana de **ENCUENTRO**, con el dibujo de la entidad flotando y su barra de salud. Usa los botones o las teclas:

| Tecla | Botón | Acción |
|---|---|---|
| `1` | **ATACAR** | 15–25 de daño (cuesta 5 de energía; sin energía atacas cuerpo a cuerpo por 8) |
| `2` | **ESCAPAR** | Si lo logras, vuelves a la casilla anterior |
| `3` | **USAR OBJETO** | Abre el inventario; usar un objeto gasta tu turno |
| `4` | **ANALIZAR** | Necesita el *Analizador* y 5 de energía |

Cada golpe hace **temblar** a la entidad. Cada entidad se comporta distinto:
- **Robot:** no ataca si tú no lo atacas. Si lo atacas, activa su **sistema defensivo** (resiste más y lanza descargas). Si lo **analizas antes de atacarlo, lo hackeas**: te regala un objeto y +20 XP.
- **Enemigo:** siempre ataca. Si lo analizas, detectas su **debilidad** y tus ataques hacen +10 de daño. Escapar funciona el 60 % de las veces.
- **Guardián de la Anomalía (jefe):** 110 de salud, golpea fuerte y puede bajar tu estabilidad. Escapar funciona el 40 % de las veces.

### Paso 8. Inventario (agregar → consultar → utilizar → eliminar)
1. Pulsa `I` o **INVENTARIO**.
2. Selecciona un objeto de la tabla.
3. Pulsa **UTILIZAR** (o doble clic) o **ELIMINAR** (pide confirmación).
4. `Esc` o **CERRAR** para volver.

| Objeto | Efecto |
|---|---|
| ♦ Célula de energía | +25 de energía (ejemplo: `Energía: 35 → 60`) |
| ■ Escudo | +30 de escudo |
| + Kit médico | +30 de salud |
| ► Módulo de ataque | El próximo ataque hace +15 |
| Θ Analizador | Se usa con **ANALIZAR** en un encuentro |
| ☼ Núcleo energético | Objetivo de la misión (no se puede eliminar) |
| ? Módulo desconocido | Recompensa final: +20 de salud, energía y estabilidad |

Empiezas con: célula de energía, escudo, analizador y kit médico.

### Paso 9. Completar "La anomalía perdida"
1. Entra a **R-404** (desde el menú de realidades, o pulsando `R` sobre una `E`).
2. **Llega a la anomalía `A`.** Camino más corto desde `E`: `D D S S D D S S D D`.
   Aparece la alerta **⚡ ANOMALÍA DETECTADA** y se revela el núcleo `N` en el mapa.
3. **Ve al núcleo `N`.** Camino desde la anomalía: `D D D S S D D D`.
   Aparece **🚨 ALERTA MÁXIMA** y el **Guardián de la Anomalía**. Consejo: primero `4` Analizar y luego `1` Atacar; cura con el inventario (`3`) si hace falta.
4. **Vence al Guardián** y recibes el **☼ núcleo energético**.
5. **Regresa al punto de extracción `E`** (el mismo camino al revés).
6. Aparece **🟢 MISIÓN COMPLETADA · +100 XP · +1 módulo desconocido**, y al aceptar vuelves al menú.

> Si la **salud** o la **estabilidad** llegan a 0 → **MISIÓN FALLIDA** y vuelves al menú.

Las alertas se cierran con **ACEPTAR**, `Enter`, `Espacio` o `Esc`.

### Paso 10. Salir
En el menú pulsa **SALIR DEL SISTEMA** y confirma. NEXUS envía la señal de cierre a IRIS (su consola se cierra sola), las estrellas se aceleran y la ventana se desvanece.

---

## 3. IRIS: comunicación entre consolas
IRIS es la inteligencia que acompaña al cadete y funciona en una **consola aparte**:

1. El juego abre otra copia de sí mismo con el argumento `--iris`; esa copia crea una consola (`AllocConsole`).
2. El juego **escribe** cada mensaje en el archivo `iris_canal.txt` (una línea: `hora|mensaje`).
3. La consola de IRIS **lee** ese archivo cada 0,3 s y muestra los mensajes nuevos con efecto de máquina de escribir (rojo = alertas, verde = misión, morado = anomalías).
4. Al salir del juego se envía `##FIN##` y la consola de IRIS se cierra sola.

Si IRIS no se puede abrir, sus mensajes siguen apareciendo en la bitácora del juego.

---

## 4. Situaciones controladas (NEXUS no se rompe)

| Situación | Respuesta de NEXUS |
|---|---|
| Pulsar una tecla que no es un control | "Opción inválida. Usa W A S D..." en la bitácora |
| Moverse fuera del mapa | "Límite de la realidad: no puedes salir del mapa" |
| Chocar con un muro | "Un muro de energía bloquea el paso" |
| Usar/eliminar sin seleccionar un objeto | "Selecciona un objeto primero" |
| Usar un objeto inexistente | "No existe ningún objeto en esa ranura" |
| Analizar sin tener analizador | "No posees un analizador" |
| Atacar cuando no hay enemigo | "No hay ninguna entidad a la que atacar" |
| Usar una célula con la energía llena | "La energía ya está al máximo. No se gastó la célula" |
| Inventario lleno | Avisa y deja el objeto en el mapa |
| Eliminar el núcleo | "No puedes abandonar el núcleo" |
| Cambiar de realidad fuera de `E` | "Solo puedes cambiar de realidad desde el punto de extracción" |
| Tocar el núcleo antes de la anomalía | "Un campo de fuerza protege este lugar" |
| Nombre vacío | Mensaje en rojo y lo vuelve a pedir |

---

## 5. Dónde está cada tema de programación

| Tema | Dónde se usa |
|---|---|
| **Variables** | Salud, energía, posición `Fila`/`Columna`, turnos (`Cadete`, `MotorJuego`) |
| **Condiciones** | `if/else` y `switch` en movimiento, eventos, encuentros, inventario y teclado |
| **Ciclos** | `for` del inventario, de las tarjetas de realidad y de los objetivos; `foreach` de pantallas |
| **Ciclos anidados** | Dibujar el mapa (`PanelMapa.OnPaint`), `Realidad.Descubrir()`, `PorcentajeExplorado()`, cargar el plano en la matriz |
| **Arreglos** | Inventario (`Objeto[8]`), objetivos (`string[]` + `bool[]`), realidades, estrellas del fondo (`float[]`), textos |
| **Matrices** | Mapa `char[,]`, niebla de guerra `bool[,] descubierto`, `bool[,] visitado` |
| **Métodos** | Toda la lógica está en métodos (`Mover`, `Atacar`, `UsarObjeto`, …) |
| **Herencia** | `Personaje` → `Cadete`, `Robot`, `Enemigo` (cada uno con su `RecibirDanio` y `Describir`) |
| **Eventos** | `MotorJuego` avisa a la ventana con `Mensaje`, `Alerta`, `Destello`, `SaltoRealidad`, `PartidaTerminada` |

---

## 6. Organización del repositorio

- `src/NEXUS/Program.cs`: punto de entrada del juego y consola IRIS.
- `src/NEXUS/Forms/`: ventana principal, controles, paneles y tema visual.
- `src/NEXUS/Models/`: personajes, objetos, inventario, misión y realidades.
- `src/NEXUS/Services/`: motor de juego y comunicación de IRIS.
- `docs/`: guías y documentación.
- `archive/`: versiones anteriores que se conservan como referencia y no se compilan.

El proyecto activo es `src/NEXUS/NEXUS.csproj`.
---

**NEXUS — Sistema de Exploración**
*El futuro de la humanidad depende de lo que descubras en cada realidad.*
