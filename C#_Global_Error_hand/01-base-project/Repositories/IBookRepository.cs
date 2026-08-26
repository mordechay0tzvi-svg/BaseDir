using ExceptionHandlingLab.Models;

namespace ExceptionHandlingLab.Repositories;

public interface IBookRepository
{
    List<Book> GetAll();
    Book? GetById(int id);
    Book Add(Book book);
    Book? Update(int id, Book book);
}
