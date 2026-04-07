using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;

namespace MusicApp.Tests.Fakes;

public class FakeUserRepository : IUserRepository
{
    private readonly List<User> _users = [];
    private int _nextId = 1;

    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.FirstOrDefault(x => x.Username == username));
    }

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.FirstOrDefault(x => x.Id == id));
    }

    public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult((IReadOnlyList<User>)_users.OrderBy(x => x.Username).ToList());
    }

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        if (user.Id == 0)
        {
            user.Id = _nextId++;
        }

        _users.Add(user);
        return Task.CompletedTask;
    }
}
