using NEXUS.Models;
using System.Text.Json;

namespace NEXUS.Services;

// De dónde salió la entidad del encuentro actual.
public enum OrigenEncuentro
{
    Aleatorio,  // apareció al explorar una zona nueva
    Mapa,       // estaba marcada en el mapa (R o X)
    Guardian    // protege el núcleo (N)
}

// ============================================================
//  MOTOR DEL JUEGO: TODA la lógica de NEXUS, sin nada visual.
//
//  La ventana (FormPrincipal) llama a sus métodos: Mover, Atacar,
//  UsarObjeto... y el motor le avisa lo que pasa con EVENTOS:
//   Mensaje          → una línea nueva para la bitácora
//   Alerta           → una ventana emergente grande
//   Destello         → la pantalla parpadea (daño)
//   SaltoRealidad    → animación al entrar a una realidad
//   PartidaTerminada → ganaste (true) o perdiste (false)
// ============================================================
public class MotorJuego
{
    public string ExportarPartidaJson()
    {
        if (!PartidaActiva || Cadete == null) throw new InvalidOperationException("No hay una partida activa que guardar.");
        return JsonSerializer.Serialize(new EstadoPartidaNexus(Cadete.Nombre, Cadete.Salud, Cadete.Energia,
            Cadete.Estabilidad, Cadete.Escudo, Cadete.XP, Cadete.DanioExtra, Cadete.Inventario.Exportar(),
            Mision.ExportarEstado(), RealidadActual?.Codigo, Fila, Columna, Turnos,
            Realidades.Select(r => r.ExportarEstado()).ToArray()));
    }

    public void ImportarPartidaJson(string json)
    {
        EstadoPartidaNexus estado = JsonSerializer.Deserialize<EstadoPartidaNexus>(json)
            ?? throw new InvalidDataException("El archivo de partida está vacío o dañado.");
        if (string.IsNullOrWhiteSpace(estado.Nombre) || estado.Nombre.Length > 14 || estado.Turnos < 0 || estado.Realidades.Length != 3)
            throw new InvalidDataException("La partida contiene datos incompatibles.");
        NuevaPartida(estado.Nombre);
        Cadete.RestaurarEstado(estado.Salud, estado.Energia, estado.Estabilidad, estado.Escudo, estado.Xp,
            estado.DanioExtra, estado.Inventario);
        Mision.ImportarEstado(estado.Objetivos);
        for (int i=0; i<Realidades.Length; i++) Realidades[i].ImportarEstado(estado.Realidades[i]);
        Turnos = estado.Turnos;
        RealidadActual = estado.CodigoRealidad == null ? null : Realidades.SingleOrDefault(r => r.Codigo == estado.CodigoRealidad);
        if (estado.CodigoRealidad != null && RealidadActual == null) throw new InvalidDataException("La realidad guardada no existe.");
        if (RealidadActual != null && !RealidadActual.EstaDentro(estado.Fila, estado.Columna)) throw new InvalidDataException("La posición guardada no es válida.");
        Fila = estado.Fila; Columna = estado.Columna; EntidadActual = null; PartidaActiva = true;
        if (RealidadActual != null) { RealidadActual.Descubrir(Fila, Columna); RealidadActual.MarcarVisitado(Fila, Columna); }
    }

    private sealed record EstadoPartidaNexus(string Nombre, int Salud, int Energia, int Estabilidad, int Escudo,
        int Xp, int DanioExtra, TipoObjeto[] Inventario, bool[] Objetivos, string CodigoRealidad,
        int Fila, int Columna, int Turnos, EstadoRealidad[] Realidades);
    public const int IndiceRealidadMision = 2; // R-404 es la tercera del arreglo

    // ---------------- ESTADO (la ventana lo lee) ----------------
    public Cadete Cadete { get; private set; }
    public Mision Mision { get; private set; }
    public Realidad[] Realidades { get; private set; }
    public Realidad RealidadActual { get; private set; }
    public int Fila { get; private set; }
    public int Columna { get; private set; }
    public int Turnos { get; private set; }
    public bool PartidaActiva { get; private set; }
    public Personaje EntidadActual { get; private set; }
    public bool EnCombate => EntidadActual != null;
    public List<MensajeBitacora> RegistroCombate { get; } = new List<MensajeBitacora>();

    private int filaAnterior, columnaAnterior;
    private bool guardianDerrotado;
    private bool alertaEstabilidadEnviada;
    private OrigenEncuentro origenEncuentro;

    // ---------------- EVENTOS ----------------
    public event Action<MensajeBitacora> Mensaje;
    public event Action<string, string[], ConsoleColor> Alerta;
    public event Action<ConsoleColor> Destello;
    public event Action<Realidad> SaltoRealidad;
    public event Action<bool> PartidaTerminada;

    // Arreglos de textos
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
    //  INICIO DE PARTIDA
    // ======================================================
    public void NuevaPartida(string nombre)
    {
        Cadete = new Cadete(nombre.Trim().ToUpper());
        Cadete.Inventario.Agregar(new Objeto(TipoObjeto.CelulaEnergia));
        Cadete.Inventario.Agregar(new Objeto(TipoObjeto.Escudo));
        Cadete.Inventario.Agregar(new Objeto(TipoObjeto.Analizador));
        Cadete.Inventario.Agregar(new Objeto(TipoObjeto.KitMedico));

        Mision = new Mision();
        Realidades = CrearRealidades();
        RealidadActual = null;
        EntidadActual = null;
        RegistroCombate.Clear();
        Turnos = 0;
        guardianDerrotado = false;
        alertaEstabilidadEnviada = false;
        PartidaActiva = true;

        Iris.ReiniciarCanal();
        Iris.Enviar($"Conexión establecida con el cadete {Cadete.Nombre}. Te acompañaré en la misión.");
    }

    // Los mapas se escriben como texto y se convierten en matrices (ver Realidad.cs).
    // Todas las filas de un mapa deben medir lo mismo (15).
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

    public void EntrarRealidad(int indice)
    {
        // Control: índice fuera del arreglo o en pleno combate.
        if (!PartidaActiva || EnCombate || indice < 0 || indice >= Realidades.Length) return;

        Realidad realidad = Realidades[indice];
        RealidadActual = realidad;
        Fila = realidad.FilaInicio;
        Columna = realidad.ColumnaInicio;
        filaAnterior = Fila;
        columnaAnterior = Columna;
        realidad.Descubrir(Fila, Columna);
        realidad.MarcarVisitado(Fila, Columna);

        Registrar($"Salto completado. Ingresaste a {realidad.Codigo} | {realidad.Nombre}.", realidad.Color);
        Iris.Enviar($"Salto completado: {realidad.Codigo} {realidad.Nombre}. {realidad.Descripcion}");
        SaltoRealidad?.Invoke(realidad);
    }

    // ======================================================
    //  MOVIMIENTO
    // ======================================================
    public void Mover(int cambioFila, int cambioColumna)
    {
        if (!PartidaActiva || EnCombate || RealidadActual == null) return;

        int nuevaFila = Fila + cambioFila;
        int nuevaColumna = Columna + cambioColumna;

        // Control: no se puede salir del mapa.
        if (!RealidadActual.EstaDentro(nuevaFila, nuevaColumna) || RealidadActual.EsBorde(nuevaFila, nuevaColumna))
        {
            Registrar("Límite de la realidad: no puedes salir del mapa.", ConsoleColor.Red);
            return;
        }
        if (RealidadActual.ObtenerCelda(nuevaFila, nuevaColumna) == '#')
        {
            Registrar("Un muro de energía bloquea el paso.", ConsoleColor.DarkGray);
            return;
        }

        // Cada paso cuesta 1 de energía. Sin energía, el traje usa la salud.
        if (Cadete.Energia > 0)
        {
            Cadete.CambiarEnergia(-1);
        }
        else
        {
            Cadete.RecibirDanioDirecto(4);
            Registrar("¡Sin energía! El traje consume tu salud para moverte (-4 salud).", ConsoleColor.Red);
        }

        filaAnterior = Fila;
        columnaAnterior = Columna;
        Fila = nuevaFila;
        Columna = nuevaColumna;
        Turnos++;
        RealidadActual.Descubrir(Fila, Columna);

        ProcesarCelda();
        ComprobarEstado();
    }

    private void Retroceder()
    {
        Fila = filaAnterior;
        Columna = columnaAnterior;
    }

    // Decide qué pasa según el símbolo de la casilla.
    private void ProcesarCelda()
    {
        char celda = RealidadActual.ObtenerCelda(Fila, Columna);

        switch (celda)
        {
            case '+':
                if (RecogerObjeto(Objeto.Aleatorio()))
                    RealidadActual.CambiarCelda(Fila, Columna, '.');
                break;

            case 'i':
                LeerRegistro();
                RealidadActual.CambiarCelda(Fila, Columna, '.');
                break;

            case '~':
                Cadete.CambiarEstabilidad(-4);
                Registrar("Zona inestable: la realidad se distorsiona (-4 estabilidad).", ConsoleColor.DarkYellow);
                break;

            case 'A': ActivarAnomalia(); break;
            case 'N': IntentarTomarNucleo(); break;
            case 'R': IniciarEncuentro(CrearRobot(), OrigenEncuentro.Mapa); break;
            case 'X': IniciarEncuentro(CrearEnemigo(), OrigenEncuentro.Mapa); break;
            case 'E': LlegarExtraccion(); break;

            case '.':
                if (!RealidadActual.FueVisitado(Fila, Columna))
                {
                    RealidadActual.MarcarVisitado(Fila, Columna);
                    EventoAleatorio();
                }
                break;
        }
    }

    // ======================================================
    //  EVENTOS DE EXPLORACIÓN
    // ======================================================
    private void EventoAleatorio()
    {
        int dado = Random.Shared.Next(100);
        int riesgo = RealidadActual.NivelRiesgo * 3; // más riesgo = más peligros

        if (dado < 14)
        {
            RecogerObjeto(Objeto.Aleatorio());
        }
        else if (dado < 22 + riesgo)
        {
            bool esEnemigo = Random.Shared.Next(100) < 30 + RealidadActual.NivelRiesgo * 20;
            if (esEnemigo) IniciarEncuentro(CrearEnemigo(), OrigenEncuentro.Aleatorio);
            else IniciarEncuentro(CrearRobot(), OrigenEncuentro.Aleatorio);
        }
        else if (dado < 30 + riesgo)
        {
            Cadete.CambiarEstabilidad(-6);
            Registrar("⚡ ANOMALÍA DETECTADA: una distorsión te sacude (-6 estabilidad).", ConsoleColor.Magenta);
        }
        else if (dado < 38 + riesgo)
        {
            int energiaAntes = Cadete.Energia;
            Cadete.CambiarEnergia(-Random.Shared.Next(5, 13));
            Registrar($"PÉRDIDA DE ENERGÍA: una fuga drena tu traje. Energía: {energiaAntes} → {Cadete.Energia}", ConsoleColor.Yellow);
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

    private bool RecogerObjeto(Objeto objeto)
    {
        if (Cadete.Inventario.Agregar(objeto))
        {
            Registrar($"OBJETO ENCONTRADO: {objeto.Simbolo} {objeto.Nombre}. Se agregó al inventario.", ConsoleColor.Yellow);
            return true;
        }
        Registrar($"Encontraste {objeto.Nombre}, pero tu inventario está lleno. Libera espacio.", ConsoleColor.DarkYellow);
        return false;
    }

    private void LeerRegistro()
    {
        string texto = registrosPerdidos[Random.Shared.Next(registrosPerdidos.Length)];
        Cadete.AgregarXP(10);
        Registrar("INFORMACIÓN: " + texto + "  (+10 XP)", ConsoleColor.Blue);
    }

    private void SistemaDaniado()
    {
        Destello?.Invoke(ConsoleColor.DarkRed);
        if (Cadete.Escudo > 0)
        {
            Cadete.CambiarEscudo(-15);
            Registrar($"SISTEMA DAÑADO: un cortocircuito afecta tu escudo (-15). Escudo: {Cadete.Escudo}", ConsoleColor.Red);
        }
        else
        {
            Cadete.RecibirDanioDirecto(8);
            Registrar($"SISTEMA DAÑADO: sin escudo, la descarga te hiere (-8 salud). Salud: {Cadete.Salud}", ConsoleColor.Red);
        }
    }

    private void LlegarExtraccion()
    {
        if (Cadete.Inventario.Tiene(TipoObjeto.NucleoEnergetico))
        {
            CompletarMision();
            return;
        }

        if (Cadete.Estabilidad < Cadete.Maximo)
        {
            Cadete.CambiarEstabilidad(10);
            Registrar("Punto de extracción: el traje se estabiliza (+10 estabilidad). Aquí puedes cambiar de realidad.", ConsoleColor.Green);
        }
        else
        {
            Registrar("Punto de extracción. Aquí puedes cambiar de realidad.", ConsoleColor.Green);
        }
    }

    // Devuelve true si el cadete está parado en el punto de extracción.
    public bool IntentarCambiarRealidad()
    {
        if (!PartidaActiva || EnCombate) return false;
        if (RealidadActual.ObtenerCelda(Fila, Columna) != 'E')
        {
            Registrar("Solo puedes cambiar de realidad desde el punto de extracción (E).", ConsoleColor.Red);
            return false;
        }
        return true;
    }

    // ======================================================
    //  MISIÓN: anomalía → guardián → núcleo → extracción
    // ======================================================
    private void ActivarAnomalia()
    {
        Cadete.CambiarEstabilidad(-10);
        Cadete.AgregarXP(30);
        Mision.Cumplir(Mision.ZonaAnomalia);
        RealidadActual.CambiarCelda(Fila, Columna, '*');
        RealidadActual.RevelarSimbolo('N');

        Destello?.Invoke(ConsoleColor.Magenta);
        Alerta?.Invoke("⚡ ANOMALÍA DETECTADA", new[]
        {
            "Una grieta brillante se abre frente a ti. El aire vibra.",
            "Tus sensores registran una señal enorme hacia el sureste.",
            "",
            "-10 estabilidad      +30 XP",
            "La ubicación del NÚCLEO (N) aparece en tu mapa."
        }, ConsoleColor.Magenta);

        Registrar("Objetivo cumplido: llegaste a la zona de anomalía.", ConsoleColor.Magenta);
        Iris.Enviar("ANOMALÍA confirmada. El núcleo está al sureste. Cuidado: algo enorme lo protege.");
    }

    private void IntentarTomarNucleo()
    {
        if (!Mision.EstaCumplido(Mision.ZonaAnomalia))
        {
            Registrar("Un campo de fuerza protege este lugar. Primero investiga la anomalía (A).", ConsoleColor.Magenta);
            Retroceder();
            return;
        }

        if (!guardianDerrotado)
        {
            Alerta?.Invoke("🚨 ALERTA MÁXIMA", new[]
            {
                "El suelo tiembla. Una figura gigante se forma con fragmentos de luz.",
                "EL GUARDIÁN DE LA ANOMALÍA ha despertado.",
                "",
                "Debes vencerlo para obtener el núcleo."
            }, ConsoleColor.Red);
            IniciarEncuentro(CrearGuardian(), OrigenEncuentro.Guardian);
            return;
        }

        TomarNucleo();
    }

    private void TomarNucleo()
    {
        if (!Cadete.Inventario.Agregar(new Objeto(TipoObjeto.NucleoEnergetico)))
        {
            Registrar("Inventario lleno: elimina un objeto y vuelve a pisar el núcleo (N).", ConsoleColor.Red);
            Retroceder();
            return;
        }

        Mision.Cumplir(Mision.Nucleo);
        Cadete.AgregarXP(50);
        RealidadActual.CambiarCelda(Fila, Columna, '.');

        Alerta?.Invoke("☼ NÚCLEO ENERGÉTICO OBTENIDO", new[]
        {
            "El núcleo late en tus manos como un pequeño sol.",
            "",
            "+50 XP",
            "Ahora regresa al PUNTO DE EXTRACCIÓN (E) para completar la misión."
        }, ConsoleColor.Green);

        Registrar("Objetivo cumplido: núcleo energético en tu inventario.", ConsoleColor.Green);
        Iris.Enviar("¡Tienes el núcleo! Regresa al punto de extracción (E) lo antes posible.");
    }

    private void CompletarMision()
    {
        Mision.Cumplir(Mision.Extraccion);
        Cadete.AgregarXP(100);
        Cadete.Inventario.Agregar(new Objeto(TipoObjeto.ModuloDesconocido));
        PartidaActiva = false;

        Alerta?.Invoke("🟢 MISIÓN COMPLETADA", new[]
        {
            Mision.Nombre,
            "",
            "■ Llegaste a la zona de anomalía",
            "■ Encontraste el núcleo energético",
            "■ Sobreviviste al encuentro con la entidad",
            "■ Regresaste al punto de extracción",
            "",
            "RECOMPENSAS:   +100 XP     +1 módulo desconocido",
            $"XP total: {Cadete.XP}     Turnos: {Turnos}     Salud final: {Cadete.Salud}",
            "",
            $"¡EL CADETE {Cadete.Nombre} REGRESÓ CON VIDA!"
        }, ConsoleColor.Green);

        Iris.Enviar($"MISIÓN COMPLETADA. Excelente trabajo, cadete {Cadete.Nombre}. Las realidades están a salvo.");
        PartidaTerminada?.Invoke(true);
    }

    public void ConsultarIris()
    {
        if (!PartidaActiva) return;

        string consejo;
        if (!Mision.EstaCumplido(Mision.ZonaAnomalia) && RealidadActual != Realidades[IndiceRealidadMision])
            consejo = "La señal de la anomalía viene de R-404. Ve al punto de extracción (E) y cambia de realidad.";
        else if (!Mision.EstaCumplido(Mision.ZonaAnomalia))
            consejo = "La anomalía (A) está en esta realidad. Explora las zonas ocultas del centro.";
        else if (!Mision.EstaCumplido(Mision.Nucleo))
            consejo = "El núcleo (N) ya aparece en tu mapa. Lo protege un Guardián: analízalo antes de atacar.";
        else
            consejo = "¡Ya tienes el núcleo! Vuelve al punto de extracción (E).";

        if (Cadete.Salud < 40) consejo += " Tu salud es baja: usa un kit médico.";
        if (Cadete.Energia < 30) consejo += " Tu energía es baja: usa una célula.";

        Registrar("IRIS: " + consejo, ConsoleColor.Magenta);
        Iris.Enviar(consejo);
    }

    // ======================================================
    //  ENCUENTROS
    // ======================================================
    private Robot CrearRobot() => new Robot("UNIDAD R-" + Random.Shared.Next(100, 1000));

    private Enemigo CrearEnemigo()
    {
        string nombre = nombresEnemigos[Random.Shared.Next(nombresEnemigos.Length)];
        int salud = 40 + RealidadActual.NivelRiesgo * 8;
        int danio = 7 + RealidadActual.NivelRiesgo * 2;
        return new Enemigo(nombre, salud, danio, false);
    }

    private Enemigo CrearGuardian() => new Enemigo("GUARDIÁN DE LA ANOMALÍA", 110, 13, true);

    private void IniciarEncuentro(Personaje entidad, OrigenEncuentro origen)
    {
        EntidadActual = entidad;
        origenEncuentro = origen;
        RegistroCombate.Clear();

        if (entidad is Robot)
        {
            RegistrarCombate("Un robot de vigilancia te detecta. Su sistema defensivo está en espera.", ConsoleColor.Cyan);
            RegistrarCombate("Consejo: si lo ANALIZAS antes de atacarlo, puedes hackearlo.", ConsoleColor.DarkGray);
            Iris.Enviar($"Robot detectado: {entidad.Nombre}. No es hostil si no lo atacas.");
        }
        else
        {
            RegistrarCombate($"¡{entidad.Nombre} es hostil! Se inicia un combate.", ConsoleColor.Red);
            Iris.Enviar($"ALERTA: entidad hostil {entidad.Nombre}. Prepárate para combatir.");
        }

        Registrar($"👾 ENTIDAD DETECTADA: {entidad.Nombre}.", entidad is Robot ? ConsoleColor.Cyan : ConsoleColor.Red);
        Destello?.Invoke(entidad is Robot ? ConsoleColor.DarkCyan : ConsoleColor.DarkRed);
    }

    public void Atacar()
    {
        // Control: atacar cuando no existe un enemigo.
        if (!EnCombate)
        {
            Registrar("No hay ninguna entidad a la que atacar.", ConsoleColor.Red);
            return;
        }

        int danio;
        if (Cadete.Energia >= 5)
        {
            Cadete.CambiarEnergia(-5);
            danio = Cadete.CalcularAtaque();
            RegistrarCombate("Disparas tu rifle de pulsos (-5 energía).", ConsoleColor.Gray);
        }
        else
        {
            danio = 8;
            RegistrarCombate("Sin energía para el rifle: atacas cuerpo a cuerpo.", ConsoleColor.DarkYellow);
        }

        if (EntidadActual is Robot robot && !robot.SistemaDefensivoActivo)
            RegistrarCombate("¡El robot activa su SISTEMA DEFENSIVO! (Defensa 8)", ConsoleColor.Cyan);

        int aplicado = EntidadActual.RecibirDanio(danio);
        RegistrarCombate($"Causas {aplicado} de daño a {EntidadActual.Nombre}. Salud restante: {EntidadActual.Salud}",
                         ConsoleColor.Yellow);
        Destello?.Invoke(ConsoleColor.DarkYellow);

        TerminarTurno(true);
    }

    public void Escapar()
    {
        if (!EnCombate) return;

        int probabilidad = 50;
        if (EntidadActual is Robot robot) probabilidad = robot.SistemaDefensivoActivo ? 75 : 100;
        else if (EntidadActual is Enemigo enemigo) probabilidad = enemigo.EsJefe ? 40 : 60;

        if (Random.Shared.Next(100) < probabilidad)
        {
            Registrar($"Escapaste de {EntidadActual.Nombre}.", ConsoleColor.Yellow);
            FinEncuentro(ResultadoEncuentro.Escape);
            return;
        }

        RegistrarCombate($"No lograste escapar ({probabilidad}% de probabilidad).", ConsoleColor.Red);
        TerminarTurno(true);
    }

    public void Analizar()
    {
        if (!EnCombate) return;

        // Control: usar un objeto que no se posee.
        if (!Cadete.Inventario.Tiene(TipoObjeto.Analizador))
        {
            RegistrarCombate("No posees un analizador. No puedes analizar la entidad.", ConsoleColor.Red);
            return;
        }
        if (Cadete.Energia < 5)
        {
            RegistrarCombate("Energía insuficiente para el analizador (necesitas 5).", ConsoleColor.Red);
            return;
        }

        Cadete.CambiarEnergia(-5);
        RegistrarCombate("ANÁLISIS: " + EntidadActual.Describir(), ConsoleColor.Cyan);

        if (EntidadActual is Robot robot)
        {
            if (!robot.SistemaDefensivoActivo)
            {
                robot.Hackear();
                RegistrarCombate("¡Hackeo exitoso! El robot te reconoce como aliado.", ConsoleColor.Green);
            }
            else
            {
                RegistrarCombate("Su sistema defensivo bloquea el hackeo.", ConsoleColor.DarkCyan);
            }
        }
        else if (EntidadActual is Enemigo enemigo)
        {
            if (!enemigo.DebilidadDetectada)
            {
                enemigo.DetectarDebilidad();
                RegistrarCombate("Debilidad detectada: tus ataques harán +10 de daño.", ConsoleColor.Green);
            }
            else
            {
                RegistrarCombate("Ya conoces su debilidad.", ConsoleColor.DarkGray);
            }
        }

        TerminarTurno(true);
    }

    // Después de cada acción del cadete en un combate.
    private void TerminarTurno(bool turnoUsado)
    {
        if (EntidadActual is Robot robot && robot.Hackeado)
        {
            FinalPacifico(robot);
            FinEncuentro(ResultadoEncuentro.Pacifico);
            return;
        }

        if (!EntidadActual.EstaVivo())
        {
            Victoria(EntidadActual);
            FinEncuentro(ResultadoEncuentro.Victoria);
            return;
        }

        if (turnoUsado) ReaccionEntidad();

        if (!Cadete.EstaVivo() || Cadete.Estabilidad <= 0)
        {
            FinEncuentro(ResultadoEncuentro.Derrota);
            return;
        }

        ComprobarEstado();
    }

    // Lo que hace la entidad después del turno del cadete.
    private void ReaccionEntidad()
    {
        if (EntidadActual is Robot robot)
        {
            if (!robot.SistemaDefensivoActivo)
            {
                RegistrarCombate("El robot te observa sin moverse. No es hostil... por ahora.", ConsoleColor.DarkCyan);
                return;
            }
            int descarga = robot.Descarga();
            Cadete.RecibirDanio(descarga);
            RegistrarCombate($"El robot lanza una descarga de {descarga}. Escudo {Cadete.Escudo} · Salud {Cadete.Salud}",
                             ConsoleColor.Red);
            Destello?.Invoke(ConsoleColor.DarkCyan);
        }
        else if (EntidadActual is Enemigo enemigo)
        {
            int danio = enemigo.Atacar();
            Cadete.RecibirDanio(danio);
            RegistrarCombate($"{enemigo.Nombre} ataca: {danio} de daño. Escudo {Cadete.Escudo} · Salud {Cadete.Salud}",
                             ConsoleColor.Red);
            Destello?.Invoke(ConsoleColor.DarkRed);

            if (enemigo.EsJefe && Random.Shared.Next(100) < 30)
            {
                Cadete.CambiarEstabilidad(-5);
                RegistrarCombate("El Guardián distorsiona la realidad: -5 estabilidad.", ConsoleColor.Magenta);
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
        Cadete.AgregarXP(xp);

        string botin = "ninguno";
        if (Random.Shared.Next(100) < probabilidadObjeto)
        {
            Objeto objeto = Objeto.Aleatorio();
            botin = Cadete.Inventario.Agregar(objeto) ? objeto.Nombre : objeto.Nombre + " (inventario lleno, se perdió)";
        }

        Alerta?.Invoke("ENTIDAD NEUTRALIZADA", new[]
        {
            $"{entidad.Nombre} fue derrotado.",
            "",
            $"+{xp} XP",
            $"Objeto obtenido: {botin}"
        }, ConsoleColor.Green);

        Registrar($"Derrotaste a {entidad.Nombre}. +{xp} XP.", ConsoleColor.Green);
    }

    private void FinalPacifico(Robot robot)
    {
        Cadete.AgregarXP(20);
        Objeto regalo = Objeto.Aleatorio();
        string texto = Cadete.Inventario.Agregar(regalo) ? regalo.Nombre : "nada (inventario lleno)";

        Alerta?.Invoke("ROBOT HACKEADO", new[]
        {
            $"{robot.Nombre} ahora te obedece y abre su compartimento de carga.",
            "",
            "+20 XP",
            $"Te entrega: {texto}"
        }, ConsoleColor.Cyan);

        Registrar($"Hackeaste a {robot.Nombre}. +20 XP.", ConsoleColor.Cyan);
    }

    // Cierra el encuentro y aplica la consecuencia según su origen.
    private void FinEncuentro(ResultadoEncuentro resultado)
    {
        EntidadActual = null;
        bool ganado = resultado == ResultadoEncuentro.Victoria || resultado == ResultadoEncuentro.Pacifico;

        if (origenEncuentro == OrigenEncuentro.Mapa)
        {
            if (ganado) RealidadActual.CambiarCelda(Fila, Columna, '.');
            else if (resultado == ResultadoEncuentro.Escape) Retroceder();
        }
        else if (origenEncuentro == OrigenEncuentro.Guardian)
        {
            if (resultado == ResultadoEncuentro.Victoria)
            {
                guardianDerrotado = true;
                Mision.Cumplir(Mision.Entidad);
                TomarNucleo();
            }
            else if (resultado == ResultadoEncuentro.Escape)
            {
                Retroceder();
            }
        }

        ComprobarEstado();
    }

    // ======================================================
    //  INVENTARIO: consultar → utilizar → eliminar
    // ======================================================
    // Devuelve el mensaje del resultado. "usado" indica si el objeto se gastó.
    public string UsarObjeto(int indice, out bool usado)
    {
        usado = false;
        Objeto objeto = Cadete.Inventario.Obtener(indice);

        // Control: objeto inexistente.
        if (objeto == null) return "No existe ningún objeto en esa ranura.";

        string mensaje;
        switch (objeto.Tipo)
        {
            case TipoObjeto.CelulaEnergia:
                if (Cadete.Energia >= Cadete.Maximo) return "La energía ya está al máximo. No se gastó la célula.";
                int energiaAntes = Cadete.Energia;
                Cadete.CambiarEnergia(25);
                mensaje = $"Utilizar célula de energía → Energía: {energiaAntes} → {Cadete.Energia}";
                break;

            case TipoObjeto.Escudo:
                if (Cadete.Escudo >= Cadete.Maximo) return "El escudo ya está al máximo.";
                int escudoAntes = Cadete.Escudo;
                Cadete.CambiarEscudo(30);
                mensaje = $"Escudo activado → Escudo: {escudoAntes} → {Cadete.Escudo}";
                break;

            case TipoObjeto.KitMedico:
                if (Cadete.Salud >= Cadete.Maximo) return "Tu salud ya está al máximo.";
                int saludAntes = Cadete.Salud;
                Cadete.Curar(30);
                mensaje = $"Kit médico aplicado → Salud: {saludAntes} → {Cadete.Salud}";
                break;

            case TipoObjeto.ModuloAtaque:
                Cadete.DanioExtra += 15;
                mensaje = "Módulo de ataque instalado → tu próximo ataque hará +15 de daño";
                break;

            case TipoObjeto.ModuloDesconocido:
                Cadete.Curar(20);
                Cadete.CambiarEnergia(20);
                Cadete.CambiarEstabilidad(20);
                mensaje = "El módulo desconocido brilla → salud, energía y estabilidad +20";
                break;

            case TipoObjeto.Analizador:
                return "El analizador se usa con el botón ANALIZAR durante un encuentro.";

            default: // núcleo
                return "El núcleo no se usa: llévalo al punto de extracción (E).";
        }

        Cadete.Inventario.Eliminar(indice);
        usado = true;
        Registrar(mensaje, ConsoleColor.Green);

        // En combate, usar un objeto gasta el turno.
        if (EnCombate)
        {
            RegistrarCombate(mensaje, ConsoleColor.Green);
            TerminarTurno(true);
        }
        return mensaje;
    }

    public string EliminarObjeto(int indice)
    {
        Objeto objeto = Cadete.Inventario.Obtener(indice);
        if (objeto == null) return "No existe ningún objeto en esa ranura.";
        if (objeto.Tipo == TipoObjeto.NucleoEnergetico) return "No puedes abandonar el núcleo: es el objetivo de la misión.";

        Cadete.Inventario.Eliminar(indice);
        Registrar($"Eliminaste {objeto.Nombre} del inventario.", ConsoleColor.DarkYellow);
        return $"{objeto.Nombre} fue eliminado del inventario.";
    }

    // ======================================================
    //  ESTADO: alertas y derrota
    // ======================================================
    private void ComprobarEstado()
    {
        if (!PartidaActiva) return;

        if (Cadete.Estabilidad <= 30 && !alertaEstabilidadEnviada)
        {
            alertaEstabilidadEnviada = true;
            Registrar("ALERTA IRIS: estabilidad crítica. Ve a un punto de extracción (E) para recuperarte.", ConsoleColor.Red);
            Iris.Enviar("ALERTA: tu estabilidad es crítica. Busca un punto de extracción.");
        }
        if (Cadete.Estabilidad > 30) alertaEstabilidadEnviada = false;

        if (!Cadete.EstaVivo())
            MisionFallida("Tu salud llegó a cero.");
        else if (Cadete.Estabilidad <= 0)
            MisionFallida("Tu estabilidad colapsó y quedaste atrapado entre realidades.");
    }

    private void MisionFallida(string motivo)
    {
        PartidaActiva = false;
        EntidadActual = null;
        Destello?.Invoke(ConsoleColor.DarkRed);

        Alerta?.Invoke("MISIÓN FALLIDA", new[]
        {
            "Motivo: " + motivo,
            "",
            $"Objetivos cumplidos: {Mision.CantidadCumplidos} de {Mision.Objetivos.Length}",
            $"XP obtenida: {Cadete.XP}     Turnos: {Turnos}",
            "",
            "NEXUS no pudo completar la misión. Vuelve a intentarlo, cadete."
        }, ConsoleColor.Red);

        Iris.Enviar("ALERTA: señal del cadete perdida. MISIÓN FALLIDA.");
        PartidaTerminada?.Invoke(false);
    }

    public void Abandonar()
    {
        if (!PartidaActiva) return;
        PartidaActiva = false;
        EntidadActual = null;
        Iris.Enviar("ALERTA: el cadete abandonó la misión.");
    }

    // ---------------- REGISTRO DE MENSAJES ----------------
    private void Registrar(string texto, ConsoleColor color)
    {
        Mensaje?.Invoke(new MensajeBitacora(texto, color));
    }

    private void RegistrarCombate(string texto, ConsoleColor color)
    {
        RegistroCombate.Add(new MensajeBitacora(texto, color));
    }
}
