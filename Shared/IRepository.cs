namespace Shared;

public interface IRepository<in T>
    where T : Entity
{
    public Task Save(T entity);
}