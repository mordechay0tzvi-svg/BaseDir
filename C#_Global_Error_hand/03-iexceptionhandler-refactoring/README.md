# שלב 3: Refactoring ל-IExceptionHandler

## 📁 מבנה התיקייה

תיקייה זו מכילה:
- **`new-files/`** - קבצים חדשים ושינויים
  - `ExceptionHandlers/GlobalExceptionHandler.cs` - Handler חדש שמחליף את ה-Middleware
  - `REMOVE.txt` - הסבר איזה קבצים למחוק (תיקיית Middleware)
- **`solution/`** - דוגמה לקובץ שהשתנה (לא פרויקט מלא!)
  - `Program.cs` - איך Program.cs אמור להיראות אחרי המעבר ל-IExceptionHandler

**חשוב:** אתה ממשיך לעבוד על אותו פרויקט מ-שלב 1 ו-2. אל תיצור פרויקט חדש!

---

##  מטרת השלב

לעבור מ-Custom Middleware לשימוש ב-`IExceptionHandler` - ההפשטה המובנית של ASP.NET Core לטיפול בחריגות.

**משך זמן משוער:** 60-75 דקות

---

##  לפני שמתחילים

ודא שיש לך את הפרויקט מ-שלב 2 (`my-books-api/`) ושהוא עובד תקין עם Middleware.

---

##  הרעיון

בשלב הקודם בנינו Middleware מותאם אישית כדי להבין את המנגנון.

עכשיו נשתמש ב-**`IExceptionHandler`** - ממשק שASP.NET Core מספק **במיוחד** לטיפול בחריגות.

### למה לעבור ל-IExceptionHandler?

 **תקן מובנה:** זו הדרך המומלצת של ASP.NET Core  
 **פשוט יותר:** פחות boilerplate code  
 **אינטגרציה טובה יותר:** עובד טוב עם שאר המערכת  
 **מודולרי:** קל לפצל למספר handlers אם צריך  

### Middleware vs IExceptionHandler

| Middleware | IExceptionHandler |
|------------|-------------------|
| גישה כללית לכל דבר | מיועד במיוחד לחריגות |
| צריך לטפל ב-HttpContext ידנית | ממשק פשוט יותר |
| גמיש מאוד | ממוקד בטיפול בחריגות |
| טוב לכל cross-cutting concern | טוב לטיפול בחריגות |

**שניהם תקפים!** אבל `IExceptionHandler` יותר מתאים למטרה הספציפית הזו.

---

##  הנחיות צעד אחר צעד

### צעד 1: מחק את תיקיית Middleware

מכיוון שאנחנו עוברים ל-`IExceptionHandler`, אנחנו לא צריכים יותר את ה-Middleware המותאם אישית.

**מחק את התיקייה:**
```
my-books-api/Middleware/
```

כולל הקובץ `ExceptionHandlingMiddleware.cs`.

---

### צעד 2: צור תיקייה ExceptionHandlers

צור תיקייה חדשה בשם `ExceptionHandlers`:

```
my-books-api/
├── Controllers/
├── Models/
├── Repositories/
├── Exceptions/
└── ExceptionHandlers/  ← חדש
```

---

### צעד 3: צור את GlobalExceptionHandler

צור קובץ חדש: `ExceptionHandlers/GlobalExceptionHandler.cs`

**העתק את הקוד המלא מהקובץ:**  
`03-iexceptionhandler-refactoring/new-files/ExceptionHandlers/GlobalExceptionHandler.cs`

---

###  הסבר הקוד - שורה אחר שורה

#### מה זה IExceptionHandler?

`IExceptionHandler` הוא ממשק (interface) שASP.NET Core מספק **במיוחד** לטיפול בחריגות.

**הממשק מגדיר מתודה אחת:**
```csharp
ValueTask<bool> TryHandleAsync(
    HttpContext httpContext,
    Exception exception,
    CancellationToken cancellationToken)
```

כל מי שמממש את הממשק הזה יכול לטפל בחריגות באפליקציה.

---

#### הגדרת הקלאס

```csharp
public class GlobalExceptionHandler : IExceptionHandler
```

**`: IExceptionHandler`** אומר שהקלאס שלנו מממש את הממשק.

זה אומר שאנחנו **חייבים** לממש את המתודה `TryHandleAsync`.

---

#### Constructor

```csharp
private readonly ILogger<GlobalExceptionHandler> _logger;

public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
{
    _logger = logger;
}
```

בדיוק כמו ב-Middleware - אנחנו מקבלים `ILogger` דרך Dependency Injection.

---

#### TryHandleAsync - הלב של ה-Handler

```csharp
public async ValueTask<bool> TryHandleAsync(
    HttpContext httpContext,
    Exception exception,
    CancellationToken cancellationToken)
```

**פרמטרים:**

**1. `HttpContext httpContext`:**
- אותו `HttpContext` שראינו ב-Middleware
- מכיל את ה-Request וה-Response
- נשתמש בו כדי לכתוב את התשובה

**2. `Exception exception`:**
- החריגה שנזרקה
- ASP.NET Core מעביר לנו אותה אוטומטית
- נבדוק מאיזה סוג היא

**3. `CancellationToken cancellationToken`:**
- מנגנון לביטול פעולות async
- נעביר אותו ל-`WriteAsJsonAsync`
- לא נעסוק בזה לעומק במעבדה הזו

**ערך מוחזר: `ValueTask<bool>`:**

- `ValueTask` דומה ל-`Task`, אבל יעיל יותר במקרים מסוימים
- `bool` - האם טיפלנו בחריגה?
  - **`true`** = טיפלנו, אל תמשיך לחפש handlers אחרים
  - **`false`** = לא טיפלנו, תמשיך לחפש

 **למה צריך את ה-bool?**  
אפשר לרשום **מספר handlers**. כל handler יכול להחליט אם הוא מטפל בחריגה הספציפית או לא.

---

#### טיפול ב-BookNotFoundException

```csharp
if (exception is BookNotFoundException bookNotFound)
{
    _logger.LogWarning(bookNotFound, "Book not found: {Message}", bookNotFound.Message);

    httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
    
    await httpContext.Response.WriteAsJsonAsync(
        new { error = bookNotFound.Message },
        cancellationToken);

    return true;
}
```

**`if (exception is BookNotFoundException bookNotFound)`:**

זו בדיקת טיפוס (type check) + cast בשורה אחת.

**מה קורה:**
1. בודק אם `exception` הוא מסוג `BookNotFoundException`
2. אם כן, יוצר משתנה `bookNotFound` עם הטיפוס הנכון
3. נכנס ל-if

**שאר הקוד:**
- רושם warning ל-log
- קובע Status Code 404
- כותב JSON עם ההודעה
- **מחזיר `true`** = טיפלנו בחריגה

---

#### טיפול בחריגות כלליות

```csharp
_logger.LogError(exception, "An unexpected error occurred");

httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

await httpContext.Response.WriteAsJsonAsync(
    new { error = "An unexpected error occurred. Please try again later." },
    cancellationToken);

return true;
```

אם לא נכנסנו ל-if (זו לא `BookNotFoundException`), מגיעים לכאן.

**טיפול:**
- רושם error ל-log
- Status Code 500
- הודעה כללית
- **מחזיר `true`** = טיפלנו

---

###  השוואה: Middleware vs IExceptionHandler

#### Middleware (שלב 2):

```csharp
public async Task InvokeAsync(HttpContext context)
{
    try
    {
        await _next(context);  // צריך לקרוא ידנית
    }
    catch (BookNotFoundException ex)
    {
        // טיפול...
    }
    catch (Exception ex)
    {
        // טיפול...
    }
}
```

**מה צריך לעשות:**
- לקרוא ל-`_next(context)` ידנית
- לעטוף ב-try/catch
- לטפל ב-HttpContext

---

#### IExceptionHandler (שלב 3):

```csharp
public async ValueTask<bool> TryHandleAsync(
    HttpContext httpContext,
    Exception exception,
    CancellationToken cancellationToken)
{
    if (exception is BookNotFoundException bookNotFound)
    {
        // טיפול...
        return true;
    }

    // טיפול כללי...
    return true;
}
```

**מה ASP.NET Core עושה בשבילנו:**
- תופס את החריגה אוטומטית
- קורא ל-`TryHandleAsync` עם החריגה
- מעביר את ה-HttpContext

**מה אנחנו צריכים לעשות:**
- רק לטפל בחריגה
- להחזיר true/false

**פשוט יותר!**

---

### צעד 4: רשום את ה-Handler ב-Dependency Injection

פתח את `Program.cs` ועדכן אותו.

**הקוד המלא המעודכן:**

```csharp
using ExceptionHandlingLab.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddSingleton<IBookRepository, InMemoryBookRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Exception Handler
builder.Services.AddExceptionHandler<ExceptionHandlingLab.ExceptionHandlers.GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseExceptionHandler();
app.MapControllers();

app.Run();
```

**שים לב:** Swagger נשאר פעיל כדי שתוכל לבדוק את ה-API!

---

###  הסבר השינויים

#### 1. רישום ה-Handler ב-DI

```csharp
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
```

**מה זה עושה:**

רושם את ה-`GlobalExceptionHandler` שלנו ב-Dependency Injection.

ASP.NET Core יידע שצריך להשתמש בו לטיפול בחריגות.

**`builder.Services` = Dependency Injection Container**

כאן אנחנו אומרים: "תיצור instance של `GlobalExceptionHandler` ותשתמש בו".

---

#### 2. הוספת ProblemDetails

```csharp
builder.Services.AddProblemDetails();
```

**מה זה ProblemDetails?**

`ProblemDetails` הוא תקן (RFC 7807) לפורמט שגיאות ב-APIs.

במקום:
```json
{ "error": "Something went wrong" }
```

אפשר להחזיר:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Book with ID 999 not found"
}
```

**למה זה טוב?**
- פורמט סטנדרטי
- clients יודעים מה לצפות
- מכיל מידע מובנה

 **הערה:** במעבדה הזו אנחנו עדיין משתמשים ב-JSON פשוט, אבל `AddProblemDetails()` מכין את המערכת לעבודה עם התקן הזה.

---

#### 3. שימוש ב-Exception Handler Middleware

```csharp
app.UseExceptionHandler();
```

**מה זה עושה:**

מוסיף את ה-**Exception Handler Middleware** המובנה של ASP.NET Core ל-Pipeline.

**זה לא ה-Middleware שלנו!**

זה Middleware מובנה שיודע לחפש `IExceptionHandler` implementations ולהשתמש בהם.

**הזרימה:**

```
Request
    ↓
UseExceptionHandler() Middleware (מובנה)
    ↓
Controller
    ↓
Exception נזרק
    ↑
UseExceptionHandler() תופס
    ↓
מחפש IExceptionHandler implementations
    ↓
קורא ל-GlobalExceptionHandler.TryHandleAsync()
    ↓
Response
```

---

#### 4. הסרת השורה הישנה

**הסר את השורה הזו (אם היא קיימת):**

```csharp
app.UseMiddleware<ExceptionHandlingMiddleware>();  // ← מחק!
```

אנחנו לא צריכים אותה יותר כי עברנו ל-`IExceptionHandler`.

---

###  הבדל חשוב: DI vs Pipeline

זה אחד המושגים החשובים ביותר ב-ASP.NET Core:

#### `builder.Services.Add...` (Dependency Injection)

```csharp
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddSingleton<IBookRepository, InMemoryBookRepository>();
```

**מה זה עושה:**

רושם **services** שהאפליקציה יכולה להשתמש בהם.

**שאלה:** "מה השירותים הזמינים?"

**דוגמאות:**
- Repositories
- Services
- Exception Handlers
- Logging
- Database Contexts

---

#### `app.Use...` (HTTP Pipeline)

```csharp
app.UseExceptionHandler();
app.MapControllers();
```

**מה זה עושה:**

מגדיר איך **בקשות HTTP זורמות** דרך האפליקציה.

**שאלה:** "איך בקשה עוברת מ-Request ל-Response?"

**דוגמאות:**
- Middleware
- Authentication
- Authorization
- Exception Handling
- Routing

---

#### הקשר ביניהם

```
1. builder.Services.AddExceptionHandler<GlobalExceptionHandler>()
   → רושם את ה-Handler ב-DI
   → "יש לי Handler שיכול לטפל בחריגות"

2. app.UseExceptionHandler()
   → מוסיף Middleware ל-Pipeline
   → "כל בקשה תעבור דרך Exception Handler Middleware"
   → ה-Middleware יחפש IExceptionHandler ב-DI וישתמש בו
```

**אנלוגיה:**

- `builder.Services` = רישום עובדים בחברה
- `app.Use` = הגדרת תהליך העבודה

---

### צעד 5: ודא ש-Controllers לא השתנו

ה-Controllers אמורים להישאר **בדיוק כמו בשלב 2**.

אין צורך לשנות שום דבר ב-`BooksController.cs`.

**למה?**

כי מנקודת המבט של ה-Controller, לא משנה אם יש Middleware או IExceptionHandler.

הוא פשוט זורק Exceptions, ומישהו אחר תופס אותן.

---

### צעד 6: בדוק שה-IExceptionHandler עובד

**1. הרץ את הפרויקט:**
```bash
dotnet run
```

**2. בדוק ב-Swagger** - הכל אמור לעבוד כמו בשלב 2.

**3. בדוק את ה-IExceptionHandler:**

כדי לוודא שה-IExceptionHandler עובד, הוסף Exception זמני ב-Repository:

```csharp
public List<Book> GetAll()
{
    // זמני - רק לבדיקה!
    throw new InvalidOperationException("Simulated database error");
    
    return _books;
}
```

**הרץ GET /api/books** - תקבל 500 עם הודעה: `"An unexpected error occurred. Please try again later."`

**✅ ה-IExceptionHandler עובד!**

**אל תשכח להסיר את השורה הזו אחרי הבדיקה!**

---

##  מבנה הפרויקט אחרי השינויים

```
my-books-api/
├── Program.cs                      (עודכן - IExceptionHandler)
├── Controllers/
│   └── BooksController.cs          (ללא שינוי)
├── Models/
│   └── Book.cs                     (ללא שינוי)
├── Repositories/
│   ├── IBookRepository.cs          (ללא שינוי)
│   └── InMemoryBookRepository.cs   (ללא שינוי)
├── Exceptions/
│   └── BookNotFoundException.cs    (ללא שינוי)
└── ExceptionHandlers/              (חדש)
    └── GlobalExceptionHandler.cs
```

**שים לב:**
-  אין יותר `Middleware/`
-  יש `ExceptionHandlers/`

---

##  💡 מתי להשתמש במספר Handlers?

עד עכשיו יש לנו handler אחד שמטפל בהכל. **מתי כדאי לפצל למספר handlers?**

### דוגמה: פיצול לפי סוג החריגה

```csharp
// Handler 1: טיפול בבעיות Database
public class DatabaseExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(...)
    {
        if (exception is DbException || exception is SqlException)
        {
            _logger.LogError(exception, "Database error occurred");
            httpContext.Response.StatusCode = 503; // Service Unavailable
            await httpContext.Response.WriteAsJsonAsync(
                new { error = "Database temporarily unavailable" });
            return true;  // טיפלנו
        }

        return false;  // לא טיפלנו, תמשיך לבעיה הבאה
    }
}

// Handler 2: טיפול בבעיות External APIs
public class ExternalApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(...)
    {
        if (exception is HttpRequestException || exception is TimeoutException)
        {
            _logger.LogWarning(exception, "External API error");
            httpContext.Response.StatusCode = 502; // Bad Gateway
            await httpContext.Response.WriteAsJsonAsync(
                new { error = "External service unavailable" });
            return true;
        }

        return false;
    }
}

// Handler 3: catch-all לכל השאר
public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(...)
    {
        _logger.LogError(exception, "Unexpected error");
        httpContext.Response.StatusCode = 500;
        await httpContext.Response.WriteAsJsonAsync(
            new { error = "An unexpected error occurred" });
        return true;  // תמיד מטפל
    }
}
```

**רישום ב-Program.cs:**
```csharp
builder.Services.AddExceptionHandler<DatabaseExceptionHandler>();
builder.Services.AddExceptionHandler<ExternalApiExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
```

**זרימה:**
```
Exception נזרק (למשל: DbException)
    ↓
DatabaseExceptionHandler → return true (טיפל!)
    ↓
סיום - לא ממשיך להאנדלרים הבאים
```

```
Exception נזרק (למשל: InvalidOperationException)
    ↓
DatabaseExceptionHandler → return false (לא רלוונטי)
    ↓
ExternalApiExceptionHandler → return false (לא רלוונטי)
    ↓
GlobalExceptionHandler → return true (תופס הכל)
```

---

### מתי זה שימושי?

✅ **כדאי לפצל:**
- אפליקציה גדולה עם סוגים שונים של כשלים
- צריך טיפול שונה לכל סוג (status codes, הודעות, logging)
- רוצים הפרדת אחריות ברורה

❌ **לא צריך לפצל:**
- אפליקציה קטנה/בינונית
- כל הכשלים מטופלים באותה צורה
- רוצים פשטות

**במעבדה שלנו:** handler אחד מספיק! 

**חשוב:** גם עם מספר handlers - תוצאות צפויות (404, 400) עדיין מטופלות ב-Controller, לא ב-handlers!

---

## 📊 השוואה: Middleware vs IExceptionHandler

| | Custom Middleware | IExceptionHandler |
|---|---|---|
| **קוד** | יותר boilerplate | פשוט יותר |
| **תוצאה** | זהה | זהה |
| **גמישות** | מאוד גמיש | ממוקד בחריגות |
| **מטרה** | טיפול בחריגות לא צפויות | טיפול בחריגות לא צפויות |
| **המלצה** | לצרכים כלליים | לטיפול בחריגות |

**חשוב:** שני הפתרונות מטפלים רק ב**חריגות לא צפויות** (500). תוצאות צפויות (404, 400) מטופלות ב-Controller!

---

##  קישורים למידע נוסף

- [IExceptionHandler in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling)
- [Exception Handling Best Practices](https://learn.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions)

---

**🎉 סיימת! עכשיו אתה יודע 3 דרכים לטפל בחריגות ב-ASP.NET Core:**
1. ✅ try/catch בכל Controller (שלב 1)
2. ✅ Custom Middleware (שלב 2)
3. ✅ IExceptionHandler (שלב 3) - **המומלץ!**

---


**מוכן לתרגיל עצמאי? עבור לשלב 4: `04-independent-exercise/`** 
