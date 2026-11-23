namespace NEXUS.Models;

// ============================================================
//  MISIÓN: "La anomalía perdida".
//  Usa dos ARREGLOS paralelos: el texto de cada objetivo y si
//  ya se cumplió (true/false).
// ============================================================
public class Mision
{
    // Posición de cada objetivo dentro de los arreglos.
    public const int ZonaAnomalia = 0;
    public const int Nucleo = 1;
    public const int Entidad = 2;
    public const int Extraccion = 3;

    public string Nombre { get; } = "LA ANOMALÍA PERDIDA";

    public string[] Objetivos { get; } = new string[]
    {
        "Llegar a la zona de anomalía",
        "Encontrar el núcleo energético",
        "Sobrevivir al encuentro con la entidad",
        "Regresar al punto de extracción"
    };

    private readonly bool[] cumplidos = new bool[4];

    public bool EstaCumplido(int objetivo)
    {
        if (objetivo < 0 || objetivo >= cumplidos.Length) return false;
        return cumplidos[objetivo];
    }

    public void Cumplir(int objetivo)
    {
        if (objetivo >= 0 && objetivo < cumplidos.Length) cumplidos[objetivo] = true;
    }

    public int CantidadCumplidos
    {
        get
        {
            int total = 0;
            for (int i = 0; i < cumplidos.Length; i++)
            {
                if (cumplidos[i]) total++;
            }
            return total;
        }
    }

    public bool Completada => CantidadCumplidos == cumplidos.Length;
}

// Un mensaje con su color (para la bitácora y los paneles).
public class MensajeBitacora
{
    public string Texto { get; }
    public ConsoleColor Color { get; }

    public MensajeBitacora(string texto, ConsoleColor color)
    {
        Texto = texto;
        Color = color;
    }
}
