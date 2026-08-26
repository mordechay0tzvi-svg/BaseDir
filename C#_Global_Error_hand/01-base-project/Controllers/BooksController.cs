using Microsoft.AspNetCore.Mvc;
using ExceptionHandlingLab.Models;
using ExceptionHandlingLab.Repositories;

namespace ExceptionHandlingLab.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookRepository _repository;

    public BooksController(IBookRepository repository)
    {
        _repository = repository;
    }

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
}
