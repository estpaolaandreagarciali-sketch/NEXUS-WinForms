# NEXUS – Sistema de Exploración

Juego de exploración, supervivencia y combate desarrollado en C# con Windows Forms. El cadete explora realidades, investiga anomalías, administra recursos y busca completar la misión.

## Requisitos

- Windows
- Visual Studio con soporte para desarrollo de escritorio con .NET, o .NET 10 SDK

## Ejecutar

Abre `src/NEXUS/NEXUS.csproj` en Visual Studio y ejecuta el proyecto. También puedes compilar desde una terminal con el SDK de .NET:

```powershell
dotnet build .\src\NEXUS\NEXUS.csproj
dotnet run --project .\src\NEXUS\NEXUS.csproj
```

El proyecto apunta a `net10.0-windows` y requiere Windows para ejecutar la interfaz WinForms.

## Estructura

```text
NEXUS-WinForms/
├── docs/
│   └── GUIA_DEL_JUEGO.md
├── src/
│   └── NEXUS/
│       ├── Forms/       # Interfaz gráfica
│       ├── Models/      # Modelos de juego
│       ├── Services/    # Lógica y motor del juego
│       ├── NEXUS.csproj
│       └── Program.cs   # Punto de entrada
├── .gitignore
└── README.md
```

## Documentación

Consulta [la guía del juego](docs/GUIA_DEL_JUEGO.md) para conocer los objetivos, controles y mecánicas.
