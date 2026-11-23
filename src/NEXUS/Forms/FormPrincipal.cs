using System.Media;
using NEXUS.Models;
using NEXUS.Services;

namespace NEXUS.Forms;

// ============================================================
//  FORM PRINCIPAL: la ventana del juego.
//
//  Tiene 4 PANTALLAS (solo una visible a la vez):
//   1. Menú principal        (fondo de estrellas + logo)
//   2. Identificación        (nombre del cadete + narrativa)
//   3. Selección de realidad (3 tarjetas)
//   4. Juego                 (estado, mapa, misión, acciones, bitácora)
//
//  Y 3 VENTANAS EMERGENTES: alerta, encuentro e inventario.
//  Toda la lógica está en MotorJuego; aquí solo se muestra.
// ============================================================
public sealed class FormPrincipal : Form
{
    private readonly MotorJuego motor = new MotorJuego();
    private readonly RepositorioPartidasSql repositorio = new RepositorioPartidasSql(
        Environment.GetEnvironmentVariable("NEXUS_SQL_CONNECTION") ??
        "Server=localhost;Database=NEXUS;Integrated Security=True;Encrypt=False;TrustServerCertificate=True");

    // ---------------- PANTALLAS ----------------
    private readonly PanelEstrellas pantallaMenu = new PanelEstrellas(true);
    private readonly PanelEstrellas pantallaIdentidad = new PanelEstrellas(false);
    private readonly PanelEstrellas pantallaRealidades = new PanelEstrellas(false);
    private readonly Panel pantallaJuego = new Panel { BackColor = Tema.Fondo };

    // ---------------- VENTANAS EMERGENTES ----------------
    private readonly PanelAlerta panelAlerta = new PanelAlerta();
    private readonly PanelEncuentro panelEncuentro;
    private readonly PanelInventario panelInventario;

    // ---------------- MENÚ ----------------
    private readonly Button[] botonesMenu = new Button[4];

    // ---------------- IDENTIFICACIÓN ----------------
    private PanelNexus cajaIdentidad;
    private TextBox txtNombre;
    private Label lblMensajeNombre;
    private CheckBox chkIris;
    private Label lblNarrativa;
    private Button btnConfirmar;
    private Button btnContinuar;
    private readonly System.Windows.Forms.Timer relojNarrativa = new System.Windows.Forms.Timer { Interval = 15 };
    private string textoNarrativa = "";
    private int letrasMostradas;

    // ---------------- SELECCIÓN DE REALIDAD ----------------
    private Label lblTituloRealidades;
    private readonly PanelNexus[] tarjetas = new PanelNexus[3];
    private readonly Label[] lblCodigo = new Label[3];
    private readonly Label[] lblNombreRealidad = new Label[3];
    private readonly Label[] lblDescripcion = new Label[3];
    private readonly Label[] lblRiesgo = new Label[3];
    private readonly Label[] lblExplorado = new Label[3];
    private readonly Label[] lblEtiquetaMision = new Label[3];
    private Button btnCancelarRealidad;
    private bool eligiendoDesdeJuego;
    private bool saltando;

    // ---------------- JUEGO ----------------
    private BarraEstado barraSalud, barraEnergia, barraEstabilidad, barraEscudo;
    private Label lblCadete, lblRealidad, lblDatos;
    private PanelNexus marcoMapa;
    private PanelMapa mapa;
    private readonly Label[] lblObjetivos = new Label[4];
    private RichTextBox bitacora;

    private bool volverAlMenu;
    private bool cerrando;

    public FormPrincipal()
    {
        Text = "NEXUS v1.0 | Sistema de Exploración Interdimensional";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1200, 800);
        Size = new Size(1400, 900);
        WindowState = FormWindowState.Maximized;
        BackColor = Tema.Fondo;
        ForeColor = Tema.Texto;
        Font = Tema.FuenteNormal;
        DoubleBuffered = true;

        panelEncuentro = new PanelEncuentro(motor);
        panelInventario = new PanelInventario(motor);

        ConstruirMenu();
        ConstruirIdentidad();
        ConstruirRealidades();
        ConstruirJuego();

        // Las pantallas ocupan toda la ventana.
        Control[] pantallas = { pantallaMenu, pantallaIdentidad, pantallaRealidades, pantallaJuego };
        foreach (Control pantalla in pantallas)
        {
            pantalla.Dock = DockStyle.Fill;
            Controls.Add(pantalla);
        }
        Controls.Add(panelEncuentro);
        Controls.Add(panelInventario);
        Controls.Add(panelAlerta);

        // El motor nos avisa lo que pasa con eventos.
        motor.Mensaje += AgregarBitacora;
        motor.Alerta += (titulo, lineas, color) => panelAlerta.Encolar(titulo, lineas, Tema.ColorDe(color));
        motor.Destello += color =>
        {
            mapa.Destellar(Tema.ColorDe(color));
            panelEncuentro.Temblar(Tema.ColorDe(color));
        };
        motor.SaltoRealidad += realidad => mapa.IniciarSalto(Tema.ColorDe(realidad.Color));
        motor.PartidaTerminada += victoria =>
        {
            volverAlMenu = true;
            if (victoria) SystemSounds.Asterisk.Play();
            else SystemSounds.Hand.Play();
        };

        panelAlerta.TodasCerradas += AlCerrarAlertas;
        panelEncuentro.AccionRealizada += Refrescar;
        panelEncuentro.PedirInventario += AbrirInventario;
        panelInventario.AccionRealizada += Refrescar;
        panelInventario.Cerrado += Refrescar;

        Resize += (s, e) => AcomodarVentanasEmergentes();
        FormClosing += (s, e) => { if (!cerrando) Iris.CerrarConsola(); };

        MostrarMenu();
    }

    // ======================================================
    //  NAVEGACIÓN ENTRE PANTALLAS
    // ======================================================
    private void MostrarPantalla(Control pantalla)
    {
        pantallaMenu.Visible = pantalla == pantallaMenu;
        pantallaIdentidad.Visible = pantalla == pantallaIdentidad;
        pantallaRealidades.Visible = pantalla == pantallaRealidades;
        pantallaJuego.Visible = pantalla == pantallaJuego;
        pantalla.BringToFront();
        pantalla.PerformLayout();
    }

    private void MostrarMenu()
    {
        panelEncuentro.Visible = false;
        panelInventario.Visible = false;
        MostrarPantalla(pantallaMenu);
        UbicarBotonesMenu();
    }

    // ======================================================
    //  1. MENÚ PRINCIPAL
    // ======================================================
    private void ConstruirMenu()
    {
        string[] textos = { "NUEVA PARTIDA", "CÓMO JUGAR", "ABRIR CONSOLA IRIS", "SALIR DEL SISTEMA" };
        for (int i = 0; i < botonesMenu.Length; i++)
        {
            botonesMenu[i] = Tema.CrearBoton(textos[i], i == 0);
            botonesMenu[i].Size = new Size(360, 52);
            botonesMenu[i].Font = Tema.FuenteSubtitulo;
            pantallaMenu.Controls.Add(botonesMenu[i]);
        }
        botonesMenu[0].Click += (s, e) => MostrarIdentidad();
        botonesMenu[1].Click += (s, e) => MostrarAyuda();
        botonesMenu[2].Click += (s, e) => AbrirIris();
        botonesMenu[3].Click += (s, e) => SalidaControlada();

        Label pie = new Label
        {
            Text = "NEXUS v1.0  ·  Ingeniería de Sistemas · Unifranz  ·  Controles: W A S D / flechas",
            Dock = DockStyle.Bottom,
            Height = 30,
            ForeColor = Tema.TextoGris,
            BackColor = Tema.Fondo,
            TextAlign = ContentAlignment.MiddleCenter
        };
        pantallaMenu.Controls.Add(pie);
        pantallaMenu.Resize += (s, e) => UbicarBotonesMenu();
    }

    private void UbicarBotonesMenu()
    {
        int x = (pantallaMenu.Width - 360) / 2;
        int y = pantallaMenu.Height * 45 / 100;
        for (int i = 0; i < botonesMenu.Length; i++)
        {
            botonesMenu[i].Location = new Point(x, y + i * 64);
        }
    }

    private void MostrarAyuda()
    {
        panelAlerta.Encolar("CÓMO JUGAR", new[]
        {
            "OBJETIVO: entra a R-404, toca la anomalía (A), vence al Guardián del núcleo (N)",
            "y vuelve al punto de extracción (E) con el núcleo.",
            "",
            "MOVERSE:  W A S D, flechas o los botones ▲ ◄ ▼ ►   (cada paso cuesta 1 de energía)",
            "TECLAS:  I inventario · C consejo de IRIS · R cambiar realidad (sobre E)",
            "               F atacar · H ayuda · Q abandonar",
            "",
            "ENCUENTROS:  [1] Atacar   [2] Escapar   [3] Usar objeto   [4] Analizar",
            "ROBOT: no ataca si no lo atacas. Analízalo ANTES de atacar y lo hackeas.",
            "ENEMIGO: siempre ataca. Analizarlo revela su debilidad (+10 de daño).",
            "",
            "Si tu SALUD o tu ESTABILIDAD llegan a 0, la misión falla.",
            "El punto de extracción (E) recupera +10 de estabilidad."
        }, Tema.Acento);
    }

    private void AbrirIris()
    {
        if (Iris.AbrirConsola())
        {
            Iris.Enviar("IRIS en línea. Canal con NEXUS establecido.");
            panelAlerta.Encolar("IRIS EN LÍNEA", new[]
            {
                "Se abrió la consola de IRIS en otra ventana.",
                "Ponla al lado del juego: ahí verás sus mensajes y alertas."
            }, Tema.Magenta);
        }
        else
        {
            panelAlerta.Encolar("IRIS NO DISPONIBLE", new[]
            {
                "No se pudo abrir la consola de IRIS.",
                "Sus mensajes seguirán apareciendo en la bitácora del juego."
            }, Tema.Rojo);
        }
    }

    // Salida controlada: confirma, cierra IRIS y la ventana se desvanece.
    private void SalidaControlada()
    {
        DialogResult respuesta = MessageBox.Show("¿Seguro que deseas salir del sistema NEXUS?", "NEXUS",
                                                 MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (respuesta != DialogResult.Yes) return;

        cerrando = true;
        Iris.CerrarConsola();
        pantallaMenu.Velocidad = 0.06f; // las estrellas se aceleran

        System.Windows.Forms.Timer desvanecer = new System.Windows.Forms.Timer { Interval = 30 };
        desvanecer.Tick += (s, e) =>
        {
            Opacity -= 0.04;
            if (Opacity <= 0.05)
            {
                desvanecer.Stop();
                Close();
            }
        };
        desvanecer.Start();
    }

    // ======================================================
    //  2. IDENTIFICACIÓN Y NARRATIVA
    // ======================================================
    private void ConstruirIdentidad()
    {
        cajaIdentidad = new PanelNexus("IDENTIFICACIÓN DEL CADETE") { Size = new Size(860, 620) };

        TableLayoutPanel tabla = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 7, BackColor = Tema.Panel };
        tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        tabla.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));

        Label instruccion = Tema.CrearEtiqueta("Antes del salto, el sistema NEXUS necesita confirmar tu identidad.",
                                               Tema.FuenteNormal, Tema.Texto);
        Label etiquetaNombre = Tema.CrearEtiqueta("NOMBRE DE CADETE  (máximo 14 letras)", Tema.FuentePequena, Tema.TextoGris);

        txtNombre = new TextBox
        {
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 16, FontStyle.Bold),
            BackColor = Tema.PanelOscuro,
            ForeColor = Tema.Acento,
            BorderStyle = BorderStyle.FixedSingle,
            MaxLength = 14
        };
        txtNombre.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ConfirmarIdentidad();
            }
        };

        lblMensajeNombre = Tema.CrearEtiqueta("", Tema.FuenteNegrita, Tema.Rojo);

        chkIris = new CheckBox
        {
            Text = "Abrir la consola de IRIS en otra ventana (recomendado)",
            Checked = true,
            Dock = DockStyle.Fill,
            ForeColor = Tema.Texto,
            BackColor = Tema.Panel
        };

        lblNarrativa = new Label
        {
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 11.5f),
            ForeColor = Tema.Texto,
            BackColor = Tema.PanelOscuro,
            Padding = new Padding(14),
            Margin = new Padding(0, 6, 0, 6)
        };
        lblNarrativa.Click += (s, e) => TerminarNarrativa();

        // Fila de botones: VOLVER a la izquierda; CONFIRMAR o CONTINUAR a la derecha.
        TableLayoutPanel botones = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, BackColor = Tema.Panel, Margin = new Padding(0) };
        botones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        botones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));

        Button btnVolver = Tema.CrearBoton("◄ VOLVER AL MENÚ");
        btnVolver.Dock = DockStyle.Fill;
        btnVolver.Click += (s, e) =>
        {
            relojNarrativa.Stop();
            motor.Abandonar();
            MostrarMenu();
        };

        Panel zonaDerecha = new Panel { Dock = DockStyle.Fill, BackColor = Tema.Panel, Margin = new Padding(0) };
        btnConfirmar = Tema.CrearBoton("CONFIRMAR IDENTIDAD", true);
        btnConfirmar.Dock = DockStyle.Fill;
        btnConfirmar.Click += (s, e) => ConfirmarIdentidad();
        btnContinuar = Tema.CrearBoton("CONTINUAR ►", true);
        btnContinuar.Dock = DockStyle.Fill;
        btnContinuar.Visible = false;
        btnContinuar.Click += (s, e) => ContinuarNarrativa();
        zonaDerecha.Controls.Add(btnConfirmar);
        zonaDerecha.Controls.Add(btnContinuar);

        botones.Controls.Add(btnVolver, 0, 0);
        botones.Controls.Add(zonaDerecha, 1, 0);

        tabla.Controls.Add(instruccion, 0, 0);
        tabla.Controls.Add(etiquetaNombre, 0, 1);
        tabla.Controls.Add(txtNombre, 0, 2);
        tabla.Controls.Add(lblMensajeNombre, 0, 3);
        tabla.Controls.Add(chkIris, 0, 4);
        tabla.Controls.Add(lblNarrativa, 0, 5);
        tabla.Controls.Add(botones, 0, 6);
        cajaIdentidad.Controls.Add(tabla);

        pantallaIdentidad.Controls.Add(cajaIdentidad);
        pantallaIdentidad.Resize += (s, e) => Tema.Centrar(cajaIdentidad);

        relojNarrativa.Tick += (s, e) =>
        {
            letrasMostradas = Math.Min(textoNarrativa.Length, letrasMostradas + 2);
            lblNarrativa.Text = textoNarrativa.Substring(0, letrasMostradas);
            if (letrasMostradas >= textoNarrativa.Length) relojNarrativa.Stop();
        };
    }

    private void MostrarIdentidad()
    {
        txtNombre.Text = "";
        txtNombre.Enabled = true;
        lblMensajeNombre.Text = "";
        lblNarrativa.Text = "\n   > Esperando identificación del cadete...";
        btnConfirmar.Visible = true;
        btnContinuar.Visible = false;

        MostrarPantalla(pantallaIdentidad);
        Tema.Centrar(cajaIdentidad);
        txtNombre.Focus();
    }

    private void ConfirmarIdentidad()
    {
        string nombre = txtNombre.Text.Trim();

        // Control: nombre vacío o demasiado largo.
        if (nombre.Length == 0)
        {
            MostrarMensajeNombre("× El nombre no puede estar vacío.", Tema.Rojo);
            return;
        }
        if (nombre.Length > 14)
        {
            MostrarMensajeNombre("× Máximo 14 caracteres.", Tema.Rojo);
            return;
        }

        motor.NuevaPartida(nombre);
        bitacora.Clear();
        PrepararBarras();

        txtNombre.Enabled = false;
        MostrarMensajeNombre("■ IDENTIDAD CONFIRMADA: CADETE " + motor.Cadete.Nombre, Tema.Verde);
        btnConfirmar.Visible = false;
        btnContinuar.Visible = true;

        textoNarrativa =
            "▌ TRANSMISIÓN ENTRANTE · CENTRAL NEXUS\n\n" +
            "Año 2297.\n" +
            "La red NEXUS detectó una anomalía en una realidad desconocida: R-404.\n" +
            "Dentro de ella late un núcleo energético capaz de estabilizar todas las realidades.\n" +
            "El equipo Alfa entró hace 3 días. Nunca regresó.\n\n" +
            $"Cadete {motor.Cadete.Nombre}: tu misión es llegar a la anomalía, recuperar el núcleo\n" +
            "y volver con vida al punto de extracción.\n\n" +
            "MISIÓN: " + motor.Mision.Nombre + "\n";
        for (int i = 0; i < motor.Mision.Objetivos.Length; i++)
        {
            textoNarrativa += $"   {i + 1}. {motor.Mision.Objetivos[i]}\n";
        }
        textoNarrativa += "\nAlgo protege ese núcleo. Ten cuidado.";

        letrasMostradas = 0;
        lblNarrativa.Text = "";
        relojNarrativa.Start();
        btnContinuar.Focus();
    }

    private void MostrarMensajeNombre(string texto, Color color)
    {
        lblMensajeNombre.Text = texto;
        lblMensajeNombre.ForeColor = color;
    }

    // Si el texto se está escribiendo, lo termina de golpe.
    private void TerminarNarrativa()
    {
        relojNarrativa.Stop();
        letrasMostradas = textoNarrativa.Length;
        lblNarrativa.Text = textoNarrativa;
    }

    private void ContinuarNarrativa()
    {
        if (letrasMostradas < textoNarrativa.Length)
        {
            TerminarNarrativa();
            return;
        }

        if (chkIris.Checked) Iris.AbrirConsola();
        MostrarRealidades(false);
    }

    // ======================================================
    //  3. SELECCIÓN DE REALIDAD (arreglo de tarjetas + ciclo for)
    // ======================================================
    private void ConstruirRealidades()
    {
        lblTituloRealidades = new Label
        {
            Text = "SELECCIÓN DE REALIDAD",
            Font = Tema.FuenteTitulo,
            ForeColor = Tema.Acento,
            BackColor = Tema.Fondo,
            AutoSize = true
        };
        pantallaRealidades.Controls.Add(lblTituloRealidades);

        for (int i = 0; i < tarjetas.Length; i++)
        {
            PanelNexus tarjeta = new PanelNexus("") { Size = new Size(370, 360) };
            TableLayoutPanel tabla = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 7, BackColor = Tema.Panel };
            tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            tabla.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));

            lblCodigo[i] = Tema.CrearEtiqueta("", new Font("Segoe UI", 26, FontStyle.Bold), Tema.Acento);
            lblNombreRealidad[i] = Tema.CrearEtiqueta("", Tema.FuenteSubtitulo, Tema.Texto);
            lblDescripcion[i] = Tema.CrearEtiqueta("", Tema.FuenteNormal, Tema.TextoGris);
            lblDescripcion[i].TextAlign = ContentAlignment.TopLeft;
            lblRiesgo[i] = Tema.CrearEtiqueta("", Tema.FuenteNegrita, Tema.Verde);
            lblExplorado[i] = Tema.CrearEtiqueta("", Tema.FuenteNormal, Tema.TextoGris);
            lblEtiquetaMision[i] = Tema.CrearEtiqueta("", Tema.FuenteNegrita, Tema.Magenta);

            Button btnSaltar = Tema.CrearBoton("SALTAR A ESTA REALIDAD ►", true);
            btnSaltar.Dock = DockStyle.Fill;
            int indice = i; // copia para usarla dentro del evento
            btnSaltar.Click += (s, e) => ElegirRealidad(indice);

            tabla.Controls.Add(lblCodigo[i], 0, 0);
            tabla.Controls.Add(lblNombreRealidad[i], 0, 1);
            tabla.Controls.Add(lblDescripcion[i], 0, 2);
            tabla.Controls.Add(lblRiesgo[i], 0, 3);
            tabla.Controls.Add(lblExplorado[i], 0, 4);
            tabla.Controls.Add(lblEtiquetaMision[i], 0, 5);
            tabla.Controls.Add(btnSaltar, 0, 6);
            tarjeta.Controls.Add(tabla);

            tarjetas[i] = tarjeta;
            pantallaRealidades.Controls.Add(tarjeta);
        }

        btnCancelarRealidad = Tema.CrearBoton("CANCELAR");
        btnCancelarRealidad.Size = new Size(260, 46);
        btnCancelarRealidad.Click += (s, e) => CancelarRealidad();
        pantallaRealidades.Controls.Add(btnCancelarRealidad);

        pantallaRealidades.Resize += (s, e) => UbicarTarjetas();
    }

    private void UbicarTarjetas()
    {
        int separacion = 30;
        int anchoTotal = tarjetas.Length * 370 + (tarjetas.Length - 1) * separacion;
        int x0 = (pantallaRealidades.Width - anchoTotal) / 2;
        int y = Math.Max(110, (pantallaRealidades.Height - 360) / 2);

        lblTituloRealidades.Location = new Point((pantallaRealidades.Width - lblTituloRealidades.Width) / 2, y - 80);
        for (int i = 0; i < tarjetas.Length; i++)
        {
            tarjetas[i].Location = new Point(x0 + i * (370 + separacion), y);
        }
        btnCancelarRealidad.Location = new Point((pantallaRealidades.Width - btnCancelarRealidad.Width) / 2, y + 360 + 30);
    }

    private void MostrarRealidades(bool desdeJuego)
    {
        eligiendoDesdeJuego = desdeJuego;
        saltando = false;
        btnCancelarRealidad.Text = desdeJuego ? "◄ VOLVER AL MAPA" : "CANCELAR MISIÓN";

        for (int i = 0; i < tarjetas.Length; i++)
        {
            Realidad r = motor.Realidades[i];
            Color colorRealidad = Tema.ColorDe(r.Color);
            Color colorRiesgo = r.NivelRiesgo == 1 ? Tema.Verde : r.NivelRiesgo == 2 ? Tema.Ambar : Tema.Rojo;

            tarjetas[i].ColorAcento = colorRealidad;
            lblCodigo[i].Text = r.Codigo;
            lblCodigo[i].ForeColor = colorRealidad;
            lblNombreRealidad[i].Text = r.Nombre;
            lblDescripcion[i].Text = r.Descripcion;
            lblRiesgo[i].Text = "RIESGO  " + new string('■', r.NivelRiesgo) + new string('□', 3 - r.NivelRiesgo);
            lblRiesgo[i].ForeColor = colorRiesgo;
            lblExplorado[i].Text = $"Explorado: {r.PorcentajeExplorado()}%";
            lblEtiquetaMision[i].Text = i == MotorJuego.IndiceRealidadMision ? "◄ AQUÍ ESTÁ LA MISIÓN" : "";
            tarjetas[i].Invalidate();
        }

        MostrarPantalla(pantallaRealidades);
        UbicarTarjetas();
    }

    // Animación: las estrellas se aceleran (salto) y luego entramos al mapa.
    private void ElegirRealidad(int indice)
    {
        if (saltando) return;
        saltando = true;
        pantallaRealidades.Velocidad = 0.07f;

        System.Windows.Forms.Timer espera = new System.Windows.Forms.Timer { Interval = 600 };
        espera.Tick += (s, e) =>
        {
            espera.Stop();
            espera.Dispose();
            pantallaRealidades.Velocidad = 0.012f;
            saltando = false;

            MostrarPantalla(pantallaJuego);
            motor.EntrarRealidad(indice);
            Refrescar();
        };
        espera.Start();
    }

    private void CancelarRealidad()
    {
        if (eligiendoDesdeJuego)
        {
            MostrarPantalla(pantallaJuego);
            Refrescar();
        }
        else
        {
            motor.Abandonar();
            MostrarMenu();
        }
    }

    // ======================================================
    //  4. PANTALLA DE JUEGO
    // ======================================================
    private void ConstruirJuego()
    {
        TableLayoutPanel raiz = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            Padding = new Padding(8),
            BackColor = Tema.Fondo
        };
        raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
        raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 108));
        raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 180));

        PanelNexus panelEstado = CrearPanelEstado();
        raiz.Controls.Add(panelEstado, 0, 0);
        raiz.SetColumnSpan(panelEstado, 2);

        marcoMapa = new PanelNexus("MAPA") { Dock = DockStyle.Fill };
        mapa = new PanelMapa { Dock = DockStyle.Fill };
        marcoMapa.Controls.Add(mapa);
        raiz.Controls.Add(marcoMapa, 0, 1);

        raiz.Controls.Add(CrearColumnaDerecha(), 1, 1);

        PanelNexus marcoBitacora = new PanelNexus("BITÁCORA DE EXPLORACIÓN") { Dock = DockStyle.Fill };
        bitacora = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            BackColor = Tema.PanelOscuro,
            ForeColor = Tema.Texto,
            Font = Tema.FuenteMono,
            TabStop = false
        };
        marcoBitacora.Controls.Add(bitacora);
        raiz.Controls.Add(marcoBitacora, 0, 2);
        raiz.SetColumnSpan(marcoBitacora, 2);

        pantallaJuego.Controls.Add(raiz);
    }

    // Arriba: nombre, realidad, XP y las 4 barras.
    private PanelNexus CrearPanelEstado()
    {
        PanelNexus panel = new PanelNexus("") { Dock = DockStyle.Fill };
        TableLayoutPanel tabla = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 5, RowCount = 1, BackColor = Tema.Panel };
        tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 420));
        for (int i = 0; i < 4; i++) tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

        TableLayoutPanel info = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = Tema.Panel, Margin = new Padding(0) };
        for (int i = 0; i < 3; i++) info.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3f));
        lblCadete = Tema.CrearEtiqueta("CADETE", Tema.FuenteSubtitulo, Tema.Texto);
        lblRealidad = Tema.CrearEtiqueta("", Tema.FuenteNegrita, Tema.Acento);
        lblDatos = Tema.CrearEtiqueta("", Tema.FuenteNegrita, Tema.Ambar);
        info.Controls.Add(lblCadete, 0, 0);
        info.Controls.Add(lblRealidad, 0, 1);
        info.Controls.Add(lblDatos, 0, 2);
        tabla.Controls.Add(info, 0, 0);

        barraSalud = new BarraEstado("SALUD", Tema.Verde);
        barraEnergia = new BarraEstado("ENERGÍA", Tema.Acento);
        barraEstabilidad = new BarraEstado("ESTABILIDAD", Tema.Magenta);
        barraEscudo = new BarraEstado("ESCUDO", Tema.Azul);
        BarraEstado[] barras = { barraSalud, barraEnergia, barraEstabilidad, barraEscudo };
        for (int i = 0; i < barras.Length; i++)
        {
            barras[i].Dock = DockStyle.Fill;
            barras[i].Margin = new Padding(10, 14, 10, 0);
            tabla.Controls.Add(barras[i], i + 1, 0);
        }

        panel.Controls.Add(tabla);
        return panel;
    }

    // Derecha: misión, acciones, movimiento y leyenda.
    private TableLayoutPanel CrearColumnaDerecha()
    {
        TableLayoutPanel columna = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, BackColor = Tema.Fondo, Margin = new Padding(0) };
        columna.RowStyles.Add(new RowStyle(SizeType.Absolute, 176));
        columna.RowStyles.Add(new RowStyle(SizeType.Absolute, 196));
        columna.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
        columna.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // --- Misión ---
        PanelNexus marcoMision = new PanelNexus("MISIÓN: LA ANOMALÍA PERDIDA") { Dock = DockStyle.Fill, ColorAcento = Tema.Verde };
        TableLayoutPanel listaObjetivos = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, BackColor = Tema.Panel };
        for (int i = 0; i < lblObjetivos.Length; i++)
        {
            listaObjetivos.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
            lblObjetivos[i] = Tema.CrearEtiqueta("", Tema.FuenteNormal, Tema.TextoGris);
            listaObjetivos.Controls.Add(lblObjetivos[i], 0, i);
        }
        marcoMision.Controls.Add(listaObjetivos);
        columna.Controls.Add(marcoMision, 0, 0);

        // --- Acciones ---
        PanelNexus marcoAcciones = new PanelNexus("CENTRO DE OPERACIONES") { Dock = DockStyle.Fill };
        TableLayoutPanel acciones = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3, BackColor = Tema.Panel };
        acciones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        acciones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        for (int i = 0; i < 3; i++) acciones.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3f));

        AgregarAccion(acciones, "INVENTARIO  [I]", 0, 0, true, AbrirInventario);
        AgregarAccion(acciones, "CONSULTAR IRIS  [C]", 1, 0, false, () => { motor.ConsultarIris(); Refrescar(); });
        AgregarAccion(acciones, "CAMBIAR REALIDAD  [R]", 0, 1, false, CambiarRealidad);
        AgregarAccion(acciones, "ATACAR  [F]", 1, 1, false, () => { motor.Atacar(); Refrescar(); });
        AgregarAccion(acciones, "AYUDA  [H]", 0, 2, false, MostrarAyuda);
        AgregarAccion(acciones, "GUARDAR  [Ctrl+S]", 1, 2, true, GuardarPartida);
        marcoAcciones.Controls.Add(acciones);
        columna.Controls.Add(marcoAcciones, 0, 1);

        // --- Movimiento (cruceta) ---
        PanelNexus marcoMovimiento = new PanelNexus("MOVIMIENTO  ·  W A S D") { Dock = DockStyle.Fill };
        TableLayoutPanel cruceta = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 2, BackColor = Tema.Panel };
        for (int i = 0; i < 3; i++) cruceta.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
        for (int i = 0; i < 2; i++) cruceta.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        AgregarAccion(cruceta, "▲", 1, 0, false, () => Mover(-1, 0));
        AgregarAccion(cruceta, "◄", 0, 1, false, () => Mover(0, -1));
        AgregarAccion(cruceta, "▼", 1, 1, false, () => Mover(1, 0));
        AgregarAccion(cruceta, "►", 2, 1, false, () => Mover(0, 1));
        marcoMovimiento.Controls.Add(cruceta);
        columna.Controls.Add(marcoMovimiento, 0, 2);

        // --- Leyenda ---
        PanelNexus marcoLeyenda = new PanelNexus("LEYENDA DEL MAPA") { Dock = DockStyle.Fill };
        Label leyenda = Tema.CrearEtiqueta(
            "● Tú (cadete)          E  Extracción\n" +
            "◆ Objeto               R  Robot\n" +
            "X Enemigo              A  Anomalía\n" +
            "N Núcleo               i  Registro de datos\n" +
            "~ Zona inestable       ▓  Muro",
            Tema.FuenteMono, Tema.TextoGris);
        leyenda.TextAlign = ContentAlignment.TopLeft;
        marcoLeyenda.Controls.Add(leyenda);
        columna.Controls.Add(marcoLeyenda, 0, 3);

        return columna;
    }

    private void AgregarAccion(TableLayoutPanel tabla, string texto, int columna, int fila, bool destacado, Action accion)
    {
        Button boton = Tema.CrearBoton(texto, destacado);
        boton.Dock = DockStyle.Fill;
        boton.Click += (s, e) => accion();
        tabla.Controls.Add(boton, columna, fila);
    }

    // Al empezar una partida, las barras se llenan sin animación.
    private void PrepararBarras()
    {
        barraSalud.FijarSinAnimar(motor.Cadete.Salud);
        barraEnergia.FijarSinAnimar(motor.Cadete.Energia);
        barraEstabilidad.FijarSinAnimar(motor.Cadete.Estabilidad);
        barraEscudo.FijarSinAnimar(motor.Cadete.Escudo);
    }

    // ======================================================
    //  ACCIONES DEL JUGADOR
    // ======================================================
    private void Mover(int cambioFila, int cambioColumna)
    {
        motor.Mover(cambioFila, cambioColumna);
        Refrescar();
    }

    private void AbrirInventario()
    {
        if (motor.Cadete == null || !motor.PartidaActiva) return;
        AcomodarSobreMapa(panelInventario, 30);
        panelInventario.Mostrar();
    }

    private void CambiarRealidad()
    {
        if (motor.IntentarCambiarRealidad()) MostrarRealidades(true);
        else Refrescar();
    }

    private void Abandonar()
    {
        if (!motor.PartidaActiva) return;
        DialogResult respuesta = MessageBox.Show("¿Abandonar la misión y volver al menú principal?", "NEXUS",
                                                 MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (respuesta != DialogResult.Yes) return;

        motor.Abandonar();
        MostrarMenu();
    }

    // Cuando se cierran todas las alertas: si la partida terminó, volvemos al menú.
    private void AlCerrarAlertas()
    {
        if (volverAlMenu)
        {
            volverAlMenu = false;
            MostrarMenu();
            return;
        }
        Refrescar();
    }

    // ======================================================
    //  TECLADO
    // ======================================================
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        // 1) Con una alerta abierta: Enter, Espacio o Esc la cierran.
        if (panelAlerta.Visible)
        {
            if (keyData == Keys.Enter || keyData == Keys.Space || keyData == Keys.Escape)
            {
                panelAlerta.Cerrar();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // Fuera de la pantalla de juego, el teclado funciona normal.
        if (!pantallaJuego.Visible || !motor.PartidaActiva) return base.ProcessCmdKey(ref msg, keyData);

        if (keyData == (Keys.Control | Keys.S)) { GuardarPartida(); return true; }
        if (keyData == (Keys.Control | Keys.L)) { CargarPartida(); return true; }

        // 2) Inventario abierto.
        if (panelInventario.Visible)
        {
            if (keyData == Keys.Escape || keyData == Keys.I)
            {
                panelInventario.Cerrar();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // 3) En un encuentro: teclas 1, 2, 3, 4.
        if (panelEncuentro.Visible)
        {
            switch (keyData)
            {
                case Keys.D1: case Keys.NumPad1: motor.Atacar(); Refrescar(); return true;
                case Keys.D2: case Keys.NumPad2: motor.Escapar(); Refrescar(); return true;
                case Keys.D3: case Keys.NumPad3: AbrirInventario(); return true;
                case Keys.D4: case Keys.NumPad4: motor.Analizar(); Refrescar(); return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // 4) Explorando el mapa.
        switch (keyData)
        {
            case Keys.W: case Keys.Up: Mover(-1, 0); return true;
            case Keys.S: case Keys.Down: Mover(1, 0); return true;
            case Keys.A: case Keys.Left: Mover(0, -1); return true;
            case Keys.D: case Keys.Right: Mover(0, 1); return true;
            case Keys.I: AbrirInventario(); return true;
            case Keys.C: motor.ConsultarIris(); Refrescar(); return true;
            case Keys.R: CambiarRealidad(); return true;
            case Keys.F: motor.Atacar(); Refrescar(); return true;
            case Keys.H: MostrarAyuda(); return true;
            case Keys.Q: case Keys.Escape: Abandonar(); return true;
        }

        // Control: cualquier otra letra es una opción inválida.
        if (keyData >= Keys.A && keyData <= Keys.Z)
        {
            AgregarBitacora(new MensajeBitacora("Opción inválida. Usa W A S D para moverte o H para ver la ayuda.", ConsoleColor.Red));
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private async void GuardarPartida()
    {
        try
        {
            string json = motor.ExportarPartidaJson();
            Guid id = await repositorio.GuardarAsync(motor.Cadete.Nombre, json);
            AgregarBitacora(new MensajeBitacora($"Partida guardada en SQL Server ({id}).", ConsoleColor.Green));
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "No se pudo guardar la partida. Comprueba la conexión y ejecuta database/01_crear_base.sql.\n\n" + ex.Message,
                "Error de persistencia", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void CargarPartida()
    {
        try
        {
            var guardada = await repositorio.CargarUltimaAsync();
            if (guardada == null) { MessageBox.Show(this, "No hay partidas guardadas.", "NEXUS", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            motor.ImportarPartidaJson(guardada.Value.EstadoJson);
            bitacora.Clear(); PrepararBarras(); MostrarPantalla(pantallaJuego); Refrescar();
            AgregarBitacora(new MensajeBitacora($"Partida de {guardada.Value.Nombre} recuperada desde SQL Server.", ConsoleColor.Green));
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "No se pudo cargar la partida. Comprueba SQL Server y el esquema.\n\n" + ex.Message,
                "Error de persistencia", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // ======================================================
    //  ACTUALIZAR LA PANTALLA CON EL ESTADO DEL MOTOR
    // ======================================================
    private void Refrescar()
    {
        if (motor.Cadete == null) return;
        Cadete cadete = motor.Cadete;

        barraSalud.Valor = cadete.Salud;
        barraEnergia.Valor = cadete.Energia;
        barraEstabilidad.Valor = cadete.Estabilidad;
        barraEscudo.Valor = cadete.Escudo;

        lblCadete.Text = "CADETE " + cadete.Nombre;
        string datos = $"XP {cadete.XP}   ·   TURNO {motor.Turnos}   ·   INVENTARIO {cadete.Inventario.Cantidad}/{Inventario.Capacidad}";
        if (cadete.DanioExtra > 0) datos += $"   ·   ATAQUE +{cadete.DanioExtra}";
        lblDatos.Text = datos;

        Realidad realidad = motor.RealidadActual;
        if (realidad != null)
        {
            Color colorRealidad = Tema.ColorDe(realidad.Color);
            lblRealidad.Text = $"{realidad.Codigo}  |  {realidad.Nombre}";
            lblRealidad.ForeColor = colorRealidad;
            marcoMapa.Titulo = $"MAPA  ·  {realidad.Codigo} {realidad.Nombre.ToUpper()}   ·   EXPLORADO {realidad.PorcentajeExplorado()}%";
            marcoMapa.ColorAcento = colorRealidad;
            marcoMapa.Invalidate();
            mapa.Mostrar(realidad, motor.Fila, motor.Columna);
        }

        for (int i = 0; i < lblObjetivos.Length; i++)
        {
            bool hecho = motor.Mision.EstaCumplido(i);
            lblObjetivos[i].Text = (hecho ? "■  " : "□  ") + motor.Mision.Objetivos[i];
            lblObjetivos[i].ForeColor = hecho ? Tema.Verde : Tema.TextoGris;
        }

        // Ventana de encuentro: visible solo durante un combate.
        if (motor.EnCombate && pantallaJuego.Visible)
        {
            AcomodarSobreMapa(panelEncuentro, 0);
            panelEncuentro.Refrescar();
            panelEncuentro.Visible = true;
            panelEncuentro.BringToFront();
        }
        else
        {
            panelEncuentro.Visible = false;
        }

        if (panelInventario.Visible)
        {
            panelInventario.Refrescar();
            panelInventario.BringToFront();
        }
        if (panelAlerta.Visible) panelAlerta.BringToFront();

        // Quitamos el foco de los botones para que el teclado mueva al cadete.
        if (!panelInventario.Visible) ActiveControl = null;
    }

    // Coloca una ventana emergente justo encima del mapa.
    private void AcomodarSobreMapa(Control ventana, int margen)
    {
        Rectangle zona = RectangleToClient(marcoMapa.RectangleToScreen(marcoMapa.ClientRectangle));
        zona.Inflate(-margen, -margen);
        ventana.Bounds = zona;
    }

    private void AcomodarVentanasEmergentes()
    {
        if (panelEncuentro.Visible) AcomodarSobreMapa(panelEncuentro, 0);
        if (panelInventario.Visible) AcomodarSobreMapa(panelInventario, 30);
        if (panelAlerta.Visible) Tema.Centrar(panelAlerta);
    }

    // Escribe una línea en la bitácora con la hora y su color.
    private void AgregarBitacora(MensajeBitacora mensaje)
    {
        bitacora.SelectionStart = bitacora.TextLength;
        bitacora.SelectionColor = Tema.TextoGris;
        bitacora.AppendText($"[{DateTime.Now:HH:mm:ss}]  ");
        bitacora.SelectionStart = bitacora.TextLength;
        bitacora.SelectionColor = Tema.ColorDe(mensaje.Color);
        bitacora.AppendText(mensaje.Texto + Environment.NewLine);
        bitacora.SelectionStart = bitacora.TextLength;
        bitacora.ScrollToCaret();
    }
}
