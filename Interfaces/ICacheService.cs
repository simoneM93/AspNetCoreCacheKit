namespace AspNetCoreCacheKit.Interfaces
{
    public interface ICacheService
    {
        void Set(string groupKey, string key, object value);

        void Set(string key, object value);

        Task<T> GetOrCreateAsync<T>(string groupKey, string key, Func<Task<T>> createFunc);

        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> createFunc);

        T GetOrCreate<T>(string groupKey, string key, Func<T> createFunc);

        T GetOrCreate<T>(string key, Func<T> createFunc);

        void Delete(string groupKey, string key);

        void Delete(string key);
    }
}
