using LibraryBooking.Domain.Entities;
using LibraryBooking.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LibraryBooking.Infrastructure.Tests;

public class BookRepositoryTests : RepositoryTestBase
{
    // "sut" = system under test, the object each test is actually exercising.
    private readonly BookRepository _sut;

    public BookRepositoryTests()
    {
        _sut = new BookRepository(Context);
    }

    [Fact]
    public async Task AddAsync_Then_SaveChanges_PersistsTheBook()
    {
        var book = Book.Create("Clean Code", "Robert C. Martin", "978-0132350884");

        // AddAsync only stages the insert; SaveChangesAsync is what actually
        // writes it to the database.
        await _sut.AddAsync(book);
        await Context.SaveChangesAsync();

        var stored = await _sut.GetByIdAsync(book.Id);

        Assert.NotNull(stored);
        Assert.Equal("Clean Code", stored!.Title);
        Assert.Equal("Robert C. Martin", stored.Author);
        Assert.Equal("978-0132350884", stored.Isbn);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNoBookHasThatId()
    {
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIsbnAsync_ReturnsTheMatchingBook()
    {
        var book = Book.Create("The Pragmatic Programmer", "Andrew Hunt", "978-0135957059");
        await _sut.AddAsync(book);
        await Context.SaveChangesAsync();

        var result = await _sut.GetByIsbnAsync("978-0135957059");

        Assert.NotNull(result);
        Assert.Equal(book.Id, result!.Id);
    }

    [Fact]
    public async Task GetByIsbnAsync_ReturnsNull_WhenIsbnIsNotInTheDatabase()
    {
        var result = await _sut.GetByIsbnAsync("does-not-exist");

        Assert.Null(result);
    }

    [Fact]
    public async Task ExistsWithIsbnAsync_ReturnsTrue_WhenIsbnIsAlreadyUsed()
    {
        var book = Book.Create("Domain-Driven Design", "Eric Evans", "978-0321125217");
        await _sut.AddAsync(book);
        await Context.SaveChangesAsync();

        var exists = await _sut.ExistsWithIsbnAsync("978-0321125217");

        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsWithIsbnAsync_ReturnsFalse_WhenIsbnIsFree()
    {
        var exists = await _sut.ExistsWithIsbnAsync("unused-isbn");

        Assert.False(exists);
    }

    // This documents why callers are expected to check ExistsWithIsbnAsync
    // before creating a new Book: the unique index configured on Isbn in
    // BookConfiguration rejects a second row with the same ISBN as soon as
    // SaveChanges runs.
    [Fact]
    public async Task SaveChanges_Throws_WhenTwoBooksShareTheSameIsbn()
    {
        var first = Book.Create("Refactoring", "Martin Fowler", "978-0134757599");
        var duplicate = Book.Create("Refactoring (second copy)", "Martin Fowler", "978-0134757599");

        await _sut.AddAsync(first);
        await Context.SaveChangesAsync();

        await _sut.AddAsync(duplicate);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() => Context.SaveChangesAsync());
    }
}
