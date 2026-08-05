using Books.Mcp.Entities;
using Microsoft.EntityFrameworkCore;

namespace Books.Mcp.Data;

public class BooksDbContext(DbContextOptions<BooksDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(book => book.Id);
            entity.Property(book => book.Name).IsRequired();
            entity.Property(book => book.Description).IsRequired();
        });
    }
}
