namespace NEXUS.Models;

public enum TipoObjeto
{
    CelulaEnergia,
    Escudo,
    ModuloAtaque,
    Analizador,
    NucleoEnergetico
}

public abstract class Objeto
{
    public string Nombre { get; }
    public TipoObjeto Tipo { get; }
    public string Descripcion { get; }

    protected Objeto(string nombre, TipoObjeto tipo, string descripcion)
    {
        Nombre = nombre;
        Tipo = tipo;
        Descripcion = descripcion;
    }
}

public sealed class CelulaEnergia : Objeto
{
    public CelulaEnergia() : base("Célula de energía", TipoObjeto.CelulaEnergia, "Recupera 25 puntos de energía.") { }
}

public sealed class EscudoObjeto : Objeto
{
    public EscudoObjeto() : base("Escudo", TipoObjeto.Escudo, "Recupera 30 puntos de escudo.") { }
}

public sealed class ModuloAtaque : Objeto
{
    public ModuloAtaque() : base("Módulo de ataque", TipoObjeto.ModuloAtaque, "Añade 15 puntos al próximo ataque.") { }
}

public sealed class Analizador : Objeto
{
    public Analizador() : base("Analizador", TipoObjeto.Analizador, "Permite analizar entidades.") { }
}

public sealed class NucleoEnergetico : Objeto
{
    public NucleoEnergetico() : base("Núcleo energético", TipoObjeto.NucleoEnergetico, "Objetivo principal de la misión.") { }
}

public class Inventario
{
    private readonly List<Objeto> objetos = new();
    public IReadOnlyList<Objeto> Objetos => objetos;
    public int Cantidad => objetos.Count;

    public void Agregar(Objeto objeto) => objetos.Add(objeto);
    public Objeto Obtener(int indice) => indice >= 0 && indice < objetos.Count ? objetos[indice] : null;
    public bool Eliminar(Objeto objeto) => objetos.Remove(objeto);
    public bool TieneTipo(TipoObjeto tipo) => objetos.Any(o => o.Tipo == tipo);
}

public abstract class Personaje
{
    public string Nombre { get; protected set; }
    public int Salud { get; protected set; }

    protected Personaje(string nombre, int salud)
    {
        Nombre = nombre;
        Salud = salud;
    }

    public bool EstaVivo() => Salud > 0;

    public virtual void RecibirDanio(int dano)
    {
        Salud = Math.Max(0, Salud - dano);
    }
}

public sealed class Cadete : Personaje
{
    public int Energia { get; set; } = 100;
    public int Estabilidad { get; set; } = 100;
    public int Escudo { get; set; } = 50;
    public int XP { get; private set; }
    public int DanoExtra { get; set; }
    public Inventario Inventario { get; } = new();

    public Cadete(string nombre) : base(nombre, 100) { }

    public void AgregarXP(int cantidad) => XP += cantidad;

    public void RecibirDanioCadete(int dano)
    {
        int absorbido = Math.Min(Escudo, dano);
        Escudo -= absorbido;
        dano -= absorbido;
        if (dano > 0) Salud = Math.Max(0, Salud - dano);
    }
}

public sealed class Robot : Personaje
{
    public int Defensa { get; } = 25;
    public Robot(string nombre) : base(nombre, 80) { }
}

public sealed class Enemigo : Personaje
{
    public int Dano { get; }
    public Enemigo(string nombre, int salud, int dano) : base(nombre, salud) => Dano = dano;
}

public sealed class Realidad
{
    public string Codigo { get; }
    public string Nombre { get; }
    public int Estabilidad { get; private set; }
    public int NivelRiesgo { get; }
    public string Estado => Estabilidad >= 70 ? "ESTABLE" : Estabilidad >= 40 ? "INESTABLE" : "CRÍTICA";

    public Realidad(string codigo, string nombre, int estabilidad, int riesgo)
    {
        Codigo = codigo;
        Nombre = nombre;
        Estabilidad = estabilidad;
        NivelRiesgo = riesgo;
    }

    public string Explorar(Cadete cadete)
    {
        if (cadete.Energia < 10) return "Energía insuficiente para explorar.";

        string resultado;
        if (cadete.Energia >= 70 && cadete.Estabilidad >= 70)
        {
            resultado = "EXPLORACIÓN SEGURA";
            cadete.Energia -= 10 + NivelRiesgo;
            cadete.Estabilidad -= NivelRiesgo <= 20 ? 3 : 5;
            Estabilidad -= 2;
        }
        else if (cadete.Energia >= 40 && cadete.Estabilidad >= 40)
        {
            resultado = "EXPLORACIÓN INESTABLE";
            cadete.Energia -= 15 + NivelRiesgo;
            cadete.Estabilidad -= 6 + NivelRiesgo / 10;
            Estabilidad -= 5;
        }
        else return "Exploración rechazada: energía o estabilidad insuficiente.";

        cadete.Energia = Math.Clamp(cadete.Energia, 0, 100);
        cadete.Estabilidad = Math.Clamp(cadete.Estabilidad, 0, 100);
        Estabilidad = Math.Max(0, Estabilidad);
        return resultado;
    }
}

public sealed class Mision
{
    public string Nombre { get; }
    public bool AnomaliaEncontrada { get; private set; }
    public bool NucleoEncontrado { get; private set; }
    public bool EnemigoSuperado { get; private set; }
    public bool ExtraccionRealizada { get; private set; }
    public bool Completada { get; private set; }

    public Mision(string nombre) => Nombre = nombre;
    public void CompletarAnomalia() => AnomaliaEncontrada = true;
    public void CompletarNucleo() => NucleoEncontrado = true;
    public void CompletarEnemigo() => EnemigoSuperado = true;

    public void CompletarExtraccion()
    {
        ExtraccionRealizada = true;
        Completada = AnomaliaEncontrada && NucleoEncontrado && EnemigoSuperado && ExtraccionRealizada;
    }
}
