using NEXUS.Models;

namespace NEXUS.Services;

public sealed class GameEngine
{
    private readonly Random azar = new();
    private readonly Realidad realidadPrincipal = new("R-17", "Ciudad de los Ecos", 90, 15);
    private readonly Realidad realidadAlterna = new("R-18", "Sector de las Sombras", 60, 35);
    private Realidad realidadActual;
    private Enemigo enemigoActual;
    private bool tieneNucleo;
    private bool jefeDerrotado;

    public Cadete Cadete { get; } = new("NEXUS-CADETE-01");
    public Mision Mision { get; } = new("La anomalía perdida");
    public Realidad RealidadActual => realidadActual;
    public Enemigo EnemigoActual => enemigoActual;
    public bool TieneNucleo => tieneNucleo;
    public bool SesionActiva { get; private set; } = true;
    public bool EnCombate => enemigoActual != null && enemigoActual.EstaVivo();

    public event Action<string> Mensaje;
    private void Log(string texto) => Mensaje?.Invoke(texto);

    public GameEngine()
    {
        realidadActual = realidadPrincipal;
        Cadete.Inventario.Agregar(new CelulaEnergia());
        Cadete.Inventario.Agregar(new EscudoObjeto());
        Cadete.Inventario.Agregar(new Analizador());
        Log("NEXUS v1.0 | Año 2297\nBienvenido, Cadete Explorer.\nMisión recibida: LA ANOMALÍA PERDIDA.\nInvestiga la anomalía y regresa con vida.");
    }

    public void Explorar()
    {
        if (!SesionActiva) return;
        string resultado = realidadActual.Explorar(Cadete);
        Log(resultado);
        if (resultado.StartsWith("Exploración rechazada") || resultado.StartsWith("Energía insuficiente")) return;
        GenerarEvento();
        ComprobarEstado();
    }

    private void GenerarEvento()
    {
        int evento = azar.Next(0, 100);
        if (realidadActual == realidadAlterna)
        {
            if (evento < 25) { EncontrarAnomalia(); return; }
            if (evento < 50) { EncontrarEnemigo(); return; }
            if (evento < 70) { EncontrarObjeto(); return; }
            if (evento < 85) { EncontrarRobot(); return; }
            PerdidaEnergia(); return;
        }

        if (evento < 25) EncontrarObjeto();
        else if (evento < 45) EncontrarRobot();
        else if (evento < 60) PerdidaEnergia();
        else if (evento < 70) EncontrarAnomalia();
        else Log("No ocurrió nada relevante en esta exploración.");
    }

    private void EncontrarObjeto()
    {
        Objeto objeto = azar.Next(0, 4) switch
        {
            0 => new CelulaEnergia(),
            1 => new EscudoObjeto(),
            2 => new ModuloAtaque(),
            _ => new Analizador()
        };
        Cadete.Inventario.Agregar(objeto);
        Log($"OBJETO ENCONTRADO: {objeto.Nombre}\n{objeto.Descripcion}\nSe agregó al inventario.");
    }

    private void EncontrarRobot()
    {
        var robot = new Robot("ROBOT GUARDIÁN");
        Log($"ENTIDAD DETECTADA: {robot.Nombre}\nSalud: {robot.Salud}/80 | Defensa: {robot.Defensa}\nEl robot no es hostil.");
    }

    private void EncontrarEnemigo()
    {
        enemigoActual = new Enemigo("GUARDIÁN DE LA ANOMALÍA", 80, 15);
        Log("ENTIDAD HOSTIL DETECTADA\nGUARDIÁN DE LA ANOMALÍA\nSalud: 80 | Daño: 15\n¡Prepárate para el combate!");
    }

    private void EncontrarAnomalia()
    {
        Mision.CompletarAnomalia();
        Log("ANOMALÍA DETECTADA\nUna perturbación desconocida aparece.\nLa señal coincide con el objetivo de la misión.");
    }

    private void PerdidaEnergia()
    {
        int perdida = azar.Next(5, 16);
        Cadete.Energia = Math.Max(0, Cadete.Energia - perdida);
        Log($"PÉRDIDA DE ENERGÍA: -{perdida}\nEnergía actual: {Cadete.Energia}/100");
    }

    public void Atacar()
    {
        if (!EnCombate) { Log("No existe ningún enemigo activo."); return; }
        if (Cadete.Energia < 5) { Log("Energía insuficiente para atacar."); return; }

        int dano = 20 + Cadete.DanoExtra;
        Cadete.Energia -= 5;
        enemigoActual.RecibirDanio(dano);
        Cadete.DanoExtra = 0;
        Log($"ATAQUE REALIZADO\nDaño causado: {dano}\nSalud enemiga: {enemigoActual.Salud}");
        if (!enemigoActual.EstaVivo())
        {
            Log("ENEMIGO DERROTADO\n+50 XP");
            Cadete.AgregarXP(50);
            jefeDerrotado = true;
            Mision.CompletarEnemigo();
            enemigoActual = null;
        }
        else
        {
            enemigoActual = enemigoActual;
            enemigoActual.RecibirDanio(0);
            Cadete.RecibirDanioCadete(15);
            Log($"El Guardián contraataca.\nEscudo: {Cadete.Escudo}/100 | Salud: {Cadete.Salud}/100");
        }
        ComprobarEstado();
    }

    public void IntentarEscapar()
    {
        if (!EnCombate) { Log("No hay ningún enemigo del que escapar."); return; }
        if (azar.Next(0, 100) < 70)
        {
            enemigoActual = null;
            Log("ESCAPE EXITOSO\nHas logrado alejarte del Guardián.");
        }
        else
        {
            Cadete.RecibirDanioCadete(15);
            Log("No pudiste escapar. El Guardián contraataca.\nEscudo: " + Cadete.Escudo + " | Salud: " + Cadete.Salud);
        }
        ComprobarEstado();
    }

    public void UsarObjeto(int indice)
    {
        Objeto objeto = Cadete.Inventario.Obtener(indice);
        if (objeto == null) { Log("Selecciona un objeto válido."); return; }

        switch (objeto.Tipo)
        {
            case TipoObjeto.CelulaEnergia:
                if (Cadete.Energia >= 100) { Log("La energía ya está al máximo."); return; }
                int energiaAnterior = Cadete.Energia;
                Cadete.Energia = Math.Min(100, Cadete.Energia + 25);
                Log($"CÉLULA DE ENERGÍA UTILIZADA\nEnergía: {energiaAnterior} → {Cadete.Energia}");
                Cadete.Inventario.Eliminar(objeto);
                break;
            case TipoObjeto.Escudo:
                if (Cadete.Escudo >= 100) { Log("El escudo ya está al máximo."); return; }
                int escudoAnterior = Cadete.Escudo;
                Cadete.Escudo = Math.Min(100, Cadete.Escudo + 30);
                Log($"ESCUDO ACTIVADO\nEscudo: {escudoAnterior} → {Cadete.Escudo}");
                Cadete.Inventario.Eliminar(objeto);
                break;
            case TipoObjeto.ModuloAtaque:
                Cadete.DanoExtra += 15;
                Log("MÓDULO DE ATAQUE ACTIVADO\nDaño adicional preparado: +15");
                Cadete.Inventario.Eliminar(objeto);
                break;
            case TipoObjeto.Analizador:
                Log("ANALIZADOR ACTIVADO\nSistema listo para analizar entidades.");
                break;
            case TipoObjeto.NucleoEnergetico:
                Log("NÚCLEO ENERGÉTICO\nDebes llevarlo al punto de extracción.");
                break;
        }
    }

    public void AnalizarAnomalia()
    {
        if (!Mision.AnomaliaEncontrada) { Log("Todavía no has encontrado una anomalía."); return; }
        if (Cadete.Energia < 10) { Log("No hay suficiente energía para analizar."); return; }
        Cadete.Energia -= 10;
        Log("ANALIZANDO ANOMALÍA...\nAnálisis completado.\nSe detectó una fuerte señal energética.\nPosible ubicación: sector central.");
    }

    public void AnalizarEnemigo()
    {
        if (!EnCombate) { Log("No existe una entidad hostil para analizar."); return; }
        if (!Cadete.Inventario.TieneTipo(TipoObjeto.Analizador)) { Log("No tienes un analizador."); return; }
        Log($"ANÁLISIS DE ENTIDAD\n{enemigoActual.Nombre}\nSalud: {enemigoActual.Salud}\nDaño: {enemigoActual.Dano}\nDebilidad detectada: ataque directo.");
    }

    public void BuscarNucleo()
    {
        if (!Mision.AnomaliaEncontrada) { Log("Primero debes encontrar la anomalía."); return; }
        if (realidadActual != realidadAlterna) { Log("La señal del núcleo se encuentra en R-18.\nDebes dirigirte al Sector de las Sombras."); return; }
        if (tieneNucleo) { Log("Ya tienes el núcleo energético."); return; }
        if (!jefeDerrotado) { Log("El Guardián todavía protege el núcleo.\nDebes sobrevivir al encuentro antes de recogerlo."); return; }

        tieneNucleo = true;
        Mision.CompletarNucleo();
        Cadete.Inventario.Agregar(new NucleoEnergetico());
        Cadete.AgregarXP(50);
        Log("NÚCLEO ENERGÉTICO ENCONTRADO\nEl núcleo estaba oculto detrás del Guardián.\n+50 XP");
    }

    public void CambiarRealidad()
    {
        realidadActual = realidadActual == realidadPrincipal ? realidadAlterna : realidadPrincipal;
        Log($"Has ingresado a {realidadActual.Codigo} - {realidadActual.Nombre}");
    }

    public void MostrarMapa()
    {
        string[] simbolos = { "·", "■", "●", "?" };
        var lineas = new List<string> { $"MAPA {realidadActual.Codigo}" };
        for (int f = 0; f < 8; f++)
        {
            var fila = new List<string>();
            for (int c = 0; c < 12; c++) fila.Add(simbolos[azar.Next(0, 4)]);
            lineas.Add(string.Join("  ", fila));
        }
        lineas.Add("Leyenda: · Zona | ■ Base | ● Recurso | ? Anomalía");
        Log(string.Join(Environment.NewLine, lineas));
    }

    public void Extraer()
    {
        if (realidadActual != realidadPrincipal) { Log("Debes regresar a R-17 para extraer."); return; }
        if (!tieneNucleo) { Log("No tienes el núcleo energético."); return; }
        if (!Mision.EnemigoSuperado) { Log("Debes sobrevivir al encuentro."); return; }

        Mision.CompletarExtraccion();
        if (Mision.Completada)
        {
            Cadete.AgregarXP(100);
            Cadete.Inventario.Agregar(new ModuloAtaque());
            Log("MISIÓN COMPLETADA\nLA ANOMALÍA PERDIDA\n✓ Llegaste a la zona de anomalía\n✓ Encontraste el núcleo energético\n✓ Sobreviviste al encuentro\n✓ Regresaste al punto de extracción\n\nRECOMPENSAS\n+100 XP\n+1 módulo de ataque\n\n¡EL CADETE REGRESÓ CON VIDA!");
            SesionActiva = false;
        }
    }

    private void ComprobarEstado()
    {
        if (Cadete.Estabilidad <= 35 || (Cadete.Energia <= 40 && Cadete.Estabilidad <= 40))
            Log("ALERTA IRIS: La estabilidad del Cadete es crítica.");
        if (Cadete.Estabilidad <= 10) FinalizarPorDerrota("La estabilidad alcanzó un nivel crítico.");
        if (!Cadete.EstaVivo()) FinalizarPorDerrota("El Cadete ha perdido toda su salud.");
    }

    private void FinalizarPorDerrota(string motivo)
    {
        Log($"MISIÓN FALLIDA\nMotivo: {motivo}\nNEXUS no pudo completar la misión.");
        SesionActiva = false;
    }
}
