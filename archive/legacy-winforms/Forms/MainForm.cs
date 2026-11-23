using NEXUS.Models;
using NEXUS.Services;

namespace NEXUS.Forms;

public sealed class MainForm : Form
{
    private readonly GameEngine juego = new();
    private readonly RichTextBox registro = new();
    private readonly Label lblRealidad = new();
    private readonly Label lblEstadoRealidad = new();
    private readonly Label lblXP = new();
    private readonly Label lblCadete = new();

    // Antes eran ProgressBar. Ahora usamos BarraEstado (archivo BarraEstado.cs).
    private readonly BarraEstado barraSalud = new() { Titulo = "SALUD", ColorBarra = Color.FromArgb(80, 230, 150) };
    private readonly BarraEstado barraEnergia = new() { Titulo = "ENERGÍA", ColorBarra = Color.FromArgb(0, 210, 220) };
    private readonly BarraEstado barraEstabilidad = new() { Titulo = "ESTABILIDAD", ColorBarra = Color.FromArgb(170, 130, 255) };
    private readonly BarraEstado barraEscudo = new() { Titulo = "ESCUDO", ColorBarra = Color.FromArgb(70, 140, 255) };
    private readonly BarraEstado barraEnemigo = new() { ColorBarra = Color.FromArgb(235, 70, 80), Maximo = 80, Visible = false };

    private readonly DataGridView listaInventario = new();
    private readonly Label lblMision = new();
    private readonly Button btnAtacar = new();
    private readonly Button btnEscapar = new();

    // Colores del juego (todos juntos para cambiarlos fácil).
    private readonly Color fondo = Color.FromArgb(12, 18, 30);
    private readonly Color panel = Color.FromArgb(22, 32, 49);
    private readonly Color acento = Color.FromArgb(0, 210, 220);
    private readonly Color texto = Color.FromArgb(225, 237, 245);
    private readonly Color verde = Color.FromArgb(80, 230, 150);
    private readonly Color rojo = Color.FromArgb(235, 70, 80);
    private readonly Color ambar = Color.FromArgb(240, 180, 50);

    // Fuentes de la bitácora (se crean una sola vez).
    private readonly Font fuenteLog = new("Consolas", 10.5f);
    private readonly Font fuenteLogNegrita = new("Consolas", 10.5f, FontStyle.Bold);

    public MainForm()
    {
        Text = "NEXUS | Sistema de exploración";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1180, 900);
        Size = new Size(1450, 900);
        WindowState = FormWindowState.Maximized;
        BackColor = fondo;
        ForeColor = texto;
        Font = new Font("Segoe UI", 10);
        DoubleBuffered = true; // evita parpadeos
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
        raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
        raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
        raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));
        raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(raiz);

        // ---------- ENCABEZADO ----------
        var encabezado = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = panel,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(20, 8, 20, 8),
            Margin = new Padding(0)
        };
        encabezado.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72));
        encabezado.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));
        encabezado.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // Línea de color debajo del encabezado.
        encabezado.Paint += (s, e) =>
        {
            using var pincel = new SolidBrush(acento);
            e.Graphics.FillRectangle(pincel, 0, encabezado.Height - 3, encabezado.Width, 3);
        };
        encabezado.Resize += (_, _) => encabezado.Invalidate();

        var tituloPanel = new Panel { Dock = DockStyle.Fill, BackColor = panel };
        var titulo = new Label
        {
            Text = "N E X U S",
            ForeColor = acento,
            Font = new Font("Segoe UI", 24, FontStyle.Bold),
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
            ForeColor = verde,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight
        };
        encabezado.Controls.Add(tituloPanel, 0, 0);
        encabezado.Controls.Add(estadoSistema, 1, 0);
        raiz.Controls.Add(encabezado, 0, 0);
        raiz.SetColumnSpan(encabezado, 2);

        // ---------- COLUMNA IZQUIERDA ----------
        var izquierda = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(0, 14, 12, 0),
            Margin = new Padding(0)
        };
        izquierda.RowStyles.Add(new RowStyle(SizeType.Absolute, 190));
        izquierda.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        raiz.Controls.Add(izquierda, 0, 1);

        // Panel ESTADO DEL CADETE
        var estado = CrearPanel("ESTADO DEL CADETE");
        estado.Dock = DockStyle.Fill;
        var estadoGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            Padding = new Padding(4, 2, 4, 2),
            BackColor = panel
        };
        estadoGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        estadoGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        estadoGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        estadoGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        estadoGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        lblCadete.Text = "NEXUS-CADETE-01";
        lblCadete.ForeColor = texto;
        lblCadete.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        lblCadete.Dock = DockStyle.Fill;
        lblCadete.TextAlign = ContentAlignment.MiddleLeft;
        lblXP.Dock = DockStyle.Fill;
        lblXP.ForeColor = ambar;
        lblXP.TextAlign = ContentAlignment.MiddleRight;
        lblXP.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        estadoGrid.Controls.Add(lblCadete, 0, 0);
        estadoGrid.Controls.Add(lblXP, 1, 0);
        AgregarBarra(estadoGrid, barraSalud, 0, 1);
        AgregarBarra(estadoGrid, barraEnergia, 1, 1);
        AgregarBarra(estadoGrid, barraEstabilidad, 0, 2);
        AgregarBarra(estadoGrid, barraEscudo, 1, 2);
        estado.Controls.Add(estadoGrid);
        izquierda.Controls.Add(estado, 0, 0);

        // Panel BITÁCORA
        var bitacoraPanel = CrearPanel("BITÁCORA DE EXPLORACIÓN");
        bitacoraPanel.Dock = DockStyle.Fill;
        registro.Dock = DockStyle.Fill;
        registro.ReadOnly = true;
        registro.BackColor = Color.FromArgb(8, 13, 23);
        registro.ForeColor = texto;
        registro.BorderStyle = BorderStyle.None;
        registro.Font = fuenteLog;
        registro.Margin = new Padding(2);
        registro.TabStop = false;
        registro.Cursor = Cursors.Default;
        bitacoraPanel.Controls.Add(registro);
        izquierda.Controls.Add(bitacoraPanel, 0, 1);

        // ---------- COLUMNA DERECHA ----------
        var derecha = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(0, 14, 0, 0),
            Margin = new Padding(0)
        };
        derecha.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        derecha.RowStyles.Add(new RowStyle(SizeType.Absolute, 128));
        derecha.RowStyles.Add(new RowStyle(SizeType.Absolute, 210));
        derecha.RowStyles.Add(new RowStyle(SizeType.Absolute, 250));
        derecha.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        raiz.Controls.Add(derecha, 1, 1);

        // Panel UBICACIÓN ACTUAL (ahora con 2 líneas: nombre y estado de la realidad)
        var realidadPanel = CrearPanel("UBICACIÓN ACTUAL");
        realidadPanel.Dock = DockStyle.Fill;
        var realidadContenido = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = panel
        };
        realidadContenido.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
        realidadContenido.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
        lblRealidad.Dock = DockStyle.Fill;
        lblRealidad.TextAlign = ContentAlignment.MiddleCenter;
        lblRealidad.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        lblRealidad.ForeColor = acento;
        lblEstadoRealidad.Dock = DockStyle.Fill;
        lblEstadoRealidad.TextAlign = ContentAlignment.TopCenter;
        lblEstadoRealidad.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
        realidadContenido.Controls.Add(lblRealidad, 0, 0);
        realidadContenido.Controls.Add(lblEstadoRealidad, 0, 1);
        realidadPanel.Controls.Add(realidadContenido);
        derecha.Controls.Add(realidadPanel, 0, 0);

        // Panel CENTRO DE OPERACIONES
        var accionesPanel = CrearPanel("CENTRO DE OPERACIONES");
        accionesPanel.Dock = DockStyle.Fill;
        var acciones = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0),
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
        AgregarBotonCuadricula(acciones, "Punto de extracción", juego.Extraer, 0, 3, true, 2);
        accionesPanel.Controls.Add(acciones);
        derecha.Controls.Add(accionesPanel, 0, 1);

        // Panel INVENTARIO
        var inventarioPanel = CrearPanel("INVENTARIO");
        inventarioPanel.Dock = DockStyle.Fill;
        var inventarioContenido = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(0),
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
        listaInventario.BorderStyle = BorderStyle.None;
        listaInventario.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        listaInventario.EnableHeadersVisualStyles = false;
        listaInventario.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        listaInventario.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 48, 68);
        listaInventario.ColumnHeadersDefaultCellStyle.ForeColor = acento;
        listaInventario.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(30, 48, 68);
        listaInventario.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
        listaInventario.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        listaInventario.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
        listaInventario.DefaultCellStyle.BackColor = Color.FromArgb(8, 13, 23);
        listaInventario.DefaultCellStyle.ForeColor = texto;
        listaInventario.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 105, 125);
        listaInventario.DefaultCellStyle.SelectionForeColor = Color.White;
        listaInventario.DefaultCellStyle.Font = new Font("Segoe UI", 9);
        listaInventario.DefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
        // Filas alternadas: una más clara que otra, se lee mejor.
        listaInventario.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(14, 22, 36);
        listaInventario.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 105, 125);
        listaInventario.RowTemplate.Height = 28;
        listaInventario.ColumnHeadersHeight = 30;
        listaInventario.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
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
        }, true);
        btnUsar.Dock = DockStyle.Fill;
        btnUsar.Margin = new Padding(0, 6, 0, 0);
        inventarioContenido.Controls.Add(listaInventario, 0, 0);
        inventarioContenido.Controls.Add(btnUsar, 0, 1);
        inventarioPanel.Controls.Add(inventarioContenido);
        derecha.Controls.Add(inventarioPanel, 0, 2);

        // Panel MISIÓN ACTUAL
        var misionPanel = CrearPanel("MISIÓN ACTUAL");
        misionPanel.Dock = DockStyle.Fill;
        var misionContenido = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(0),
            BackColor = panel
        };
        misionContenido.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        misionContenido.RowStyles.Add(new RowStyle(SizeType.Absolute, 44)); // barra del enemigo
        misionContenido.RowStyles.Add(new RowStyle(SizeType.Absolute, 54)); // botones
        lblMision.Dock = DockStyle.Fill;
        lblMision.ForeColor = texto;
        lblMision.Font = new Font("Segoe UI", 9.5f);
        barraEnemigo.Dock = DockStyle.Fill;
        barraEnemigo.Margin = new Padding(0, 2, 0, 2);
        var combate = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = false,
            BackColor = panel,
            Padding = new Padding(0, 6, 0, 0)
        };
        btnAtacar.Text = "ATACAR";
        btnAtacar.Click += (_, _) => { juego.Atacar(); Refrescar(); };
        EstilizarBoton(btnAtacar, true);
        btnEscapar.Text = "ESCAPAR";
        btnEscapar.Click += (_, _) => { juego.IntentarEscapar(); Refrescar(); };
        EstilizarBoton(btnEscapar, false);
        btnAtacar.Margin = new Padding(0, 0, 8, 0);
        combate.Controls.Add(btnAtacar);
        combate.Controls.Add(btnEscapar);
        misionContenido.Controls.Add(lblMision, 0, 0);
        misionContenido.Controls.Add(barraEnemigo, 0, 1);
        misionContenido.Controls.Add(combate, 0, 2);
        misionPanel.Controls.Add(misionContenido);
        derecha.Controls.Add(misionPanel, 0, 3);
    }

    // Panel con borde, franja de color y línea debajo del título.
    private Panel CrearPanel(string encabezado)
    {
        var p = new PanelNexus
        {
            BackColor = panel,
            Padding = new Padding(12, 44, 12, 12),
            Margin = new Padding(0, 0, 0, 10)
        };

        var titulo = new Label
        {
            Text = encabezado,
            Location = new Point(14, 8),
            Size = new Size(500, 26),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            ForeColor = acento,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true
        };
        p.Controls.Add(titulo);

        p.Paint += (s, e) =>
        {
            using var borde = new Pen(Color.FromArgb(40, 58, 80));
            e.Graphics.DrawRectangle(borde, 0, 0, p.Width - 1, p.Height - 1);
            using var franja = new SolidBrush(acento);
            e.Graphics.FillRectangle(franja, 0, 8, 4, 26);          // franja de color a la izquierda
            e.Graphics.DrawLine(borde, 12, 38, p.Width - 12, 38);   // línea bajo el título
        };
        return p;
    }

    private void AgregarBarra(TableLayoutPanel grid, BarraEstado barra, int col, int fila)
    {
        barra.Dock = DockStyle.Fill;
        barra.Margin = new Padding(6, 2, 10, 2);
        grid.Controls.Add(barra, col, fila);
    }

    private void AgregarBotonCuadricula(TableLayoutPanel contenedor, string textoBoton,
        Action accion, int columna, int fila, bool destacado = false, int spanColumnas = 1)
    {
        var boton = CrearBoton(textoBoton, () => { accion(); Refrescar(); }, destacado);
        boton.Dock = DockStyle.Fill;
        boton.Margin = new Padding(4);
        boton.MinimumSize = new Size(0, 32);
        boton.AutoSize = false;
        contenedor.Controls.Add(boton, columna, fila);
        if (spanColumnas > 1)
            contenedor.SetColumnSpan(boton, spanColumnas);
    }

    private Button CrearBoton(string textoBoton, Action accion, bool destacado = false)
    {
        var b = new Button { Text = textoBoton.ToUpper(), Width = 190, Height = 38 };
        EstilizarBoton(b, destacado);
        b.Click += (_, _) => accion();
        return b;
    }

    // Estilo común de todos los botones: plano, con efecto al pasar el mouse y al estar desactivado.
    private void EstilizarBoton(Button b, bool destacado)
    {
        Color colorNormal = destacado ? Color.FromArgb(0, 115, 130) : Color.FromArgb(38, 55, 76);
        Color colorHover = destacado ? Color.FromArgb(0, 150, 168) : Color.FromArgb(56, 78, 106);
        Color colorPresionado = destacado ? Color.FromArgb(0, 85, 98) : Color.FromArgb(26, 40, 58);
        Color colorDesactivado = Color.FromArgb(26, 36, 52);

        b.Width = Math.Max(b.Width, 140);
        b.Height = 36;
        b.FlatStyle = FlatStyle.Flat;
        b.UseVisualStyleBackColor = false;
        b.BackColor = colorNormal;
        b.ForeColor = Color.White;
        b.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        b.Cursor = Cursors.Hand;
        b.FlatAppearance.BorderSize = 1;
        b.FlatAppearance.BorderColor = destacado ? acento : Color.FromArgb(65, 85, 110);
        b.FlatAppearance.MouseOverBackColor = colorHover;
        b.FlatAppearance.MouseDownBackColor = colorPresionado;

        // Cuando el botón se desactiva (por ejemplo ATACAR sin combate) se ve apagado.
        b.EnabledChanged += (_, _) =>
        {
            b.BackColor = b.Enabled ? colorNormal : colorDesactivado;
            b.ForeColor = b.Enabled ? Color.White : Color.FromArgb(100, 115, 135);
            b.Cursor = b.Enabled ? Cursors.Hand : Cursors.Default;
        };
    }

    // Escribe un mensaje en la bitácora con colores según su contenido.
    private void AgregarRegistro(string mensaje)
    {
        if (InvokeRequired) { BeginInvoke(new Action(() => AgregarRegistro(mensaje))); return; }

        Color color = ElegirColor(mensaje);

        // Separamos la primera línea (título del mensaje) del resto.
        string titulo = mensaje;
        string resto = "";
        int salto = mensaje.IndexOf('\n');
        if (salto >= 0)
        {
            titulo = mensaje.Substring(0, salto).TrimEnd('\r');
            resto = mensaje.Substring(salto + 1);
        }

        EscribirTexto($"[{DateTime.Now:HH:mm:ss}] ", Color.FromArgb(100, 120, 145), fuenteLog);
        EscribirTexto(titulo + Environment.NewLine, color, fuenteLogNegrita);
        if (resto != "")
            EscribirTexto(resto + Environment.NewLine, Color.FromArgb(205, 225, 235), fuenteLog);
        EscribirTexto(Environment.NewLine, texto, fuenteLog);

        registro.SelectionStart = registro.TextLength;
        registro.ScrollToCaret();
    }

    private void EscribirTexto(string t, Color color, Font fuente)
    {
        registro.SelectionStart = registro.TextLength;
        registro.SelectionLength = 0;
        registro.SelectionColor = color;
        registro.SelectionFont = fuente;
        registro.AppendText(t);
    }

    // Verde = bien, rojo = peligro, ámbar = objetos, cian = información.
    private Color ElegirColor(string mensaje)
    {
        string m = mensaje.ToUpper();
        if (m.Contains("MISIÓN COMPLETADA") || m.Contains("DERROTADO") || m.Contains("ESCAPE EXITOSO") || m.Contains("NÚCLEO ENERGÉTICO ENCONTRADO"))
            return verde;
        if (m.Contains("FALLIDA") || m.Contains("ALERTA") || m.Contains("HOSTIL") || m.Contains("PÉRDIDA") || m.Contains("NO PUDISTE") || m.Contains("INSUFICIENTE"))
            return rojo;
        if (m.Contains("OBJETO") || m.Contains("UTILIZADA") || m.Contains("ACTIVADO") || m.Contains("ACTIVADA") || m.Contains("ATAQUE"))
            return ambar;
        if (m.Contains("ANOMALÍA") || m.Contains("ANÁLISIS") || m.Contains("ANALIZANDO") || m.Contains("MAPA") || m.Contains("ENTIDAD"))
            return acento;
        return texto;
    }

    private void Refrescar()
    {
        // Ubicación
        var r = juego.RealidadActual;
        lblRealidad.Text = $"{r.Codigo}  |  {r.Nombre}";
        lblEstadoRealidad.Text = $"ESTADO: {r.Estado}   ·   RIESGO: {r.NivelRiesgo}   ·   ESTABILIDAD: {r.Estabilidad}%";
        lblEstadoRealidad.ForeColor = r.Estado == "ESTABLE" ? verde : r.Estado == "INESTABLE" ? ambar : rojo;

        // Cadete
        lblXP.Text = $"EXPERIENCIA  {juego.Cadete.XP} XP";
        barraSalud.Valor = juego.Cadete.Salud;
        barraEnergia.Valor = juego.Cadete.Energia;
        barraEstabilidad.Valor = juego.Cadete.Estabilidad;
        barraEscudo.Valor = juego.Cadete.Escudo;

        // Inventario: guardamos la fila seleccionada para no perderla al refrescar.
        int seleccionada = listaInventario.SelectedRows.Count > 0 ? listaInventario.SelectedRows[0].Index : -1;
        listaInventario.Rows.Clear();
        foreach (Objeto objeto in juego.Cadete.Inventario.Objetos)
            listaInventario.Rows.Add(objeto.Nombre, objeto.Descripcion);
        if (seleccionada >= 0 && seleccionada < listaInventario.Rows.Count)
            listaInventario.Rows[seleccionada].Selected = true;
        else
            listaInventario.ClearSelection();

        // Misión
        var m = juego.Mision;
        lblMision.Text = $"{m.Nombre.ToUpper()}\n\n{Marca(m.AnomaliaEncontrada)} Investigar anomalía      {Marca(m.NucleoEncontrado)} Encontrar núcleo\n{Marca(m.EnemigoSuperado)} Superar al Guardián      {Marca(m.ExtraccionRealizada)} Regresar a extracción\n" +
                         (juego.SesionActiva ? (juego.EnCombate ? "\n● COMBATE ACTIVO" : "\nSin combate activo.") : "\nSESIÓN FINALIZADA.");

        // Barra del enemigo: solo se ve durante el combate.
        barraEnemigo.Visible = juego.EnCombate && juego.SesionActiva;
        if (juego.EnCombate)
        {
            barraEnemigo.Titulo = juego.EnemigoActual.Nombre;
            barraEnemigo.Valor = juego.EnemigoActual.Salud;
        }

        btnAtacar.Enabled = juego.EnCombate && juego.SesionActiva;
        btnEscapar.Enabled = juego.EnCombate && juego.SesionActiva;
    }

    private static string Marca(bool ok) => ok ? "✓" : "□";

    // Panel sin parpadeo que se redibuja al cambiar de tamaño (para que el borde no se corte).
    private sealed class PanelNexus : Panel
    {
        public PanelNexus()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
        }
    }
}
