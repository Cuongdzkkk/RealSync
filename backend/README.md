# Backend — RealSync API

> ASP.NET Core 8 Web API

## Project Structure
```
backend/
├── src/
│   ├── RealSync.Api/         # Controllers, Middlewares, Hubs
│   ├── RealSync.Core/        # Domain Models, Interfaces
│   ├── RealSync.Services/    # Business Logic
│   ├── RealSync.Data/        # EF Core, Migrations
│   └── RealSync.Shared/      # DTOs, Constants, Helpers
└── tests/
    ├── RealSync.UnitTests/
    └── RealSync.IntegrationTests/
```

## Quick Start
```bash
cd src/RealSync.Api
dotnet restore
dotnet run
```

From the `backend/` directory, run the API with:

```bash
dotnet run --project src/RealSync.Api/RealSync.Api.csproj
```

## API Docs
- Swagger: `https://localhost:5001/swagger`

## Conventions
See `../agent-skills/SKILL.md` → Section 4 (Backend Rules)
