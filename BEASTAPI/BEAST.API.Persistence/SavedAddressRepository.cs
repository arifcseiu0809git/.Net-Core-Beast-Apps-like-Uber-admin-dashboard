using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Extensions.Caching.Memory;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.Contract.Persistence;
using BEASTAPI.Core.Model.Passenger;

namespace BEASTAPI.Persistence;

public class SavedAddressRepository : ISavedAddressRepository
{
    private readonly IDataAccessHelper _dataAccessHelper;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private const string SavedAddressCache = "SavedAddressData";

    public SavedAddressRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
    {
        this._dataAccessHelper = dataAccessHelper;
        this._config = config;
        this._cache = cache;
    }

    #region "DataAccessHelper Methods"
    public async Task<PaginatedListModel<SavedAddressModel>> GetSavedAddresses(int pageNumber)
    {
        PaginatedListModel<SavedAddressModel> output = _cache.Get<PaginatedListModel<SavedAddressModel>>(SavedAddressCache + pageNumber);

        if (output is null)
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("PageNumber", pageNumber);
            p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
            p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

            var result = await _dataAccessHelper.QueryData<SavedAddressModel, dynamic>("USP_SavedAddress_GetAll", p);
            int TotalRecords = p.Get<int>("TotalRecords");
            int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

            output = new PaginatedListModel<SavedAddressModel>
            {
                PageIndex = pageNumber,
                TotalRecords = TotalRecords,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages,
                Items = result.ToList()
            };

            _cache.Set(SavedAddressCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

            List<string> keys = _cache.Get<List<string>>(SavedAddressCache);
            if (keys is null)
                keys = new List<string> { SavedAddressCache + pageNumber };
            else
                keys.Add(SavedAddressCache + pageNumber);
            _cache.Set(SavedAddressCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }

        return output;
    }


    public async Task<SavedAddressModel> GetSavedAddressById(string SavedAddressId)
    {
        return (await _dataAccessHelper.QueryData<SavedAddressModel, dynamic>("USP_SavedAddress_GetById", new { Id = SavedAddressId })).FirstOrDefault();
    }

    public async Task<List<SavedAddressModel>> GetSavedAddressByName(string AddressName)
    {
        return (await _dataAccessHelper.QueryData<SavedAddressModel, dynamic>("USP_SavedAddress_GetByName", new { Address = AddressName })).ToList();
    }

    public async Task<string> InsertSavedAddress(SavedAddressModel savedAddress, LogModel logModel)
    {
        ClearCache(SavedAddressCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", savedAddress.Id);
        p.Add("UserId", savedAddress.UserId);
        p.Add("HomeAddress", savedAddress.HomeAddress);
        p.Add("HomeLatiitude", savedAddress.HomeLatiitude);
        p.Add("HomeLongitude", savedAddress.HomeLongitude);
        p.Add("OfficeAddress", savedAddress.OfficeAddress);
        p.Add("OfficeLatiitude", savedAddress.OfficeLatiitude);
        p.Add("OfficeLongitude", savedAddress.OfficeLongitude);
        p.Add("OtherSavedPlace", savedAddress.OtherSavedPlace);
        p.Add("OtherLatiitude", savedAddress.OtherLatiitude);
        p.Add("OtherLongitude", savedAddress.OtherLongitude);
        p.Add("IsActive", savedAddress.IsActive);
        p.Add("IsDeleted", savedAddress.IsDeleted);
        p.Add("CreatedBy", savedAddress.CreatedBy);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_SavedAddress_Insert", p);
        return savedAddress.Id;
    }

    public async Task UpdateSavedAddress(SavedAddressModel savedAddress, LogModel logModel)
    {
        ClearCache(SavedAddressCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", savedAddress.Id);
        p.Add("UserId", savedAddress.UserId);
        p.Add("HomeAddress", savedAddress.HomeAddress);
        p.Add("HomeLatiitude", savedAddress.HomeLatiitude);
        p.Add("HomeLongitude", savedAddress.HomeLongitude);
        p.Add("OfficeAddress", savedAddress.OfficeAddress);
        p.Add("OfficeLatiitude", savedAddress.OfficeLatiitude);
        p.Add("OfficeLongitude", savedAddress.OfficeLongitude);
        p.Add("OtherSavedPlace", savedAddress.OtherSavedPlace);
        p.Add("OtherLatiitude", savedAddress.OtherLatiitude);
        p.Add("OtherLongitude", savedAddress.OtherLongitude);
        p.Add("IsActive", savedAddress.IsActive);
        p.Add("IsDeleted", savedAddress.IsDeleted);
        p.Add("ModifiedBy", savedAddress.ModifiedBy);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_SavedAddress_Update", p);

    }

    public async Task DeleteSavedAddress(string savedAddressId, LogModel logModel)
    {
        ClearCache(SavedAddressCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", savedAddressId);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_SavedAddress_Delete", p);
    }

 

    public async Task<List<SavedAddressModel>> Export()
    {
        return await _dataAccessHelper.QueryData<SavedAddressModel, dynamic>("USP_SavedAddress_Export", new { });
    }
    #endregion

    #region "Helper Methods"
    private void ClearCache(string key)
    {
        switch (key)
        {
            case SavedAddressCache:
                var keys = _cache.Get<List<string>>(SavedAddressCache);
                if (keys is not null)
                {
                    foreach (var item in keys)
                        _cache.Remove(item);
                    _cache.Remove(SavedAddressCache);
                }
                break;
            default:
                break;
        }
    }
    #endregion
}