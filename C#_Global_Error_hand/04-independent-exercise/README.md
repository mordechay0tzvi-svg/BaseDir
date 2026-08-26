# שלב 4: תרגיל עצמאי - Tasks API

##  מטרת התרגיל

לבנות בעצמך את כל התהליך שלמדת, מהתחלה ועד הסוף:
1. פרויקט בסיסי עם try/catch
2. Refactoring ל-Custom Middleware
3. Refactoring ל-IExceptionHandler

**משך זמן משוער:** 60-90 דקות

---

##  הדרישות

בנה **Tasks API** - מערכת לניהול משימות (To-Do List).

### המודל: Task

```csharp
public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

 **שים לב:** השם `TaskItem` ולא `Task` כי `Task` כבר קיים ב-C# (System.Threading.Tasks.Task).

---

### Endpoints נדרשים

צור Controller עם הendpoints הבאים:

1. **GET /api/tasks** - קבלת כל המשימות
2. **GET /api/tasks/{id}** - קבלת משימה לפי ID
3. **GET /api/tasks/status/{status}** - קבלת משימות לפי סטטוס (completed/pending)
4. **POST /api/tasks** - יצירת משימה חדשה
5. **PUT /api/tasks/{id}/complete** - סימון משימה כהושלמה

**Validation פשוט:**
- `GET /api/tasks/status/{status}` - אם status לא "completed" או "pending" → `return BadRequest("Status must be 'completed' or 'pending'")`

---

### Repository Pattern

צור:
- `ITaskRepository` - ממשק
- `InMemoryTaskRepository` - מימוש In-Memory

**נתונים ראשוניים (seed data):**
```csharp
new TaskItem { Id = 1, Title = "Learn C#", Description = "Complete C# basics", IsCompleted = true, CreatedAt = DateTime.Now.AddDays(-5) },
new TaskItem { Id = 2, Title = "Build API", Description = "Create REST API", IsCompleted = false, CreatedAt = DateTime.Now.AddDays(-2) },
new TaskItem { Id = 3, Title = "Deploy", Description = "Deploy to production", IsCompleted = false, CreatedAt = DateTime.Now.AddDays(-1) }
```

---

### Validation

**השתמש ב-Data Annotations על המודל:**
```csharp
[Required]
public string Title { get; set; } = string.Empty;

[MaxLength(500)]
public string Description { get; set; } = string.Empty;
```


---

### תוצאות צפויות vs חריגות לא צפויות

**תוצאות צפויות** (מטופלות ב-Controller):
- משימה לא נמצאה → `return NotFound($"Task with ID {id} not found")`
- Validation נכשל → `return BadRequest("...")`

**חריגות לא צפויות** (מטופלות ב-Middleware/IExceptionHandler):
- בעיה טכנית בלתי צפויה → `InvalidOperationException` → 500
- פעולה שנכשלה בגלל timeout → `TimeoutException` → 500

---

### 🧪 תרחישי בדיקה לחריגות טכניות

כדי לבדוק שה-Middleware/IExceptionHandler עובדים, תצטרך ליצור **2 תרחישים טכניים**:

#### תרחיש A: InvalidOperationException

**הוסף endpoint חדש:**
```
GET /api/tasks/stats
```

**מטרה:** מחזיר סטטיסטיקות על המשימות (כמה הושלמו, כמה פתוחות).

**בדיקת Exception:**
- הוסף **זמנית** בתוך המתודה ב-Repository:
  ```csharp
  throw new InvalidOperationException("Database connection failed");
  ```
- הרץ `GET /api/tasks/stats`
- **תוצאה צפויה:** 500 Internal Server Error

**אחרי הבדיקה:** הסר את ה-`throw` והחזר את הקוד התקין.

---

#### תרחיש B: TimeoutException

**הוסף endpoint חדש:**
```
POST /api/tasks/{id}/archive
```

**מטרה:** מעביר משימה מושלמת לארכיון (פעולה שלוקחת זמן).

**בדיקת Exception:**
- השתמש ב-`Task.Delay` עם `CancellationToken` כדי לסמלץ timeout:
  ```csharp
  using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
  await Task.Delay(5000, cts.Token); // ינסה לחכות 5 שניות, אבל יבוטל אחרי 100ms
  ```
- זה יזרוק `TaskCanceledException` (שהוא סוג של `OperationCanceledException`)
- טפל בזה כ-`TimeoutException` ב-Middleware/Handler
- הרץ `POST /api/tasks/1/archive`
- **תוצאה צפויה:** 500 Internal Server Error

**חלופה פשוטה:** אם זה מסובך מדי, פשוט זרוק:
```csharp
throw new TimeoutException("Archive operation timed out");
```

---

##  התהליך - 3 גרסאות

### גרסה 1: try/catch (branch: `try-catch`)

**מה לבנות:**
1. צור פרויקט ASP.NET Core Web API חדש
2. צור: `TaskItem`, `ITaskRepository`, `InMemoryTaskRepository`, `TasksController`
3. הוסף try/catch בכל action ב-Controller
4. **תוצאות צפויות:** טפל עם `return NotFound()` / `return BadRequest()`
5. **חריגות טכניות:** תפוס ב-`catch` והחזר `StatusCode(500, "...")`

**בדיקות:**
- ✅ כל ה-endpoints הרגילים עובדים
- ✅ `GET /api/tasks/stats` עם `throw InvalidOperationException` → 500
- ✅ `POST /api/tasks/1/archive` עם timeout → 500

**Git:**
```bash
git commit -m "Version 1: try/catch in controllers"
git checkout -b try-catch
```

---

### גרסה 2: Custom Middleware (branch: `middleware`)

**מה לשנות:**
1. **חזור ל-main:** `git checkout main`
2. צור `Middleware/ExceptionHandlingMiddleware.cs`
3. רשום ב-`Program.cs`: `app.UseMiddleware<ExceptionHandlingMiddleware>()`
4. **הסר try/catch** מכל ה-Controllers
5. **תוצאות צפויות:** השאר `return NotFound()` / `return BadRequest()`
6. **חריגות טכניות:** פשוט תזרוק - ה-Middleware יתפוס

**מה לא משתנה:**
- ❌ אל תשנה את הלוגיקה של 404/400
- ❌ אל תיצור `TaskNotFoundException`
- ✅ רק הסר try/catch והעבר טיפול ב-500 ל-Middleware

**בדיקות:**
- ✅ כל ה-endpoints הרגילים עובדים **בדיוק כמו קודם**
- ✅ `GET /api/tasks/stats` עם `throw InvalidOperationException` → 500
- ✅ `POST /api/tasks/1/archive` עם timeout → 500

**Git:**
```bash
git commit -m "Version 2: Custom Exception Handling Middleware"
git checkout -b middleware
```

---

### גרסה 3: IExceptionHandler (branch: `iexceptionhandler`)

**מה לשנות:**
1. **חזור ל-main:** `git checkout main`
2. **מחק** `Middleware/`
3. צור `ExceptionHandlers/GlobalExceptionHandler.cs` שמממש `IExceptionHandler`
4. עדכן `Program.cs`:
   ```csharp
   builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
   builder.Services.AddProblemDetails();
   
   app.UseExceptionHandler();
   ```
5. **Controllers:** זהים לגרסה 2 (ללא try/catch)

**מה לא משתנה:**
- ✅ Controllers זהים לגרסה 2
- ✅ התנהגות זהה לגרסה 2
- ✅ רק השיטה הטכנית השתנתה

**בדיקות:**
- ✅ כל ה-endpoints הרגילים עובדים **בדיוק כמו קודם**
- ✅ `GET /api/tasks/stats` עם `throw InvalidOperationException` → 500
- ✅ `POST /api/tasks/1/archive` עם timeout → 500

**Git:**
```bash
git commit -m "Version 3: IExceptionHandler"
git checkout -b iexceptionhandler
```

---

##  בדיקות נדרשות

לכל גרסה, בדוק:

### ✅ תוצאות צפויות (404, 400)

```
GET /api/tasks/999
→ 404 Not Found

POST /api/tasks (title ריק)
→ 400 Bad Request

PUT /api/tasks/999/complete
→ 404 Not Found
```

### ✅ חריגות טכניות (500)

```
GET /api/tasks/stats (עם throw InvalidOperationException)
→ 500 Internal Server Error

POST /api/tasks/1/archive (עם timeout)
→ 500 Internal Server Error
```

### ✅ פעולות תקינות (200, 201)

```
GET /api/tasks
→ 200 OK

GET /api/tasks/1
→ 200 OK

POST /api/tasks (תקין)
→ 201 Created

PUT /api/tasks/2/complete
→ 200 OK
```

---

##  מבנה הפרויקט הסופי

```
tasks-api/
├── Program.cs
├── Controllers/
│   └── TasksController.cs
├── Models/
│   └── TaskItem.cs
├── Repositories/
│   ├── ITaskRepository.cs
│   └── InMemoryTaskRepository.cs
├── Middleware/                  (בגרסה 2)
│   └── ExceptionHandlingMiddleware.cs
└── ExceptionHandlers/          (בגרסה 3)
    └── GlobalExceptionHandler.cs
```

---

##  רשימת בדיקה (Checklist)

### גרסה 1 (try/catch):
- [ ] יש try/catch בכל action
- [ ] תוצאות צפויות: `return NotFound()` / `return BadRequest()`
- [ ] חריגות טכניות: נתפסות ב-`catch` → `StatusCode(500)`
- [ ] `GET /api/tasks/stats` עם Exception → 500 ✅
- [ ] `POST /api/tasks/1/archive` עם timeout → 500 ✅
- [ ] `GET /api/tasks/999` → 404 ✅
- [ ] commit + branch `try-catch` ✅

### גרסה 2 (Middleware):
- [ ] יש `Middleware/ExceptionHandlingMiddleware.cs`
- [ ] ה-Middleware רשום ב-`Program.cs`
- [ ] **אין** try/catch ב-Controllers
- [ ] תוצאות צפויות: עדיין `return NotFound()` / `return BadRequest()`
- [ ] חריגות טכניות: פשוט נזרקות, ה-Middleware תופס
- [ ] **אין** `TaskNotFoundException` - לא צריך!
- [ ] `GET /api/tasks/stats` עם Exception → 500 ✅
- [ ] `POST /api/tasks/1/archive` עם timeout → 500 ✅
- [ ] `GET /api/tasks/999` → 404 ✅
- [ ] commit + branch `middleware` ✅

### גרסה 3 (IExceptionHandler):
- [ ] **אין** `Middleware/`
- [ ] יש `ExceptionHandlers/GlobalExceptionHandler.cs`
- [ ] `Program.cs`: `AddExceptionHandler` + `UseExceptionHandler`
- [ ] Controllers **זהים** לגרסה 2
- [ ] התנהגות **זהה** לגרסה 2
- [ ] `GET /api/tasks/stats` עם Exception → 500 ✅
- [ ] `POST /api/tasks/1/archive` עם timeout → 500 ✅
- [ ] `GET /api/tasks/999` → 404 ✅
- [ ] commit + branch `iexceptionhandler` ✅

---

##  🎁 בונוס: Service Layer + Custom Business Exceptions

**אם סיימת מוקדם**, הוסף שכבת Service עם לוגיקה עסקית:

### מבנה:
```
Controller → Service → Repository
```

### מה לבנות:

#### 1. צור `ITaskService` ו-`TaskService`

**אחריות של ה-Service:**
- לוגיקה עסקית (business rules)
- זריקת custom exceptions עסקיות
- **אין** מושגי HTTP (NotFound, BadRequest, וכו')

#### 2. צור 2 Custom Business Exceptions:

**Exception 1: `TaskAlreadyCompletedException`**
- נזרק כאשר מנסים לסמן משימה שכבר הושלמה
- הודעה: `"Task {id} is already completed"`

**Exception 2: `DuplicateTaskTitleException`**
- נזרק כאשר מנסים ליצור משימה עם Title שכבר קיים
- הודעה: `"Task with title '{title}' already exists"`

#### 3. הוסף Business Rules ב-Service:

```
CreateTask():
- אם יש כבר Task עם אותו Title → throw DuplicateTaskTitleException
- אם Task מסומן Completed אבל אין Description → throw InvalidOperationException

CompleteTask():
- אם Task כבר Completed → throw TaskAlreadyCompletedException
- אם Task לא קיים → החזר null (Controller יטפל)
```

#### 4. עדכן את ה-IExceptionHandler:

טפל בחריגות העסקיות:
- `TaskAlreadyCompletedException` → **409 Conflict**
- `DuplicateTaskTitleException` → **409 Conflict**
- `InvalidOperationException` → **500 Internal Server Error** (כמו קודם)

**דוגמה (אל תעתיק - כתוב בעצמך!):**
```csharp
if (exception is TaskAlreadyCompletedException)
{
    httpContext.Response.StatusCode = 409;
    await httpContext.Response.WriteAsJsonAsync(new 
    { 
        error = exception.Message 
    });
    return true;
}
```

#### 5. עדכן את ה-Controller:

```
Controller:
- מקבל ITaskService (לא ITaskRepository)
- קורא ל-Service methods
- מטפל רק ב-null (NotFound)
- לא מטפל בחריגות - ה-IExceptionHandler יטפל
```

### בדיקות הבונוס:

```
POST /api/tasks (עם Title קיים)
→ 409 Conflict
→ "Task with title 'Learn C#' already exists"

PUT /api/tasks/1/complete (משימה 1 כבר completed)
→ 409 Conflict
→ "Task 1 is already completed"

POST /api/tasks (Completed=true, Description ריק)
→ 500 Internal Server Error
→ "An unexpected error occurred"
```

### מה ללמוד מהבונוס:

✅ **הפרדת אחריות:** Service = business logic, Controller = HTTP  
✅ **Custom Exceptions:** מתי הם הגיוניים (שכבת business)  
✅ **Mapping Exceptions:** איך להמיר exceptions ל-HTTP status codes  
✅ **409 Conflict:** מתי להשתמש בו (business rule violation)  

---

##  סיימת?

**כל הכבוד!** 🎊

אם הצלחת לבנות את כל 3 הגרסאות (ואולי גם הבונוס), אתה מבין לעומק:
- ✅ ההבדל בין תוצאות צפויות (404, 400) לחריגות לא צפויות (500)
- ✅ איך לבנות Custom Middleware
- ✅ איך להשתמש ב-IExceptionHandler
- ✅ מתי custom exceptions הגיוניים (Service Layer)
- ✅ איך לעשות refactoring ארכיטקטוני

---

**בהצלחה! **
