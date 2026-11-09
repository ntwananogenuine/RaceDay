# RaceDay – Part 2: RESTful API

ASP.NET Core Web API (C#) with Entity Framework Core (Code-First) on SQL Server LocalDB.

## Project structure
- `raceday_api/Controllers` – the request methods (GET, POST, PUT, DELETE)
- `raceday_api/database` – the classes that store, get, update and delete the data (+ `AppDbContext`)
- `raceday_api/Models` – the tables (Roles, Users, Events, Categories, Enrolments, Results)
- `raceday_api/Auth` – session and role checking
- `raceday_tests` – unit tests (xUnit)
- `RaceDay.sql` – the database script (RaceDayDB)

## Setup and run
1. Install Visual Studio with the ASP.NET workload (includes SQL Server LocalDB) and the .NET 8 SDK.
2. Make sure the LocalDB instance exists: `sqllocaldb create api_web` then `sqllocaldb start api_web`.
3. Create the database: run `part2/RaceDay.sql` against `(localdb)\api_web` (creates `RaceDayDB`).
4. The connection string (`raceday_api/appsettings.json`) is `Server=(localdb)\api_web;Database=RaceDayDB;...`
5. Run `raceday_api` and open `https://localhost:7080/swagger`. On start-up the API links to `RaceDayDB`
   (and creates it if it does not exist yet). Every registered user is saved in `dbo.Users`.
6. Run tests: `dotnet test part2/RaceDay.sln`.

## Image storage (Azure Blob Storage)
The API stores event banners and profile pictures in Azure Blob Storage.
Add `AzureBlobStorage:ConnectionString` (and `ContainerName`, default `raceday-images`) with **Manage User Secrets** on `raceday_api`.
Endpoints: `POST/GET /api/events/{id}/banner`, `POST/GET /api/users/me/picture`.
Run the API with the `http` profile (`http://localhost:5080`) so the Part 3 MVC project can reach it.

## Roles
- **Organiser** – creates, updates and deletes their own events and categories, views enrolments, and captures/edits results.
- **Participant** – browses events and categories, enrols, manages their own enrolments, and views leaderboards and their own results.

## Authentication
Session-based. `POST /api/auth/login` stores the user id and role in the server session (cookie). Passwords are hashed with BCrypt.

## CI/CD
![Green build](docs/ci-green-build.png)  <!-- add your screenshot here -->

## Demo video
YouTube (unlisted): <paste link>
