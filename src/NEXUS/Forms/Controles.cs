using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace NEXUS.Forms;

// ============================================================
//  PANEL NEXUS: panel oscuro con borde, franja de color y título.
//  Se usa para todos los recuadros de la interfaz.
// ============================================================
public class PanelNexus : Panel
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Titulo { get; set; } = "";

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color ColorAcento { get; set; } = Tema.Acento;

    public PanelNexus(string titulo)
    {
        Titulo = titulo;
        BackColor = Tema.Panel;
        DoubleBuffered = true;
        ResizeRedraw = true;
        Padding = new Padding(12, titulo == "" ? 12 : 40, 12, 12);
        Margin = new Padding(6);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;

        using (Pen borde = new Pen(Tema.Borde))
            g.DrawRectangle(borde, 0, 0, Width - 1, Height - 1);

        // Esquinas de color (estilo "pantalla futurista").
        using (Pen esquina = new Pen(ColorAcento, 2))
        {
            g.DrawLine(esquina, 0, 0, 16, 0);
            g.DrawLine(esquina, 0, 0, 0, 16);
            g.DrawLine(esquina, Width - 17, Height - 1, Width - 1, Height - 1);
            g.DrawLine(esquina, Width - 1, Height - 17, Width - 1, Height - 1);
        }

        if (Titulo != "")
        {
            using (SolidBrush franja = new SolidBrush(ColorAcento))
                g.FillRectangle(franja, 0, 10, 4, 20);

            TextRenderer.DrawText(g, Titulo, Tema.FuenteNegrita, new Point(14, 10), ColorAcento);

            using (Pen linea = new Pen(Tema.Borde))
                g.DrawLine(linea, 12, 34, Width - 12, 34);
        }
    }
}

// ============================================================
//  BARRA DE ESTADO animada (Salud, Energía, Estabilidad, Escudo).
//  Cuando el valor cambia, la barra "se desliza" hasta el nuevo valor.
// ============================================================
public sealed class BarraEstado : Control
{
    private int valor;
    private float valorMostrado;   // el valor que se dibuja (se acerca poco a poco a "valor")
    private int maximo = 100;
    private readonly System.Windows.Forms.Timer animacion = new System.Windows.Forms.Timer { Interval = 15 };

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Titulo { get; set; } = "";

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color ColorBarra { get; set; } = Tema.Acento;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Maximo
    {
        get => maximo;
        set { maximo = Math.Max(1, value); Invalidate(); }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Valor
    {
        get => valor;
        set
        {
            valor = Math.Clamp(value, 0, maximo);
            animacion.Start();
        }
    }

    public BarraEstado(string titulo, Color color)
    {
        Titulo = titulo;
        ColorBarra = color;
        BackColor = Tema.Panel;
        DoubleBuffered = true;
        SetStyle(ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
        Height = 42;
        animacion.Tick += Animar;
    }

    // Saltar la animación (por ejemplo, al empezar una partida nueva).
    public void FijarSinAnimar(int nuevoValor)
    {
        valor = Math.Clamp(nuevoValor, 0, maximo);
        valorMostrado = valor;
        Invalidate();
    }

    private void Animar(object sender, EventArgs e)
    {
        float diferencia = valor - valorMostrado;
        if (Math.Abs(diferencia) < 0.5f)
        {
            valorMostrado = valor;
            animacion.Stop();
        }
        else
        {
            valorMostrado += diferencia * 0.18f;
        }
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(BackColor);

        // Título a la izquierda y "valor/máximo" a la derecha.
        TextRenderer.DrawText(g, Titulo, Tema.FuentePequena, new Point(0, 0), Tema.TextoGris);
        string texto = $"{valor}/{maximo}";
        Size tamTexto = TextRenderer.MeasureText(texto, Tema.FuentePequena);
        TextRenderer.DrawText(g, texto, Tema.FuentePequena, new Point(Width - tamTexto.Width, 0), Tema.Texto);

        // Fondo de la barra.
        Rectangle pista = new Rectangle(0, 21, Width - 1, 14);
        using (GraphicsPath ruta = Redondeado(pista, 7))
        using (SolidBrush fondo = new SolidBrush(Tema.PanelOscuro))
        using (Pen borde = new Pen(Tema.Borde))
        {
            g.FillPath(fondo, ruta);
            g.DrawPath(borde, ruta);
        }

        // Parte llena: roja bajo 25%, ámbar bajo 50%.
        double porcentaje = valorMostrado / maximo;
        int anchoLleno = (int)((pista.Width - 4) * porcentaje);
        if (anchoLleno >= 12)
        {
            Color color = ColorBarra;
            if (porcentaje < 0.25) color = Tema.Rojo;
            else if (porcentaje < 0.50) color = Tema.Ambar;

            Rectangle lleno = new Rectangle(pista.X + 2, pista.Y + 2, anchoLleno, pista.Height - 4);
            using GraphicsPath ruta = Redondeado(lleno, 5);
            using LinearGradientBrush degradado = new LinearGradientBrush(lleno, ControlPaint.Light(color, 0.4f), color,
                                                                          LinearGradientMode.Vertical);
            g.FillPath(degradado, ruta);
        }
    }

    public static GraphicsPath Redondeado(Rectangle r, int radio)
    {
        int d = radio * 2;
        GraphicsPath ruta = new GraphicsPath();
        ruta.AddArc(r.X, r.Y, d, d, 180, 90);
        ruta.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        ruta.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        ruta.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        ruta.CloseFigure();
        return ruta;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) animacion.Dispose();
        base.Dispose(disposing);
    }
}
