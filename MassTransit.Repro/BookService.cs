namespace MassTransit.Repro;

public class BookService : IBookService
{
    public async Task StoreBook()
    {
        await Task.Delay(100);
    }
}

public interface IBookService
{
    public Task StoreBook();
}