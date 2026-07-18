# InspectionTracker

Full-stack technical exercise: equipment inspection tracking for field crews.
Built with .NET 8, ASP.NET Core Web API, EF Core (SQLite), JWT auth, and Angular 20,
following Clean Architecture and TDD.

## User story

> As a field crew supervisor, I want to record and manage equipment inspection
> results, so that I can track which assets are safe to operate and follow up on failures.

Key business rules (all enforced in the business layer and covered by tests):
- An inspection date cannot be in the future
- A **Failed** inspection must include notes
- Only the **creator** of an inspection can update or delete it
- The inspection list is public; creating/editing requires login

## Architecture

```
src/
├── InspectionTracker.Domain                    # Entities, enums 
├── InspectionTracker.Application               # Business rules, DTOs, interfaces
├── InspectionTracker.Infrastructure            # EF Core + SQLite, repositories, JWT
└── InspectionTracker.WebApi                    # Controllers, middleware
tests/
├── InspectionTracker.Application.Tests         # Business rules 
├── InspectionTracker.Infrastructure.Tests      # Repositories
└── InspectionTracker.WebApi.Tests              # API integration
client/                                         # Angular 20
```

## Prerequisites

- .NET 8 SDK
- Node.js 22+ and Angular CLI 20

## Running

**API** (terminal 1):

    dotnet run --project src/InspectionTracker.WebApi

Runs at `http://localhost:5264`,  Swagger UI at `http://localhost:5264/swagger`.
The SQLite database is created and seeded automatically on first run.

**Client** (terminal 2):

    cd client
    npm install
    ng serve

Open `http://localhost:4200`.

If your API starts on a different port, update `client/src/environments/environment.ts`.


## Demo credentials

| Email | Password | Notes |
|---|---|---|
| demo@inspectiontracker.com | Demo1234! | Seeded; owns the sample inspections 
| demo2@inspectiontracker.com | Demo1234! | Seeded;


## Tests

    dotnet test

32 tests across the three layers. TDD workflow is visible in the commit history

## AI Exercise

 📄 The Generative AI exercise (prompt, output review, and corrections) is in
 [docs/genai-exercise.md](docs/genai-exercise.md).



  
