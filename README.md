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

| RID | Режим | Розмір publish | Файлів | Потрібен runtime |
|-----|-------|----------------|--------|------------------|
| win-x64 | self-contained | 78 MB | ~200 | Ні |
| win-x64 | framework-dependent | 229 KB | ~200 | Так (.NET 10) |
| win-x64 | self-contained + SingleFile | 71 MB | 3 | Ні |
| win-x64 | self-contained + Trimmed | 20 MB | 31 | Ні |

**Self-contained** містить копію .NET runtime — застосунок працює без встановленого .NET, але займає багато місця.

**Framework-dependent** містить лише код і залежності — каталог малий, але на машині користувача має бути встановлений .NET 10.

**SingleFile** об'єднує всі `.dll` в один `.exe` — зручно копіювати, але перший запуск повільніший.

**Trimmed** видаляє невикористаний код — розмір менший у 4 рази, але небезпечний для коду з рефлексією.

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
## Доменна модель (lab04)

### Сутності

**BookCopy** (примірник книги) — головна сутність зі станом IsIssued:

    public sealed class BookCopy
    {
        private bool _isIssued;
        public string Id { get; }
        public string Isbn { get; }
        public string Title { get; }
        public int Year { get; }
        public bool IsIssued => _isIssued;
        
        private BookCopy(...) { ... }              // приватний конструктор
        public static BookCopy Create(...) { }      // фабричний метод
        public void Issue() { ... }                 // видати
        public void Return() { ... }                // повернути
        public BookDto ToDto() => ...;
        public static BookCopy FromDto(BookDto dto) => ...;
    }

**Loan** (видача) — сутність з датами і станом IsClosed:

    public sealed class Loan
    {
        private DateTime? _returnedOn;
        public string Id { get; }
        public BookCopy Copy { get; }
        public string ReaderId { get; }
        public DateTime IssuedOn { get; }
        public DateTime? ReturnedOn => _returnedOn;
        public bool IsClosed => _returnedOn.HasValue;
        
        private Loan(...) { ... }                      // приватний конструктор
        public static Loan Open(...) { }                // фабричний метод
        public void Close(DateTime returnedOn) { ... }  // закрити
    }

### Перелік інваріантів

| № | Правило | Тип винятку | Де перевіряється |
|---|---------|-------------|------------------|
| 1 | Ідентифікатор примірника не порожній | ArgumentException | BookCopy.Create |
| 2 | ISBN не порожній | ArgumentException | BookCopy.Create |
| 3 | Назва не порожня | ArgumentException | BookCopy.Create |
| 4 | Рік у межах 1450...поточний | ArgumentOutOfRangeException | BookCopy.Create |
| 5 | Не видати вже виданий примірник | InvalidOperationException | BookCopy.Issue |
| 6 | Не повернути невиданий примірник | InvalidOperationException | BookCopy.Return |
| 7 | Ідентифікатор видачі не порожній | ArgumentException | Loan.Open |
| 8 | Примірник обов'язковий | ArgumentNullException | Loan.Open |
| 9 | Ідентифікатор читача не порожній | ArgumentException | Loan.Open |
| 10 | Не закрити вже закриту видачу | InvalidOperationException | Loan.Close |
| 11 | Дата повернення не раніше дати видачі | ArgumentOutOfRangeException | Loan.Close |

### Принципи

- **Інкапсуляція:** приватні поля (_isIssued, _returnedOn), публічні властивості лише для читання ({ get; }, => _field)
- **Фабричний метод:** BookCopy.Create і Loan.Open — єдині точки створення; конструктор приватний
- **Інваріанти:** перевіряються ДО зміни стану; якщо виняток — стан не змінюється
- **Типи винятків:** ArgumentException — для вхідних аргументів, InvalidOperationException — для стану, ArgumentOutOfRangeException — для діапазонів
- **Повідомлення:** містять ключ запису і конкретні числа, щоб зрозуміти причину без коду
- **Мапінг:** ToDto() / FromDto() — сутність ↔ DTO; FromDto проходить ТІ САМІ перевірки, що й Create

### Чим DTO відрізняється від сутності

**DTO** (BookDto) — це **формат даних** для зберігання і передачі (тиждень 3): record, без поведінки, без інваріантів.

**Сутність** (BookCopy) — це **модель з правилами**: class, приватні поля, методи з бізнес-назвами, інваріанти.

DTO — пасивний контейнер, сутність — активна модель, яка **сама гарантує коректність** свого стану.
### Інваріант, що охоплює дві сутності (додаткове завдання 2)

**Правило:** «Не видати примірник читачу, у якого вже 5 активних видач».

**Де:** `LoanService.OpenLoan` — сервіс, а не сутність.

**Чому НЕ в сутності:**

1. **Сутність `Loan` не знає про інші видачі** — вона описує одну видачу. Щоб перевірити ліміт, треба мати доступ до **всієї колекції видач** — це виходить за межі однієї сутності.

2. **Сутність `BookCopy` не знає про читача** — примірник не має посилання на `Reader`. Він не може перевірити, скільки видач у конкретного читача.

3. **Це координація між сутностями** — `LoanService` координує `Loan` + колекцію видач + ліміт. Це відповідальність сервісу.

4. **На тижні 5 з'явиться `CatalogService`** — він теж координуватиме операції. `LoanService` — заготовка під нього.

**Переваги сервісу:**
- ✅ Одна точка перевірки — не дублюється в Cli, Api, Blazor
- ✅ Легко змінити ліміт (`MaxActiveLoansPerReader`)
- ✅ Легко тестувати (можна передати різні колекції)
- ✅ Сутності залишаються простими (кожна відповідає за своє)

## Сервісний шар (lab05)

**Інтерфейс:** `IBookStore` — контракт сховища з методами `List`, `GetById`, `Add`, `Update`, `Remove`.

**Реалізації:**
- `InMemoryBookStore` — зберігає дані у `Dictionary` у пам'яті
- `FileBookStore` — зберігає дані у JSON-файлі `catalog.json`

**Сервіс:** `LendingService` — бізнес-операції `AddBook`, `IssueCopy`, `ReturnCopy`, `All`, `Find`.

**Схема залежностей:**
Cli → LendingService → IBookStore → InMemory | File

**Composition Root (у `Program.cs`):**

```csharp
IBookStore store = useFile
    ? new FileBookStore(dataPath)
    : new InMemoryBookStore(SampleData.Books());

var service = new LendingService(store);
```

Що зміниться на тижні 11 (БД): достатньо створити EfBookStore : IBookStore і змінити один рядок у Program.cs для вибору реалізації. LendingService при цьому не змінюється, оскільки залежить від інтерфейсу, а не від конкретного класу.