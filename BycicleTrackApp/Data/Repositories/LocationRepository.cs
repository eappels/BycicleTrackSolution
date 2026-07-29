using BycicleTrackApp.Data.Models;
using BycicleTrackApp.Services.Interfaces;
using SQLite;

namespace BycicleTrackApp.Data.Repositories;

public class LocationRepository : IRepository<LocationOnMap>
{
    private readonly SQLiteAsyncConnection database;
    private readonly Task initializationTask;

    public LocationRepository()
    {
        var databasePath = Path.Combine(FileSystem.AppDataDirectory, "locations.db3");
        database = new SQLiteAsyncConnection(databasePath);
        initializationTask = database.CreateTableAsync<LocationOnMap>();
    }

    public async Task<int> AddAsync(LocationOnMap item)
    {
        await initializationTask;
        return await database.InsertAsync(item);
    }

    public async Task<int> DeleteAsync(LocationOnMap item)
    {
        await initializationTask;
        return await database.DeleteAsync(item);
    }

    public async Task<List<LocationOnMap>> GetAllAsync()
    {
        await initializationTask;
        return await database.Table<LocationOnMap>().ToListAsync();
    }

    public async Task<LocationOnMap?> GetByIdAsync(int id)
    {
        await initializationTask;
        return await database.FindAsync<LocationOnMap>(id);
    }
}
