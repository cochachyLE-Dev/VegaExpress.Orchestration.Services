namespace VegaExpress.Worker.Core.Persistence.Interfaces
{
    public interface IGenericRepository<T>
    {
        Task<bool> CreateAsync(T entity);
        Task<bool> UpdateAsync(T entity);
        Task<bool> DeleteAsync(string id);
        Task<T> GetByIdAsync(string id);
        IAsyncEnumerable<T> GetAllAsync();
    }
}
