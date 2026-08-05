using Books.Mcp.Data;
using Books.Mcp.Entities;
using Microsoft.EntityFrameworkCore;

namespace Books.Mcp.Services;

public class BookService(BooksDbContext dbContext)
{
    public async Task<Book> CreateAsync(string name, string description, CancellationToken cancellationToken)
    {
        var book = new Book
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
        };

        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync(cancellationToken);

        return book;
    }

    public async Task<List<Book>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Books.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Books.AsNoTracking().FirstOrDefaultAsync(book => book.Id == id, cancellationToken);
    }

    public async Task<Book?> UpdateAsync(Guid id, string? name, string? description, CancellationToken cancellationToken)
    {
        var book = await dbContext.Books.FirstOrDefaultAsync(book => book.Id == id, cancellationToken);
        if (book is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            book.Name = name;
        }

        if (!string.IsNullOrWhiteSpace(description))
        {
            book.Description = description;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return book;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var book = await dbContext.Books.FirstOrDefaultAsync(book => book.Id == id, cancellationToken);
        if (book is null)
        {
            return false;
        }

        dbContext.Books.Remove(book);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
