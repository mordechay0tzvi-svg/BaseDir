# Exception Handling Lab - Global Exception Handling in ASP.NET Core

מעבדה מודרכת ללימוד **Global Exception Handling** ב-ASP.NET Core Web API.

---

## מטרת המעבדה

ללמוד כיצד לטפל בחריגות (Exceptions) בצורה מרכזית ועקבית באפליקציית Web API, תוך הבנת:

- **העיקרון המרכזי:** ההבדל בין תוצאות צפויות (404, 400) לחריגות לא צפויות (500)
- הבעיה עם try/catch חוזר בכל Controller
- איך עובד ה-HTTP Pipeline של ASP.NET Core
- כיצד לבנות Custom Middleware לטיפול בחריגות **לא צפויות**
- כיצד להשתמש ב-IExceptionHandler של ASP.NET Core
- מתי custom exceptions הגיוניים (Service Layer)
- איפה כל קובץ שייך ארכיטקטורית

---

## משך זמן משוער

**3-4 שעות** (כולל תרגיל עצמאי)

---

## דרישות מקדימות

לפני שמתחילים, ודא שאתה מכיר:

- C# בסיסי (classes, methods, exceptions)
- ASP.NET Core Controllers
- Dependency Injection בסיסי
- HTTP Status Codes (200, 404, 500)
- Repository Pattern (בסיסי)

**לא צריך לדעת מראש:**
- Middleware
- IExceptionHandler
- HttpContext / RequestDelegate
- ProblemDetails

---

## דרישות טכניות

- .NET 8.0 SDK או גרסה חדשה יותר
- IDE: Visual Studio / Visual Studio Code / Rider
- כלי לבדיקת API: Postman / Thunder Client / curl

---

## מבנה המעבדה

המעבדה מורכבת מ-4 שלבים:

### **01-base-project/** (45-60 דקות)
פרויקט בסיסי עם try/catch ב-Controllers.  
**תלמד:** את הבעיה שאנחנו באים לפתור + **ההבדל בין 404/400 (צפוי) ל-500 (לא צפוי)**.

### **02-middleware-refactoring/** (75-90 דקות)
Refactoring לשימוש ב-Custom Exception Handling Middleware.  
**תלמד:** איך עובד ה-HTTP Pipeline, איך להסיר try/catch מ-Controllers, ו-Middleware תופס רק **חריגות לא צפויות**.

### **03-iexceptionhandler-refactoring/** (60-75 דקות)
Refactoring לשימוש ב-IExceptionHandler של ASP.NET Core.  
**תלמד:** את ההפשטה (abstraction) שהפריימוורק מספק, ומתי לפצל למספר handlers.

### **04-independent-exercise/** (60-90 דקות)
תרגיל עצמאי - תבנה את כל התהליך בעצמך על פרויקט חדש.  
**בונוס:** Service Layer עם custom business exceptions (409 Conflict).

---

## איך להתחיל?

1. **שכפל את התיקייה `01-base-project/` לתיקייה חדשה:**
   ```bash
   cp -r 01-base-project/ my-books-api/
   cd my-books-api/
   ```

2. **פתח את `01-base-project/README.md` ועקוב אחרי ההנחיות**

3. **בכל שלב:**
   - קרא את ה-README
   - בצע את השינויים בפרויקט שלך (`my-books-api/`)
   - הרץ ובדוק
   - אם תקוע - השווה ל-`solution/`

---

## מה תלמד?

בסיום המעבדה תדע:

### 🎯 עיקרון מרכזי:
- **תוצאות צפויות** (404, 400) → `return NotFound()` / `return BadRequest()` ב-Controller
- **חריגות לא צפויות** (500) → Middleware/IExceptionHandler תופס

### 🛠️ טכנולוגיות:
- איך עובד ה-HTTP request pipeline ב-ASP.NET Core
- מה זה Middleware ואיך לבנות Middleware מותאם אישית
- מה זה `HttpContext`, `RequestDelegate`, ו-`_next(context)`
- איך חריגות זורמות חזרה דרך ה-pipeline
- מה זה `IExceptionHandler` ומתי להשתמש בו (המומלץ!)
- ההבדל בין `builder.Services` (DI) ל-`app.Use` (Pipeline)

### 🏗️ ארכיטקטורה:
- איך לארגן קבצים (Controllers, Middleware, ExceptionHandlers)
- מתי לפצל handler אחד למספר handlers
- **מתי custom exceptions הגיוניים** (Service Layer, business rules)
- איך למפות exceptions ל-HTTP status codes (409 Conflict, 500, וכו')  

---

## ⚠️ הערה חשובה - העיקרון המרכזי

**לאורך כל המעבדה, זכור:**

```
תוצאות צפויות (404, 400):
→ return NotFound() / return BadRequest()
→ מטופלות ב-Controller
→ לא exceptions!

חריגות לא צפויות (500):
→ throw InvalidOperationException / TimeoutException
→ מטופלות ב-Middleware/IExceptionHandler
→ באגים, בעיות טכניות
```

**אל תיצור custom exceptions לתוצאות צפויות!**  
(למעט בשכבת Service - ראה בונוס בשלב 4)

---

## טיפים להצלחה

- **קרא את ההסברים לפני שמעתיק קוד** - חשוב להבין למה, לא רק איך
- **הרץ את הפרויקט אחרי כל שינוי** - ודא שהכל עובד לפני שממשיך
- **השתמש ב-solution/ רק אם תקוע** - נסה לפתור בעצמך קודם
- **בדוק endpoints עם Swagger** - ראה את התוצאות בפועל
- **שים לב להבדל בין 404 ל-500** - זה העיקרון המרכזי!  

---

## משאבים נוספים

- [ASP.NET Core Error Handling - Microsoft Docs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling)
- [ASP.NET Core Middleware - Microsoft Docs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/)
- [IExceptionHandler - Microsoft Docs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling#iexceptionhandler)

---

## תמיכה

אם נתקעת:
1. בדוק את ה-README של השלב הנוכחי
2. השווה את הקוד שלך ל-`solution/`
3. ודא שהפרויקט רץ בלי שגיאות compile
4. בדוק את ה-console output לשגיאות runtime

---

**בהצלחה!**
