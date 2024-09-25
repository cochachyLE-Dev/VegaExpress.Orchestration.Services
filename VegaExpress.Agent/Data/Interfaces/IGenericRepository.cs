namespace VegaExpress.Agent.Data.Interfaces
{
    public interface IGenericRepository<T>
    {
        Task<bool> CreateAsync(T entity);
        Task<bool> UpdateAsync(T entity);
        Task<bool> DeleteAsync(string id);
        Task<T> GetByIdAsync(string id);
        Task<IEnumerable<T>> GetAllAsync();
    }
}
