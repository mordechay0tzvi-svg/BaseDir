# שלב 1: Base Project - הבנת הבעיה

## מטרת השלב

להבין את הבעיה עם try/catch חוזר בכל Controller action ולהכיר את ההבדל בין תוצאות HTTP רגילות לבין Exceptions.

**משך זמן משוער:** 45-60 דקות

---

## מה יש בפרויקט הזה?

פרויקט ASP.NET Core Web API פשוט לניהול ספרים (Books).

**מבנה הפרויקט:**
```
01-base-project/
├── Program.cs                      # נקודת הכניסה, הגדרת Services ו-Pipeline
├── Controllers/
│   └── BooksController.cs          # 4 endpoints עם try/catch
├── Models/
│   └── Book.cs                     # המודל Book
├── Repositories/
│   ├── IBookRepository.cs          # ממשק Repository
│   └── InMemoryBookRepository.cs   # מימוש In-Memory
└── exception-handling-lab.csproj   # קובץ הפרויקט
```

---

## התחלה מהירה

### 1. העתק את הפרויקט

```bash
cp -r 01-base-project/ my-books-api/
cd my-books-api/
```

### 2. הרץ את הפרויקט

```bash
dotnet run
```

אמור להופיע:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

### 3. בדוק שהפרויקט עובד

פתח Postman / Thunder Client ובדוק:

**GET** `http://localhost:5000/api/books`
```json
[
  {
    "id": 1,
    "title": "Clean Code",
    "author": "Robert C. Martin",
    "year": 2008
  },
  {
    "id": 2,
    "title": "Design Patterns",
    "author": "Gang of Four",
    "year": 1994
  }
]
```

אם קיבלת את הרשימה - הפרויקט עובד!

---

## הכרת הפרויקט

### Book.cs - המודל

```csharp
public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public int Year { get; set; }
}
```

מודל פשוט שמייצג ספר.

---

### IBookRepository - הממשק

```csharp
public interface IBookRepository
{
    List<Book> GetAll();
    Book? GetById(int id);
    Book Add(Book book);
    Book? Update(int id, Book book);
}
```

**למה Repository Pattern?**
- מפריד בין לוגיקת הגישה לנתונים לבין ה-Controller
- מאפשר להחליף את המימוש (למשל, לעבור מ-In-Memory ל-Database)
- מקל על בדיקות (testing)

---

### InMemoryBookRepository - המימוש

```csharp
public class InMemoryBookRepository : IBookRepository
{
    private readonly List<Book> _books = new()
    {
        new Book { Id = 1, Title = "Clean Code", Author = "Robert C. Martin", Year = 2008 },
        new Book { Id = 2, Title = "Design Patterns", Author = "Gang of Four", Year = 1994 },
        new Book { Id = 3, Title = "Refactoring", Author = "Martin Fowler", Year = 1999 }
    };
    private int _nextId = 4;

    public List<Book> GetAll() => _books;

    public Book? GetById(int id) => _books.FirstOrDefault(b => b.Id == id);

    public Book Add(Book book)
    {
        book.Id = _nextId++;
        _books.Add(book);
        return book;
    }

    public Book? Update(int id, Book book)
    {
        var existing = GetById(id);
        if (existing == null) return null;

        existing.Title = book.Title;
        existing.Author = book.Author;
        existing.Year = book.Year;
        return existing;
    }
}
```

**שים לב:**
- `GetById` מחזיר `null` אם הספר לא נמצא
- `Update` מחזיר `null` אם הספר לא קיים
- אין זריקת Exceptions כאן (בשלב זה)

---

### Program.cs - הגדרת האפליקציה

```csharp
var builder = WebApplication.CreateBuilder(args);

// הוספת Services
builder.Services.AddControllers();
builder.Services.AddSingleton<IBookRepository, InMemoryBookRepository>();

var app = builder.Build();

// הגדרת HTTP Pipeline
app.MapControllers();

app.Run();
```

**מה קורה כאן:**

**`builder.Services.AddControllers()`**
→ מוסיף תמיכה ב-Controllers

**`builder.Services.AddSingleton<IBookRepository, InMemoryBookRepository>()`**
→ רושם את ה-Repository ב-Dependency Injection כ-**Singleton**
→ **למה Singleton?** כי ה-Repository מכיל רשימה (`List<Book>`) שצריכה להישמר בין בקשות
→ אם היינו משתמשים ב-`AddScoped` או `AddTransient`, הנתונים היו נמחקים אחרי כל בקשה

**`app.MapControllers()`**
→ מפה את כל ה-Controllers ל-endpoints

---

## ⚠️ עיקרון קריטי: תוצאות צפויות vs חריגות

### הכלל החשוב ביותר

**לא כל שגיאת HTTP היא Exception!**

```text
תוצאה צפויה של endpoint
        ↓
Controller Result (Ok, NotFound, BadRequest)

כשל לא צפוי/חריג
        ↓
Exception
        ↓
Global Exception Handling
```

### דוגמאות נכונות

#### ✅ ספר לא נמצא - תוצאה צפויה

```csharp
var book = _repository.GetById(id);
if (book == null)
    return NotFound($"Book with ID {id} not found");
```

**למה?** משתמש שמבקש ספר שלא קיים זה **מצב עסקי רגיל**, לא שגיאה טכנית.

#### ✅ Validation נכשל - תוצאה צפויה

```csharp
if (string.IsNullOrWhiteSpace(book.Title))
    return BadRequest("Title is required");
```

**למה?** משתמש ששולח נתונים לא תקינים זה **תרחיש צפוי**, לא חריג.

#### ✅ בעיה בחיבור ל-DB - Exception אמיתי

```csharp
try
{
    var books = _repository.GetAll();
    return Ok(books);
}
catch (Exception ex)
{
    // זה חריג אמיתי - משהו השתבש בצורה לא צפויה
    return StatusCode(500, new { error = "An error occurred" });
}
```

**למה?** בעיה בחיבור, קובץ לא נגיש, או bug בקוד - אלה **כשלים לא צפויים**.

### ❌ דוגמאות שגויות

```csharp
// ❌ לא נכון!
var book = _repository.GetById(id);
if (book == null)
    throw new BookNotFoundException(id);  // זה לא Exception!
```

```csharp
// ❌ לא נכון!
if (string.IsNullOrWhiteSpace(book.Title))
    throw new ValidationException("Title required");  // זה לא Exception!
```

### המודל הנכון

```text
האם הבקשה הניבה תוצאה צפויה?
        │
        ├── כן
        │
        ↓
Controller מחזיר HTTP result מתאים
        │
        ├── Ok(200)
        ├── NotFound(404)
        ├── BadRequest(400)
        └── Created(201)
        
האם קרה משהו חריג/לא צפוי?
        │
        └── כן
             ↓
          Exception
             ↓
       Global Exception Handling
             ↓
       תשובה בטוחה (500)
```

### למה זה חשוב?

1. **ביצועים** - Exceptions יקרים מבחינת ביצועים
2. **סמנטיקה** - Exception = משהו השתבש, לא "לא מצאתי"
3. **ארכיטקטורה** - Repository לא צריך לדעת על HTTP
4. **Best Practices** - זה התקן ב-ASP.NET Core

### הערה על ארכיטקטורות מתקדמות

באפליקציות גדולות עם שכבות נוספות:

```text
Controller → Service Layer → Repository
```

ב-Service Layer, **לפעמים** מתאים להשתמש ב-Application Exceptions:

```csharp
// Service Layer
public Book GetBook(int id)
{
    var book = _repository.GetById(id);
    if (book == null)
        throw new BookNotFoundException(id);  // OK בשכבת Service
    return book;
}

// Controller
[HttpGet("{id}")]
public IActionResult GetById(int id)
{
    var book = _service.GetBook(id);  // Exception יתפס ב-Global Handler
    return Ok(book);
}

// Global Handler
if (exception is BookNotFoundException)
    return 404;
```

**אבל** בארכיטקטורה הפשוטה שלנו (Controller + Repository), **אין צורך** ב-BookNotFoundException.

---

## 🧪 בדיקת הפרויקט עם Swagger

### הרצת הפרויקט

1. **הרץ את הפרויקט:**
   ```bash
   dotnet run
   ```

2. **פתח דפדפן וגש ל-Swagger UI:**
   ```
   https://localhost:5001/swagger
   ```
   או
   ```
   http://localhost:5000/swagger
   ```

---

## בואו נבחן את BooksController

פתח את `Controllers/BooksController.cs` ותראה 4 endpoints:

### 1. GET /api/books - קבלת כל הספרים

```csharp
[HttpGet]
public IActionResult GetAll()
{
    try
    {
        var books = _repository.GetAll();
        return Ok(books);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { error = "An error occurred while retrieving books" });
    }
}
```

**מה קורה:**
- קורא ל-`_repository.GetAll()`
- אם הכל תקין → מחזיר `Ok(books)` (Status 200)
- אם יש Exception → מחזיר Status 500

**שאלה:** האם באמת צריך try/catch כאן?  
→ `GetAll()` לא זורק Exceptions בדרך כלל, אבל אם יהיה bug בקוד...

---

### 2. GET /api/books/{id} - קבלת ספר לפי ID

```csharp
[HttpGet("{id}")]
public IActionResult GetById(int id)
{
    try
    {
        var book = _repository.GetById(id);
        if (book == null)
            return NotFound($"Book with ID {id} not found" );
        
        return Ok(book);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { error = "An error occurred while retrieving the book" });
    }
}
```

**שים לב לזרימה:**

```
Repository.GetById(id)
    ↓
null?
    ↓ כן
NotFound() → 404
    ↓ לא
Ok(book) → 200
```

**זו תוצאה צפויה, לא Exception!**

ספר לא נמצא → זה לא באג, זה תרחיש לגיטימי.  
לכן אנחנו מטפלים בזה עם `if` רגיל, לא עם Exception.

---

### 3. POST /api/books - יצירת ספר חדש

```csharp
[HttpPost]
public IActionResult Create([FromBody] Book book)
{
    try
    {
        if (string.IsNullOrWhiteSpace(book.Title))
            return BadRequest("Title is required");

        if (string.IsNullOrWhiteSpace(book.Author))
            return BadRequest("Author is required");

        if (book.Year < 1000 || book.Year > DateTime.Now.Year)
            return BadRequest("Invalid year");

        var created = _repository.Add(book);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { error = "An error occurred while creating the book" });
    }
}
```

**Validation Logic:**

אנחנו בודקים:
- Title לא ריק
- Author לא ריק
- Year בטווח סביר

**אלו גם תוצאות צפויות!**

משתמש שלח נתונים לא תקינים → `BadRequest()` (400), לא Exception.

**`CreatedAtAction`:**
```csharp
return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
```
→ מחזיר Status 201 (Created)  
→ מוסיף Header: `Location: /api/books/{id}`  
→ מחזיר את הספר שנוצר ב-body

---

### 4. PUT /api/books/{id} - עדכון ספר

```csharp
[HttpPut("{id}")]
public IActionResult Update(int id, [FromBody] Book book)
{
    try
    {
        if (string.IsNullOrWhiteSpace(book.Title))
            return BadRequest("Title is required");

        if (string.IsNullOrWhiteSpace(book.Author))
            return BadRequest("Author is required");

        var updated = _repository.Update(id, book);
        if (updated == null)
            return NotFound($"Book with ID {id} not found");

        return Ok(updated);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { error = "An error occurred while updating the book" });
    }
}
```

דומה ל-Create, אבל:
- אם הספר לא קיים → `NotFound()` (404)
- אם הכל תקין → `Ok(updated)` (200)

---

## בדיקת ה-Endpoints

### בדיקה 1: קבלת כל הספרים

**Request:**
```
GET http://localhost:5000/api/books
```

**Expected Response:** 200 OK
```json
[
  { "id": 1, "title": "Clean Code", "author": "Robert C. Martin", "year": 2008 },
  { "id": 2, "title": "Design Patterns", "author": "Gang of Four", "year": 1994 },
  { "id": 3, "title": "Refactoring", "author": "Martin Fowler", "year": 1999 }
]
```

---

### בדיקה 2: קבלת ספר קיים

**Request:**
```
GET http://localhost:5000/api/books/1
```

**Expected Response:** 200 OK
```json
{
  "id": 1,
  "title": "Clean Code",
  "author": "Robert C. Martin",
  "year": 2008
}
```

---

### בדיקה 3: קבלת ספר שלא קיים

**Request:**
```
GET http://localhost:5000/api/books/999
```

**Expected Response:** 404 Not Found
```json
{
  "error": "Book with ID 999 not found"
}
```

**שים לב:** זו **לא** שגיאה! זו תוצאה צפויה.

---

### בדיקה 4: יצירת ספר חדש

**Request:**
```
POST http://localhost:5000/api/books
Content-Type: application/json

{
  "title": "The Pragmatic Programmer",
  "author": "Andrew Hunt",
  "year": 1999
}
```

**Expected Response:** 201 Created
```json
{
  "id": 4,
  "title": "The Pragmatic Programmer",
  "author": "Andrew Hunt",
  "year": 1999
}
```

**Headers:**
```
Location: http://localhost:5000/api/books/4
```

---

### בדיקה 5: יצירת ספר עם נתונים לא תקינים

**Request:**
```
POST http://localhost:5000/api/books
Content-Type: application/json

{
  "title": "",
  "author": "Andrew Hunt",
  "year": 1999
}
```

**Expected Response:** 400 Bad Request
```json
{
  "error": "Title is required"
}
```

**שוב:** זו **לא** שגיאה! זו תוצאה צפויה.

---

### בדיקה 6: עדכון ספר

**Request:**
```
PUT http://localhost:5000/api/books/1
Content-Type: application/json

{
  "title": "Clean Code (2nd Edition)",
  "author": "Robert C. Martin",
  "year": 2020
}
```

**Expected Response:** 200 OK
```json
{
  "id": 1,
  "title": "Clean Code (2nd Edition)",
  "author": "Robert C. Martin",
  "year": 2020
}
```

---

## הבעיה עם הקוד הנוכחי

עכשיו שראינו את הפרויקט, בואו נזהה את הבעיות:

### בעיה 1: כפילות קוד

```csharp
catch (Exception ex)
{
    return StatusCode(500, new { error = "..." });
}
```

**הבלוק הזה חוזר על עצמו 4 פעמים!**

מה אם נרצה לשנות את פורמט השגיאה?  
→ צריך לשנות ב-4 מקומות

מה אם יש לנו 20 endpoints?  
→ 20 בלוקים זהים

---

### בעיה 2: אחריות מעורבת

ה-Controller עוסק ב:
- קבלת הבקשה
- קריאה ל-Repository
- החזרת תשובה
- טיפול טכני בחריגות (בעיה)

**טיפול בחריגות הוא אחריות cross-cutting** - היא רלוונטית לכל ה-Controllers.

---

### בעיה 3: קשה לתחזוקה

מה אם נרצה:
- לרשום (log) את כל השגיאות?
- לשלוח התראה כשיש שגיאה?
- להחזיר פורמט שונה (למשל ProblemDetails)?

→ צריך לשנות בכל Controller action בנפרד.

---

### בעיה 4: אי-עקביות

מפתח אחד כותב:
```csharp
return StatusCode(500, new { error = "..." });
```

מפתח אחר כותב:
```csharp
return StatusCode(500, "Error occurred");
```

מפתח שלישי שוכח try/catch לגמרי...

→ אין עקביות באפליקציה.

---

## מה אנחנו רוצים להשיג?

במקום:
```csharp
[HttpGet("{id}")]
public IActionResult GetById(int id)
{
    try
    {
        var book = _repository.GetById(id);
        if (book == null)
            return NotFound();
        return Ok(book);
    }
    catch (Exception ex)
    {
        return StatusCode(500, "...");
    }
}
```

נרצה:
```csharp
[HttpGet("{id}")]
public IActionResult GetById(int id)
{
    var book = _repository.GetById(id);
    if (book == null)
        return NotFound();
    return Ok(book);
}
```

**והטיפול בחריגות יקרה אוטומטית במקום מרכזי!**

---

## סיכום השלב

### מה ראינו?

- פרויקט ASP.NET Core Web API בסיסי
- Repository Pattern עם Dependency Injection
- Controllers עם try/catch חוזר
- ההבדל בין תוצאות HTTP רגילות (`NotFound`, `BadRequest`) לבין Exceptions  

### מה הבעיה?

- כפילות קוד (try/catch בכל action)
- אחריות מעורבת (Controller מטפל בחריגות טכניות)
- קשה לתחזוקה (שינוי אחד = שינוי בהרבה מקומות)
- אי-עקביות (כל מפתח עושה אחרת)  

### מה הלאה?

בשלב הבא נעביר את הטיפול בחריגות ל-**Middleware מרכזי**.

---

## קישורים למידע נוסף

- [ASP.NET Core Controllers](https://learn.microsoft.com/en-us/aspnet/core/web-api/)
- [Action Results](https://learn.microsoft.com/en-us/aspnet/core/web-api/action-return-types)
- [Dependency Injection](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection)
- [Repository Pattern](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)

---

**מוכן? עבור לשלב 2: `02-middleware-refactoring/`**

---
