# NEXUS — Sistema de Exploración Interdimensional

Juego de exploración, supervivencia y combate ambientado en 2297. Esta versión utiliza C# y Windows Forms; incluye menú, mapa con niebla de guerra, inventario, entidades y la consola de IRIS.

## Requisitos

- Windows
- .NET 10 SDK con soporte para Windows Forms, o Visual Studio con la carga de trabajo de escritorio .NET

## Ejecutar

Abre `src/NEXUS/NEXUS.csproj` en Visual Studio y ejecuta el proyecto. Desde PowerShell también puedes compilar y arrancar la app:

```powershell
dotnet build .\src\NEXUS\NEXUS.csproj
dotnet run --project .\src\NEXUS\NEXUS.csproj
```

## Estructura

```text
NEXUS-WinForms/
├── archive/
│   ├── legacy-console/       # Respaldo histórico; no se compila
│   └── legacy-winforms/      # Respaldo histórico; no se compila
├── docs/
│   └── GUIA_DEL_JUEGO.md
├── src/
│   └── NEXUS/
│       ├── Forms/            # Interfaz WinForms
│       ├── Models/           # Personajes, objetos, misiones y realidades
│       ├── Services/         # Motor del juego e integración de IRIS
│       ├── NEXUS.csproj
│       └── Program.cs
└── .gitignore
```

La guía completa de controles y misión está en [docs/GUIA_DEL_JUEGO.md](docs/GUIA_DEL_JUEGO.md). Los respaldos históricos quedan aislados en `archive/` y no forman parte del proyecto compilable.