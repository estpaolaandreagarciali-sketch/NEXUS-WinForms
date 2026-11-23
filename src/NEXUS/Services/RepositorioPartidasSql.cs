using System.Data;
using Microsoft.Data.SqlClient;
using NEXUS.Models;

namespace NEXUS.Services;

/// <summary>Persistencia SQL Server para el estado serializable de la partida.</summary>
public sealed class RepositorioPartidasSql
{
    private readonly string connectionString;

    public RepositorioPartidasSql(string connectionString)
    {
        this.connectionString = string.IsNullOrWhiteSpace(connectionString)
            ? throw new ArgumentException("La cadena de conexión es obligatoria.", nameof(connectionString))
            : connectionString;
    }

    public async Task<Guid> GuardarAsync(string nombreCadete, string estadoJson, Guid? partidaId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(nombreCadete) || nombreCadete.Length > 14)
            throw new ArgumentException("El nombre debe tener entre 1 y 14 caracteres.", nameof(nombreCadete));
        try { using var _ = System.Text.Json.JsonDocument.Parse(estadoJson); }
        catch (System.Text.Json.JsonException ex)
        {
            throw new ArgumentException("El estado de partida debe ser JSON válido.", nameof(estadoJson), ex);
        }

        Guid id = partidaId ?? Guid.NewGuid();
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        const string sql = """
            MERGE dbo.Partida AS destino
            USING (SELECT @PartidaId AS PartidaId) AS origen ON destino.PartidaId = origen.PartidaId
            WHEN MATCHED THEN UPDATE SET NombreCadete=@Nombre, EstadoJson=@Estado, ActualizadaUtc=SYSUTCDATETIME()
            WHEN NOT MATCHED THEN INSERT (PartidaId, NombreCadete, EstadoJson) VALUES (@PartidaId, @Nombre, @Estado);
            """;
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@PartidaId", SqlDbType.UniqueIdentifier).Value = id;
        command.Parameters.Add("@Nombre", SqlDbType.NVarChar, 14).Value = nombreCadete;
        command.Parameters.Add("@Estado", SqlDbType.NVarChar, -1).Value = estadoJson;
        await command.ExecuteNonQueryAsync(cancellationToken);
        return id;
    }

    public async Task<(Guid Id, string Nombre, string EstadoJson)?> CargarAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(
            "SELECT PartidaId, NombreCadete, EstadoJson FROM dbo.Partida WHERE PartidaId=@Id", connection);
        command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = id;
        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;
        return (reader.GetGuid(0), reader.GetString(1), reader.GetString(2));
    }

    public async Task<(Guid Id, string Nombre, string EstadoJson)?> CargarUltimaAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("SELECT TOP (1) PartidaId, NombreCadete, EstadoJson FROM dbo.Partida ORDER BY ActualizadaUtc DESC", connection);
        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;
        return (reader.GetGuid(0), reader.GetString(1), reader.GetString(2));
    }
}
