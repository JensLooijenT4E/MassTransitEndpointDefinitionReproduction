using Microsoft.AspNetCore.Mvc;

namespace MassTransit.Repro.Controllers;

public class BooksController : ControllerBase
{
    [HttpGet("book")]
    public object GetBook()
    {
        return new Book("The Wizard of Oz");
    }
}