namespace NEXUS.Models;

// ============================================================
//  REALIDAD: un mundo con su MAPA guardado en una MATRIZ char[,].
//
//  Símbolos del mapa:
//   #  muro              .  zona libre        E  punto de extracción
//   +  objeto            R  robot             X  enemigo
//   A  anomalía          N  núcleo            i  registro de datos
//   ~  zona inestable    *  anomalía ya investigada
//
//  Además usa dos matrices bool[,]:
//   descubierto → lo que el cadete ya puede ver (niebla de guerra)
//   visitado    → zonas pisadas (los eventos aleatorios ocurren 1 vez)
// ============================================================
public class Realidad
{
    public string Codigo { get; }
    public string Nombre { get; }
    public string Descripcion { get; }
    public int NivelRiesgo { get; }          // 1 = bajo, 3 = alto
    public ConsoleColor Color { get; }
    public int FilaInicio { get; private set; }
    public int ColumnaInicio { get; private set; }

    private readonly char[,] mapa;
    private readonly bool[,] descubierto;
    private readonly bool[,] visitado;

    public int Filas => mapa.GetLength(0);
    public int Columnas => mapa.GetLength(1);

    public Realidad(string codigo, string nombre, string descripcion, int nivelRiesgo,
                    ConsoleColor color, string[] plano)
    {
        Codigo = codigo;
        Nombre = nombre;
        Descripcion = descripcion;
        NivelRiesgo = nivelRiesgo;
        Color = color;

        int filas = plano.Length;
        int columnas = plano[0].Length;
        mapa = new char[filas, columnas];
        descubierto = new bool[filas, columnas];
        visitado = new bool[filas, columnas];

        // Ciclos anidados: copiamos el plano (texto) a la matriz.
        for (int f = 0; f < filas; f++)
        {
            for (int c = 0; c < columnas; c++)
            {
                mapa[f, c] = c < plano[f].Length ? plano[f][c] : '#';

                if (mapa[f, c] == 'E')
                {
                    FilaInicio = f;
                    ColumnaInicio = c;
                }
            }
        }
    }

    public bool EstaDentro(int fila, int columna)
    {
        return fila >= 0 && fila < Filas && columna >= 0 && columna < Columnas;
    }

    // El borde del mapa es el límite de la realidad.
    public bool EsBorde(int fila, int columna)
    {
        return fila == 0 || columna == 0 || fila == Filas - 1 || columna == Columnas - 1;
    }

    public char ObtenerCelda(int fila, int columna)
    {
        if (!EstaDentro(fila, columna)) return '#';
        return mapa[fila, columna];
    }

    public void CambiarCelda(int fila, int columna, char nuevoSimbolo)
    {
        if (EstaDentro(fila, columna)) mapa[fila, columna] = nuevoSimbolo;
    }

    public bool EstaDescubierto(int fila, int columna)
    {
        return EstaDentro(fila, columna) && descubierto[fila, columna];
    }

    public bool FueVisitado(int fila, int columna)
    {
        return EstaDentro(fila, columna) && visitado[fila, columna];
    }

    public void MarcarVisitado(int fila, int columna)
    {
        if (EstaDentro(fila, columna)) visitado[fila, columna] = true;
    }

    // Revela la celda y las 8 que la rodean (ciclos anidados de -1 a +1).
    public void Descubrir(int fila, int columna)
    {
        for (int f = fila - 1; f <= fila + 1; f++)
        {
            for (int c = columna - 1; c <= columna + 1; c++)
            {
                if (EstaDentro(f, c)) descubierto[f, c] = true;
            }
        }
    }

    // Busca un símbolo en todo el mapa y revela su zona.
    public void RevelarSimbolo(char simbolo)
    {
        for (int f = 0; f < Filas; f++)
        {
            for (int c = 0; c < Columnas; c++)
            {
                if (mapa[f, c] == simbolo) Descubrir(f, c);
            }
        }
    }

    // Porcentaje de zonas transitables que ya se descubrieron.
    public int PorcentajeExplorado()
    {
        int total = 0;
        int vistas = 0;
        for (int f = 0; f < Filas; f++)
        {
            for (int c = 0; c < Columnas; c++)
            {
                if (mapa[f, c] == '#') continue;
                total++;
                if (descubierto[f, c]) vistas++;
            }
        }
        return total == 0 ? 0 : vistas * 100 / total;
    }
}
