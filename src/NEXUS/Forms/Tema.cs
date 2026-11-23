namespace NEXUS.Forms;

// ============================================================
//  TEMA: colores, fuentes y botones de toda la interfaz.
//  Si quieres cambiar el estilo del juego, cámbialo solo aquí.
// ============================================================
public static class Tema
{
    // ---------------- COLORES ----------------
    public static readonly Color Fondo = Color.FromArgb(8, 12, 22);
    public static readonly Color Panel = Color.FromArgb(18, 26, 42);
    public static readonly Color PanelOscuro = Color.FromArgb(10, 15, 26);
    public static readonly Color Borde = Color.FromArgb(40, 58, 84);
    public static readonly Color Acento = Color.FromArgb(0, 210, 220);
    public static readonly Color Texto = Color.FromArgb(225, 237, 245);
    public static readonly Color TextoGris = Color.FromArgb(130, 148, 168);
    public static readonly Color Verde = Color.FromArgb(80, 230, 150);
    public static readonly Color Rojo = Color.FromArgb(240, 75, 85);
    public static readonly Color Ambar = Color.FromArgb(245, 185, 55);
    public static readonly Color Magenta = Color.FromArgb(205, 105, 255);
    public static readonly Color Azul = Color.FromArgb(80, 150, 255);

    // ---------------- FUENTES ----------------
    public static readonly Font FuenteLogo = new Font("Segoe UI", 54, FontStyle.Bold);
    public static readonly Font FuenteTitulo = new Font("Segoe UI", 20, FontStyle.Bold);
    public static readonly Font FuenteSubtitulo = new Font("Segoe UI", 12, FontStyle.Bold);
    public static readonly Font FuenteNormal = new Font("Segoe UI", 10);
    public static readonly Font FuenteNegrita = new Font("Segoe UI", 10, FontStyle.Bold);
    public static readonly Font FuentePequena = new Font("Segoe UI", 8.5f, FontStyle.Bold);
    public static readonly Font FuenteMono = new Font("Consolas", 10);
    public static readonly Font FuenteMonoGrande = new Font("Consolas", 13, FontStyle.Bold);

    // El motor usa ConsoleColor (no depende de la ventana). Aquí lo traducimos.
    public static Color ColorDe(ConsoleColor color)
    {
        switch (color)
        {
            case ConsoleColor.Green: return Verde;
            case ConsoleColor.DarkGreen: return Color.FromArgb(40, 150, 90);
            case ConsoleColor.Red: return Rojo;
            case ConsoleColor.DarkRed: return Color.FromArgb(170, 30, 40);
            case ConsoleColor.Yellow: return Ambar;
            case ConsoleColor.DarkYellow: return Color.FromArgb(205, 140, 40);
            case ConsoleColor.Cyan: return Acento;
            case ConsoleColor.DarkCyan: return Color.FromArgb(0, 130, 150);
            case ConsoleColor.Magenta: return Magenta;
            case ConsoleColor.DarkMagenta: return Color.FromArgb(130, 60, 170);
            case ConsoleColor.Blue: return Azul;
            case ConsoleColor.DarkBlue: return Color.FromArgb(40, 70, 150);
            case ConsoleColor.DarkGray: return TextoGris;
            case ConsoleColor.Gray: return Color.FromArgb(190, 200, 212);
            default: return Texto;
        }
    }

    // Botón plano con efecto al pasar el mouse, al presionarlo y al desactivarlo.
    public static Button CrearBoton(string texto, bool destacado = false)
    {
        Color normal = destacado ? Color.FromArgb(0, 110, 128) : Color.FromArgb(32, 48, 72);
        Color encima = destacado ? Color.FromArgb(0, 150, 170) : Color.FromArgb(50, 74, 104);
        Color presionado = destacado ? Color.FromArgb(0, 80, 95) : Color.FromArgb(22, 34, 52);
        Color apagado = Color.FromArgb(24, 32, 46);

        Button boton = new Button
        {
            Text = texto,
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false,
            BackColor = normal,
            ForeColor = Color.White,
            Font = FuenteNegrita,
            Cursor = Cursors.Hand,
            Height = 40,
            Margin = new Padding(4),
            TabStop = false
        };
        boton.FlatAppearance.BorderSize = 1;
        boton.FlatAppearance.BorderColor = destacado ? Acento : Borde;
        boton.FlatAppearance.MouseOverBackColor = encima;
        boton.FlatAppearance.MouseDownBackColor = presionado;

        boton.EnabledChanged += (s, e) =>
        {
            boton.BackColor = boton.Enabled ? normal : apagado;
            boton.ForeColor = boton.Enabled ? Color.White : Color.FromArgb(90, 104, 122);
        };
        return boton;
    }

    public static Label CrearEtiqueta(string texto, Font fuente, Color color)
    {
        return new Label
        {
            Text = texto,
            Font = fuente,
            ForeColor = color,
            BackColor = Color.Transparent,
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    // Pone un control en el centro de su contenedor.
    public static void Centrar(Control control)
    {
        if (control.Parent == null) return;
        control.Left = Math.Max(0, (control.Parent.ClientSize.Width - control.Width) / 2);
        control.Top = Math.Max(0, (control.Parent.ClientSize.Height - control.Height) / 2);
    }
}
