# EF Core

## Migrations

### Add migration

```shell
migration="Init"

cd src
dotnet ef migrations add $migration -s SpinRallyBot -p SpinRallyBot.Database.Sqlite -- --provider Sqlite
dotnet ef migrations add $migration -s SpinRallyBot -p SpinRallyBot.Database.Postgres -- --provider Postgres
```

### Remove last migration

```shell
cd src
dotnet ef migrations remove -s SpinRallyBot -p SpinRallyBot.Database.Sqlite -- --provider Sqlite
dotnet ef migrations remove -s SpinRallyBot -p SpinRallyBot.Database.Postgres -- --provider Postgres
```