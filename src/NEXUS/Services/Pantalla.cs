using System.Text;

namespace NEXUS.Services;

// ============================================================
//  PANTALLA: todo lo que dibuja en la consola.
//  Colores, cajas, barras, animaciones y lectura de opciones.
//  Así la lógica del juego (Juego.cs) queda más limpia.
// ============================================================
public static class Pantalla
{
    public const int Ancho = 96;

    // Se puede apagar desde el menú principal para jugar más rápido.
    public static bool AnimacionesActivas = true;

    private static readonly string[] logo =
    {
        "███╗   ██╗███████╗██╗  ██╗██╗   ██╗███████╗",
        "████╗  ██║██╔════╝╚██╗██╔╝██║   ██║██╔════╝",
        "██╔██╗ ██║█████╗   ╚███╔╝ ██║   ██║███████╗",
        "██║╚██╗██║██╔══╝   ██╔██╗ ██║   ██║╚════██║",
        "██║ ╚████║███████╗██╔╝ ██╗╚██████╔╝███████║",
        "╚═╝  ╚═══╝╚══════╝╚═╝  ╚═╝ ╚═════╝ ╚══════╝"
    };

    // ---------------- CONFIGURACIÓN ----------------

    public static void Configurar()
    {
        Console.OutputEncoding = Encoding.UTF8;   // para ver tildes y símbolos █ ░ ═
        Console.Title = "NEXUS v1.0 | Sistema de Exploración";
        try
        {
            if (OperatingSystem.IsWindows())
            {
                int ancho = Math.Min(Ancho + 4, Console.LargestWindowWidth);
                int alto = Math.Min(42, Console.LargestWindowHeight);
                Console.SetWindowSize(ancho, alto);
            }
        }
        catch
        {
            // Algunas terminales no dejan cambiar el tamaño. No pasa nada.
        }
        OcultarCursor();
    }

    public static void Limpiar()
    {
        try { Console.Clear(); } catch { }
    }

    public static void OcultarCursor()
    {
        try { Console.CursorVisible = false; } catch { }
    }

    public static void MostrarCursor()
    {
        try { Console.CursorVisible = true; } catch { }
    }

    public static void Esperar(int milisegundos)
    {
        if (AnimacionesActivas) Thread.Sleep(milisegundos);
    }

    // ---------------- ESCRITURA BÁSICA ----------------

    public static void Escribir(string texto, ConsoleColor color = ConsoleColor.Gray)
    {
        Console.ForegroundColor = color;
        Console.Write(texto);
        Console.ForegroundColor = ConsoleColor.Gray;
    }

    public static void EscribirLinea(string texto = "", ConsoleColor color = ConsoleColor.Gray)
    {
        Escribir(texto, color);
        Console.WriteLine();
    }

    public static void Centrar(string texto, ConsoleColor color = ConsoleColor.Gray)
    {
        int espacios = Math.Max(0, (Ancho - texto.Length) / 2);
        EscribirLinea(new string(' ', espacios) + texto, color);
    }

    public static void Separador(ConsoleColor color = ConsoleColor.DarkGray)
    {
        EscribirLinea(new string('─', Ancho), color);
    }

    // Corta o rellena un texto para que mida exactamente "largo".
    public static string Ajustar(string texto, int largo)
    {
        if (texto.Length > largo) return texto.Substring(0, largo);
        return texto.PadRight(largo);
    }

    // Dibuja una caja con título y varias líneas de texto.
    public static void Caja(string titulo, string[] lineas, ConsoleColor color)
    {
        int interior = Ancho - 2;
        EscribirLinea("╔" + new string('═', interior) + "╗", color);

        string cabecera = " " + titulo + " ";
        int izquierda = Math.Max(0, (interior - cabecera.Length) / 2);
        EscribirLinea("║" + Ajustar(new string(' ', izquierda) + cabecera, interior) + "║", color);
        EscribirLinea("╠" + new string('═', interior) + "╣", color);

        foreach (string linea in lineas)
        {
            Escribir("║ ", color);
            Escribir(Ajustar(linea, interior - 2), ConsoleColor.White);
            EscribirLinea(" ║", color);
        }
        EscribirLinea("╚" + new string('═', interior) + "╝", color);
    }

    // Barra de estado: SALUD  [██████████░░░░░░░░░░]  50/100
    // Cambia a amarillo bajo 50% y a rojo bajo 25%.
    public static void Barra(string nombre, int valor, int maximo, ConsoleColor colorNormal, int largo = 20)
    {
        double porcentaje = maximo > 0 ? (double)valor / maximo : 0;
        int llenos = (int)Math.Round(porcentaje * largo);
        llenos = Math.Clamp(llenos, 0, largo);

        ConsoleColor color = colorNormal;
        if (porcentaje < 0.25) color = ConsoleColor.Red;
        else if (porcentaje < 0.50) color = ConsoleColor.Yellow;

        Escribir(nombre.PadRight(12), ConsoleColor.Gray);
        Escribir("[", ConsoleColor.DarkGray);
        Escribir(new string('█', llenos), color);
        Escribir(new string('░', largo - llenos), ConsoleColor.DarkGray);
        Escribir("]", ConsoleColor.DarkGray);
        Escribir($" {valor,3}/{maximo,-3}", ConsoleColor.White);
    }

    // ---------------- ANIMACIONES ----------------

    // Efecto "máquina de escribir". Si presionas una tecla, termina al instante.
    public static void Maquina(string texto, ConsoleColor color = ConsoleColor.Gray, int retardo = 14)
    {
        Console.ForegroundColor = color;
        bool saltar = !AnimacionesActivas;

        foreach (char letra in texto)
        {
            Console.Write(letra);
            if (!saltar)
            {
                if (Console.KeyAvailable)
                {
                    Console.ReadKey(true);
                    saltar = true;
                }
                else
                {
                    Thread.Sleep(retardo);
                }
            }
        }
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Gray;
    }

    // Logo de NEXUS. Animado: primero "falla la señal" y luego se estabiliza.
    public static void DibujarLogo(bool animado)
    {
        if (animado && AnimacionesActivas)
        {
            char[] ruido = { '▓', '▒', '░', ' ' };
            for (int cuadro = 0; cuadro < 6; cuadro++)
            {
                Limpiar();
                Console.WriteLine();
                foreach (string linea in logo)
                {
                    // Cambiamos algunas letras al azar por "ruido".
                    StringBuilder distorsion = new StringBuilder();
                    foreach (char letra in linea)
                    {
                        if (letra != ' ' && Random.Shared.Next(100) < 45 - cuadro * 7)
                            distorsion.Append(ruido[Random.Shared.Next(ruido.Length)]);
                        else
                            distorsion.Append(letra);
                    }
                    Centrar(distorsion.ToString(), cuadro % 2 == 0 ? ConsoleColor.DarkCyan : ConsoleColor.Magenta);
                }
                Thread.Sleep(90);
            }
            Limpiar();
        }

        Console.WriteLine();
        for (int i = 0; i < logo.Length; i++)
        {
            ConsoleColor color = i < 3 ? ConsoleColor.Cyan : ConsoleColor.DarkCyan;
            Centrar(logo[i], color);
            if (animado) Esperar(60);
        }
        Console.WriteLine();
    }

    // Barra de carga:  Iniciando reactores........ [■■■■■■■■■■] OK
    public static void BarraCarga(string texto, ConsoleColor color)
    {
        Escribir("   " + texto.PadRight(40, '.') + " ", ConsoleColor.Gray);
        Escribir("[", ConsoleColor.DarkGray);
        for (int i = 0; i < 20; i++)
        {
            Escribir("■", color);
            Esperar(20 + Random.Shared.Next(25));
        }
        Escribir("]", ConsoleColor.DarkGray);
        EscribirLinea(" OK", ConsoleColor.Green);
    }

    // Salto entre realidades: estelas de luz que se aceleran.
    public static void Salto(ConsoleColor color)
    {
        if (!AnimacionesActivas) return;

        char[] estelas = { '-', '=', '·', '*', '─' };
        for (int cuadro = 1; cuadro <= 14; cuadro++)
        {
            Limpiar();
            for (int f = 0; f < 22; f++)
            {
                StringBuilder linea = new StringBuilder();
                for (int c = 0; c < Ancho; c++)
                {
                    if (Random.Shared.Next(100) < cuadro * 3)
                        linea.Append(estelas[Random.Shared.Next(estelas.Length)]);
                    else
                        linea.Append(' ');
                }
                EscribirLinea(linea.ToString(), f % 3 == 0 ? ConsoleColor.White : color);
            }
            Thread.Sleep(40);
        }
        Limpiar();
    }

    // Alerta que parpadea 3 veces antes de quedarse fija.
    public static void Alerta(string titulo, string[] lineas, ConsoleColor color)
    {
        if (AnimacionesActivas)
        {
            for (int i = 0; i < 3; i++)
            {
                Limpiar();
                Console.WriteLine("\n\n\n");
                Caja(titulo, lineas, color);
                Thread.Sleep(150);
                Limpiar();
                Thread.Sleep(80);
            }
        }
        Limpiar();
        Console.WriteLine("\n\n\n");
        Caja(titulo, lineas, color);
    }

    // Destello de color en toda la pantalla (cuando recibes o haces daño).
    public static void Destello(ConsoleColor fondo)
    {
        if (!AnimacionesActivas) return;
        Console.BackgroundColor = fondo;
        Limpiar();
        Thread.Sleep(70);
        Console.BackgroundColor = ConsoleColor.Black;
        Limpiar();
    }

    // Pequeña melodía de victoria (solo suena en Windows).
    public static void Fanfarria()
    {
        if (!AnimacionesActivas || !OperatingSystem.IsWindows()) return;
        try
        {
            Console.Beep(523, 120);
            Console.Beep(659, 120);
            Console.Beep(784, 120);
            Console.Beep(1046, 300);
        }
        catch { }
    }

    // ---------------- LECTURA DE DATOS ----------------

    // Borra las teclas que se presionaron durante una animación.
    public static void VaciarTeclado()
    {
        while (Console.KeyAvailable) Console.ReadKey(true);
    }

    public static void Pausa()
    {
        VaciarTeclado();
        EscribirLinea();
        EscribirLinea("   Presiona cualquier tecla para continuar...", ConsoleColor.DarkGray);
        Console.ReadKey(true);
    }

    public static ConsoleKey LeerTecla()
    {
        return Console.ReadKey(true).Key;
    }

    // Pide un número entre minimo y maximo. Si escribes otra cosa,
    // muestra "Opción inválida" y vuelve a preguntar (no se rompe).
    public static int LeerOpcion(int minimo, int maximo)
    {
        VaciarTeclado();
        while (true)
        {
            Escribir("   ► Elige una opción: ", ConsoleColor.Cyan);
            MostrarCursor();
            string entrada = Console.ReadLine();
            OcultarCursor();

            if (entrada == null) return minimo; // la entrada se cerró

            if (int.TryParse(entrada.Trim(), out int opcion) && opcion >= minimo && opcion <= maximo)
                return opcion;

            EscribirLinea($"   × Opción inválida. Escribe un número del {minimo} al {maximo}.", ConsoleColor.Red);
        }
    }

    public static string LeerTexto(string pregunta)
    {
        VaciarTeclado();
        Escribir("   " + pregunta, ConsoleColor.Cyan);
        MostrarCursor();
        string texto = Console.ReadLine();
        OcultarCursor();
        return texto == null ? "" : texto.Trim();
    }

    // Pregunta de Sí o No. Solo acepta S o N.
    public static bool PreguntarSiNo(string pregunta)
    {
        VaciarTeclado();
        Escribir("   " + pregunta + " (S/N): ", ConsoleColor.Yellow);
        while (true)
        {
            ConsoleKey tecla = Console.ReadKey(true).Key;
            if (tecla == ConsoleKey.S) { EscribirLinea("Sí", ConsoleColor.White); return true; }
            if (tecla == ConsoleKey.N) { EscribirLinea("No", ConsoleColor.White); return false; }
        }
    }
}
