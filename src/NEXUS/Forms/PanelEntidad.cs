using NEXUS.Models;

namespace NEXUS.Forms;

// ============================================================
//  PANEL ENTIDAD: dibuja la entidad del encuentro con arte ASCII.
//  - La entidad "flota" arriba y abajo.
//  - Al recibir o hacer daño, tiembla y destella.
// ============================================================
public sealed class PanelEntidad : Control
{
    private string[] arte = new string[0];
    private Color color = Tema.Rojo;
    private int fase;
    private int cuadrosTemblor;
    private Color colorDestello;

    private readonly System.Windows.Forms.Timer reloj = new System.Windows.Forms.Timer { Interval = 40 };
    private readonly Font fuenteArte = new Font("Consolas", 14, FontStyle.Bold);

    public PanelEntidad()
    {
        BackColor = Tema.PanelOscuro;
        DoubleBuffered = true;
        SetStyle(ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
        reloj.Tick += (s, e) =>
        {
            fase++;
            if (cuadrosTemblor > 0) cuadrosTemblor--;
            Invalidate();
        };
        reloj.Start();
    }

    public void MostrarEntidad(Personaje entidad)
    {
        arte = ArteDe(entidad);
        color = ColorDe(entidad);
        Invalidate();
    }

    public void Temblar(Color destello)
    {
        colorDestello = destello;
        cuadrosTemblor = 8;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.Clear(BackColor);

        if (cuadrosTemblor > 0)
        {
            using SolidBrush capa = new SolidBrush(Color.FromArgb(cuadrosTemblor * 16, colorDestello));
            g.FillRectangle(capa, ClientRectangle);
        }

        int altoLinea = fuenteArte.Height;
        int altoTotal = arte.Length * altoLinea;
        int flotar = (int)(Math.Sin(fase * 0.12) * 5);
        int temblor = cuadrosTemblor > 0 ? Random.Shared.Next(-6, 7) : 0;
        int y = (Height - altoTotal) / 2 + flotar;

        for (int i = 0; i < arte.Length; i++)
        {
            Size tam = TextRenderer.MeasureText(arte[i], fuenteArte);
            int x = (Width - tam.Width) / 2 + temblor;
            TextRenderer.DrawText(g, arte[i], fuenteArte, new Point(x, y + i * altoLinea), color);
        }
    }

    public static Color ColorDe(Personaje entidad)
    {
        if (entidad is Robot) return Tema.Acento;
        if (entidad is Enemigo enemigo && enemigo.EsJefe) return Tema.Magenta;
        return Tema.Rojo;
    }

    // Dibujos ASCII de cada tipo de entidad (arreglos de texto).
    public static string[] ArteDe(Personaje entidad)
    {
        if (entidad is Robot)
        {
            return new[]
            {
                "  ┌─────────┐  ",
                "  │  ■   ■  │  ",
                "  │   ───   │  ",
                "  └────┬────┘  ",
                " ╔═════╧═════╗ ",
                "═╣ R O B O T ╠═",
                " ╚═╤═══════╤═╝ ",
                "   ╨       ╨   "
            };
        }

        if (entidad is Enemigo enemigo && enemigo.EsJefe)
        {
            return new[]
            {
                "    ▄▄▄████████▄▄▄    ",
                "  ▄██▀▀  ▄▄▄▄  ▀▀██▄  ",
                " ███   ◄██████►   ███ ",
                " ███  ▀▀▀▀▀▀▀▀▀▀  ███ ",
                "  ▀██▄  ▀▀▀▀▀▀  ▄██▀  ",
                " ▄▀ ▀███▄▄▄▄▄▄███▀ ▀▄ ",
                "▀    ▀▀GUARDIÁN▀▀    ▀"
            };
        }

        return new[]
        {
            "   ▄▀▀▀▀▀▀▀▄   ",
            "  █  ◄   ►  █  ",
            "  █   ▀▀▀   █  ",
            "   ▀▄▄▄▄▄▄▄▀   ",
            "  ▄▀ ▀ ▀ ▀ ▀▄  ",
            " ▀           ▀ "
        };
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            reloj.Dispose();
            fuenteArte.Dispose();
        }
        base.Dispose(disposing);
    }
}
