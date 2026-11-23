using NEXUS.Models;

namespace NEXUS.Services;

// ============================================================
//  JUEGO: el "cerebro" de NEXUS.
//
//  Flujo general:
//   Menú principal → Identificar cadete → Narrativa → Elegir realidad
//   → Explorar el mapa (bucle) → Encuentros / eventos / inventario
//   → Misión completada o fallida → Menú principal → Salida controlada
// ============================================================
public class Juego
{
    // ---------------- DATOS DE LA PARTIDA ----------------
    private Cadete cadete;
    private Mision mision;
    private Realidad[] realidades;           // arreglo con las 3 realidades
    private Realidad realidadActual;

    private int fila, columna;               // posición del cadete en la matriz
    private int filaAnterior, columnaAnterior;
    private int turnos;
    private bool partidaActiva;
    private bool guardianDerrotado;
    private bool alertaEstabilidadEnviada;

    private readonly List<MensajeBitacora> bitacora = new List<MensajeBitacora>();
    private const int MensajesVisibles = 6;
    private const int IndiceRealidadMision = 2; // R-404 es la tercera del arreglo

    // Arreglo de textos que se encuentran en los registros de datos (i).
    private readonly string[] registrosPerdidos =
    {
        "Registro 0412: \"El núcleo no es una fuente de energía... es una puerta.\"",
        "Registro 0977: \"Los robots de vigilancia obedecen a quien conoce su código.\"",
        "Registro 1203: \"El Guardián nació de la misma anomalía que protege.\"",
        "Registro 1550: \"Si la estabilidad llega a cero, el cadete se pierde entre realidades.\"",
        "Registro 2001: \"Los puntos de extracción estabilizan el traje del explorador.\"",
        "Registro 2296: \"Última transmisión del equipo Alfa: no volvimos de R-404.\""
    };

    private readonly string[] nombresEnemigos =
    {
        "Espectro de datos", "Cazador sombrío", "Devorador de ecos", "Fragmento hostil"
    };

    // ======================================================
    //  MENÚ PRINCIPAL
    // ======================================================
    public void Iniciar()
    {
        Pantalla.Configurar();
        Console.CancelKeyPress += (remitente, evento) => Iris.CerrarConsola(); // Ctrl+C también cierra IRIS
        Introduccion();

        bool salir = false;
        while (!salir)
        {
            MostrarMenuPrincipal();
            int opcion = Pantalla.LeerOpcion(0, 5);

            switch (opcion)
            {
                case 1: NuevaPartida(); break;
                case 2: MostrarComoJugar(); break;
                case 3: AbrirIris(); break;
                case 4: Pantalla.AnimacionesActivas = !Pantalla.AnimacionesActivas; break;
                case 5: MostrarCreditos(); break;
                case 0: salir = Pantalla.PreguntarSiNo("¿Seguro que deseas salir del sistema NEXUS?"); break;
            }
        }

        SalidaControlada();
    }

    private void Introduccion()
    {
        Pantalla.Limpiar();
        Pantalla.DibujarLogo(true);
        Pantalla.Centrar("SISTEMA DE EXPLORACIÓN INTERDIMENSIONAL  //  AÑO 2297", ConsoleColor.Gray);
        Console.WriteLine();
        Pantalla.BarraCarga("Iniciando reactores del traje", ConsoleColor.Cyan);
        Pantalla.BarraCarga("Calibrando sensores de anomalías", ConsoleColor.Magenta);
        Pantalla.BarraCarga("Cargando mapas de realidades", ConsoleColor.Yellow);
        Pantalla.BarraCarga("Conectando con IRIS", ConsoleColor.Green);
        Pantalla.Pausa();
    }

    private void MostrarMenuPrincipal()
    {
        Pantalla.Limpiar();
        Pantalla.DibujarLogo(false);

        string estadoAnimaciones = Pantalla.AnimacionesActivas ? "ACTIVADAS" : "DESACTIVADAS";
        string estadoIris = Iris.ConsolaAbierta ? "EN LÍNEA" : "sin abrir";

        Pantalla.Caja("MENÚ PRINCIPAL", new[]
        {
            "",
            "   [1]  Nueva partida",
            "   [2]  Cómo jugar",
            "   [3]  Abrir consola IRIS            (" + estadoIris + ")",
            "   [4]  Animaciones                   (" + estadoAnimaciones + ")",
            "   [5]  Créditos",
            "",
            "   [0]  Salir del sistema",
            ""
        }, ConsoleColor.Cyan);
    }

    private void AbrirIris()
    {
        Console.WriteLine();
        if (Iris.AbrirConsola())
        {
            Pantalla.EscribirLinea("   Consola IRIS abierta en otra ventana. Ponla al lado de esta.", ConsoleColor.Green);
            Iris.Enviar("IRIS en línea. Canal con NEXUS establecido.");
        }
        else
        {
            Pantalla.EscribirLinea("   No se pudo abrir la consola IRIS. Sus mensajes seguirán en la bitácora.", ConsoleColor.Red);
        }
        Pantalla.Pausa();
    }

    // ======================================================
    //  NUEVA PARTIDA
    // ======================================================
    private void NuevaPartida()
    {
        Iris.ReiniciarCanal();
        string nombre = IdentificarCadete();
        CrearPartida(nombre);
        Narrativa();

        if (!Iris.ConsolaAbierta && Pantalla.PreguntarSiNo("¿Deseas abrir la consola de IRIS en otra ventana?"))
        {
            Iris.AbrirConsola();
            Pantalla.Esperar(800);
        }
        Iris.Enviar($"Conexión establecida con el cadete {cadete.Nombre}. Te acompañaré en la misión.");

        if (!SeleccionarRealidad())
        {
            partidaActiva = false;
            return;
        }

        BucleExploracion();
    }

    // Pide el nombre y lo valida (no vacío, máximo 14 letras).
    private string IdentificarCadete()
    {
        Pantalla.Limpiar();
        Console.WriteLine();
        Pantalla.Caja("IDENTIFICACIÓN DEL CADETE", new[]
        {
            "Antes de ingresar, el sistema NEXUS necesita confirmar tu identidad."
        }, ConsoleColor.Cyan);
        Console.WriteLine();

        while (true)
        {
            string nombre = Pantalla.LeerTexto("Nombre de cadete: ");

            if (nombre.Length == 0)
                Pantalla.EscribirLinea("   × El nombre no puede estar vacío.", ConsoleColor.Red);
            else if (nombre.Length > 14)
                Pantalla.EscribirLinea("   × Máximo 14 caracteres.", ConsoleColor.Red);
            else
            {
                Console.WriteLine();
                Pantalla.BarraCarga("Verificando identidad", ConsoleColor.Green);
                Pantalla.EscribirLinea("   IDENTIDAD CONFIRMADA: CADETE " + nombre.ToUpper(), ConsoleColor.Green);
                Pantalla.Esperar(700);
                return nombre.ToUpper();
            }
        }
    }

    private void CrearPartida(string nombre)
    {
        cadete = new Cadete(nombre);
        cadete.Inventario.Agregar(new Objeto(TipoObjeto.CelulaEnergia));
        cadete.Inventario.Agregar(new Objeto(TipoObjeto.Escudo));
        cadete.Inventario.Agregar(new Objeto(TipoObjeto.Analizador));
        cadete.Inventario.Agregar(new Objeto(TipoObjeto.KitMedico));

        mision = new Mision();
        realidades = CrearRealidades();
        turnos = 0;
        guardianDerrotado = false;
        alertaEstabilidadEnviada = false;
        bitacora.Clear();
        partidaActiva = true;
    }

    // Los mapas se escriben como texto y luego se convierten en matrices.
    // Todas las filas de un mapa deben tener el mismo largo (15).
    private Realidad[] CrearRealidades()
    {
        Realidad ciudad = new Realidad("R-17", "Ciudad de los Ecos",
            "Ruinas tranquilas. Buen lugar para conseguir recursos.", 1, ConsoleColor.Cyan,
            new[]
            {
                "###############",
                "#E....+...#..i#",
                "#.###.##..#.#.#",
                "#...#..R....#.#",
                "###.#.####..+.#",
                "#+..#....#..#.#",
                "#.###.##.#.##.#",
                "#.....+#...X..#",
                "###############"
            });

        Realidad sombras = new Realidad("R-18", "Sector de las Sombras",
            "Zona hostil. Más entidades, pero también más objetos.", 2, ConsoleColor.DarkYellow,
            new[]
            {
                "###############",
                "#E..~~..X...+.#",
                "#.#.##.####.#.#",
                "#+#..R.#..i.#.#",
                "#.####.#.##.#.#",
                "#...X..~~.#...#",
                "#.#.####..#.#X#",
                "#...+...#...#+#",
                "###############"
            });

        Realidad desconocida = new Realidad("R-404", "Realidad Desconocida",
            "Origen de la anomalía. Aquí está la misión.", 3, ConsoleColor.Magenta,
            new[]
            {
                "###############",
                "#E..#...~~..i.#",
                "#.#.#.#.##.##.#",
                "#.#...#.R#..+.#",
                "#.###.####.#..#",
                "#..X#..A...#..#",
                "##.~#.####.##.#",
                "#+...~.X.#...N#",
                "###############"
            });

        return new Realidad[] { ciudad, sombras, desconocida };
    }

    private void Narrativa()
    {
        Pantalla.Limpiar();
        Console.WriteLine("\n");
        Pantalla.Maquina("   ▌ TRANSMISIÓN ENTRANTE · CENTRAL NEXUS", ConsoleColor.Magenta, 20);
        Console.WriteLine();
        Pantalla.Maquina("   Año 2297.", ConsoleColor.White);
        Pantalla.Maquina("   La red NEXUS detectó una anomalía en una realidad desconocida: R-404.", ConsoleColor.Gray);
        Pantalla.Maquina("   Dentro de ella late un núcleo energético capaz de estabilizar todas las realidades.", ConsoleColor.Gray);
        Pantalla.Maquina("   El equipo Alfa entró hace 3 días. Nunca regresó.", ConsoleColor.Gray);
        Console.WriteLine();
        Pantalla.Maquina($"   Cadete {cadete.Nombre}: tu misión es llegar a la anomalía, recuperar el núcleo", ConsoleColor.Cyan);
        Pantalla.Maquina("   y volver con vida al punto de extracción.", ConsoleColor.Cyan);
        Console.WriteLine();
        Pantalla.Maquina("   Algo protege ese núcleo. Ten cuidado.", ConsoleColor.Red, 35);
        Console.WriteLine();

        string[] lineas = new string[mision.Objetivos.Length];
        for (int i = 0; i < mision.Objetivos.Length; i++)
        {
            lineas[i] = $"  {i + 1}. {mision.Objetivos[i]}";
        }
        Pantalla.Caja("MISIÓN: " + mision.Nombre, lineas, ConsoleColor.Green);
        Pantalla.Pausa();
    }

    // ======================================================
    //  SELECCIÓN DE REALIDAD (arreglo + ciclo for)
    // ======================================================
    private bool SeleccionarRealidad()
    {
        Pantalla.Limpiar();
        Console.WriteLine();
        Pantalla.Caja("SELECCIÓN DE REALIDAD", new[] { "Elige a qué realidad quieres saltar." }, ConsoleColor.Cyan);
        Console.WriteLine();

        for (int i = 0; i < realidades.Length; i++)
        {
            Realidad r = realidades[i];
            ConsoleColor colorRiesgo = r.NivelRiesgo == 1 ? ConsoleColor.Green
                                     : r.NivelRiesgo == 2 ? ConsoleColor.Yellow
                                     : ConsoleColor.Red;

            Pantalla.Escribir($"   [{i + 1}] ", ConsoleColor.Yellow);
            Pantalla.Escribir($"{r.Codigo,-6} {r.Nombre,-24}", r.Color);
            Pantalla.Escribir("Riesgo ", ConsoleColor.Gray);
            Pantalla.Escribir(new string('■', r.NivelRiesgo) + new string('□', 3 - r.NivelRiesgo), colorRiesgo);
            Pantalla.Escribir($"   Explorado {r.PorcentajeExplorado(),3}%", ConsoleColor.DarkGray);
            if (i == IndiceRealidadMision) Pantalla.Escribir("   ◄ MISIÓN", ConsoleColor.Magenta);
            Console.WriteLine();
            Pantalla.EscribirLinea("         " + r.Descripcion, ConsoleColor.DarkGray);
        }
        Console.WriteLine();
        Pantalla.EscribirLinea("   [0] Cancelar", ConsoleColor.Yellow);
        Console.WriteLine();

        int opcion = Pantalla.LeerOpcion(0, realidades.Length);
        if (opcion == 0) return false;

        EntrarRealidad(realidades[opcion - 1]);
        return true;
    }

    private void EntrarRealidad(Realidad realidad)
    {
        Pantalla.Salto(realidad.Color);

        realidadActual = realidad;
        fila = realidad.FilaInicio;
        columna = realidad.ColumnaInicio;
        filaAnterior = fila;
        columnaAnterior = columna;
        realidad.Descubrir(fila, columna);
        realidad.MarcarVisitado(fila, columna);

        Pantalla.Limpiar();
        Console.WriteLine("\n\n\n\n");
        Pantalla.Centrar("════════════════════════════════════════", realidad.Color);
        Pantalla.Centrar($"{realidad.Codigo}  |  {realidad.Nombre.ToUpper()}", ConsoleColor.White);
        Pantalla.Centrar("════════════════════════════════════════", realidad.Color);
        Pantalla.Esperar(900);

        Registrar($"Ingresaste a {realidad.Codigo} | {realidad.Nombre}.", realidad.Color);
        Iris.Enviar($"Salto completado: {realidad.Codigo} {realidad.Nombre}. {realidad.Descripcion}");
    }

    // ======================================================
    //  BUCLE DE EXPLORACIÓN (el corazón del juego)
    // ======================================================
    private void BucleExploracion()
    {
        while (partidaActiva)
        {
            DibujarHud();
            ConsoleKey tecla = Pantalla.LeerTecla();

            switch (tecla)
            {
                case ConsoleKey.W:
                case ConsoleKey.UpArrow: Mover(-1, 0); break;
                case ConsoleKey.S:
                case ConsoleKey.DownArrow: Mover(1, 0); break;
                case ConsoleKey.A:
                case ConsoleKey.LeftArrow: Mover(0, -1); break;
                case ConsoleKey.D:
                case ConsoleKey.RightArrow: Mover(0, 1); break;

                case ConsoleKey.I: MenuInventario(false); break;
                case ConsoleKey.F: Registrar("No hay ninguna entidad a la que atacar.", ConsoleColor.Red); break;
                case ConsoleKey.C: ConsultarIris(); break;
                case ConsoleKey.R: IntentarCambiarRealidad(); break;
                case ConsoleKey.M: MostrarMision(); break;
                case ConsoleKey.H: MostrarComoJugar(); break;

                case ConsoleKey.Q:
                case ConsoleKey.Escape:
                    if (Pantalla.PreguntarSiNo("¿Abandonar la misión y volver al menú principal?"))
                    {
                        partidaActiva = false;
                        Iris.Enviar("ALERTA: el cadete abandonó la misión.");
                    }
                    break;

                default:
                    Registrar("Opción inválida. Usa W A S D para moverte o H para ver la ayuda.", ConsoleColor.Red);
                    break;
            }

            if (partidaActiva) ComprobarEstado();
        }
    }

    private void Registrar(string texto, ConsoleColor color)
    {
        bitacora.Add(new MensajeBitacora(texto, color));
        if (bitacora.Count > 40) bitacora.RemoveAt(0);
    }

    // ---------------- DIBUJO DE LA PANTALLA DE EXPLORACIÓN ----------------
    private void DibujarHud()
    {
        Pantalla.Limpiar();

        // Encabezado
        Console.BackgroundColor = ConsoleColor.DarkCyan;
        Pantalla.Escribir(" NEXUS v1.0 ", ConsoleColor.White);
        Console.BackgroundColor = ConsoleColor.Black;
        Pantalla.Escribir($"  CADETE {cadete.Nombre}", ConsoleColor.White);
        Pantalla.Escribir($"   {realidadActual.Codigo} | {realidadActual.Nombre}", realidadActual.Color);
        Pantalla.EscribirLinea($"   Turno {turnos}   Explorado {realidadActual.PorcentajeExplorado()}%", ConsoleColor.DarkGray);
        Pantalla.Separador();

        // Estado del cadete en dos columnas
        Pantalla.Escribir(" ");
        Pantalla.Barra("SALUD", cadete.Salud, Cadete.Maximo, ConsoleColor.Green);
        Pantalla.Escribir("     ");
        Pantalla.Barra("ENERGÍA", cadete.Energia, Cadete.Maximo, ConsoleColor.Cyan);
        Console.WriteLine();
        Pantalla.Escribir(" ");
        Pantalla.Barra("ESTABILIDAD", cadete.Estabilidad, Cadete.Maximo, ConsoleColor.Magenta);
        Pantalla.Escribir("     ");
        Pantalla.Barra("ESCUDO", cadete.Escudo, Cadete.Maximo, ConsoleColor.Blue);
        Console.WriteLine();
        Pantalla.Escribir($" XP {cadete.XP}", ConsoleColor.Yellow);
        Pantalla.Escribir($"     Inventario {cadete.Inventario.Cantidad}/{Inventario.Capacidad}", ConsoleColor.Gray);
        if (cadete.DanioExtra > 0) Pantalla.Escribir($"     Ataque +{cadete.DanioExtra} preparado", ConsoleColor.Red);
        Console.WriteLine();
        Pantalla.Separador();

        // Mapa (matriz) a la izquierda y panel de misión a la derecha
        List<MensajeBitacora> panel = ConstruirPanelLateral();
        int anchoMapa = 2 + realidadActual.Columnas * 2;

        for (int f = 0; f < realidadActual.Filas; f++)
        {
            Pantalla.Escribir("  ");
            for (int c = 0; c < realidadActual.Columnas; c++)
            {
                DibujarCelda(f, c);
            }

            if (f < panel.Count)
            {
                Pantalla.Escribir("     ");
                Pantalla.EscribirLinea(panel[f].Texto, panel[f].Color);
            }
            else Console.WriteLine();
        }
        // Si el panel tiene más líneas que el mapa, las mostramos debajo.
        for (int i = realidadActual.Filas; i < panel.Count; i++)
        {
            Pantalla.Escribir(new string(' ', anchoMapa + 5));
            Pantalla.EscribirLinea(panel[i].Texto, panel[i].Color);
        }
        Pantalla.Separador();

        // Bitácora: los últimos mensajes
        Pantalla.EscribirLinea(" BITÁCORA", ConsoleColor.Cyan);
        int inicio = Math.Max(0, bitacora.Count - MensajesVisibles);
        for (int i = inicio; i < bitacora.Count; i++)
        {
            Pantalla.Escribir("  » ", ConsoleColor.DarkGray);
            Pantalla.EscribirLinea(Pantalla.Ajustar(bitacora[i].Texto, Pantalla.Ancho - 4).TrimEnd(), bitacora[i].Color);
        }
        for (int i = bitacora.Count - inicio; i < MensajesVisibles; i++) Console.WriteLine();

        Pantalla.Separador();
        Pantalla.EscribirLinea(" W A S D mover · I inventario · F atacar · C IRIS · R realidad · M misión · H ayuda · Q salir",
                               ConsoleColor.DarkGray);
    }

    // Cada celda ocupa 2 caracteres para que el mapa se vea cuadrado.
    private void DibujarCelda(int f, int c)
    {
        if (f == fila && c == columna)
        {
            Pantalla.Escribir("@ ", ConsoleColor.Yellow);
            return;
        }
        if (!realidadActual.EstaDescubierto(f, c))
        {
            Pantalla.Escribir("░░", ConsoleColor.DarkGray);
            return;
        }

        char celda = realidadActual.ObtenerCelda(f, c);
        switch (celda)
        {
            case '#': Pantalla.Escribir("██", ConsoleColor.DarkBlue); break;
            case '.': Pantalla.Escribir("· ", ConsoleColor.DarkGray); break;
            case 'E': Pantalla.Escribir("E ", ConsoleColor.Green); break;
            case '+': Pantalla.Escribir("+ ", ConsoleColor.Yellow); break;
            case 'R': Pantalla.Escribir("R ", ConsoleColor.Cyan); break;
            case 'X': Pantalla.Escribir("X ", ConsoleColor.Red); break;
            case 'A': Pantalla.Escribir("A ", ConsoleColor.Magenta); break;
            case '*': Pantalla.Escribir("* ", ConsoleColor.DarkMagenta); break;
            case 'N': Pantalla.Escribir("N ", ConsoleColor.White); break;
            case 'i': Pantalla.Escribir("i ", ConsoleColor.Blue); break;
            case '~': Pantalla.Escribir("~ ", ConsoleColor.DarkYellow); break;
            default: Pantalla.Escribir("? ", ConsoleColor.Gray); break;
        }
    }

    private List<MensajeBitacora> ConstruirPanelLateral()
    {
        List<MensajeBitacora> panel = new List<MensajeBitacora>();
        panel.Add(new MensajeBitacora($"MISIÓN: {mision.Nombre}  ({mision.CantidadCumplidos}/4)", ConsoleColor.Green));

        for (int i = 0; i < mision.Objetivos.Length; i++)
        {
            bool hecho = mision.EstaCumplido(i);
            string marca = hecho ? "[■] " : "[ ] ";
            panel.Add(new MensajeBitacora(marca + mision.Objetivos[i], hecho ? ConsoleColor.Green : ConsoleColor.Gray));
        }

        panel.Add(new MensajeBitacora("", ConsoleColor.Gray));
        panel.Add(new MensajeBitacora("LEYENDA", ConsoleColor.Cyan));
        panel.Add(new MensajeBitacora("@ Tú   E Extracción   + Objeto", ConsoleColor.DarkGray));
        panel.Add(new MensajeBitacora("R Robot   X Enemigo   A Anomalía", ConsoleColor.DarkGray));
        panel.Add(new MensajeBitacora("N Núcleo   i Datos   ~ Inestable", ConsoleColor.DarkGray));
        panel.Add(new MensajeBitacora("░ Sin explorar   ██ Muro", ConsoleColor.DarkGray));
        return panel;
    }

    // ======================================================
    //  MOVIMIENTO Y CELDAS DEL MAPA
    // ======================================================
    private void Mover(int cambioFila, int cambioColumna)
    {
        int nuevaFila = fila + cambioFila;
        int nuevaColumna = columna + cambioColumna;

        // Control: no se puede salir del mapa.
        if (!realidadActual.EstaDentro(nuevaFila, nuevaColumna) || realidadActual.EsBorde(nuevaFila, nuevaColumna))
        {
            Registrar("Límite de la realidad: no puedes salir del mapa.", ConsoleColor.Red);
            return;
        }
        if (realidadActual.ObtenerCelda(nuevaFila, nuevaColumna) == '#')
        {
            Registrar("Un muro de energía bloquea el paso.", ConsoleColor.DarkGray);
            return;
        }

        // Moverse cuesta 1 de energía. Sin energía, el traje usa la salud.
        if (cadete.Energia > 0)
        {
            cadete.CambiarEnergia(-1);
        }
        else
        {
            cadete.RecibirDanioDirecto(4);
            Registrar("¡Sin energía! El traje consume tu salud para moverte (-4 salud).", ConsoleColor.Red);
        }

        filaAnterior = fila;
        columnaAnterior = columna;
        fila = nuevaFila;
        columna = nuevaColumna;
        turnos++;
        realidadActual.Descubrir(fila, columna);

        ProcesarCelda();
    }

    // Vuelve a la casilla anterior (al escapar o si algo bloquea el paso).
    private void Retroceder()
    {
        fila = filaAnterior;
        columna = columnaAnterior;
    }

    // Decide qué pasa según el símbolo de la casilla donde se paró el cadete.
    private void ProcesarCelda()
    {
        char celda = realidadActual.ObtenerCelda(fila, columna);

        switch (celda)
        {
            case '+':
                if (RecogerObjeto(Objeto.Aleatorio()))
                    realidadActual.CambiarCelda(fila, columna, '.');
                break;

            case 'i':
                LeerRegistro();
                realidadActual.CambiarCelda(fila, columna, '.');
                break;

            case '~':
                cadete.CambiarEstabilidad(-4);
                Registrar("Zona inestable: la realidad se distorsiona a tu alrededor (-4 estabilidad).", ConsoleColor.DarkYellow);
                break;

            case 'A': ActivarAnomalia(); break;
            case 'N': IntentarTomarNucleo(); break;
            case 'R': EncuentroEnMapa(CrearRobot()); break;
            case 'X': EncuentroEnMapa(CrearEnemigo()); break;
            case 'E': LlegarExtraccion(); break;

            case '.':
                if (!realidadActual.FueVisitado(fila, columna))
                {
                    realidadActual.MarcarVisitado(fila, columna);
                    EventoAleatorio();
                }
                break;
        }
    }

    private bool RecogerObjeto(Objeto objeto)
    {
        if (cadete.Inventario.Agregar(objeto))
        {
            Registrar($"OBJETO ENCONTRADO: {objeto.Simbolo} {objeto.Nombre}. Se agregó al inventario.", ConsoleColor.Yellow);
            return true;
        }
        Registrar($"Encontraste {objeto.Nombre}, pero tu inventario está lleno. Libera espacio con [I].", ConsoleColor.DarkYellow);
        return false;
    }

    private void LeerRegistro()
    {
        string texto = registrosPerdidos[Random.Shared.Next(registrosPerdidos.Length)];
        cadete.AgregarXP(10);
        Registrar("INFORMACIÓN: " + texto, ConsoleColor.Blue);
        Registrar("+10 XP por recuperar datos perdidos.", ConsoleColor.Green);
    }

    private void LlegarExtraccion()
    {
        // Con el núcleo en el inventario, la misión termina aquí.
        if (cadete.Inventario.Tiene(TipoObjeto.NucleoEnergetico))
        {
            CompletarMision();
            return;
        }

        if (cadete.Estabilidad < Cadete.Maximo)
        {
            cadete.CambiarEstabilidad(10);
            Registrar("Punto de extracción: el traje se estabiliza (+10 estabilidad). Pulsa R para cambiar de realidad.", ConsoleColor.Green);
        }
        else
        {
            Registrar("Punto de extracción. Pulsa R para cambiar de realidad.", ConsoleColor.Green);
        }
    }

    private void IntentarCambiarRealidad()
    {
        if (realidadActual.ObtenerCelda(fila, columna) != 'E')
        {
            Registrar("Solo puedes cambiar de realidad desde el punto de extracción (E).", ConsoleColor.Red);
            return;
        }
        SeleccionarRealidad(); // si cancela, sigue en la misma realidad
    }

    // ======================================================
    //  EVENTOS ALEATORIOS AL EXPLORAR
    // ======================================================
    private void EventoAleatorio()
    {
        int dado = Random.Shared.Next(100);
        int riesgo = realidadActual.NivelRiesgo * 3; // más riesgo = más peligros

        if (dado < 14)
        {
            RecogerObjeto(Objeto.Aleatorio());
        }
        else if (dado < 22 + riesgo)
        {
            Personaje entidad = Random.Shared.Next(100) < 30 + realidadActual.NivelRiesgo * 20 ? CrearEnemigo() : CrearRobot();
            Encuentro(entidad);
        }
        else if (dado < 30 + riesgo)
        {
            cadete.CambiarEstabilidad(-6);
            Registrar("ANOMALÍA DETECTADA: una distorsión te sacude (-6 estabilidad).", ConsoleColor.Magenta);
        }
        else if (dado < 38 + riesgo)
        {
            int energiaAntes = cadete.Energia;
            cadete.CambiarEnergia(-Random.Shared.Next(5, 13));
            Registrar($"PÉRDIDA DE ENERGÍA: una fuga drena tu traje. Energía: {energiaAntes} → {cadete.Energia}", ConsoleColor.Yellow);
        }
        else if (dado < 44 + riesgo)
        {
            SistemaDaniado();
        }
        else if (dado < 52 + riesgo)
        {
            LeerRegistro();
        }
        else
        {
            Registrar("Zona despejada. No encontraste nada.", ConsoleColor.DarkGray);
        }
    }

    private void SistemaDaniado()
    {
        Pantalla.Destello(ConsoleColor.DarkRed);
        if (cadete.Escudo > 0)
        {
            cadete.CambiarEscudo(-15);
            Registrar($"SISTEMA DAÑADO: un cortocircuito afecta tu escudo (-15). Escudo: {cadete.Escudo}", ConsoleColor.Red);
        }
        else
        {
            cadete.RecibirDanioDirecto(8);
            Registrar($"SISTEMA DAÑADO: sin escudo, la descarga te hiere (-8 salud). Salud: {cadete.Salud}", ConsoleColor.Red);
        }
    }

    // ======================================================
    //  MISIÓN: anomalía, núcleo, extracción
    // ======================================================
    private void ActivarAnomalia()
    {
        Pantalla.Alerta("ANOMALÍA DETECTADA", new[]
        {
            "",
            "  Una grieta brillante se abre frente a ti. El aire vibra.",
            "  Tus sensores registran una señal enorme hacia el sureste.",
            "",
            "  -10 estabilidad      +30 XP",
            "  La ubicación del NÚCLEO (N) aparece en tu mapa.",
            ""
        }, ConsoleColor.Magenta);
        Pantalla.Pausa();

        cadete.CambiarEstabilidad(-10);
        cadete.AgregarXP(30);
        mision.Cumplir(Mision.ZonaAnomalia);
        realidadActual.CambiarCelda(fila, columna, '*');
        realidadActual.RevelarSimbolo('N');

        Registrar("Objetivo cumplido: llegaste a la zona de anomalía.", ConsoleColor.Magenta);
        Iris.Enviar("ANOMALÍA confirmada. El núcleo está al sureste. Cuidado: algo enorme lo protege.");
    }

    private void IntentarTomarNucleo()
    {
        if (!mision.EstaCumplido(Mision.ZonaAnomalia))
        {
            Registrar("Un campo de fuerza protege este lugar. Primero investiga la anomalía (A).", ConsoleColor.Magenta);
            Retroceder();
            return;
        }

        if (!guardianDerrotado)
        {
            Pantalla.Alerta("ALERTA MÁXIMA", new[]
            {
                "",
                "  El suelo tiembla. Una figura gigante se forma con fragmentos de luz.",
                "  EL GUARDIÁN DE LA ANOMALÍA ha despertado.",
                "",
                "  Debes vencerlo para obtener el núcleo.",
                ""
            }, ConsoleColor.Red);
            Pantalla.Pausa();

            ResultadoEncuentro resultado = Encuentro(CrearGuardian());
            if (resultado == ResultadoEncuentro.Derrota) return;
            if (resultado == ResultadoEncuentro.Escape)
            {
                Retroceder();
                return;
            }
            guardianDerrotado = true;
            mision.Cumplir(Mision.Entidad);
        }

        if (!cadete.Inventario.Agregar(new Objeto(TipoObjeto.NucleoEnergetico)))
        {
            Registrar("Inventario lleno: elimina un objeto con [I] para poder llevar el núcleo.", ConsoleColor.Red);
            Retroceder();
            return;
        }

        mision.Cumplir(Mision.Nucleo);
        cadete.AgregarXP(50);
        realidadActual.CambiarCelda(fila, columna, '.');

        Pantalla.Alerta("NÚCLEO ENERGÉTICO OBTENIDO", new[]
        {
            "",
            "  ☼  El núcleo late en tus manos como un pequeño sol.",
            "",
            "  +50 XP",
            "  Ahora regresa al PUNTO DE EXTRACCIÓN (E) para completar la misión.",
            ""
        }, ConsoleColor.Green);
        Pantalla.Pausa();

        Registrar("Objetivo cumplido: núcleo energético en tu inventario.", ConsoleColor.Green);
        Iris.Enviar("¡Tienes el núcleo! Regresa al punto de extracción (E) lo antes posible.");
    }

    private void CompletarMision()
    {
        mision.Cumplir(Mision.Extraccion);
        cadete.AgregarXP(100);
        cadete.Inventario.Agregar(new Objeto(TipoObjeto.ModuloDesconocido));

        Pantalla.Salto(ConsoleColor.Green);
        Pantalla.Alerta("MISIÓN COMPLETADA", new[]
        {
            "",
            "  " + mision.Nombre,
            "",
            "  [■] Llegaste a la zona de anomalía",
            "  [■] Encontraste el núcleo energético",
            "  [■] Sobreviviste al encuentro con la entidad",
            "  [■] Regresaste al punto de extracción",
            "",
            "  RECOMPENSAS:   +100 XP     +1 módulo desconocido",
            "",
            $"  XP total: {cadete.XP}     Turnos: {turnos}     Salud final: {cadete.Salud}",
            "",
            $"  ¡EL CADETE {cadete.Nombre} REGRESÓ CON VIDA!",
            ""
        }, ConsoleColor.Green);
        Pantalla.Fanfarria();

        Iris.Enviar($"MISIÓN COMPLETADA. Excelente trabajo, cadete {cadete.Nombre}. Las realidades están a salvo.");
        Pantalla.Pausa();
        partidaActiva = false;
    }

    private void MostrarMision()
    {
        Pantalla.Limpiar();
        Console.WriteLine();

        string[] lineas = new string[mision.Objetivos.Length + 2];
        for (int i = 0; i < mision.Objetivos.Length; i++)
        {
            string marca = mision.EstaCumplido(i) ? "[■]" : "[ ]";
            lineas[i] = $"  {marca} {mision.Objetivos[i]}";
        }
        lineas[mision.Objetivos.Length] = "";
        lineas[mision.Objetivos.Length + 1] = $"  Progreso: {mision.CantidadCumplidos} de {mision.Objetivos.Length} objetivos";

        Pantalla.Caja("MISIÓN: " + mision.Nombre, lineas, ConsoleColor.Green);
        Pantalla.Pausa();
    }

    private void ConsultarIris()
    {
        string consejo;
        if (!mision.EstaCumplido(Mision.ZonaAnomalia) && realidadActual != realidades[IndiceRealidadMision])
            consejo = "La señal de la anomalía viene de R-404. Ve al punto de extracción (E) y pulsa R.";
        else if (!mision.EstaCumplido(Mision.ZonaAnomalia))
            consejo = "La anomalía (A) está en esta realidad. Explora las zonas ocultas del centro.";
        else if (!mision.EstaCumplido(Mision.Nucleo))
            consejo = "El núcleo (N) ya aparece en tu mapa. Lo protege un Guardián: analízalo antes de atacar.";
        else
            consejo = "¡Ya tienes el núcleo! Vuelve al punto de extracción (E).";

        if (cadete.Salud < 40) consejo += " Tu salud es baja: usa un kit médico.";
        if (cadete.Energia < 30) consejo += " Tu energía es baja: usa una célula.";

        Registrar("IRIS: " + consejo, ConsoleColor.Magenta);
        Iris.Enviar(consejo);
    }

    // ======================================================
    //  ENCUENTROS CON ENTIDADES
    // ======================================================
    private Robot CrearRobot()
    {
        return new Robot("UNIDAD R-" + Random.Shared.Next(100, 1000));
    }

    private Enemigo CrearEnemigo()
    {
        string nombre = nombresEnemigos[Random.Shared.Next(nombresEnemigos.Length)];
        int salud = 40 + realidadActual.NivelRiesgo * 8;
        int danio = 7 + realidadActual.NivelRiesgo * 2;
        return new Enemigo(nombre, salud, danio, false);
    }

    private Enemigo CrearGuardian()
    {
        return new Enemigo("GUARDIÁN DE LA ANOMALÍA", 110, 13, true);
    }

    // Encuentro con una entidad que estaba marcada en el mapa (R o X).
    private void EncuentroEnMapa(Personaje entidad)
    {
        ResultadoEncuentro resultado = Encuentro(entidad);

        if (resultado == ResultadoEncuentro.Victoria || resultado == ResultadoEncuentro.Pacifico)
            realidadActual.CambiarCelda(fila, columna, '.');
        else if (resultado == ResultadoEncuentro.Escape)
            Retroceder();
    }

    private ResultadoEncuentro Encuentro(Personaje entidad)
    {
        bool esRobot = entidad is Robot;
        Pantalla.Alerta("ENTIDAD DETECTADA", new[]
        {
            "",
            "  " + entidad.Nombre,
            esRobot ? "  Tipo: ROBOT · Sistema defensivo en espera" : "  Tipo: ENEMIGO · ¡Hostil! Se inicia un combate",
            ""
        }, esRobot ? ConsoleColor.Cyan : ConsoleColor.Red);
        Pantalla.Esperar(700);
        Iris.Enviar(esRobot
            ? $"Robot detectado: {entidad.Nombre}. No es hostil si no lo atacas. Puedes hackearlo con el analizador."
            : $"ALERTA: entidad hostil {entidad.Nombre}. Prepárate para combatir.");

        List<MensajeBitacora> registroCombate = new List<MensajeBitacora>();

        while (true)
        {
            DibujarEncuentro(entidad, registroCombate);
            int opcion = Pantalla.LeerOpcion(1, 4);
            bool turnoUsado = true;

            switch (opcion)
            {
                case 1:
                    AtacarEntidad(entidad, registroCombate);
                    break;
                case 2:
                    if (IntentarEscapar(entidad, registroCombate))
                    {
                        Registrar($"Escapaste de {entidad.Nombre}.", ConsoleColor.Yellow);
                        return ResultadoEncuentro.Escape;
                    }
                    break;
                case 3:
                    turnoUsado = MenuInventario(true);
                    if (turnoUsado) registroCombate.Add(new MensajeBitacora("Usaste un objeto.", ConsoleColor.Green));
                    break;
                case 4:
                    turnoUsado = AnalizarEntidad(entidad, registroCombate);
                    break;
            }

            // ¿El robot fue hackeado? Termina en paz.
            if (entidad is Robot robot && robot.Hackeado)
            {
                FinalPacifico(robot);
                return ResultadoEncuentro.Pacifico;
            }

            // ¿La entidad cayó?
            if (!entidad.EstaVivo())
            {
                Victoria(entidad);
                return ResultadoEncuentro.Victoria;
            }

            // La entidad responde solo si el cadete gastó su turno.
            if (turnoUsado) ReaccionEntidad(entidad, registroCombate);

            if (!cadete.EstaVivo() || cadete.Estabilidad <= 0)
            {
                DibujarEncuentro(entidad, registroCombate);
                Pantalla.Pausa();
                return ResultadoEncuentro.Derrota;
            }
        }
    }

    private void AtacarEntidad(Personaje entidad, List<MensajeBitacora> registro)
    {
        int danio;
        if (cadete.Energia >= 5)
        {
            cadete.CambiarEnergia(-5);
            danio = cadete.CalcularAtaque();
            registro.Add(new MensajeBitacora("Disparas tu rifle de pulsos (-5 energía).", ConsoleColor.Gray));
        }
        else
        {
            danio = 8;
            registro.Add(new MensajeBitacora("Sin energía para el rifle: atacas cuerpo a cuerpo.", ConsoleColor.DarkYellow));
        }

        if (entidad is Robot robot && !robot.SistemaDefensivoActivo)
            registro.Add(new MensajeBitacora("¡El robot activa su SISTEMA DEFENSIVO! (Defensa 8)", ConsoleColor.Cyan));

        int aplicado = entidad.RecibirDanio(danio);
        Pantalla.Destello(ConsoleColor.DarkYellow);
        registro.Add(new MensajeBitacora($"Causas {aplicado} de daño a {entidad.Nombre}. Salud restante: {entidad.Salud}",
                                         ConsoleColor.Yellow));
    }

    private bool IntentarEscapar(Personaje entidad, List<MensajeBitacora> registro)
    {
        int probabilidad = 50;
        if (entidad is Robot robot) probabilidad = robot.SistemaDefensivoActivo ? 75 : 100;
        else if (entidad is Enemigo enemigo) probabilidad = enemigo.EsJefe ? 40 : 60;

        if (Random.Shared.Next(100) < probabilidad) return true;

        registro.Add(new MensajeBitacora($"No lograste escapar ({probabilidad}% de probabilidad).", ConsoleColor.Red));
        return false;
    }

    // Devuelve true si el análisis gastó el turno.
    private bool AnalizarEntidad(Personaje entidad, List<MensajeBitacora> registro)
    {
        // Control: usar un objeto que no se posee.
        if (!cadete.Inventario.Tiene(TipoObjeto.Analizador))
        {
            registro.Add(new MensajeBitacora("No posees un analizador. No puedes analizar la entidad.", ConsoleColor.Red));
            return false;
        }
        if (cadete.Energia < 5)
        {
            registro.Add(new MensajeBitacora("Energía insuficiente para el analizador (necesitas 5).", ConsoleColor.Red));
            return false;
        }

        cadete.CambiarEnergia(-5);
        registro.Add(new MensajeBitacora("ANÁLISIS: " + entidad.Describir(), ConsoleColor.Cyan));

        if (entidad is Robot robot)
        {
            if (!robot.SistemaDefensivoActivo)
            {
                robot.Hackear();
                registro.Add(new MensajeBitacora("¡Hackeo exitoso! El robot te reconoce como aliado.", ConsoleColor.Green));
            }
            else
            {
                registro.Add(new MensajeBitacora("Su sistema defensivo bloquea el hackeo.", ConsoleColor.DarkCyan));
            }
        }
        else if (entidad is Enemigo enemigo)
        {
            if (!enemigo.DebilidadDetectada)
            {
                enemigo.DetectarDebilidad();
                registro.Add(new MensajeBitacora("Debilidad detectada: tus ataques harán +10 de daño.", ConsoleColor.Green));
            }
            else
            {
                registro.Add(new MensajeBitacora("Ya conoces su debilidad.", ConsoleColor.DarkGray));
            }
        }
        return true;
    }

    // Lo que hace la entidad después del turno del cadete.
    private void ReaccionEntidad(Personaje entidad, List<MensajeBitacora> registro)
    {
        if (entidad is Robot robot)
        {
            if (!robot.SistemaDefensivoActivo)
            {
                registro.Add(new MensajeBitacora("El robot te observa sin moverse. No es hostil... por ahora.", ConsoleColor.DarkCyan));
                return;
            }
            int descarga = robot.Descarga();
            cadete.RecibirDanio(descarga);
            Pantalla.Destello(ConsoleColor.DarkCyan);
            registro.Add(new MensajeBitacora($"El robot lanza una descarga de {descarga}. Escudo {cadete.Escudo} · Salud {cadete.Salud}",
                                             ConsoleColor.Red));
        }
        else if (entidad is Enemigo enemigo)
        {
            int danio = enemigo.Atacar();
            cadete.RecibirDanio(danio);
            Pantalla.Destello(ConsoleColor.DarkRed);
            registro.Add(new MensajeBitacora($"{enemigo.Nombre} ataca: {danio} de daño. Escudo {cadete.Escudo} · Salud {cadete.Salud}",
                                             ConsoleColor.Red));

            if (enemigo.EsJefe && Random.Shared.Next(100) < 30)
            {
                cadete.CambiarEstabilidad(-5);
                registro.Add(new MensajeBitacora("El Guardián distorsiona la realidad: -5 estabilidad.", ConsoleColor.Magenta));
            }
        }
    }

    private void Victoria(Personaje entidad)
    {
        int xp = 25;
        int probabilidadObjeto = 50;
        if (entidad is Enemigo enemigo)
        {
            xp = enemigo.EsJefe ? 80 : 40;
            probabilidadObjeto = enemigo.EsJefe ? 100 : 60;
        }
        cadete.AgregarXP(xp);

        string botin = "Ninguno";
        if (Random.Shared.Next(100) < probabilidadObjeto)
        {
            Objeto objeto = Objeto.Aleatorio();
            botin = cadete.Inventario.Agregar(objeto) ? objeto.Nombre : objeto.Nombre + " (inventario lleno, se perdió)";
        }

        Pantalla.Alerta("ENTIDAD NEUTRALIZADA", new[]
        {
            "",
            $"  {entidad.Nombre} fue derrotado.",
            "",
            $"  +{xp} XP          Objeto obtenido: {botin}",
            ""
        }, ConsoleColor.Green);
        Pantalla.Pausa();

        Registrar($"Derrotaste a {entidad.Nombre}. +{xp} XP.", ConsoleColor.Green);
    }

    private void FinalPacifico(Robot robot)
    {
        cadete.AgregarXP(20);
        Objeto regalo = Objeto.Aleatorio();
        string texto = cadete.Inventario.Agregar(regalo) ? regalo.Nombre : "nada (inventario lleno)";

        Pantalla.Alerta("ROBOT HACKEADO", new[]
        {
            "",
            $"  {robot.Nombre} ahora te obedece. Abre su compartimento de carga.",
            "",
            $"  +20 XP          Te entrega: {texto}",
            ""
        }, ConsoleColor.Cyan);
        Pantalla.Pausa();

        Registrar($"Hackeaste a {robot.Nombre}. +20 XP.", ConsoleColor.Cyan);
    }

    // ---------------- DIBUJO DEL ENCUENTRO ----------------
    private void DibujarEncuentro(Personaje entidad, List<MensajeBitacora> registro)
    {
        Pantalla.Limpiar();
        ConsoleColor color = ConsoleColor.Red;
        string estado = "";

        if (entidad is Robot robot)
        {
            color = ConsoleColor.Cyan;
            estado = robot.SistemaDefensivoActivo ? "Sistema defensivo: ACTIVO" : "Sistema defensivo: en espera";
        }
        else if (entidad is Enemigo enemigo)
        {
            color = enemigo.EsJefe ? ConsoleColor.Magenta : ConsoleColor.Red;
            estado = enemigo.DebilidadDetectada ? "Debilidad: DETECTADA (+10 daño)" : "Debilidad: desconocida";
        }

        Pantalla.EscribirLinea($" ENCUENTRO · {realidadActual.Codigo} {realidadActual.Nombre}", ConsoleColor.DarkGray);
        Pantalla.Separador(color);

        foreach (string linea in ArteDe(entidad))
        {
            Pantalla.Centrar(linea, color);
        }
        Console.WriteLine();

        Pantalla.Escribir("  " + Pantalla.Ajustar(entidad.Nombre, 28), color);
        Pantalla.Barra("SALUD", entidad.Salud, entidad.SaludMaxima, ConsoleColor.Green, 24);
        Console.WriteLine();
        Pantalla.EscribirLinea("  " + estado, ConsoleColor.DarkGray);
        Pantalla.Separador();

        Pantalla.EscribirLinea($"  CADETE {cadete.Nombre}", ConsoleColor.Yellow);
        Pantalla.Escribir("  ");
        Pantalla.Barra("SALUD", cadete.Salud, Cadete.Maximo, ConsoleColor.Green);
        Pantalla.Escribir("     ");
        Pantalla.Barra("ENERGÍA", cadete.Energia, Cadete.Maximo, ConsoleColor.Cyan);
        Console.WriteLine();
        Pantalla.Escribir("  ");
        Pantalla.Barra("ESTABILIDAD", cadete.Estabilidad, Cadete.Maximo, ConsoleColor.Magenta);
        Pantalla.Escribir("     ");
        Pantalla.Barra("ESCUDO", cadete.Escudo, Cadete.Maximo, ConsoleColor.Blue);
        Console.WriteLine();
        Pantalla.Separador();

        int inicio = Math.Max(0, registro.Count - 6);
        for (int i = inicio; i < registro.Count; i++)
        {
            Pantalla.Escribir("  » ", ConsoleColor.DarkGray);
            Pantalla.EscribirLinea(Pantalla.Ajustar(registro[i].Texto, Pantalla.Ancho - 4).TrimEnd(), registro[i].Color);
        }
        for (int i = registro.Count - inicio; i < 6; i++) Console.WriteLine();

        Pantalla.Separador();
        Pantalla.EscribirLinea("   [1] Atacar     [2] Escapar     [3] Usar objeto     [4] Analizar", ConsoleColor.Yellow);
    }

    // Dibujos ASCII de cada tipo de entidad (arreglos de texto).
    private string[] ArteDe(Personaje entidad)
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
                "═╣  R O B O T ╠═",
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
                "▀    ▀▀ GUARDIÁN ▀▀   ▀"
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

    // ======================================================
    //  INVENTARIO: consultar → utilizar → eliminar
    // ======================================================
    // Devuelve true si se usó un objeto (en combate eso gasta el turno).
    private bool MenuInventario(bool enCombate)
    {
        while (true)
        {
            Pantalla.Limpiar();
            Console.WriteLine();
            Pantalla.Caja($"INVENTARIO  ({cadete.Inventario.Cantidad}/{Inventario.Capacidad})", new[]
            {
                $"Salud {cadete.Salud}   Energía {cadete.Energia}   Escudo {cadete.Escudo}   Estabilidad {cadete.Estabilidad}"
            }, ConsoleColor.Yellow);
            Console.WriteLine();

            for (int i = 0; i < Inventario.Capacidad; i++)
            {
                Objeto objeto = cadete.Inventario.Obtener(i);
                Pantalla.Escribir($"   [{i + 1}] ", ConsoleColor.Yellow);
                if (objeto == null)
                {
                    Pantalla.EscribirLinea("— ranura vacía —", ConsoleColor.DarkGray);
                }
                else
                {
                    Pantalla.Escribir(objeto.Simbolo + " " + objeto.Nombre.PadRight(22), objeto.Color);
                    Pantalla.EscribirLinea(objeto.Descripcion, ConsoleColor.Gray);
                }
            }
            Console.WriteLine();
            Pantalla.EscribirLinea("   [0] Volver", ConsoleColor.Yellow);
            Console.WriteLine();

            int opcion = Pantalla.LeerOpcion(0, Inventario.Capacidad);
            if (opcion == 0) return false;

            int indice = opcion - 1;
            Objeto elegido = cadete.Inventario.Obtener(indice);

            // Control: objeto inexistente (ranura vacía).
            if (elegido == null)
            {
                Pantalla.EscribirLinea("   × No existe ningún objeto en esa ranura.", ConsoleColor.Red);
                Pantalla.Pausa();
                continue;
            }

            Console.WriteLine();
            Pantalla.EscribirLinea($"   Seleccionaste: {elegido.Simbolo} {elegido.Nombre}", elegido.Color);
            Pantalla.EscribirLinea("   [1] Utilizar     [2] Eliminar     [0] Volver", ConsoleColor.Yellow);
            int accion = Pantalla.LeerOpcion(0, 2);

            if (accion == 1)
            {
                bool usado = UsarObjeto(indice);
                Pantalla.Pausa();
                if (usado && enCombate) return true;
            }
            else if (accion == 2)
            {
                EliminarObjeto(indice);
                Pantalla.Pausa();
            }
        }
    }

    // Aplica el efecto del objeto. Devuelve true si se gastó.
    private bool UsarObjeto(int indice)
    {
        Objeto objeto = cadete.Inventario.Obtener(indice);
        if (objeto == null)
        {
            Pantalla.EscribirLinea("   × Ese objeto no existe.", ConsoleColor.Red);
            return false;
        }

        string mensaje;
        switch (objeto.Tipo)
        {
            case TipoObjeto.CelulaEnergia:
                if (cadete.Energia >= Cadete.Maximo) { Pantalla.EscribirLinea("   La energía ya está al máximo. No se gastó la célula.", ConsoleColor.DarkYellow); return false; }
                int energiaAntes = cadete.Energia;
                cadete.CambiarEnergia(25);
                mensaje = $"Utilizar célula de energía → Energía: {energiaAntes} → {cadete.Energia}";
                break;

            case TipoObjeto.Escudo:
                if (cadete.Escudo >= Cadete.Maximo) { Pantalla.EscribirLinea("   El escudo ya está al máximo.", ConsoleColor.DarkYellow); return false; }
                int escudoAntes = cadete.Escudo;
                cadete.CambiarEscudo(30);
                mensaje = $"Escudo activado → Escudo: {escudoAntes} → {cadete.Escudo}";
                break;

            case TipoObjeto.KitMedico:
                if (cadete.Salud >= Cadete.Maximo) { Pantalla.EscribirLinea("   Tu salud ya está al máximo.", ConsoleColor.DarkYellow); return false; }
                int saludAntes = cadete.Salud;
                cadete.Curar(30);
                mensaje = $"Kit médico aplicado → Salud: {saludAntes} → {cadete.Salud}";
                break;

            case TipoObjeto.ModuloAtaque:
                cadete.DanioExtra += 15;
                mensaje = "Módulo de ataque instalado → tu próximo ataque hará +15 de daño";
                break;

            case TipoObjeto.ModuloDesconocido:
                cadete.Curar(20);
                cadete.CambiarEnergia(20);
                cadete.CambiarEstabilidad(20);
                mensaje = "El módulo desconocido brilla → salud, energía y estabilidad +20";
                break;

            case TipoObjeto.Analizador:
                Pantalla.EscribirLinea("   El analizador se usa con la opción [4] ANALIZAR durante un encuentro.", ConsoleColor.DarkYellow);
                return false;

            default: // núcleo energético
                Pantalla.EscribirLinea("   El núcleo no se usa: llévalo al punto de extracción (E).", ConsoleColor.DarkYellow);
                return false;
        }

        cadete.Inventario.Eliminar(indice);
        Pantalla.EscribirLinea("   " + mensaje, ConsoleColor.Green);
        Registrar(mensaje, ConsoleColor.Green);
        return true;
    }

    private void EliminarObjeto(int indice)
    {
        Objeto objeto = cadete.Inventario.Obtener(indice);
        if (objeto == null) return;

        if (objeto.Tipo == TipoObjeto.NucleoEnergetico)
        {
            Pantalla.EscribirLinea("   × No puedes abandonar el núcleo: es el objetivo de la misión.", ConsoleColor.Red);
            return;
        }

        if (Pantalla.PreguntarSiNo($"¿Seguro que quieres eliminar {objeto.Nombre}?"))
        {
            cadete.Inventario.Eliminar(indice);
            Pantalla.EscribirLinea($"   {objeto.Nombre} fue eliminado del inventario.", ConsoleColor.DarkYellow);
            Registrar($"Eliminaste {objeto.Nombre} del inventario.", ConsoleColor.DarkYellow);
        }
    }

    // ======================================================
    //  ESTADO DEL CADETE: alertas y derrota
    // ======================================================
    private void ComprobarEstado()
    {
        if (cadete.Estabilidad <= 30 && !alertaEstabilidadEnviada)
        {
            alertaEstabilidadEnviada = true;
            Registrar("ALERTA IRIS: estabilidad crítica. Ve a un punto de extracción (E) para recuperarte.", ConsoleColor.Red);
            Iris.Enviar("ALERTA: tu estabilidad es crítica. Busca un punto de extracción.");
        }
        if (cadete.Estabilidad > 30) alertaEstabilidadEnviada = false;

        if (!cadete.EstaVivo())
            MisionFallida("Tu salud llegó a cero.");
        else if (cadete.Estabilidad <= 0)
            MisionFallida("Tu estabilidad colapsó y quedaste atrapado entre realidades.");
    }

    private void MisionFallida(string motivo)
    {
        partidaActiva = false;
        Pantalla.Destello(ConsoleColor.DarkRed);
        Pantalla.Alerta("MISIÓN FALLIDA", new[]
        {
            "",
            "  Motivo: " + motivo,
            "",
            $"  Objetivos cumplidos: {mision.CantidadCumplidos} de {mision.Objetivos.Length}",
            $"  XP obtenida: {cadete.XP}     Turnos: {turnos}",
            "",
            "  NEXUS no pudo completar la misión. Vuelve a intentarlo, cadete.",
            ""
        }, ConsoleColor.Red);
        Iris.Enviar("ALERTA: señal del cadete perdida. MISIÓN FALLIDA.");
        Pantalla.Pausa();
    }

    // ======================================================
    //  PANTALLAS DE INFORMACIÓN Y SALIDA
    // ======================================================
    private void MostrarComoJugar()
    {
        Pantalla.Limpiar();
        Console.WriteLine();
        Pantalla.Caja("CÓMO JUGAR", new[]
        {
            "OBJETIVO: entra a R-404, toca la anomalía (A), vence al Guardián en el núcleo (N)",
            "y vuelve al punto de extracción (E) con el núcleo.",
            "",
            "EXPLORACIÓN:  W A S D o flechas = moverse (cada paso cuesta 1 de energía)",
            "              I = inventario   C = pedir consejo a IRIS   M = ver la misión",
            "              R = cambiar de realidad (solo parado sobre E)   Q = abandonar",
            "",
            "ENCUENTROS:   [1] Atacar (-5 energía)   [2] Escapar   [3] Usar objeto   [4] Analizar",
            "  ROBOT:   no ataca si no lo atacas. Analízalo ANTES de atacar y lo hackeas.",
            "  ENEMIGO: siempre ataca. Analizarlo revela su debilidad (+10 daño).",
            "",
            "ESTADO:  si la SALUD o la ESTABILIDAD llegan a 0, la misión falla.",
            "         El punto de extracción (E) recupera +10 de estabilidad.",
            "",
            "CONSEJO: abre la consola IRIS (opción 3 del menú) y ponla al lado del juego."
        }, ConsoleColor.Cyan);
        Pantalla.Pausa();
    }

    private void MostrarCreditos()
    {
        Pantalla.Limpiar();
        Console.WriteLine();
        Pantalla.Caja("CRÉDITOS", new[]
        {
            "",
            "NEXUS v1.0 · Sistema de Exploración Interdimensional",
            "Proyecto de programación · Ingeniería de Sistemas · Unifranz",
            "",
            "Desarrollado en C# (.NET) como aplicación de consola.",
            ""
        }, ConsoleColor.Magenta);
        Pantalla.Pausa();
    }

    // Salida controlada: guarda, cierra IRIS y se despide.
    private void SalidaControlada()
    {
        Pantalla.Limpiar();
        Pantalla.DibujarLogo(false);
        Pantalla.BarraCarga("Guardando bitácora de exploración", ConsoleColor.Cyan);
        Pantalla.BarraCarga("Cerrando canal con IRIS", ConsoleColor.Magenta);
        Iris.CerrarConsola();
        Pantalla.BarraCarga("Apagando reactores del traje", ConsoleColor.Yellow);
        Console.WriteLine();
        Pantalla.Maquina("   Sistema NEXUS desconectado. Hasta la próxima, cadete.", ConsoleColor.Green, 25);
        Pantalla.Esperar(900);
        Pantalla.MostrarCursor();
        Console.ResetColor();
    }
}
