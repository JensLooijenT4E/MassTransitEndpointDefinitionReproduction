namespace MassTransit.Repro;

public class StoreBookConsumer : IConsumer<IStoreBook>
{
    private readonly IBookService _service;

    public StoreBookConsumer(IBookService service)
    {
        _service = service;
    }

    public async Task Consume(ConsumeContext<IStoreBook> context)
    {
        await _service.StoreBook();
    }
}

public interface IStoreBook
{
    string Title { get; set; }
    Dictionary<string, object> Characters { get; set; }
    TimeSpan? TimeToRead { get; set; }
}

public record Book(string Title);