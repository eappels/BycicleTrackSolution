namespace BycicleTrackApp.Services.Interfaces;

public interface IRepository<T>
{
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<int> AddAsync(T item);
    Task<int> DeleteAsync(T item);
}