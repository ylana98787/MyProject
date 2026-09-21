# MyProject

Наскрізний проєкт з крос-платформного програмування.

## Предметна область

**Бібліотека**

Сутності:
- **Book** (видання) — книга з назвою, автором, ISBN, роком видання
- **BookCopy** (примірник) — конкретний екземпляр книги з інвентарним номером
- **Reader** (читач) — користувач бібліотеки з даними для реєстрації
- **Loan** (видача) — запис про видачу книги читачеві з датами видачі та повернення

Призначення: облік видач примірників книг читачам та їх повернень.

## Структура проєкту (після lab02)

```
MyProject/
├── .git/
├── .gitignore
├── MyProject.slnx
├── README.md
└── src/
    ├── Core/
    │   ├── Core.csproj
    │   └── EnvironmentInfo.cs
    └── Cli/
        ├── CLI.csproj
        └── Program.cs
```

Залежність одностороння: **Cli → Core**.

## Команди

```bash
dotnet build
dotnet run --project src/Cli
dotnet publish src/Cli -c Release -r win-x64 --self-contained true
dotnet publish src/Cli -c Release -r win-x64 --self-contained false
./src/Cli/bin/Release/net10.0/win-x64/publish/CLI.exe
```

## Порівняння режимів публікації

| RID | Режим | Розмір | Потрібен runtime |
|-----|-------|--------|------------------|
| win-x64 | self-contained | ~78 MB | Ні |
| win-x64 | framework-dependent | ~229 KB | Так (.NET 10) |

## Multi-targeting Core

`Core.csproj` використовує `<TargetFrameworks>net8.0;net10.0</TargetFrameworks>`.

## Архітектура Core

- `EnvironmentReport` — record для даних.
- `EnvironmentInfo` — static class для поведінки.
- `DetectRid()` — вручну збирає RID.

## Додаткове завдання

```bash
dotnet run --project src/Cli -- --json
```
