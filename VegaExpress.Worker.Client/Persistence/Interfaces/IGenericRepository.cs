namespace VegaExpress.Worker.Client.Persistence.Interfaces
{    
    public interface IGenericRepository<T>
    {
        Task<bool> CreateAsync(T entity);
        Task<bool> UpdateAsync(T entity);

        bool Exists(Predicate<T> match);
        T? Find(Predicate<T> match);
        IEnumerable<T> FindAll(Predicate<T> match);
        int FindIndex(Predicate<T> match);
        void RemoveAt(int index);
        Task RemoveAsync(T item);
        IAsyncEnumerable<T> ToListAsync(Func<T, bool> predicate);
        IAsyncEnumerable<T> ToListAsync();
    }
}