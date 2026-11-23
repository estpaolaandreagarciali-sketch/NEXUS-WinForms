using System.Diagnostics;

namespace NEXUS.Services;

// ============================================================
//  IRIS: la inteligencia que acompaña al cadete.
//
//  COMUNICACIÓN ENTRE CONSOLAS:
//   1. El juego abre una SEGUNDA consola (el mismo programa con "--iris").
//   2. La consola del juego ESCRIBE mensajes en un archivo de texto
//      (iris_canal.txt), una línea por mensaje: "hora|mensaje".
//   3. La consola de IRIS LEE ese archivo cada 0,3 segundos y muestra
//      los mensajes nuevos.
//   4. Al salir del juego se envía "##FIN##" y la consola de IRIS se cierra.
// ============================================================
public static class Iris
{
    private const string MensajeFin = "##FIN##";
    private static readonly string rutaCanal = Path.Combine(AppContext.BaseDirectory, "iris_canal.txt");
    private static Process procesoIris;

    public static bool ConsolaAbierta
    {
        get
        {
            try { return procesoIris != null && !procesoIris.HasExited; }
            catch { return false; }
        }
    }

    // ================= LADO DEL JUEGO (envía) =================

    public static void ReiniciarCanal()
    {
        try { File.WriteAllText(rutaCanal, ""); } catch { }
    }

    public static void Enviar(string mensaje)
    {
        try
        {
            string linea = DateTime.Now.ToString("HH:mm:ss") + "|" + mensaje + Environment.NewLine;
            File.AppendAllText(rutaCanal, linea);
        }
        catch
        {
            // Si el archivo está ocupado, el juego continúa igual.
        }
    }

    public static bool AbrirConsola()
    {
        if (ConsolaAbierta) return true;
        try
        {
            string programa = Environment.ProcessPath;
            string argumentos = "--iris";

            // Si el juego se ejecutó con "dotnet NEXUS.dll", también pasamos la ruta del .dll.
            if (Path.GetFileNameWithoutExtension(programa).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
                argumentos = "\"" + typeof(Iris).Assembly.Location + "\" --iris";

            ProcessStartInfo info = new ProcessStartInfo(programa, argumentos);
            info.UseShellExecute = true; // abre una ventana nueva
            procesoIris = Process.Start(info);
            return procesoIris != null;
        }
        catch
        {
            procesoIris = null;
            return false;
        }
    }

    public static void CerrarConsola()
    {
        Enviar(MensajeFin);
    }

    // ================= LADO DE IRIS (recibe) =================

    public static void EjecutarConsola()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.Title = "IRIS | Canal de comunicación NEXUS";
        Pantalla.OcultarCursor();
        DibujarCabecera();

        // Ignoramos los mensajes antiguos: solo mostramos los nuevos.
        int lineasLeidas = LeerCanal().Length;

        while (true)
        {
            string[] lineas = LeerCanal();

            // Si el juego reinició el canal (nueva partida), empezamos de cero.
            if (lineas.Length < lineasLeidas) lineasLeidas = 0;

            for (int i = lineasLeidas; i < lineas.Length; i++)
            {
                int separador = lineas[i].IndexOf('|');
                if (separador < 0) continue;

                string hora = lineas[i].Substring(0, separador);
                string mensaje = lineas[i].Substring(separador + 1);

                if (mensaje == MensajeFin)
                {
                    Pantalla.EscribirLinea();
                    Pantalla.Maquina("   IRIS > El cadete cerró el sistema. Desconectando canal...", ConsoleColor.DarkMagenta);
                    Thread.Sleep(2000);
                    return;
                }
                MostrarMensaje(hora, mensaje);
            }

            lineasLeidas = lineas.Length;
            Thread.Sleep(300);
        }
    }

    // Lee solo las líneas COMPLETAS (las que ya terminan en salto de línea).
    private static string[] LeerCanal()
    {
        try
        {
            if (!File.Exists(rutaCanal)) return new string[0];

            using FileStream flujo = new FileStream(rutaCanal, FileMode.Open, FileAccess.Read,
                                                    FileShare.ReadWrite | FileShare.Delete);
            using StreamReader lector = new StreamReader(flujo);
            string texto = lector.ReadToEnd();

            string[] partes = texto.Split('\n');
            // La última parte está vacía o es una línea a medio escribir: no la contamos.
            string[] completas = new string[partes.Length - 1];
            for (int i = 0; i < completas.Length; i++)
            {
                completas[i] = partes[i].TrimEnd('\r');
            }
            return completas;
        }
        catch
        {
            return new string[0];
        }
    }

    private static void DibujarCabecera()
    {
        Pantalla.Limpiar();
        Console.WriteLine();
        Pantalla.EscribirLinea("    ██╗██████╗ ██╗███████╗", ConsoleColor.Magenta);
        Pantalla.EscribirLinea("    ██║██╔══██╗██║██╔════╝", ConsoleColor.Magenta);
        Pantalla.EscribirLinea("    ██║██████╔╝██║███████╗", ConsoleColor.DarkMagenta);
        Pantalla.EscribirLinea("    ██║██╔══██╗██║╚════██║", ConsoleColor.DarkMagenta);
        Pantalla.EscribirLinea("    ██║██║  ██║██║███████║", ConsoleColor.DarkMagenta);
        Pantalla.EscribirLinea("    ╚═╝╚═╝  ╚═╝╚═╝╚══════╝", ConsoleColor.DarkMagenta);
        Pantalla.EscribirLinea("    Inteligencia de Respaldo e Información de Sistemas", ConsoleColor.Gray);
        Pantalla.EscribirLinea("    " + new string('─', 60), ConsoleColor.DarkGray);
        Pantalla.EscribirLinea("    Canal abierto. Esperando transmisiones del cadete...", ConsoleColor.DarkGray);
        Console.WriteLine();
    }

    private static void MostrarMensaje(string hora, string mensaje)
    {
        ConsoleColor color = ConsoleColor.Cyan;
        if (mensaje.Contains("ALERTA")) color = ConsoleColor.Red;
        else if (mensaje.Contains("MISIÓN")) color = ConsoleColor.Green;
        else if (mensaje.Contains("ANOMAL")) color = ConsoleColor.Magenta;

        if (mensaje.Contains("ALERTA")) Console.Beep();

        Pantalla.Escribir("   [" + hora + "] ", ConsoleColor.DarkGray);
        Pantalla.Escribir("IRIS > ", ConsoleColor.Magenta);
        Pantalla.Maquina(mensaje, color, 10);
    }
}
