using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace NEXUS.Forms;

// ============================================================
//  PANEL ESTRELLAS: fondo animado del espacio (viaje entre estrellas).
//  Se usa en el menú, en la identificación y en la selección de realidad.
//
//  Cada estrella tiene una posición X, Y y una profundidad Z guardadas
//  en ARREGLOS. En cada tic la Z baja (se acerca) y la estrella se
//  dibuja más grande y más lejos del centro.
// ============================================================
public sealed class PanelEstrellas : Control
{
    private const int CantidadEstrellas = 220;
    private readonly float[] estrellaX = new float[CantidadEstrellas];
    private readonly float[] estrellaY = new float[CantidadEstrellas];
    private readonly float[] estrellaZ = new float[CantidadEstrellas];

    private readonly System.Windows.Forms.Timer reloj = new System.Windows.Forms.Timer { Interval = 30 };
    private int fase;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool MostrarLogo { get; set; }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public float Velocidad { get; set; } = 0.012f;

    public PanelEstrellas(bool mostrarLogo)
    {
        MostrarLogo = mostrarLogo;
        BackColor = Tema.Fondo;
        DoubleBuffered = true;
        SetStyle(ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);

        for (int i = 0; i < CantidadEstrellas; i++) NuevaEstrella(i, true);

        reloj.Tick += (s, e) => Avanzar();
        reloj.Start();
    }

    // Pone la estrella "i" en una posición al azar.
    private void NuevaEstrella(int i, bool profundidadAlAzar)
    {
        estrellaX[i] = (float)(Random.Shared.NextDouble() * 2 - 1);
        estrellaY[i] = (float)(Random.Shared.NextDouble() * 2 - 1);
        estrellaZ[i] = profundidadAlAzar ? (float)Random.Shared.NextDouble() : 1f;
    }

    private void Avanzar()
    {
        if (!Visible) return;
        fase++;
        for (int i = 0; i < CantidadEstrellas; i++)
        {
            estrellaZ[i] -= Velocidad;
            if (estrellaZ[i] <= 0.02f) NuevaEstrella(i, false);
        }
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(BackColor);

        float centroX = Width / 2f;
        float centroY = Height / 2f;

        for (int i = 0; i < CantidadEstrellas; i++)
        {
            // Proyección: cuanto menor es Z, más lejos del centro y más grande.
            float x = centroX + estrellaX[i] / estrellaZ[i] * centroX * 0.5f;
            float y = centroY + estrellaY[i] / estrellaZ[i] * centroY * 0.5f;
            if (x < 0 || x > Width || y < 0 || y > Height) continue;

            float tam = (1 - estrellaZ[i]) * 3.5f + 0.5f;
            int brillo = (int)Math.Clamp((1 - estrellaZ[i]) * 255, 40, 255);
            Color color = i % 9 == 0 ? Color.FromArgb(brillo, Tema.Acento)
                        : i % 13 == 0 ? Color.FromArgb(brillo, Tema.Magenta)
                        : Color.FromArgb(brillo, 220, 230, 255);

            using SolidBrush pincel = new SolidBrush(color);
            g.FillEllipse(pincel, x, y, tam, tam);
        }

        if (MostrarLogo) DibujarLogo(g);
    }

    // Logo "N E X U S" con brillo que late.
    private void DibujarLogo(Graphics g)
    {
        string titulo = "N E X U S";
        Size tam = TextRenderer.MeasureText(titulo, Tema.FuenteLogo);
        int x = (Width - tam.Width) / 2;
        int y = Height / 6;

        int alfa = (int)(60 + 50 * Math.Sin(fase * 0.08));
        Color brillo = Color.FromArgb(alfa, Tema.Acento);

        // Varias copias desplazadas y transparentes = efecto de resplandor.
        using (SolidBrush resplandor = new SolidBrush(brillo))
        {
            for (int dx = -3; dx <= 3; dx += 3)
                for (int dy = -3; dy <= 3; dy += 3)
                    g.DrawString(titulo, Tema.FuenteLogo, resplandor, x + dx, y + dy);
        }
        using (SolidBrush principal = new SolidBrush(Tema.Acento))
            g.DrawString(titulo, Tema.FuenteLogo, principal, x, y);

        string subtitulo = "SISTEMA DE EXPLORACIÓN INTERDIMENSIONAL  ·  AÑO 2297";
        Size tamSub = TextRenderer.MeasureText(subtitulo, Tema.FuenteSubtitulo);
        TextRenderer.DrawText(g, subtitulo, Tema.FuenteSubtitulo,
                              new Point((Width - tamSub.Width) / 2, y + tam.Height + 4), Tema.TextoGris);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) reloj.Dispose();
        base.Dispose(disposing);
    }
}
