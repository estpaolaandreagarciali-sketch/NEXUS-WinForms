using System.Drawing.Drawing2D;
using NEXUS.Models;

namespace NEXUS.Forms;

// ============================================================
//  PANEL MAPA: dibuja la MATRIZ de la realidad actual.
//
//  - Recorre el mapa con dos ciclos anidados (filas y columnas).
//  - Las zonas sin descubrir se ven oscuras (niebla de guerra).
//  - Un reloj (Timer) hace "latir" la anomalía, el núcleo y al cadete.
//  - También dibuja el salto entre realidades y los destellos de daño.
// ============================================================
public sealed class PanelMapa : Control
{
    private Realidad realidad;
    private int filaJugador, columnaJugador;

    private int fase;                 // contador que avanza en cada tic (animación)
    private int cuadrosSalto;         // > 0 mientras dura la animación de salto
    private Color colorSalto;
    private int cuadrosDestello;      // > 0 mientras dura el destello de daño
    private Color colorDestello;

    private readonly System.Windows.Forms.Timer reloj = new System.Windows.Forms.Timer { Interval = 50 };

    public PanelMapa()
    {
        BackColor = Tema.PanelOscuro;
        DoubleBuffered = true;
        SetStyle(ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);

        reloj.Tick += (s, e) =>
        {
            fase++;
            if (cuadrosSalto > 0) cuadrosSalto--;
            if (cuadrosDestello > 0) cuadrosDestello--;
            Invalidate();
        };
        reloj.Start();
    }

    public void Mostrar(Realidad nuevaRealidad, int fila, int columna)
    {
        realidad = nuevaRealidad;
        filaJugador = fila;
        columnaJugador = columna;
        Invalidate();
    }

    public void IniciarSalto(Color color)
    {
        colorSalto = color;
        cuadrosSalto = 26;
    }

    public void Destellar(Color color)
    {
        colorDestello = color;
        cuadrosDestello = 7;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(BackColor);
        if (realidad == null) return;

        // Tamaño de cada celda para que el mapa entre completo.
        int tam = Math.Min((Width - 20) / realidad.Columnas, (Height - 20) / realidad.Filas);
        if (tam < 6) return;
        int x0 = (Width - tam * realidad.Columnas) / 2;
        int y0 = (Height - tam * realidad.Filas) / 2;

        using Font fuente = new Font("Consolas", Math.Max(6f, tam * 0.38f), FontStyle.Bold);

        // Ciclos anidados: recorremos toda la matriz.
        for (int f = 0; f < realidad.Filas; f++)
        {
            for (int c = 0; c < realidad.Columnas; c++)
            {
                Rectangle celda = new Rectangle(x0 + c * tam, y0 + f * tam, tam, tam);
                DibujarCelda(g, celda, f, c, fuente);
            }
        }

        Rectangle jugador = new Rectangle(x0 + columnaJugador * tam, y0 + filaJugador * tam, tam, tam);
        DibujarJugador(g, jugador);

        if (cuadrosDestello > 0)
        {
            using SolidBrush capa = new SolidBrush(Color.FromArgb(cuadrosDestello * 18, colorDestello));
            g.FillRectangle(capa, ClientRectangle);
        }
        if (cuadrosSalto > 0) DibujarSalto(g);
    }

    private void DibujarCelda(Graphics g, Rectangle r, int f, int c, Font fuente)
    {
        // Zona sin descubrir: casi negra con un puntito.
        if (!realidad.EstaDescubierto(f, c))
        {
            using SolidBrush niebla = new SolidBrush(Color.FromArgb(13, 17, 28));
            g.FillRectangle(niebla, r);
            using SolidBrush punto = new SolidBrush(Color.FromArgb(28, 34, 48));
            g.FillRectangle(punto, r.X + r.Width / 2 - 1, r.Y + r.Height / 2 - 1, 2, 2);
            return;
        }

        char simbolo = realidad.ObtenerCelda(f, c);

        // Muro
        if (simbolo == '#')
        {
            using SolidBrush muro = new SolidBrush(Color.FromArgb(30, 44, 78));
            using Pen brillo = new Pen(Color.FromArgb(60, 88, 140));
            g.FillRectangle(muro, r);
            g.DrawLine(brillo, r.X, r.Y, r.Right - 1, r.Y);
            g.DrawRectangle(Pens.Black, r);
            return;
        }

        // Suelo
        using (SolidBrush suelo = new SolidBrush(Color.FromArgb(18, 25, 40)))
        using (Pen linea = new Pen(Color.FromArgb(28, 38, 58)))
        {
            g.FillRectangle(suelo, r);
            g.DrawRectangle(linea, r);
        }

        // Pulso entre 0 y 80 que sube y baja con el tiempo.
        int pulso = (int)(40 + 40 * Math.Sin(fase * 0.25));

        switch (simbolo)
        {
            case '.':
                using (SolidBrush puntoSuelo = new SolidBrush(Color.FromArgb(50, 62, 82)))
                    g.FillEllipse(puntoSuelo, r.X + r.Width / 2 - 2, r.Y + r.Height / 2 - 2, 4, 4);
                break;
            case 'E': DibujarIcono(g, r, "E", Tema.Verde, pulso, fuente, true); break;
            case '+': DibujarRombo(g, r, Tema.Ambar); break;
            case 'R': DibujarIcono(g, r, "R", Tema.Acento, 20, fuente, true); break;
            case 'X': DibujarIcono(g, r, "X", Tema.Rojo, pulso / 2, fuente, false); break;
            case 'A': DibujarAnomalia(g, r); break;
            case '*': DibujarIcono(g, r, "*", Color.FromArgb(120, 70, 160), 0, fuente, false); break;
            case 'N': DibujarIcono(g, r, "N", Color.White, pulso, fuente, false); break;
            case 'i': DibujarIcono(g, r, "i", Tema.Azul, 10, fuente, true); break;
            case '~': DibujarOndas(g, r); break;
        }
    }

    // Círculo o cuadrado de color con una letra en el centro.
    private void DibujarIcono(Graphics g, Rectangle r, string letra, Color color, int brillo, Font fuente, bool cuadrado)
    {
        Rectangle interior = Rectangle.Inflate(r, -r.Width / 6, -r.Height / 6);
        using SolidBrush relleno = new SolidBrush(Color.FromArgb(Math.Clamp(50 + brillo, 0, 255), color));
        using Pen borde = new Pen(color, 2);

        if (cuadrado)
        {
            g.FillRectangle(relleno, interior);
            g.DrawRectangle(borde, interior);
        }
        else
        {
            g.FillEllipse(relleno, interior);
            g.DrawEllipse(borde, interior);
        }
        TextRenderer.DrawText(g, letra, fuente, r, Color.White,
                              TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    private void DibujarRombo(Graphics g, Rectangle r, Color color)
    {
        int cx = r.X + r.Width / 2;
        int cy = r.Y + r.Height / 2;
        int t = r.Width / 3;
        Point[] puntos = { new Point(cx, cy - t), new Point(cx + t, cy), new Point(cx, cy + t), new Point(cx - t, cy) };
        using SolidBrush relleno = new SolidBrush(color);
        g.FillPolygon(relleno, puntos);
    }

    // Anillos que crecen desde el centro (la anomalía "respira").
    private void DibujarAnomalia(Graphics g, Rectangle r)
    {
        int cx = r.X + r.Width / 2;
        int cy = r.Y + r.Height / 2;
        for (int i = 0; i < 3; i++)
        {
            int radio = ((fase * 2 + i * 7) % 20) * r.Width / 40 + 2;
            int alfa = Math.Clamp(230 - radio * 10, 30, 230);
            using Pen anillo = new Pen(Color.FromArgb(alfa, Tema.Magenta), 2);
            g.DrawEllipse(anillo, cx - radio, cy - radio, radio * 2, radio * 2);
        }
        using SolidBrush centro = new SolidBrush(Tema.Magenta);
        g.FillEllipse(centro, cx - 4, cy - 4, 8, 8);
    }

    // Líneas onduladas para las zonas inestables.
    private void DibujarOndas(Graphics g, Rectangle r)
    {
        using Pen onda = new Pen(Color.FromArgb(150, Tema.Ambar), 1.5f);
        for (int linea = 1; linea <= 3; linea++)
        {
            int y = r.Y + r.Height * linea / 4;
            PointF[] puntos = new PointF[8];
            for (int i = 0; i < puntos.Length; i++)
            {
                float x = r.X + 2 + i * (r.Width - 4) / 7f;
                float desplazamiento = (float)Math.Sin(fase * 0.3 + i + linea) * 2;
                puntos[i] = new PointF(x, y + desplazamiento);
            }
            g.DrawLines(onda, puntos);
        }
    }

    // El cadete: círculo amarillo con un anillo que se expande.
    private void DibujarJugador(Graphics g, Rectangle r)
    {
        int cx = r.X + r.Width / 2;
        int cy = r.Y + r.Height / 2;
        int radioAnillo = r.Width / 3 + (fase % 12);
        int alfa = Math.Clamp(220 - (fase % 12) * 18, 0, 220);

        using (Pen anillo = new Pen(Color.FromArgb(alfa, Tema.Ambar), 2))
            g.DrawEllipse(anillo, cx - radioAnillo, cy - radioAnillo, radioAnillo * 2, radioAnillo * 2);

        int radio = r.Width / 3;
        using (SolidBrush cuerpo = new SolidBrush(Tema.Ambar))
            g.FillEllipse(cuerpo, cx - radio, cy - radio, radio * 2, radio * 2);
        using (SolidBrush visor = new SolidBrush(Color.FromArgb(20, 30, 50)))
            g.FillEllipse(visor, cx - radio / 2, cy - radio / 3, radio, radio / 2 + 1);
    }

    // Salto entre realidades: estelas de luz que cruzan la pantalla.
    private void DibujarSalto(Graphics g)
    {
        using (SolidBrush capa = new SolidBrush(Color.FromArgb(Math.Min(220, cuadrosSalto * 9), Tema.Fondo)))
            g.FillRectangle(capa, ClientRectangle);

        int largo = (27 - cuadrosSalto) * 12;
        for (int i = 0; i < 70; i++)
        {
            int y = Random.Shared.Next(Height);
            int x = Random.Shared.Next(-largo, Width);
            Color color = i % 3 == 0 ? Color.White : colorSalto;
            using Pen estela = new Pen(Color.FromArgb(Random.Shared.Next(80, 230), color), Random.Shared.Next(1, 3));
            g.DrawLine(estela, x, y, x + largo, y);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) reloj.Dispose();
        base.Dispose(disposing);
    }
}
