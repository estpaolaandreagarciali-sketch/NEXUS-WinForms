using NEXUS.Models;
using NEXUS.Services;

namespace NEXUS.Forms;

public sealed class MainForm : Form
{
    private readonly GameEngine juego = new();
    private readonly RichTextBox registro = new();
    private readonly Label lblRealidad = new();
    private readonly Label lblXP = new();
    private readonly Label lblCadete = new();
    private readonly ProgressBar barraSalud = new();
    private readonly ProgressBar barraEnergia = new();
    private readonly ProgressBar barraEstabilidad = new();
    private readonly ProgressBar barraEscudo = new();
    private readonly DataGridView listaInventario = new();
    private readonly Label lblMision = new();
    private readonly Button btnAtacar = new();
    private readonly Button btnEscapar = new();
    private bool iniciado;

    private readonly Color fondo = Color.FromArgb(12, 18, 30);
    private readonly Color panel = Color.FromArgb(22, 32, 49);
    private readonly Color acento = Color.FromArgb(0, 210, 220);
    private readonly Color texto = Color.FromArgb(225, 237, 245);

    public MainForm()
    {
        Text = "NEXUS | Sistema de exploración";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1180, 740);
        Size = new Size(1450, 860);
        WindowState = FormWindowState.Maximized;
        BackColor = fondo;
        ForeColor = texto;
        Font = new Font("Segoe UI", 10);
        ConstruirInterfaz();
        juego.Mensaje += AgregarRegistro;
        Refrescar();
        AgregarRegistro("SISTEMA NEXUS INICIALIZADO.\nSelecciona una acción para comenzar la misión.");
    }

    private void ConstruirInterfaz()
    {
        var raiz = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(20),
            BackColor = fondo
        };
        raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64));
        raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
        raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
        raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(raiz);

        // Encabezado adaptable: evita que el subtítulo se corte.
        var encabezado = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = panel,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(20, 10, 20, 10),
            Margin = new Padding(0)
        };
        encabezado.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72));
        encabezado.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));
        encabezado.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var tituloPanel = new Panel { Dock = DockStyle.Fill, BackColor = panel };
        var titulo = new Label
        {
            Text = "N E X U S",
            ForeColor = acento,
            Font = new Font("Segoe UI", 25, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(0, 0)
        };
        var subtitulo = new Label
        {
            Text = "SISTEMA DE EXPLORACIÓN  //  AÑO 2297",
            ForeColor = Color.Silver,
            Font = new Font("Segoe UI", 9),
            AutoSize = true,
            Location = new Point(4, 52)
        };
        tituloPanel.Controls.Add(titulo);
        tituloPanel.Controls.Add(subtitulo);

        var estadoSistema = new Label
        {
            Text = "● SISTEMA EN LÍNEA",
            ForeColor = Color.FromArgb(80, 230, 150),
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight
        };
        encabezado.Controls.Add(tituloPanel, 0, 0);
        encabezado.Controls.Add(estadoSistema, 1, 0);
        raiz.Controls.Add(encabezado, 0, 0);
        raiz.SetColumnSpan(encabezado, 2);

        var izquierda = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(0, 14, 12, 0),
            Margin = new Padding(0)
        };
        izquierda.RowStyles.Add(new RowStyle(SizeType.Absolute, 172));
        izquierda.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        raiz.Controls.Add(izquierda, 0, 1);

        var estado = CrearPanel("ESTADO DEL CADETE");
        estado.Dock = DockStyle.Fill;
        var estadoGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            Padding = new Padding(12, 5, 12, 10),
            BackColor = panel
        };
        estadoGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        estadoGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        estadoGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        estadoGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        estadoGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        lblCadete.Text = "NEXUS-CADETE-01";
        lblCadete.ForeColor = acento;
        lblCadete.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        lblCadete.Dock = DockStyle.Fill;
        lblXP.Dock = DockStyle.Fill;
        lblXP.TextAlign = ContentAlignment.MiddleRight;
        lblXP.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        estadoGrid.Controls.Add(lblCadete, 0, 0);
        estadoGrid.Controls.Add(lblXP, 1, 0);
        AgregarBarra(estadoGrid, "SALUD", barraSalud, 0, 1);
        AgregarBarra(estadoGrid, "ENERGÍA", barraEnergia, 1, 1);
        AgregarBarra(estadoGrid, "ESTABILIDAD", barraEstabilidad, 0, 2);
        AgregarBarra(estadoGrid, "ESCUDO", barraEscudo, 1, 2);
        estado.Controls.Add(estadoGrid);
        izquierda.Controls.Add(estado, 0, 0);

        var bitacoraPanel = CrearPanel("BITÁCORA DE EXPLORACIÓN");
        bitacoraPanel.Dock = DockStyle.Fill;
        registro.Dock = DockStyle.Fill;
        registro.ReadOnly = true;
        registro.BackColor = Color.FromArgb(8, 13, 23);
        registro.ForeColor = Color.FromArgb(205, 225, 235);
        registro.BorderStyle = BorderStyle.None;
        registro.Font = new Font("Consolas", 10.5f);
        registro.Margin = new Padding(2);
        registro.SelectionColor = Color.FromArgb(205, 225, 235);
        bitacoraPanel.Controls.Add(registro);
        izquierda.Controls.Add(bitacoraPanel, 0, 1);

        // Todos los paneles ocupan la columna completa; antes quedaban anclados
        // arriba a la izquierda y dejaban una gran franja vacía a la derecha.
        var derecha = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(0, 14, 0, 0),
            Margin = new Padding(0)
        };
        derecha.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        derecha.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
        derecha.RowStyles.Add(new RowStyle(SizeType.Absolute, 238));
        derecha.RowStyles.Add(new RowStyle(SizeType.Absolute, 190));
        derecha.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        raiz.Controls.Add(derecha, 1, 1);

        var realidadPanel = CrearPanel("UBICACIÓN ACTUAL");
        realidadPanel.Dock = DockStyle.Fill;
        lblRealidad.Dock = DockStyle.Fill;
        lblRealidad.Padding = new Padding(0, 16, 0, 0);
        lblRealidad.TextAlign = ContentAlignment.MiddleCenter;
        lblRealidad.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        lblRealidad.ForeColor = acento;
        realidadPanel.Controls.Add(lblRealidad);
        derecha.Controls.Add(realidadPanel, 0, 0);

        var accionesPanel = CrearPanel("CENTRO DE OPERACIONES");
        accionesPanel.Dock = DockStyle.Fill;
        var acciones = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(7, 5, 7, 7),
            ColumnCount = 2,
            RowCount = 4,
            BackColor = panel,
            Margin = new Padding(0)
        };
        acciones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        acciones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        for (int fila = 0; fila < 4; fila++)
            acciones.RowStyles.Add(new RowStyle(SizeType.Percent, 25));

        AgregarBotonCuadricula(acciones, "Explorar realidad", juego.Explorar, 0, 0, true);
        AgregarBotonCuadricula(acciones, "Cambiar realidad", juego.CambiarRealidad, 1, 0);
        AgregarBotonCuadricula(acciones, "Analizar anomalía", juego.AnalizarAnomalia, 0, 1);
        AgregarBotonCuadricula(acciones, "Analizar enemigo", juego.AnalizarEnemigo, 1, 1);
        AgregarBotonCuadricula(acciones, "Buscar núcleo", juego.BuscarNucleo, 0, 2);
        AgregarBotonCuadricula(acciones, "Ver mapa", juego.MostrarMapa, 1, 2);
        AgregarBotonCuadricula(acciones, "Punto de extracción", juego.Extraer, 0, 3, false, 2);
        accionesPanel.Controls.Add(acciones);
        derecha.Controls.Add(accionesPanel, 0, 1);

        var inventarioPanel = CrearPanel("INVENTARIO");
        inventarioPanel.Dock = DockStyle.Fill;
        var inventarioContenido = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(10),
            BackColor = panel
        };
        inventarioContenido.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        inventarioContenido.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        listaInventario.Dock = DockStyle.Fill;
        listaInventario.AllowUserToAddRows = false;
        listaInventario.AllowUserToDeleteRows = false;
        listaInventario.AllowUserToResizeRows = false;
        listaInventario.AllowUserToResizeColumns = false;
        listaInventario.ReadOnly = true;
        listaInventario.MultiSelect = false;
        listaInventario.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        listaInventario.RowHeadersVisible = false;
        listaInventario.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        listaInventario.BackgroundColor = Color.FromArgb(8, 13, 23);
        listaInventario.GridColor = Color.FromArgb(38, 55, 76);
        listaInventario.BorderStyle = BorderStyle.FixedSingle;
        listaInventario.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        listaInventario.EnableHeadersVisualStyles = false;
        listaInventario.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 48, 68);
        listaInventario.ColumnHeadersDefaultCellStyle.ForeColor = acento;
        listaInventario.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
        listaInventario.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        listaInventario.DefaultCellStyle.BackColor = Color.FromArgb(8, 13, 23);
        listaInventario.DefaultCellStyle.ForeColor = texto;
        listaInventario.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 105, 125);
        listaInventario.DefaultCellStyle.SelectionForeColor = Color.White;
        listaInventario.DefaultCellStyle.Font = new Font("Segoe UI", 9);
        listaInventario.RowTemplate.Height = 30;
        listaInventario.ColumnHeadersHeight = 32;
        listaInventario.Columns.Add("Objeto", "OBJETO");
        listaInventario.Columns.Add("Descripcion", "DESCRIPCIÓN");
        listaInventario.Columns[0].FillWeight = 36;
        listaInventario.Columns[1].FillWeight = 64;
        var btnUsar = CrearBoton("USAR OBJETO", () =>
        {
            if (listaInventario.SelectedRows.Count == 0)
            {
                AgregarRegistro("Selecciona un objeto del inventario.");
                return;
            }
            juego.UsarObjeto(listaInventario.SelectedRows[0].Index);
            Refrescar();
        });
        btnUsar.Dock = DockStyle.Fill;
        inventarioContenido.Controls.Add(listaInventario, 0, 0);
        inventarioContenido.Controls.Add(btnUsar, 0, 1);
        inventarioPanel.Controls.Add(inventarioContenido);
        derecha.Controls.Add(inventarioPanel, 0, 2);

        var misionPanel = CrearPanel("MISIÓN ACTUAL");
        misionPanel.Dock = DockStyle.Fill;
        var misionContenido = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(10),
            BackColor = panel
        };
        misionContenido.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        misionContenido.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        lblMision.Dock = DockStyle.Fill;
        lblMision.ForeColor = texto;
        lblMision.Font = new Font("Segoe UI", 9);
        var combate = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = false,
            BackColor = panel,
            Padding = new Padding(0, 2, 0, 0)
        };
        btnAtacar.Text = "ATACAR";
        btnAtacar.Click += (_, _) => { juego.Atacar(); Refrescar(); };
        EstilizarBoton(btnAtacar, true);
        btnEscapar.Text = "ESCAPAR";
        btnEscapar.Click += (_, _) => { juego.IntentarEscapar(); Refrescar(); };
        EstilizarBoton(btnEscapar, false);
        combate.Controls.Add(btnAtacar);
        combate.Controls.Add(btnEscapar);
        misionContenido.Controls.Add(lblMision, 0, 0);
        misionContenido.Controls.Add(combate, 0, 1);
        misionPanel.Controls.Add(misionContenido);
        derecha.Controls.Add(misionPanel, 0, 3);
    }

    private Panel CrearPanel(string encabezado)
    {
        var p = new Panel
        {
            BackColor = panel,
            Padding = new Padding(12, 42, 12, 12),
            Margin = new Padding(0, 0, 0, 10)
        };

        var titulo = new Label
        {
            Text = encabezado,
            Location = new Point(12, 8),
            Size = new Size(500, 26),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            ForeColor = acento,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true
        };

        p.Controls.Add(titulo);

        return p;
    }

    private void AgregarBarra(TableLayoutPanel grid, string nombre, ProgressBar barra, int col, int fila)
    {
        var cont = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(5, 3, 8, 3),
            BackColor = panel,
            Margin = new Padding(2)
        };
        cont.RowStyles.Add(new RowStyle(SizeType.Absolute, 19));
        cont.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var etiqueta = new Label
        {
            Text = nombre,
            ForeColor = Color.Silver,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 8, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };
        barra.Dock = DockStyle.None;
        barra.Height = 18;
        barra.Width = 260;
        barra.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
        barra.Maximum = 100;
        cont.Controls.Add(etiqueta, 0, 0);
        cont.Controls.Add(barra, 0, 1);
        grid.Controls.Add(cont, col, fila);
    }

    private void AgregarBotonCuadricula(TableLayoutPanel contenedor, string textoBoton,
        Action accion, int columna, int fila, bool destacado = false, int spanColumnas = 1)
    {
        var boton = CrearBoton(textoBoton, () => { accion(); Refrescar(); }, destacado);
        boton.Dock = DockStyle.Fill;
        boton.Margin = new Padding(4);
        boton.MinimumSize = new Size(0, 34);
        boton.AutoSize = false;
        contenedor.Controls.Add(boton, columna, fila);
        if (spanColumnas > 1)
            contenedor.SetColumnSpan(boton, spanColumnas);
    }

    private Button CrearBoton(string textoBoton, Action accion, bool destacado = false)
    {
        var b = new Button { Text = textoBoton, Width = 190, Height = 38, FlatStyle = FlatStyle.Flat, BackColor = destacado ? Color.FromArgb(0, 115, 130) : Color.FromArgb(38, 55, 76), ForeColor = Color.White, Cursor = Cursors.Hand, Margin = new Padding(5) };
        b.FlatAppearance.BorderColor = destacado ? acento : Color.FromArgb(65, 85, 110);
        b.Click += (_, _) => accion();
        return b;
    }

    private void EstilizarBoton(Button b, bool destacado)
    {
        b.Width = 125;
        b.Height = 34;
        b.FlatStyle = FlatStyle.Flat;
        b.BackColor = destacado ? Color.FromArgb(0, 115, 130) : Color.FromArgb(38, 55, 76);
        b.ForeColor = Color.White;
        b.FlatAppearance.BorderColor = destacado ? acento : Color.FromArgb(65, 85, 110);
    }

    private void AgregarRegistro(string mensaje)
    {
        if (InvokeRequired) { BeginInvoke(new Action(() => AgregarRegistro(mensaje))); return; }
        registro.AppendText($"[{DateTime.Now:HH:mm:ss}] {mensaje}{Environment.NewLine}{Environment.NewLine}");
        registro.SelectionStart = registro.TextLength;
        registro.ScrollToCaret();
    }

    private void Refrescar()
    {
        lblRealidad.Text = $"{juego.RealidadActual.Codigo}  |  {juego.RealidadActual.Nombre}";
        lblXP.Text = $"EXPERIENCIA  {juego.Cadete.XP} XP";
        barraSalud.Value = Math.Clamp(juego.Cadete.Salud, 0, 100);
        barraEnergia.Value = Math.Clamp(juego.Cadete.Energia, 0, 100);
        barraEstabilidad.Value = Math.Clamp(juego.Cadete.Estabilidad, 0, 100);
        barraEscudo.Value = Math.Clamp(juego.Cadete.Escudo, 0, 100);

        listaInventario.Rows.Clear();
        foreach (Objeto objeto in juego.Cadete.Inventario.Objetos)
            listaInventario.Rows.Add(objeto.Nombre, objeto.Descripcion);

        var m = juego.Mision;
        lblMision.Text = $"{m.Nombre}\n{Marca(m.AnomaliaEncontrada)} Investigar anomalía   {Marca(m.NucleoEncontrado)} Encontrar núcleo\n{Marca(m.EnemigoSuperado)} Superar al Guardián   {Marca(m.ExtraccionRealizada)} Regresar a extracción\n" +
                         (juego.EnCombate ? $"\nENEMIGO: {juego.EnemigoActual.Nombre} | Salud: {juego.EnemigoActual.Salud}" : juego.SesionActiva ? "\nSin combate activo." : "\nSESIÓN FINALIZADA.");
        btnAtacar.Enabled = juego.EnCombate && juego.SesionActiva;
        btnEscapar.Enabled = juego.EnCombate && juego.SesionActiva;
    }

    private static string Marca(bool ok) => ok ? "✓" : "□";
}
