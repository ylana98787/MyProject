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

## Запуск

```bash
dotnet build
dotnet run --project src/Cli
```

## Середовище

- .NET SDK 10.0
- Windows 11 x64
- RID: win-x64

## Структура проєкту

```
MyProject/
├── .git/
├── .gitignore
├── MyProject.slnx
├── README.md
└── src/
    └── Cli/
        ├── Cli.csproj
        └── Program.cs
```

## Додаткове завдання

Підтримка аргументу командного рядка `--json`:

```bash
dotnet run --project src/Cli -- --json
```

## Self-contained публікації

| RID | Розмір |
|-----|--------|
| `win-x64` | 78 MB |
| `linux-x64` | 80 MB |

```bash
dotnet publish src/Cli -c Release -r win-x64 --self-contained true
dotnet publish src/Cli -c Release -r linux-x64 --self-contained true
```

Запуск бінарника без `dotnet run` (Windows):

```bash
./src/Cli/bin/Release/net10.0/win-x64/publish/Cli.exe
```
