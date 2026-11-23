/* NEXUS — esquema inicial para SQL Server. Ejecutar en SSMS. */
IF DB_ID(N'NEXUS') IS NULL CREATE DATABASE NEXUS;
GO
USE NEXUS;
GO
IF OBJECT_ID(N'dbo.Partida', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Partida (
        PartidaId uniqueidentifier NOT NULL CONSTRAINT PK_Partida PRIMARY KEY,
        NombreCadete nvarchar(14) NOT NULL,
        EstadoJson nvarchar(max) NOT NULL,
        CreadaUtc datetime2(0) NOT NULL CONSTRAINT DF_Partida_Creada DEFAULT SYSUTCDATETIME(),
        ActualizadaUtc datetime2(0) NOT NULL CONSTRAINT DF_Partida_Actualizada DEFAULT SYSUTCDATETIME(),
        CONSTRAINT CK_Partida_Json CHECK (ISJSON(EstadoJson) = 1)
    );
    CREATE INDEX IX_Partida_ActualizadaUtc ON dbo.Partida(ActualizadaUtc DESC);
END;
GO
