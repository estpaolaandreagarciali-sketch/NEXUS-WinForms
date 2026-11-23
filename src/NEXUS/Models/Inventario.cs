namespace NEXUS.Models;

// ============================================================
//  INVENTARIO: un ARREGLO de 8 ranuras.
//  Operaciones: Agregar → Consultar (Obtener) → Utilizar → Eliminar
//  Una ranura vacía vale null.
// ============================================================
public class Inventario
{
    public const int Capacidad = 8;

    private Objeto[] ranuras = new Objeto[Capacidad];

    public TipoObjeto[] Exportar() => ranuras.Where(x => x != null).Select(x => x.Tipo).ToArray();

    public void Importar(IEnumerable<TipoObjeto> tipos)
    {
        ranuras = new Objeto[Capacidad];
        foreach (TipoObjeto tipo in tipos.Take(Capacidad)) Agregar(new Objeto(tipo));
    }

    // Cuenta las ranuras ocupadas recorriendo el arreglo.
    public int Cantidad
    {
        get
        {
            int cantidad = 0;
            for (int i = 0; i < Capacidad; i++)
            {
                if (ranuras[i] != null) cantidad++;
            }
            return cantidad;
        }
    }

    public bool EstaLleno => Cantidad == Capacidad;

    // Guarda el objeto en la primera ranura libre. Devuelve false si no hay espacio.
    public bool Agregar(Objeto objeto)
    {
        for (int i = 0; i < Capacidad; i++)
        {
            if (ranuras[i] == null)
            {
                ranuras[i] = objeto;
                return true;
            }
        }
        return false;
    }

    // Consulta una ranura. Si el índice no existe, devuelve null (no se rompe).
    public Objeto Obtener(int indice)
    {
        if (indice < 0 || indice >= Capacidad) return null;
        return ranuras[indice];
    }

    // Quita el objeto de la ranura y junta los demás hacia adelante.
    public bool Eliminar(int indice)
    {
        if (Obtener(indice) == null) return false;

        ranuras[indice] = null;
        Compactar();
        return true;
    }

    // Busca un tipo de objeto. Devuelve su posición o -1 si no lo tiene.
    public int BuscarTipo(TipoObjeto tipo)
    {
        for (int i = 0; i < Capacidad; i++)
        {
            if (ranuras[i] != null && ranuras[i].Tipo == tipo) return i;
        }
        return -1;
    }

    public bool Tiene(TipoObjeto tipo) => BuscarTipo(tipo) >= 0;

    // Mueve los objetos para que no queden huecos entre ellos.
    private void Compactar()
    {
        Objeto[] ordenado = new Objeto[Capacidad];
        int siguiente = 0;
        for (int i = 0; i < Capacidad; i++)
        {
            if (ranuras[i] != null)
            {
                ordenado[siguiente] = ranuras[i];
                siguiente++;
            }
        }
        ranuras = ordenado;
    }
}
