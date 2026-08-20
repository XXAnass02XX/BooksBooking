using LibraryBooking.Domain.Entities;
using LibraryBooking.Infrastructure.Repositories;

namespace LibraryBooking.Infrastructure.Tests;

public class BookCopyRepositoryTests : RepositoryTestBase
{
    private readonly BookCopyRepository _sut;
    private readonly LibraryRepository _libraryRepository;

    public BookCopyRepositoryTests()
    {
        _sut = new BookCopyRepository(Context);
        // BookCopy has no Create/Add path of its own -- copies only ever
        // come into existence through Library.AddCopy, so a Library
        // repository is needed to get one into the database for these tests.
        _libraryRepository = new LibraryRepository(Context);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsTheMatchingCopy()
    {
        var library = Library.Create("Central Library", "1 Main St");
        var book = Book.Create("Dune", "Frank Herbert", "978-0441013593");
        var copy = library.AddCopy(book);

        await _libraryRepository.AddAsync(library);
        await Context.SaveChangesAsync();

        var stored = await _sut.GetByIdAsync(copy.Id);

        Assert.NotNull(stored);
        Assert.Equal(copy.Id, stored!.Id);
        Assert.Equal(book.Id, stored.BookId);
        Assert.Equal(library.Id, stored.LibraryId);
        Assert.True(stored.IsAvailable);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNoCopyHasThatId()
    {
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }
}
