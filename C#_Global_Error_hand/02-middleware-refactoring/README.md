# שלב 2: Refactoring ל-Exception Handling Middleware

## 📁 מבנה התיקייה

תיקייה זו מכילה:
- **`new-files/`** - קבצים חדשים שצריך להוסיף לפרויקט שלך מ-שלב 1
  - `Middleware/ExceptionHandlingMiddleware.cs` - Middleware לטיפול בחריגות לא צפויות
- **`solution/`** - דוגמאות לקבצים שהשתנו (לא פרויקט מלא!)
  - `Program.cs` - איך Program.cs אמור להיראות אחרי השינויים
  - `Controllers/BooksController.cs` - דוגמה לקוד נקי (משתמש ב-return NotFound/BadRequest)

**חשוב:** אתה ממשיך לעבוד על אותו פרויקט מ-שלב 1. אל תיצור פרויקט חדש!

---

##  מטרת השלב

להעביר את הטיפול בחריגות מה-Controllers ל-Middleware מרכזי, תוך הבנת:
- מה זה Middleware ואיך הוא עובד
- איך עובד ה-HTTP Pipeline של ASP.NET Core
- מה זה `HttpContext` ו-`RequestDelegate`
- איך חריגות זורמות חזרה דרך ה-Pipeline

**משך זמן משוער:** 75-90 דקות

---

##  לפני שמתחילים

ודא שיש לך את הפרויקט מ-שלב 1 (`my-books-api/`) ושהוא עובד תקין.

---

##  הרעיון

במקום שכל Controller יטפל בחריגות **לא צפויות** בנפרד (עם try/catch), אנחנו יוצרים רכיב אחד שיושב **לפני** כל ה-Controllers ב-Pipeline.

### ⚠️ חשוב להבין

Middleware **לא** מחליף את `return NotFound()` או `return BadRequest()`!

```text
תוצאות צפויות (404, 400)
        ↓
Controller returns NotFound/BadRequest
        ↓
לא עובר דרך Exception Middleware

חריגות לא צפויות (500)
        ↓
Exception נזרק
        ↓
Middleware תופס
        ↓
מחזיר 500 בטוח
```

### זרימת הבקשה (Request Flow)

```
HTTP Request
    ↓
ExceptionHandlingMiddleware (מקיף הכל ב-try/catch)
    ↓
Controller
    ↓
Repository
    ↓
HTTP Response
```

### זרימת החריגה (Exception Flow)

```
Repository → Exception לא צפוי נזרק (בעיית DB, bug, וכו')
    ↑
Controller (לא תופס, ממשיך למעלה)
    ↑
ExceptionHandlingMiddleware (תופס כאן!)
    ↓
HTTP Response 500 (בטוח, ללא פרטים רגישים)
```

**היתרון:**
- טיפול מרכזי ב-**חריגות לא צפויות**
- Controllers נקיים מ-try/catch
- עקביות בכל האפליקציה
- **Controllers ממשיכים להחזיר NotFound/BadRequest לתוצאות צפויות**

---

##  הנחיות צעד אחר צעד

### צעד 1: צור תיקייה Middleware

בתוך הפרויקט שלך (`my-books-api/`), צור תיקייה בשם `Middleware`:

```
my-books-api/
├── Controllers/
├── Models/
├── Repositories/
└── Middleware/  ← חדש
```

---

### צעד 2: צור את ExceptionHandlingMiddleware

צור קובץ חדש: `Middleware/ExceptionHandlingMiddleware.cs`

**העתק את הקוד המלא מהקובץ:**  
`02-middleware-refactoring/new-files/Middleware/ExceptionHandlingMiddleware.cs`

---

###  הסבר הקוד - שורה אחר שורה

#### מה זה Middleware?

**Middleware** הוא רכיב שיושב ב-HTTP Pipeline ויכול:
- לבדוק/לשנות את הבקשה (Request)
- להעביר את הבקשה לרכיב הבא
- לבדוק/לשנות את התשובה (Response)

**כל בקשה HTTP עוברת דרך שרשרת של Middleware components:**

```
Request → Middleware 1 → Middleware 2 → Controller → Response
```

---

#### Constructor (בנאי)

```csharp
private readonly RequestDelegate _next;
private readonly ILogger<ExceptionHandlingMiddleware> _logger;

public ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    _next = next;
    _logger = logger;
}
```

**`RequestDelegate _next`:**

`RequestDelegate` הוא delegate (מצביע לפונקציה) שמייצג את הרכיב הבא ב-Pipeline.

**מאיפה זה מגיע?**  
ASP.NET Core בונה את ה-Pipeline בזמן הרצה ומזריק אוטומטית את הרכיב הבא.

**למה צריך את זה?**  
כדי להעביר את הבקשה הלאה ל-Controller או ל-Middleware הבא.

**`ILogger<ExceptionHandlingMiddleware> _logger`:**

שירות logging מובנה של ASP.NET Core.  
נשתמש בו כדי לרשום שגיאות לצורכי debugging.

 **הערה:** לא נלמד Logging לעומק במעבדה הזו. זה נושא נפרד.

---

#### הלוגיקה המרכזית

```csharp
public async Task InvokeAsync(HttpContext context)
{
    try
    {
        await _next(context);  // ← מעביר לרכיב הבא
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "An unexpected error occurred");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new 
        { 
            error = "An unexpected error occurred. Please try again later." 
        });
    }
}
```

**איך זה עובד?**

1. **`await _next(context)`** - מעביר את הבקשה לרכיב הבא (Controller)
2. אם יש **Exception** בכל מקום ב-Pipeline - ה-`catch` תופס אותו
3. **`_logger.LogError`** - רושם את השגיאה המלאה לצורכי debugging
4. **`StatusCode = 500`** - מחזיר Internal Server Error
5. **הודעה כללית** - לא חושפים פרטים טכניים ל-client

---

### צעד 3: רשום את ה-Middleware ב-Pipeline

פתח את `Program.cs` והוסף את השורה הזו **אחרי** `var app = builder.Build();`:

```csharp
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

**הקובץ המלא אמור להיראות כך:**

```csharp
using ExceptionHandlingLab.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddSingleton<IBookRepository, InMemoryBookRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ExceptionHandlingMiddleware>();  // ← הוסף כאן
app.MapControllers();

app.Run();
```

**⚠️ חשוב:** ה-Middleware צריך להיות **לפני** `MapControllers()` כדי לתפוס Exceptions!

---

## 💡 מה השתנה ב-Controller?

**התשובה הקצרה: כמעט כלום!**

ה-Controller נשאר **בדיוק אותו דבר** כמו בשלב 1:
- ✅ `return NotFound()` לספר שלא נמצא
- ✅ `return BadRequest()` ל-validation
- ✅ `return Ok()` לתוצאות תקינות

**מה כן השתנה?**
- ❌ הסרנו את כל ה-`try/catch` blocks

### לפני (שלב 1):

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
        return StatusCode(500, "An error occurred");
    }
}
```

### אחרי (שלב 2):

```csharp
[HttpGet]
public IActionResult GetAll()
{
    var books = _repository.GetAll();
    return Ok(books);
}
```

**זהו! פשוט הסרנו את ה-try/catch.**

**למה זה עובד?**
- אם יש Exception לא צפוי (bug, בעיית DB, וכו') - ה-Middleware יתפוס אותו
- תוצאות צפויות (404, 400) - ממשיכות לעבוד בדיוק כמו קודם

---

## 🧪 איך לבדוק שה-Middleware עובד?

ה-Middleware תופס רק חריגות **לא צפויות**. כדי לבדוק אותו, נצטרך ליצור Exception מלאכותי:

```bash
dotnet run
```

---

### צעד 4: הוסף Exception מלאכותי לבדיקה

כדי לבדוק שה-Middleware עובד, תוסיף Exception זמני ב-Repository:

**ערוך את `Repositories/InMemoryBookRepository.cs`:**

```csharp
public List<Book> GetAll()
{
    // זמני - רק לבדיקה!
    throw new InvalidOperationException("Simulated database error");
    
    return _books;
}
```

**הרץ את הפרויקט:**
```bash
dotnet run
```

**בדוק ב-Swagger:**
- GET /api/books
- תקבל 500 עם הודעה: `"A data error occurred. Please try again later."`

**✅ ה-Middleware עובד!**

**אל תשכח להסיר את השורה הזו אחרי הבדיקה!**

---

##  בדיקות נוספות

### בדיקה 1: ספר שלא קיים (404)

**Request:**
```
GET http://localhost:5000/api/books/999
```

**Expected Response:** 404 Not Found
```
"Book with ID 999 not found"
```

**מה קרה:**
- Controller החזיר `NotFound()` ישירות
- **לא** עבר דרך ה-Middleware
- זו תוצאה צפויה, לא Exception

---

### בדיקה 2: Validation error (400)

**Request:**
```
POST http://localhost:5000/api/books
Content-Type: application/json

{
  "title": "",
  "author": "Test",
  "year": 2020
}
```

**Expected Response:** 400 Bad Request
```
"Title is required"
```

**מה קרה:**
- Controller החזיר `BadRequest()` ישירות
- **לא** עבר דרך ה-Middleware
- זו תוצאה צפויה, לא Exception

---

##  מבנה הפרויקט אחרי השינויים

```
my-books-api/
├── Program.cs                      (עודכן - הוספת Middleware)
├── Controllers/
│   └── BooksController.cs          (ללא שינוי - משתמש ב-return NotFound())
├── Models/
│   └── Book.cs                     (ללא שינוי)
├── Repositories/
│   ├── IBookRepository.cs          (ללא שינוי)
│   └── InMemoryBookRepository.cs   (ללא שינוי)
└── Middleware/                     (חדש)
    └── ExceptionHandlingMiddleware.cs
```

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

### בדיקת ה-Middleware ב-Swagger

עכשיו נבדוק שה-Middleware תופס **חריגות לא צפויות** כמו שצריך:

**1. בדיקת ספר שלא קיים (404) - תוצאה צפויה:**
- פתח את **GET /api/books/{id}**
- לחץ על "Try it out"
- הזן `id: 999`
- לחץ על "Execute"
- **תוצאה צפויה:**
  - Status Code: **404 Not Found**
  - Response Body:
    ```json
    {
      "error": "Book with ID 999 not found"
    }
    ```
  - **שים לב:** זה **לא** עבר דרך ה-Middleware! Controller החזיר `NotFound()` ישירות.

**2. בדיקת validation errors (400) - תוצאה צפויה:**
- פתח את **POST /api/books**
- לחץ על "Try it out"
- הזן title ריק:
  ```json
  {
    "title": "",
    "author": "Test",
    "year": 2020
  }
  ```
- לחץ על "Execute"
- **תוצאה צפויה:** 400 Bad Request
- **שים לב:** גם זה **לא** עבר דרך ה-Middleware! Controller החזיר `BadRequest()` ישירות.

**3. בדיקת פעולות תקינות:**
- GET /api/books - תראה את כל הספרים
- POST /api/books עם נתונים תקינים - יצירת ספר חדש
- PUT /api/books/1 - עדכון ספר קיים

### מה השתנה מהשלב הקודם?

- **לפני:** כל Controller action היה עטוף ב-try/catch
- **עכשיו:** הסרנו את כל ה-try/catch מה-Controllers
- **Middleware:** תופס רק **חריגות לא צפויות** (500 errors)
- **Controllers:** ממשיכים להחזיר `NotFound()` ו-`BadRequest()` לתוצאות צפויות

### איך לבדוק שה-Middleware עובד?

ה-Middleware תופס חריגות **לא צפויות**. כדי לבדוק אותו, תצטרך ליצור מצב שגורם ל-Exception אמיתי:

**דוגמה:** אם תוסיף קוד ב-Repository שזורק Exception:
```csharp
public List<Book> GetAll()
{
    throw new InvalidOperationException("Simulated error");
    return _books;
}
```

אז תקבל 500 מה-Middleware עם הודעה בטוחה:
```json
{
  "error": "A data error occurred. Please try again later."
}
```

**חשוב:** במצב רגיל, ה-Middleware לא אמור לתפוס כלום - כל התוצאות הצפויות מטופלות ב-Controller!

---

##  קישורים למידע נוסף

- [ASP.NET Core Middleware](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/)
- [Write custom ASP.NET Core middleware](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/write)
- [HttpContext](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext)
- [RequestDelegate](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.requestdelegate)
- [Exception Handling in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling)

---

**מוכן? עבור לשלב 3: `03-iexceptionhandler-refactoring/`** 
