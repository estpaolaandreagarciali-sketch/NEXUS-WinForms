# Guía de preparación para la evaluación

## Estado comprobado del repositorio

La aplicación actual está organizada en `src/NEXUS/{Forms,Models,Services}` y la guía del juego está en `docs/GUIA_DEL_JUEGO.md`. El método de lógica recomendado para explicar y mejorar es `MotorJuego.Mover(int cambioFila, int cambioColumna)`: valida el estado de la partida y el combate, controla límites y muros, consume energía (o salud), actualiza posición y turnos, descubre el mapa y procesa la casilla.

## Base de datos

1. En SQL Server Management Studio, abrir y ejecutar `database/01_crear_base.sql`.
2. Ejecutar `database/02_verificar.sql` para comprobar el esquema.
3. La tabla `dbo.Partida` reserva un identificador, el nombre del cadete y una columna JSON para el estado serializado.
4. La conexión de desarrollo recomendada usa autenticación integrada de Windows: `Server=localhost;Database=NEXUS;Integrated Security=True;TrustServerCertificate=True`.

El servicio `NEXUS.Services.RepositorioPartidasSql` implementa guardado, carga por GUID y carga de la última partida. `FormPrincipal` conecta el botón GUARDAR y los atajos Ctrl+S/Ctrl+L; `MotorJuego` serializa y restaura estadísticas, inventario, objetivos, posición y estado de mapas. El flujo requiere SQL Server iniciado y la dependencia NuGet restaurada. **No se pudo compilar ni verificar punta a punta en el entorno actual**: NuGet no resolvió `api.nuget.org` y no respondió `localhost:1433`.

## Matriz de evidencias

| Criterio | Evidencia que debe mostrarse |
|---|---|
| Arquitectura | Explicar formularios, modelos, motor, base de datos y documentación en máximo cinco diapositivas. |
| Mejora C# | Mostrar el método antes y después, ejecutar casos válidos e inválidos y enseñar el commit/push de Visual Studio. |
| Clonación limpia | Clonar en una carpeta nueva, restaurar/compilar/ejecutar desde Visual Studio y mostrar que no se usó el proyecto original. |
| Persistencia | Guardar una partida, cerrar, reabrir y cargarla; verificar los datos en SSMS. |
| Release | Crear una versión etiquetada, adjuntar el ejecutable publicado y abrirlo en una máquina sin Visual Studio. |
| Testing & Debugging | En `testing-debugging`, registrar entrada inválida, guardado/recuperación y desconexión SQL; corregir defectos y repetir. Integrar los cambios comprobados en `main`. |

## Registro de pruebas (rellenar durante ejecución)

| Fecha | Caso | Pasos/datos | Resultado esperado | Resultado observado | Evidencia/defecto |
|---|---|---|---|---|---|
| — | Nombre vacío | Confirmar identidad sin nombre | Validación visible; no inicia | Pendiente | — |
| — | Movimiento contra muro | Intentar avanzar a `#` | Posición y recursos sin cambio | Pendiente | — |
| — | Guardar y recuperar | Guardar, cerrar, abrir y cargar | Estado restaurado | No ejecutada: SQL Server no disponible | — |
| — | SQL desconectado | Detener servicio e intentar guardar | Error entendible; juego no se cierra inesperadamente | Pendiente | — |

No marcar una prueba como aprobada hasta ejecutarla y guardar evidencia.
