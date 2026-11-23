using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace NEXUS.Forms;

// Barra de estado dibujada por nosotros (como un "ProgressBar" pero con colores y texto).
// Se usa para Salud, Energía, Estabilidad, Escudo y la vida del enemigo.
public sealed class BarraEstado : Control
{
    private int valor;
    private int maximo = 100;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Titulo { get; set; } = "";
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color ColorBarra { get; set; } = Color.FromArgb(0, 210, 220);

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
        set { valor = Math.Clamp(value, 0, maximo); Invalidate(); }
    }

    public BarraEstado()
    {
        // DoubleBuffered evita el parpadeo al redibujar.
        DoubleBuffered = true;
        SetStyle(ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
        Height = 44;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Parent?.BackColor ?? Color.Black);

        // 1) Texto: título a la izquierda y "valor/máximo" a la derecha.
        using var fuenteTitulo = new Font("Segoe UI", 8, FontStyle.Bold);
        using var fuenteValor = new Font("Consolas", 9, FontStyle.Bold);
        string textoValor = $"{valor}/{maximo}";
        var tamValor = g.MeasureString(textoValor, fuenteValor);

        using (var pincelTitulo = new SolidBrush(Color.FromArgb(160, 175, 190)))
            g.DrawString(Titulo, fuenteTitulo, pincelTitulo, 0, 0);
        using (var pincelValor = new SolidBrush(Color.FromArgb(225, 237, 245)))
            g.DrawString(textoValor, fuenteValor, pincelValor, Width - tamValor.Width, 0);

        // 2) Fondo de la barra (la "pista").
        var pista = new Rectangle(0, 22, Width - 1, 14);
        using (var ruta = Redondeado(pista, 7))
        using (var pincelPista = new SolidBrush(Color.FromArgb(8, 13, 23)))
        using (var lapiz = new Pen(Color.FromArgb(45, 62, 84)))
        {
            g.FillPath(pincelPista, ruta);
            g.DrawPath(lapiz, ruta);
        }

        // 3) Parte llena. Si queda poco (menos de 25%) se pone roja; hasta 50% amarilla.
        double porcentaje = (double)valor / maximo;
        int anchoLleno = (int)((pista.Width - 4) * porcentaje);
        if (anchoLleno >= 8)
        {
            Color color = porcentaje < 0.25 ? Color.FromArgb(235, 70, 80)
                        : porcentaje < 0.50 ? Color.FromArgb(240, 180, 50)
                        : ColorBarra;
            var lleno = new Rectangle(pista.X + 2, pista.Y + 2, anchoLleno, pista.Height - 4);
            using var ruta = Redondeado(lleno, 5);
            using var degradado = new LinearGradientBrush(lleno, ControlPaint.Light(color, 0.35f), color, LinearGradientMode.Vertical);
            g.FillPath(degradado, ruta);
        }
    }

    // Crea un rectángulo con esquinas redondeadas.
    private static GraphicsPath Redondeado(Rectangle r, int radio)
    {
        int d = radio * 2;
        var ruta = new GraphicsPath();
        ruta.AddArc(r.X, r.Y, d, d, 180, 90);
        ruta.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        ruta.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        ruta.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        ruta.CloseFigure();
        return ruta;
    }
}
