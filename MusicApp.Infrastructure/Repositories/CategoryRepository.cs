using Microsoft.EntityFrameworkCore;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;
using MusicApp.Infrastructure.Data;

namespace MusicApp.Infrastructure.Repositories;

public class CategoryRepository(AppDbContext context) : ICategoryRepository
{
    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default)
        => await context.Categories.OrderBy(x => x.Name).ToListAsync(ct);

    public async Task<Category?> GetByIdAsync(int id, CancellationToken ct = default)
        => await context.Categories.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Category?> GetByNameAsync(string name, CancellationToken ct = default)
        => await context.Categories.FirstOrDefaultAsync(x => x.Name == name, ct);

    public async Task AddAsync(Category category, CancellationToken ct = default)
    {
        context.Categories.Add(category);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Category category, CancellationToken ct = default)
    {
        context.Categories.Update(category);
        await context.SaveChangesAsync(ct);
    }

    public async Task<bool> DeleteByIdAsync(int id, CancellationToken ct = default)
    {
        var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (category is null) return false;
        context.Categories.Remove(category);
        await context.SaveChangesAsync(ct);
        return true;
    }
}