using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Extensions.Caching.Memory;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Vehicle;
using BEASTAPI.Core.Contract.Vehicle;
using BEASTAPI.Core.Contract.Persistence;

namespace BEAST.API.Persistence;

public class VehicleBrandRepository : IVehicleBrandRepository
{
    private readonly IDataAccessHelper _dataAccessHelper;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private const string VehicleBrandCache = "VehicleBrandData";
    private const string DistinctVehicleBrandCache = "DistinctVehicleBrandData";
    private const string VehicleBrandsWithVehicle = "VehicleBrandsWithVehiclesData";

	public VehicleBrandRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"
	public async Task<PaginatedListModel<VehicleBrandModel>> GetVehicleBrands(int pageNumber)
	{
		PaginatedListModel<VehicleBrandModel> output = _cache.Get<PaginatedListModel<VehicleBrandModel>>(VehicleBrandCache + pageNumber);

		if (output is null)
		{
			DynamicParameters p = new DynamicParameters();
			p.Add("PageNumber", pageNumber);
			p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
			p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

			var result = await _dataAccessHelper.QueryData<VehicleBrandModel, dynamic>("USP_VehicleBrand_GetAll", p);
			int TotalRecords = p.Get<int>("TotalRecords");
			int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

			output = new PaginatedListModel<VehicleBrandModel>
			{
				PageIndex = pageNumber,
				TotalRecords = TotalRecords,
				TotalPages = totalPages,
				HasPreviousPage = pageNumber > 1,
				HasNextPage = pageNumber < totalPages,
				Items = result.ToList()
			};

			_cache.Set(VehicleBrandCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

			List<string> keys = _cache.Get<List<string>>(VehicleBrandCache);
			if (keys is null)
				keys = new List<string> { VehicleBrandCache + pageNumber };
			else
				keys.Add(VehicleBrandCache + pageNumber);
			_cache.Set(VehicleBrandCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}
	public async Task<List<VehicleBrandModel>> GetDistinctVehicleBrands()
	{
		var output = _cache.Get<List<VehicleBrandModel>>(VehicleBrandCache);

		if (output is null)
		{
			output = await _dataAccessHelper.QueryData<VehicleBrandModel, dynamic>("USP_VehicleBrand_GetDistinct", new { });
			_cache.Set(VehicleBrandCache, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<VehicleBrandModel> GetVehicleBrandById(string vehicleBrandId)
	{
		return (await _dataAccessHelper.QueryData<VehicleBrandModel, dynamic>("USP_VehicleBrand_GetById", new { Id = vehicleBrandId })).FirstOrDefault();
	}

    public async Task<string> InsertVehicleBrand(VehicleBrandModel vehicleBrand, LogModel logModel)
    {
        ClearCache(VehicleBrandCache);
        ClearCache(VehicleBrandsWithVehicle);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", vehicleBrand.Id);
		p.Add("Name", vehicleBrand.Name);
		p.Add("CreatedBy", vehicleBrand.CreatedBy);
		p.Add("IsActive", vehicleBrand.IsActive);
		p.Add("IsDeleted", vehicleBrand.IsDeleted);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_VehicleBrand_Insert", p);
        return vehicleBrand.Id;
    }

    public async Task UpdateVehicleBrand(VehicleBrandModel vehicleBrand, LogModel logModel)
    {
        ClearCache(VehicleBrandCache);
        ClearCache(VehicleBrandsWithVehicle);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", vehicleBrand.Id);
        p.Add("Name", vehicleBrand.Name);
        p.Add("ModifiedBy", vehicleBrand.ModifiedBy);
        p.Add("IsActive", vehicleBrand.IsActive);
        p.Add("IsDeleted", vehicleBrand.IsDeleted);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_VehicleBrand_Update", p);
    }

	public async Task DeleteVehicleBrand(string vehicleBrandId, LogModel logModel)
	{
		ClearCache(VehicleBrandCache);
		ClearCache(VehicleBrandsWithVehicle);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", vehicleBrandId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_VehicleBrand_Delete", p);
	}

	public async Task<List<VehicleBrandModel>> Export()
	{
		return await _dataAccessHelper.QueryData<VehicleBrandModel, dynamic>("USP_VehicleBrand_Export", new { });
	}
	#endregion

	#region "Customized Methods"

	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			//case CategoriesWithPiesCache:
			//    _cache.Remove(CategoriesWithPiesCache);
			//    break;
			case VehicleBrandCache:
				var keys = _cache.Get<List<string>>(VehicleBrandCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(VehicleBrandCache);
				}
				break;
			default:
				break;
		}
	}
	#endregion
}