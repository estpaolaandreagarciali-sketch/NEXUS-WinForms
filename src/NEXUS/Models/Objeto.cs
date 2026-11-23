namespace NEXUS.Models;

public enum TipoObjeto
{
    CelulaEnergia,
    Escudo,
    ModuloAtaque,
    Analizador,
    KitMedico,
    NucleoEnergetico,
    ModuloDesconocido
}

// ============================================================
//  OBJETO: algo que el cadete puede guardar en su inventario.
//  El constructor rellena nombre, descripción, símbolo y color
//  según el tipo.
// ============================================================
public class Objeto
{
    public TipoObjeto Tipo { get; }
    public string Nombre { get; }
    public string Descripcion { get; }
    public string Simbolo { get; }
    public ConsoleColor Color { get; }

    public Objeto(TipoObjeto tipo)
    {
        Tipo = tipo;
        switch (tipo)
        {
            case TipoObjeto.CelulaEnergia:
                Nombre = "Célula de energía"; Descripcion = "Recupera 25 de energía.";
                Simbolo = "♦"; Color = ConsoleColor.Yellow;
                break;
            case TipoObjeto.Escudo:
                Nombre = "Escudo"; Descripcion = "Recupera 30 de escudo.";
                Simbolo = "■"; Color = ConsoleColor.Blue;
                break;
            case TipoObjeto.ModuloAtaque:
                Nombre = "Módulo de ataque"; Descripcion = "Tu próximo ataque hace +15 de daño.";
                Simbolo = "►"; Color = ConsoleColor.Red;
                break;
            case TipoObjeto.Analizador:
                Nombre = "Analizador"; Descripcion = "Analiza entidades durante un encuentro.";
                Simbolo = "Θ"; Color = ConsoleColor.Cyan;
                break;
            case TipoObjeto.KitMedico:
                Nombre = "Kit médico"; Descripcion = "Recupera 30 de salud.";
                Simbolo = "+"; Color = ConsoleColor.Green;
                break;
            case TipoObjeto.NucleoEnergetico:
                Nombre = "Núcleo energético"; Descripcion = "Objetivo de la misión. Llévalo a extracción.";
                Simbolo = "☼"; Color = ConsoleColor.Magenta;
                break;
            default:
                Nombre = "Módulo desconocido"; Descripcion = "Tecnología extraña: restaura +20 a todo.";
                Simbolo = "?"; Color = ConsoleColor.White;
                break;
        }
    }

    // Arreglo con los objetos que pueden aparecer al explorar.
    // (Las células se repiten para que salgan más seguido.)
    private static readonly TipoObjeto[] encontrables =
    {
        TipoObjeto.CelulaEnergia, TipoObjeto.CelulaEnergia, TipoObjeto.Escudo,
        TipoObjeto.ModuloAtaque, TipoObjeto.KitMedico, TipoObjeto.Analizador
    };

    public static Objeto Aleatorio()
    {
        int indice = Random.Shared.Next(encontrables.Length);
        return new Objeto(encontrables[indice]);
    }
}
