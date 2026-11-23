using System.Runtime.InteropServices;
using NEXUS.Forms;
using NEXUS.Services;

namespace NEXUS;

// ============================================================
//  NEXUS v1.0 · Punto de entrada
//  - Normal:        abre la ventana del juego (Windows Forms).
//  - Con "--iris":  abre la CONSOLA de IRIS (segunda ventana).
// ============================================================
internal static class Program
{
    // Un programa de ventanas no tiene consola. Esta función de Windows crea una.
    [DllImport("kernel32.dll")]
    private static extern bool AllocConsole();

    [STAThread]
    private static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--iris")
        {
            AllocConsole();
            Iris.EjecutarConsola();
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new FormPrincipal());
    }
}
