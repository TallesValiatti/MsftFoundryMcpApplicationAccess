using System.ComponentModel;
using Books.Mcp.Services;
using ModelContextProtocol.Server;

namespace Books.Mcp.Tools;

[McpServerToolType]
public sealed class BookTools
{
    [McpServerTool(Name = "create_book"), Description("Creates a new book with a name and description.")]
    public static async Task<object> CreateBook(
        BookService bookService,
        [Description("Name of the book. Required.")] string name,
        [Description("Description of the book. Required.")] string description,
        CancellationToken cancellationToken)
    {
        var book = await bookService.CreateAsync(name, description, cancellationToken);
        return book;
    }

    [McpServerTool(Name = "get_books"), Description("Lists all books.")]
    public static async Task<object> GetBooks(BookService bookService, CancellationToken cancellationToken)
    {
        return await bookService.GetAllAsync(cancellationToken);
    }

    [McpServerTool(Name = "get_book_by_id"), Description("Gets a single book by its unique identifier.")]
    public static async Task<object> GetBookById(
        BookService bookService,
        [Description("Unique identifier (GUID) of the book. Required.")] Guid id,
        CancellationToken cancellationToken)
    {
        var book = await bookService.GetByIdAsync(id, cancellationToken);
        return book is null ? new { error = "Book not found." } : book;
    }

    [McpServerTool(Name = "update_book"), Description("Updates the name and/or description of an existing book.")]
    public static async Task<object> UpdateBook(
        BookService bookService,
        [Description("Unique identifier (GUID) of the book. Required.")] Guid id,
        [Description("New name of the book. Null to keep unchanged.")] string? name,
        [Description("New description of the book. Null to keep unchanged.")] string? description,
        CancellationToken cancellationToken)
    {
        var book = await bookService.UpdateAsync(id, name, description, cancellationToken);
        return book is null ? new { error = "Book not found." } : book;
    }

    [McpServerTool(Name = "delete_book"), Description("Deletes a book by its unique identifier.")]
    public static async Task<object> DeleteBook(
        BookService bookService,
        [Description("Unique identifier (GUID) of the book. Required.")] Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await bookService.DeleteAsync(id, cancellationToken);
        return new { deleted };
    }
}
