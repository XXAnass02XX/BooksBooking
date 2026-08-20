using LibraryBooking.Domain.Entities;
using LibraryBooking.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LibraryBooking.Infrastructure.Tests;

public class PatronRepositoryTests : RepositoryTestBase
{
    private readonly PatronRepository _sut;

    public PatronRepositoryTests()
    {
        _sut = new PatronRepository(Context);
    }

    [Fact]
    public async Task AddAsync_Then_SaveChanges_PersistsThePatron()
    {
        var patron = Patron.Create("Alice", "alice@example.com");

        await _sut.AddAsync(patron);
        await Context.SaveChangesAsync();

        var stored = await _sut.GetByIdAsync(patron.Id);

        Assert.NotNull(stored);
        Assert.Equal("Alice", stored!.Name);
        Assert.Equal("alice@example.com", stored.Email);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNoPatronHasThatId()
    {
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByEmailAsync_ReturnsTheMatchingPatron()
    {
        var patron = Patron.Create("Bob", "bob@example.com");
        await _sut.AddAsync(patron);
        await Context.SaveChangesAsync();

        var result = await _sut.GetByEmailAsync("bob@example.com");

        Assert.NotNull(result);
        Assert.Equal(patron.Id, result!.Id);
    }

    [Fact]
    public async Task GetByEmailAsync_ReturnsNull_WhenEmailIsNotInTheDatabase()
    {
        var result = await _sut.GetByEmailAsync("nobody@example.com");

        Assert.Null(result);
    }

    // Documents the effect of the unique index on Email configured in
    // PatronConfiguration: a second patron with an email already in use is
    // rejected when SaveChanges runs.
    [Fact]
    public async Task SaveChanges_Throws_WhenTwoPatronsShareTheSameEmail()
    {
        var first = Patron.Create("Carol", "carol@example.com");
        var duplicate = Patron.Create("Carol Again", "carol@example.com");

        await _sut.AddAsync(first);
        await Context.SaveChangesAsync();

        await _sut.AddAsync(duplicate);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() => Context.SaveChangesAsync());
    }
}
