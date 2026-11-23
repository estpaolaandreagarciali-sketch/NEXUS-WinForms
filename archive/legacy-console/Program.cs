using NEXUS.Services;

// ============================================================
//  NEXUS v1.0 · Punto de entrada
//  - Normal:          inicia el juego.
//  - Con "--iris":    abre la consola de IRIS (la segunda ventana).
// ============================================================
if (args.Length > 0 && args[0] == "--iris")
{
    Iris.EjecutarConsola();
    return;
}

Juego juego = new Juego();
juego.Iniciar();
