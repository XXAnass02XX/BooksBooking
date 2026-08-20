using LibraryBooking.Domain.Entities;
using LibraryBooking.Infrastructure.Repositories;

namespace LibraryBooking.Infrastructure.Tests;

public class LibraryRepositoryTests : RepositoryTestBase
{
    private readonly LibraryRepository _sut;

    public LibraryRepositoryTests()
    {
        _sut = new LibraryRepository(Context);
    }

    [Fact]
    public async Task AddAsync_Then_SaveChanges_PersistsTheLibrary()
    {
        var library = Library.Create("Central Library", "1 Main St");

        await _sut.AddAsync(library);
        await Context.SaveChangesAsync();

        var stored = await _sut.GetByIdAsync(library.Id);

        Assert.NotNull(stored);
        Assert.Equal("Central Library", stored!.Name);
        Assert.Equal("1 Main St", stored.Address);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNoLibraryHasThatId()
    {
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    // GetByIdAsync is required to eager-load Copies and each copy's Book,
    // because a caller checking availability needs that data without
    // issuing extra queries per copy.
    [Fact]
    public async Task GetByIdAsync_IncludesCopiesAndTheirBooks()
    {
        var library = Library.Create("Branch Library", "2 Side St");
        var book = Book.Create("1984", "George Orwell", "978-0451524935");
        library.AddCopy(book);

        await _sut.AddAsync(library);
        await Context.SaveChangesAsync();

        var stored = await _sut.GetByIdAsync(library.Id);

        Assert.NotNull(stored);
        var copy = Assert.Single(stored!.Copies);
        Assert.Equal("1984", copy.Book.Title);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEveryLibrary()
    {
        await _sut.AddAsync(Library.Create("Library A", "Address A"));
        await _sut.AddAsync(Library.Create("Library B", "Address B"));
        await Context.SaveChangesAsync();

        var all = await _sut.GetAllAsync();

        Assert.Equal(2, all.Count());
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmpty_WhenNoLibrariesExist()
    {
        var all = await _sut.GetAllAsync();

        Assert.Empty(all);
    }
}
