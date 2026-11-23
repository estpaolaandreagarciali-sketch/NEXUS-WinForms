USE NEXUS;
GO
SELECT PartidaId, NombreCadete, CreadaUtc, ActualizadaUtc,
       ISJSON(EstadoJson) AS EstadoJsonValido
FROM dbo.Partida ORDER BY ActualizadaUtc DESC;
GO
