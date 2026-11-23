namespace NEXUS.Models;

// ============================================================
//  PERSONAJE: clase base (padre) de todos los seres del juego.
//
//                    PERSONAJE
//                       │
//          ┌────────────┼────────────┐
//       CADETE        ROBOT       ENEMIGO
//
//  Cada hijo cambia (override) cómo recibe daño y cómo se describe.
// ============================================================
public abstract class Personaje
{
    public string Nombre { get; protected set; }
    public int Salud { get; protected set; }
    public int SaludMaxima { get; protected set; }

    protected Personaje(string nombre, int saludMaxima)
    {
        Nombre = nombre;
        SaludMaxima = saludMaxima;
        Salud = saludMaxima;
    }

    public bool EstaVivo() => Salud > 0;

    // Resta salud y devuelve cuánto daño se aplicó de verdad.
    public virtual int RecibirDanio(int danio)
    {
        if (danio < 0) danio = 0;
        int saludAntes = Salud;
        Salud = Math.Max(0, Salud - danio);
        return saludAntes - Salud;
    }

    // Cada tipo de personaje se describe a su manera (lo usa el analizador).
    public abstract string Describir();
}

// ------------------------------------------------------------
//  CADETE: el jugador. Tiene energía, estabilidad, escudo,
//  experiencia e inventario.
// ------------------------------------------------------------
public class Cadete : Personaje
{
    public const int Maximo = 100;

    public int Energia { get; private set; } = 100;
    public int Estabilidad { get; private set; } = 100;
    public int Escudo { get; private set; } = 50;
    public int XP { get; private set; }
    public int DanioExtra { get; set; }          // lo suma el módulo de ataque
    public Inventario Inventario { get; } = new Inventario();

    public Cadete(string nombre) : base(nombre, 100) { }

    // Todos los valores se mantienen entre 0 y 100.
    public void CambiarEnergia(int cantidad) => Energia = Limitar(Energia + cantidad);
    public void CambiarEstabilidad(int cantidad) => Estabilidad = Limitar(Estabilidad + cantidad);
    public void CambiarEscudo(int cantidad) => Escudo = Limitar(Escudo + cantidad);
    public void Curar(int cantidad) => Salud = Limitar(Salud + cantidad);
    public void AgregarXP(int cantidad) => XP += cantidad;

    // El escudo absorbe el daño primero; lo que sobra baja la salud.
    public override int RecibirDanio(int danio)
    {
        int absorbido = Math.Min(Escudo, danio);
        Escudo -= absorbido;
        return base.RecibirDanio(danio - absorbido);
    }

    // Daño que ignora el escudo (fugas del traje, falta de energía).
    public void RecibirDanioDirecto(int danio)
    {
        base.RecibirDanio(danio);
    }

    // Daño del rifle de pulsos: 15 a 25 + el bonus del módulo de ataque.
    public int CalcularAtaque()
    {
        int danio = 15 + Random.Shared.Next(0, 11) + DanioExtra;
        DanioExtra = 0; // el bonus se gasta en un solo ataque
        return danio;
    }

    public override string Describir()
    {
        return $"Cadete {Nombre} | Salud {Salud} | Energía {Energia} | Escudo {Escudo}";
    }

    private static int Limitar(int valor) => Math.Clamp(valor, 0, Maximo);
}

// ------------------------------------------------------------
//  ROBOT: no es hostil al principio. Si lo atacas, activa su
//  SISTEMA DEFENSIVO (resiste más y lanza descargas).
//  Si lo analizas antes de atacarlo, puedes hackearlo.
// ------------------------------------------------------------
public class Robot : Personaje
{
    public int Defensa { get; } = 8;
    public bool SistemaDefensivoActivo { get; private set; }
    public bool Hackeado { get; private set; }

    public Robot(string nombre) : base(nombre, 60) { }

    public override int RecibirDanio(int danio)
    {
        SistemaDefensivoActivo = true;              // al primer golpe se defiende
        int danioReducido = Math.Max(1, danio - Defensa);
        return base.RecibirDanio(danioReducido);
    }

    public int Descarga() => 10 + Random.Shared.Next(0, 5);

    public void Hackear() => Hackeado = true;

    public override string Describir()
    {
        string estado = SistemaDefensivoActivo ? "sistema defensivo ACTIVO" : "pacífico, se puede hackear";
        return $"{Nombre}: robot de vigilancia | Salud {Salud}/{SaludMaxima} | Defensa {Defensa} | {estado}";
    }
}

// ------------------------------------------------------------
//  ENEMIGO: siempre hostil, inicia un combate.
//  Si el analizador detecta su debilidad, recibe +10 de daño.
// ------------------------------------------------------------
public class Enemigo : Personaje
{
    public int DanioBase { get; }
    public bool EsJefe { get; }
    public bool DebilidadDetectada { get; private set; }

    public Enemigo(string nombre, int salud, int danioBase, bool esJefe) : base(nombre, salud)
    {
        DanioBase = danioBase;
        EsJefe = esJefe;
    }

    public override int RecibirDanio(int danio)
    {
        if (DebilidadDetectada) danio += 10;
        return base.RecibirDanio(danio);
    }

    public int Atacar() => DanioBase + Random.Shared.Next(0, 6);

    public void DetectarDebilidad() => DebilidadDetectada = true;

    public override string Describir()
    {
        string tipo = EsJefe ? "entidad suprema" : "entidad hostil";
        return $"{Nombre}: {tipo} | Salud {Salud}/{SaludMaxima} | Daño {DanioBase}-{DanioBase + 5}";
    }
}

// Resultado de un encuentro con una entidad.
public enum ResultadoEncuentro
{
    Victoria,   // la entidad fue derrotada
    Escape,     // el cadete huyó
    Pacifico,   // el robot fue hackeado
    Derrota     // el cadete cayó
}
