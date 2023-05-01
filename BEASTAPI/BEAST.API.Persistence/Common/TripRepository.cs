using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using BEASTAPI.Core.Contract.Persistence.Common;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Common;
using System.Data;
using BEASTAPI.Core.Contract.Persistence;
using BEASTAPI.Core.Model.Map;

namespace BEASTAPI.Persistence.Common;

public class TripRepository : ITripRepository
{
    private readonly IDataAccessHelper _dataAccessHelper;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private const string TripCache = "TripData";

    public TripRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
    {
        this._dataAccessHelper = dataAccessHelper;
        this._config = config;
        this._cache = cache;
    }

    #region "DataAccessHelper Methods"
    public async Task<PaginatedListModel<TripModel>> GetTrips(string statusId, int pageNumber)
    {
        PaginatedListModel<TripModel> output = _cache.Get<PaginatedListModel<TripModel>>(TripCache + pageNumber+ statusId);

        if (output is null)
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("PageNumber", pageNumber);
            p.Add("statusId", statusId);
            p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
            p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

            var result = await _dataAccessHelper.QueryData<TripModel, dynamic>("USP_Trip_GetAll", p);
            int TotalRecords = p.Get<int>("TotalRecords");
            int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

            output = new PaginatedListModel<TripModel>
            {
                PageIndex = pageNumber,
                TotalRecords = TotalRecords,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages,
                Items = result.ToList()
            };

            _cache.Set(TripCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

            List<string> keys = _cache.Get<List<string>>(TripCache);
            if (keys is null)
                keys = new List<string> { TripCache + pageNumber };
            else
                keys.Add(TripCache + pageNumber);
            _cache.Set(TripCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }

        return output;
    }

    public async Task<TripModel> GetTripById(string tripId)
    {
        return (await _dataAccessHelper.QueryData<TripModel, dynamic>("USP_Trip_GetById", new { Id = tripId })).FirstOrDefault();
    }

	public async Task<PaginatedListModel<TripModel>> GetTripsByDriverId(string driverId,int pageNumber)
	{
		PaginatedListModel<TripModel> output = _cache.Get<PaginatedListModel<TripModel>>(TripCache + pageNumber+ driverId);

		if (output is null)
		{
			DynamicParameters p = new DynamicParameters();
			p.Add("DriverId", driverId);
			p.Add("PageNumber", pageNumber);
			p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
			p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

			var result = await _dataAccessHelper.QueryData<TripModel, dynamic>("USP_Trip_GetByDriverId", p);
			int TotalRecords = p.Get<int>("TotalRecords");
			int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

			output = new PaginatedListModel<TripModel>
			{
				PageIndex = pageNumber,
				TotalRecords = TotalRecords,
				TotalPages = totalPages,
				HasPreviousPage = pageNumber > 1,
				HasNextPage = pageNumber < totalPages,
				Items = result.ToList()
			};

			_cache.Set(TripCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

			List<string> keys = _cache.Get<List<string>>(TripCache);
			if (keys is null)
				keys = new List<string> { TripCache + pageNumber };
			else
				keys.Add(TripCache + pageNumber);
			_cache.Set(TripCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}



    public async Task<string> InsertTrip(TripModel trip, LogModel logModel)
    {
        ClearCache(TripCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", trip.Id);
        p.Add("PassengerId", trip.PassengerId);
        p.Add("DriverId", trip.DriverId);
        p.Add("TripInitialId", trip.TripInitialId);
        p.Add("VehicleId", trip.VehicleId);
        p.Add("VehicleFareId", trip.VehicleFareId);
        p.Add("StartLocationName", trip.StartLocationName);
        p.Add("InitialDistance", trip.InitialDistance);
        p.Add("InitialRent", trip.InitialRent);
        p.Add("EstimatedTime", trip.EstimatedTime);
        p.Add("StartLocationLongitude", trip.StartLocationLongitude);
        p.Add("StartLocationLatitude", trip.StartLocationLatitude);
        p.Add("EndLocationName", trip.EndLocationName);
        p.Add("EndLocationLongitude", trip.EndLocationLongitude);
        p.Add("EndLocationLatitude", trip.EndLocationLatitude);
        p.Add("RequestTime", trip.RequestTime);
        p.Add("StartTime", trip.StartTime);
        p.Add("EndTime", trip.EndTime);
        p.Add("BaseAmount", trip.BaseAmount);
        p.Add("Total", trip.Total);
        p.Add("PaymentTime", trip.PaymentTime);
        p.Add("StatusId", trip.StatusId);
        p.Add("PickupTimeFrom", trip.PickupTimeFrom);
        p.Add("PickupTimeTo", trip.PickupTimeTo);

        p.Add("CreatedBy", trip.CreatedBy);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_Trip_Insert", p);
        return trip.Id;
    }

    public async Task UpdateTrip(TripModel trip, LogModel logModel)
    {
        ClearCache(TripCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", trip.Id);
        p.Add("PassengerId", trip.PassengerId);
        p.Add("DriverId", trip.DriverId);
        p.Add("TripInitialId", trip.TripInitialId);
        p.Add("VehicleId", trip.VehicleId);
        p.Add("VehicleFareId", trip.VehicleFareId);
        p.Add("StartLocationName", trip.StartLocationName);
        p.Add("InitialDistance", trip.InitialDistance);
        p.Add("InitialRent", trip.InitialRent);
        p.Add("EstimatedTime", trip.EstimatedTime);
        p.Add("StartLocationLongitude", trip.StartLocationLongitude);
        p.Add("StartLocationLatitude", trip.StartLocationLatitude);
        p.Add("EndLocationName", trip.EndLocationName);
        p.Add("EndLocationLongitude", trip.EndLocationLongitude);
        p.Add("EndLocationLatitude", trip.EndLocationLatitude);
        p.Add("RequestTime", trip.RequestTime);
        p.Add("StartTime", trip.StartTime);
        p.Add("EndTime", trip.EndTime);
        p.Add("BaseAmount", trip.BaseAmount);
        p.Add("Total", trip.Total);
        p.Add("PaymentTime", trip.PaymentTime);
        p.Add("StatusId", trip.StatusId);
        p.Add("PickupTimeFrom", trip.PickupTimeFrom);
        p.Add("PickupTimeTo", trip.PickupTimeTo);

        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_Trip_Update", p);
    }

    public async Task DeleteTrip(string tripId, LogModel logModel)
    {
        ClearCache(TripCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", tripId);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_Trip_Delete", p);
    }


    public async Task<List<TripModel>> Filter(string StatusId, string VehicleTypeId, string DriverName, string PassengerName, string ContactNo)
    {

        DynamicParameters p = new DynamicParameters();
        p.Add("StatusId", StatusId);
        p.Add("VehicleTypeId", VehicleTypeId);
        p.Add("DriverName", DriverName);
        p.Add("PassengerName", PassengerName);
        p.Add("ContactNo", ContactNo);

        var output = await _dataAccessHelper.QueryData<TripModel, dynamic>("Filter_Trips", p);

        return output;
    }

    public async Task<List<TripModel>> Export()
    {
        return await _dataAccessHelper.QueryData<TripModel, dynamic>("USP_Trip_Export", new { });
    }
    #endregion

    #region "Helper Methods"
    private void ClearCache(string key)
    {
        switch (key)
        {
            case TripCache:
                var keys = _cache.Get<List<string>>(TripCache);
                if (keys is not null)
                {
                    foreach (var item in keys)
                        _cache.Remove(item);
                    _cache.Remove(TripCache);
                }
                break;
            default:
                break;
        }
    }
    #endregion
}