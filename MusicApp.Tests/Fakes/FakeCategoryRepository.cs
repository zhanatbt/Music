using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;

namespace MusicApp.Tests.Fakes;

public class FakeCategoryRepository : ICategoryRepository
{
    private readonly List<Category> _categories = [];
    private int _nextId = 1;

    public Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult((IReadOnlyList<Category>)_categories.OrderBy(x => x.Name).ToList());
    }

    public Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_categories.FirstOrDefault(x => x.Id == id));
    }

    public Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_categories.FirstOrDefault(x => x.Name == name));
    }

    public Task AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        if (category.Id == 0)
        {
            category.Id = _nextId++;
        }

        _categories.Add(category);
        return Task.CompletedTask;
    }
}
