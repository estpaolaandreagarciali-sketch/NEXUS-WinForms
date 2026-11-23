using NEXUS.Models;
using NEXUS.Services;

namespace NEXUS.Forms;

// ============================================================
//  PANEL ALERTA: ventana emergente grande con borde que parpadea.
//  Si llegan varias alertas seguidas, se guardan en una COLA y se
//  muestran una por una (cada vez que el jugador pulsa ACEPTAR).
// ============================================================
public sealed class PanelAlerta : PanelNexus
{
    private class AlertaPendiente
    {
        public string Titulo;
        public string[] Lineas;
        public Color Color;
    }

    private readonly Queue<AlertaPendiente> cola = new Queue<AlertaPendiente>();
    private readonly Label lblTitulo;
    private readonly Label lblTexto;
    private readonly Button btnAceptar;
    private readonly System.Windows.Forms.Timer parpadeo = new System.Windows.Forms.Timer { Interval = 110 };
    private int parpadeos;
    private Color colorActual;

    public event Action TodasCerradas;

    public PanelAlerta() : base("")
    {
        Size = new Size(780, 400);
        Visible = false;

        TableLayoutPanel tabla = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = Tema.Panel };
        tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
        tabla.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));

        lblTitulo = Tema.CrearEtiqueta("", Tema.FuenteTitulo, Tema.Acento);
        lblTitulo.TextAlign = ContentAlignment.MiddleCenter;

        lblTexto = Tema.CrearEtiqueta("", new Font("Segoe UI", 11.5f), Tema.Texto);
        lblTexto.TextAlign = ContentAlignment.TopCenter;
        lblTexto.Padding = new Padding(10, 8, 10, 0);

        btnAceptar = Tema.CrearBoton("ACEPTAR  [Enter]", true);
        btnAceptar.Dock = DockStyle.Fill;
        btnAceptar.Margin = new Padding(230, 8, 230, 6);
        btnAceptar.Click += (s, e) => Cerrar();

        tabla.Controls.Add(lblTitulo, 0, 0);
        tabla.Controls.Add(lblTexto, 0, 1);
        tabla.Controls.Add(btnAceptar, 0, 2);
        Controls.Add(tabla);

        parpadeo.Tick += Parpadear;
    }

    public void Encolar(string titulo, string[] lineas, Color color)
    {
        cola.Enqueue(new AlertaPendiente { Titulo = titulo, Lineas = lineas, Color = color });
        if (!Visible) MostrarSiguiente();
    }

    private void MostrarSiguiente()
    {
        AlertaPendiente alerta = cola.Dequeue();
        lblTitulo.Text = alerta.Titulo;
        lblTitulo.ForeColor = alerta.Color;
        lblTexto.Text = string.Join(Environment.NewLine, alerta.Lineas);
        colorActual = alerta.Color;
        ColorAcento = alerta.Color;

        // La altura depende de cuántas líneas tiene el mensaje.
        Height = Math.Min(680, 190 + alerta.Lineas.Length * 27);
        Tema.Centrar(this);
        Visible = true;
        BringToFront();

        parpadeos = 0;
        parpadeo.Start();
    }

    // El borde alterna entre blanco y el color de la alerta.
    private void Parpadear(object sender, EventArgs e)
    {
        parpadeos++;
        ColorAcento = parpadeos % 2 == 0 ? colorActual : Color.White;
        BackColor = parpadeos % 2 == 0 ? Tema.Panel : Color.FromArgb(34, 42, 60);
        if (parpadeos >= 8)
        {
            parpadeo.Stop();
            ColorAcento = colorActual;
            BackColor = Tema.Panel;
        }
        Invalidate();
    }

    public void Cerrar()
    {
        if (!Visible) return;

        if (cola.Count > 0)
        {
            MostrarSiguiente();
            return;
        }
        parpadeo.Stop();
        Visible = false;
        TodasCerradas?.Invoke();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) parpadeo.Dispose();
        base.Dispose(disposing);
    }
}

// ============================================================
//  PANEL ENCUENTRO: pantalla de combate contra un robot o enemigo.
//  Botones: Atacar, Escapar, Usar objeto, Analizar.
// ============================================================
public sealed class PanelEncuentro : PanelNexus
{
    private readonly MotorJuego motor;
    private readonly PanelEntidad dibujo = new PanelEntidad { Dock = DockStyle.Fill };
    private readonly Label lblNombre;
    private readonly Label lblEstado;
    private readonly BarraEstado barraEntidad = new BarraEstado("SALUD DE LA ENTIDAD", Tema.Verde);
    private readonly RichTextBox registro = new RichTextBox();
    private Personaje entidadMostrada;

    public event Action AccionRealizada;
    public event Action PedirInventario;

    public PanelEncuentro(MotorJuego motorJuego) : base("ENCUENTRO")
    {
        motor = motorJuego;
        Visible = false;

        TableLayoutPanel tabla = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, BackColor = Tema.Panel };
        tabla.RowStyles.Add(new RowStyle(SizeType.Percent, 45));   // dibujo
        tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));  // nombre y estado
        tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));  // barra de salud
        tabla.RowStyles.Add(new RowStyle(SizeType.Percent, 55));   // registro del combate
        tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));  // botones

        // Nombre (izquierda) y estado (derecha)
        TableLayoutPanel fila = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, BackColor = Tema.Panel, Margin = new Padding(0) };
        fila.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
        fila.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        lblNombre = Tema.CrearEtiqueta("", Tema.FuenteSubtitulo, Tema.Rojo);
        lblEstado = Tema.CrearEtiqueta("", Tema.FuenteNegrita, Tema.TextoGris);
        lblEstado.TextAlign = ContentAlignment.MiddleRight;
        fila.Controls.Add(lblNombre, 0, 0);
        fila.Controls.Add(lblEstado, 1, 0);

        barraEntidad.Dock = DockStyle.Fill;
        barraEntidad.Margin = new Padding(4, 6, 4, 0);

        registro.Dock = DockStyle.Fill;
        registro.ReadOnly = true;
        registro.BorderStyle = BorderStyle.None;
        registro.BackColor = Tema.PanelOscuro;
        registro.ForeColor = Tema.Texto;
        registro.Font = Tema.FuenteMono;
        registro.TabStop = false;

        // Botones de acción
        TableLayoutPanel botones = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, BackColor = Tema.Panel, Margin = new Padding(0) };
        for (int i = 0; i < 4; i++) botones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

        Button btnAtacar = Tema.CrearBoton("ATACAR  [1]", true);
        Button btnEscapar = Tema.CrearBoton("ESCAPAR  [2]");
        Button btnUsar = Tema.CrearBoton("USAR OBJETO  [3]");
        Button btnAnalizar = Tema.CrearBoton("ANALIZAR  [4]");
        Button[] lista = { btnAtacar, btnEscapar, btnUsar, btnAnalizar };
        for (int i = 0; i < lista.Length; i++)
        {
            lista[i].Dock = DockStyle.Fill;
            botones.Controls.Add(lista[i], i, 0);
        }

        btnAtacar.Click += (s, e) => { motor.Atacar(); AccionRealizada?.Invoke(); };
        btnEscapar.Click += (s, e) => { motor.Escapar(); AccionRealizada?.Invoke(); };
        btnAnalizar.Click += (s, e) => { motor.Analizar(); AccionRealizada?.Invoke(); };
        btnUsar.Click += (s, e) => PedirInventario?.Invoke();

        tabla.Controls.Add(dibujo, 0, 0);
        tabla.Controls.Add(fila, 0, 1);
        tabla.Controls.Add(barraEntidad, 0, 2);
        tabla.Controls.Add(registro, 0, 3);
        tabla.Controls.Add(botones, 0, 4);
        Controls.Add(tabla);
    }

    public void Refrescar()
    {
        Personaje entidad = motor.EntidadActual;
        if (entidad == null) return;

        // ¿Es una entidad nueva? Preparamos el dibujo y la barra desde cero.
        if (entidad != entidadMostrada)
        {
            entidadMostrada = entidad;
            dibujo.MostrarEntidad(entidad);
            barraEntidad.Maximo = entidad.SaludMaxima;
            barraEntidad.FijarSinAnimar(entidad.Salud);
            ColorAcento = PanelEntidad.ColorDe(entidad);
        }
        else
        {
            barraEntidad.Valor = entidad.Salud;
        }

        lblNombre.Text = entidad.Nombre;
        lblNombre.ForeColor = ColorAcento;

        if (entidad is Robot robot)
        {
            Titulo = "ENCUENTRO · ROBOT";
            lblEstado.Text = robot.SistemaDefensivoActivo ? "Sistema defensivo: ACTIVO" : "Sistema defensivo: en espera";
        }
        else if (entidad is Enemigo enemigo)
        {
            Titulo = enemigo.EsJefe ? "ENCUENTRO · JEFE FINAL" : "ENCUENTRO · ENEMIGO";
            lblEstado.Text = enemigo.DebilidadDetectada ? "Debilidad: DETECTADA (+10 daño)" : "Debilidad: desconocida";
        }

        // Últimos 8 mensajes del combate, cada uno con su color.
        registro.Clear();
        int inicio = Math.Max(0, motor.RegistroCombate.Count - 8);
        for (int i = inicio; i < motor.RegistroCombate.Count; i++)
        {
            MensajeBitacora m = motor.RegistroCombate[i];
            registro.SelectionStart = registro.TextLength;
            registro.SelectionColor = Tema.ColorDe(m.Color);
            registro.AppendText("» " + m.Texto + Environment.NewLine);
        }
        Invalidate();
    }

    public void Temblar(Color color)
    {
        dibujo.Temblar(color);
    }
}

// ============================================================
//  PANEL INVENTARIO: consultar → utilizar → eliminar.
// ============================================================
public sealed class PanelInventario : PanelNexus
{
    private readonly MotorJuego motor;
    private readonly DataGridView tabla = new DataGridView();
    private readonly Label lblResultado;

    public event Action AccionRealizada;
    public event Action Cerrado;

    public PanelInventario(MotorJuego motorJuego) : base("INVENTARIO")
    {
        motor = motorJuego;
        Visible = false;
        ColorAcento = Tema.Ambar;

        TableLayoutPanel contenido = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = Tema.Panel };
        contenido.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        contenido.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        contenido.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));

        ConfigurarTabla();

        lblResultado = Tema.CrearEtiqueta("", Tema.FuenteNegrita, Tema.Verde);
        lblResultado.TextAlign = ContentAlignment.MiddleCenter;

        TableLayoutPanel botones = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, BackColor = Tema.Panel, Margin = new Padding(0) };
        for (int i = 0; i < 3; i++) botones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
        Button btnUsar = Tema.CrearBoton("UTILIZAR", true);
        Button btnEliminar = Tema.CrearBoton("ELIMINAR");
        Button btnCerrar = Tema.CrearBoton("CERRAR  [Esc]");
        btnUsar.Dock = DockStyle.Fill;
        btnEliminar.Dock = DockStyle.Fill;
        btnCerrar.Dock = DockStyle.Fill;
        botones.Controls.Add(btnUsar, 0, 0);
        botones.Controls.Add(btnEliminar, 1, 0);
        botones.Controls.Add(btnCerrar, 2, 0);

        btnUsar.Click += (s, e) => Utilizar();
        btnEliminar.Click += (s, e) => Eliminar();
        btnCerrar.Click += (s, e) => Cerrar();
        tabla.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) Utilizar(); };

        contenido.Controls.Add(tabla, 0, 0);
        contenido.Controls.Add(lblResultado, 0, 1);
        contenido.Controls.Add(botones, 0, 2);
        Controls.Add(contenido);
    }

    private void ConfigurarTabla()
    {
        tabla.Dock = DockStyle.Fill;
        tabla.AllowUserToAddRows = false;
        tabla.AllowUserToDeleteRows = false;
        tabla.AllowUserToResizeRows = false;
        tabla.AllowUserToResizeColumns = false;
        tabla.ReadOnly = true;
        tabla.MultiSelect = false;
        tabla.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        tabla.RowHeadersVisible = false;
        tabla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        tabla.BackgroundColor = Tema.PanelOscuro;
        tabla.GridColor = Tema.Borde;
        tabla.BorderStyle = BorderStyle.None;
        tabla.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        tabla.EnableHeadersVisualStyles = false;
        tabla.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        tabla.ColumnHeadersHeight = 34;
        tabla.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        tabla.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 46, 68);
        tabla.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(30, 46, 68);
        tabla.ColumnHeadersDefaultCellStyle.ForeColor = Tema.Ambar;
        tabla.ColumnHeadersDefaultCellStyle.Font = Tema.FuentePequena;
        tabla.DefaultCellStyle.BackColor = Tema.PanelOscuro;
        tabla.DefaultCellStyle.ForeColor = Tema.Texto;
        tabla.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 100, 120);
        tabla.DefaultCellStyle.SelectionForeColor = Color.White;
        tabla.DefaultCellStyle.Font = Tema.FuenteNormal;
        tabla.DefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
        tabla.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(16, 23, 36);
        tabla.RowTemplate.Height = 36;

        tabla.Columns.Add("Ranura", "N.º");
        tabla.Columns.Add("Simbolo", "");
        tabla.Columns.Add("Objeto", "OBJETO");
        tabla.Columns.Add("Descripcion", "DESCRIPCIÓN");
        tabla.Columns[0].FillWeight = 8;
        tabla.Columns[1].FillWeight = 6;
        tabla.Columns[2].FillWeight = 30;
        tabla.Columns[3].FillWeight = 56;
        tabla.Columns[1].DefaultCellStyle.Font = new Font("Segoe UI", 13, FontStyle.Bold);
    }

    public void Mostrar()
    {
        lblResultado.Text = "Selecciona un objeto y pulsa UTILIZAR o ELIMINAR (doble clic también lo usa).";
        lblResultado.ForeColor = Tema.TextoGris;
        Refrescar();
        Visible = true;
        BringToFront();
        tabla.Focus();
    }

    // Recorre el arreglo del inventario y llena la tabla.
    public void Refrescar()
    {
        Inventario inventario = motor.Cadete.Inventario;
        Titulo = $"INVENTARIO  ({inventario.Cantidad}/{Inventario.Capacidad})" + (motor.EnCombate ? "  ·  EN COMBATE: usar un objeto gasta tu turno" : "");

        tabla.Rows.Clear();
        for (int i = 0; i < Inventario.Capacidad; i++)
        {
            Objeto objeto = inventario.Obtener(i);
            if (objeto == null) continue;

            int fila = tabla.Rows.Add(i + 1, objeto.Simbolo, objeto.Nombre, objeto.Descripcion);
            tabla.Rows[fila].Cells[1].Style.ForeColor = Tema.ColorDe(objeto.Color);
            tabla.Rows[fila].Cells[2].Style.ForeColor = Tema.ColorDe(objeto.Color);
        }
        Invalidate();
    }

    private int IndiceSeleccionado()
    {
        if (tabla.SelectedRows.Count == 0) return -1;
        return tabla.SelectedRows[0].Index; // las filas están en el mismo orden que el arreglo
    }

    private void Utilizar()
    {
        int indice = IndiceSeleccionado();
        if (indice < 0)
        {
            MostrarResultado("Selecciona un objeto primero.", Tema.Rojo);
            return;
        }

        bool estabaEnCombate = motor.EnCombate;
        string mensaje = motor.UsarObjeto(indice, out bool usado);
        MostrarResultado(mensaje, usado ? Tema.Verde : Tema.Ambar);
        Refrescar();
        AccionRealizada?.Invoke();

        // En combate, usar un objeto gasta el turno: volvemos a la pelea.
        if (usado && estabaEnCombate) Cerrar();
    }

    private void Eliminar()
    {
        int indice = IndiceSeleccionado();
        if (indice < 0)
        {
            MostrarResultado("Selecciona un objeto primero.", Tema.Rojo);
            return;
        }

        Objeto objeto = motor.Cadete.Inventario.Obtener(indice);
        DialogResult respuesta = MessageBox.Show($"¿Seguro que quieres eliminar {objeto.Nombre}?", "NEXUS · Inventario",
                                                 MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (respuesta != DialogResult.Yes) return;

        MostrarResultado(motor.EliminarObjeto(indice), Tema.Ambar);
        Refrescar();
        AccionRealizada?.Invoke();
    }

    private void MostrarResultado(string texto, Color color)
    {
        lblResultado.Text = texto;
        lblResultado.ForeColor = color;
    }

    public void Cerrar()
    {
        Visible = false;
        Cerrado?.Invoke();
    }
}
