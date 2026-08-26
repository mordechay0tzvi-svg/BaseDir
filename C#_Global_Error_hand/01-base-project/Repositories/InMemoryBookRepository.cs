using ExceptionHandlingLab.Models;

namespace ExceptionHandlingLab.Repositories;

public class InMemoryBookRepository : IBookRepository
{
    private readonly List<Book> _books = new()
    {
        new Book { Id = 1, Title = "Clean Code", Author = "Robert C. Martin", Year = 2008 },
        new Book { Id = 2, Title = "Design Patterns", Author = "Gang of Four", Year = 1994 },
        new Book { Id = 3, Title = "Refactoring", Author = "Martin Fowler", Year = 1999 }
    };
    
    private int _nextId = 4;

    public List<Book> GetAll()
    {
        return _books;
    }

    public Book? GetById(int id)
    {
        return _books.FirstOrDefault(b => b.Id == id);
    }

    public Book Add(Book book)
    {
        book.Id = _nextId++;
        _books.Add(book);
        return book;
    }

    public Book? Update(int id, Book book)
    {
        var existing = GetById(id);
        if (existing == null)
            return null;

        existing.Title = book.Title;
        existing.Author = book.Author;
        existing.Year = book.Year;
        
        return existing;
    }
}
