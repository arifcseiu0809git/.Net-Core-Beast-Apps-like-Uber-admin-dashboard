using BEASTAPI.Core.Model;

namespace BEASTAPI.Core.Contract.Persistence;

public interface ICityRepository
{
    Task<PaginatedListModel<CityModel>> GetCities(int pageNumber);
    Task<List<CityModel>> GetDistinctCities();
    Task<CityModel> GetCityById(string cityId);
    Task<CityModel> GetCityByName(string cityName);
    Task<List<CityModel>> GetCityByCuntryId(string Id);
    Task<string> InsertCity(CityModel city, LogModel logModel);
    Task UpdateCity(CityModel city, LogModel logModel);
    Task DeleteCity(string cityId, LogModel logModel);
    Task<List<CityModel>> Export();
}