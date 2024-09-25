namespace VegaExpress.Agent.Data.Interfaces
{
    public interface IGatewayRoutingEndPointRepository<T>
    {
        Task<bool> CreateAsync(T entity);
        Task<bool> UpdateAsync(T entity);
        Task<bool> DeleteAsync(int id);
        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
    }
}
