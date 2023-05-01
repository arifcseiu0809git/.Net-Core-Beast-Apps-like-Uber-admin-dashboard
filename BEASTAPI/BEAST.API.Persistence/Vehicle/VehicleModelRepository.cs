using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Extensions.Caching.Memory;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Vehicle;
using BEASTAPI.Core.Contract.Vehicle;
using BEASTAPI.Core.Contract.Persistence;

namespace BEAST.API.Persistence;

public class VehicleModelRepository : IVehicleModelRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string VehicleModelCache = "VehicleModelData";
	private const string DistinctCategoryCache = "DistinctCategoryData";
	private const string VehicleModelsWithVehicleBrand = "VehicleModelsWithVehicleBrandsData";

	public VehicleModelRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"
	public async Task<PaginatedListModel<VehicleModel>> GetVehicleModels(int pageNumber)
	{
		PaginatedListModel<VehicleModel> output = _cache.Get<PaginatedListModel<VehicleModel>>(VehicleModelCache + pageNumber);

		if (output is null)
		{
			DynamicParameters p = new DynamicParameters();
			p.Add("PageNumber", pageNumber);
			p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
			p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

			var result = await _dataAccessHelper.QueryData<VehicleModel, dynamic>("USP_VehicleModel_GetAll", p);
			int TotalRecords = p.Get<int>("TotalRecords");
			int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

			output = new PaginatedListModel<VehicleModel>
			{
				PageIndex = pageNumber,
				TotalRecords = TotalRecords,
				TotalPages = totalPages,
				HasPreviousPage = pageNumber > 1,
				HasNextPage = pageNumber < totalPages,
				Items = result.ToList()
			};

			_cache.Set(VehicleModelCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

			List<string> keys = _cache.Get<List<string>>(VehicleModelCache);
			if (keys is null)
				keys = new List<string> { VehicleModelCache + pageNumber };
			else
				keys.Add(VehicleModelCache + pageNumber);
			_cache.Set(VehicleModelCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}
	public async Task<List<VehicleModel>> GetDistinctVehicleModels()
	{
		var output = _cache.Get<List<VehicleModel>>(VehicleModelCache);

		if (output is null)
		{
			output = await _dataAccessHelper.QueryData<VehicleModel, dynamic>("USP_VehicleModel_GetDistinct", new { });
			_cache.Set(VehicleModelCache, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}
	public async Task<VehicleModel> GetVehicleModelById(string vehicleModelId)
	{
		return (await _dataAccessHelper.QueryData<VehicleModel, dynamic>("USP_VehicleModel_GetById", new { Id = vehicleModelId })).FirstOrDefault();
	}

	public async Task<string> InsertVehicleModel(VehicleModel vehicleModel, LogModel logModel)
	{
		ClearCache(VehicleModelCache);
		ClearCache(VehicleModelsWithVehicleBrand);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", vehicleModel.Id);
		p.Add("Name", vehicleModel.Name);
		p.Add("VehicleBrandId", vehicleModel.VehicleBrandId);
		p.Add("Year", vehicleModel.Year);
		p.Add("CubicCapacity", vehicleModel.CubicCapacity);
		p.Add("CreatedBy", vehicleModel.CreatedBy);
		p.Add("IsActive", vehicleModel.IsActive);
		p.Add("IsDeleted", vehicleModel.IsDeleted);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_VehicleModel_Insert", p);
		return vehicleModel.Id;
	}

	public async Task UpdateVehicleModel(VehicleModel vehicleModel, LogModel logModel)
	{
		ClearCache(VehicleModelCache);
		ClearCache(VehicleModelsWithVehicleBrand);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", vehicleModel.Id);
		p.Add("Name", vehicleModel.Name);
		p.Add("VehicleBrandId", vehicleModel.VehicleBrandId);
		p.Add("Year", vehicleModel.Year);
		p.Add("CubicCapacity", vehicleModel.CubicCapacity);
		p.Add("ModifiedBy", vehicleModel.ModifiedBy);
		p.Add("IsActive", vehicleModel.IsActive);
		p.Add("IsDeleted", vehicleModel.IsDeleted);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_VehicleModel_Update", p);
	}

	public async Task DeleteVehicleModel(string vehicleModelId, LogModel logModel)
	{
		ClearCache(VehicleModelCache);
		ClearCache(VehicleModelsWithVehicleBrand);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", vehicleModelId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_VehicleModel_Delete", p);
	}

	public async Task<List<VehicleModel>> Export()
	{
		return await _dataAccessHelper.QueryData<VehicleModel, dynamic>("USP_VehicleModel_Export", new { });
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
			case VehicleModelCache:
				var keys = _cache.Get<List<string>>(VehicleModelCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(VehicleModelCache);
				}
				break;
			default:
				break;
		}
	}
	#endregion
}