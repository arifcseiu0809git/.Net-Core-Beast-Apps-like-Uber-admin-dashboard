using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Common;

namespace BEASTAPI.Core.Contract.Persistence.Common;

public interface ICountryRepository
{
    Task<PaginatedListModel<CountryModel>> GetCountries(int pageNumber);
    Task<CountryModel> GetCountryById(string countryId);
    Task<List<CountryModel>> GetDistinctCountries();
    Task<CountryModel> GetCountryByName(string countryName);
    Task<string> InsertCountry(CountryModel country, LogModel logModel);
    Task UpdateCountry(CountryModel country, LogModel logModel);
    Task DeleteCountry(string countryId, LogModel logModel);
    Task<List<CountryModel>> Export();
}