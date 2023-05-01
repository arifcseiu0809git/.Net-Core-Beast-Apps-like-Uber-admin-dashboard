using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using BEASTAPI.Core.Contract.Persistence.Map;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Map;
using System.Data;
using BEASTAPI.Core.Contract.Persistence;
using BEASTAPI.Core.Model.Driver;
using CsvHelper;
using System.Net.Http.Json;

namespace BEASTAPI.Persistence.Map;

public class TripInitialRepository : ITripInitialRepository
{
    private readonly IDataAccessHelper _dataAccessHelper;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private const string TripInitialCache = "TripInitialData";

    public TripInitialRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
    {
        this._dataAccessHelper = dataAccessHelper;
        this._config = config;
        this._cache = cache;
    }

    #region "DataAccessHelper Methods"
    public async Task<PaginatedListModel<TripInitialModel>> GetTripInitials(int pageNumber)
    {
        PaginatedListModel<TripInitialModel> output = _cache.Get<PaginatedListModel<TripInitialModel>>(TripInitialCache + pageNumber);

        if (output is null)
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("PageNumber", pageNumber);
            p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
            p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

            var result = await _dataAccessHelper.QueryData<TripInitialModel, dynamic>("USP_TripInitial_GetAll", p);
            int TotalRecords = p.Get<int>("TotalRecords");
            int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

            output = new PaginatedListModel<TripInitialModel>
            {
                PageIndex = pageNumber,
                TotalRecords = TotalRecords,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages,
                Items = result.ToList()
            };

            _cache.Set(TripInitialCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

            List<string> keys = _cache.Get<List<string>>(TripInitialCache);
            if (keys is null)
                keys = new List<string> { TripInitialCache + pageNumber };
            else
                keys.Add(TripInitialCache + pageNumber);
            _cache.Set(TripInitialCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }

        return output;
    }

    public async Task<TripInitialModel> GetTripInitialById(string tripInitialId)
    {
        return (await _dataAccessHelper.QueryData<TripInitialModel, dynamic>("USP_TripInitial_GetById", new { Id = tripInitialId })).FirstOrDefault();
    }


	public async Task<List<TripInitialModel>> Filter(string StatusId, string VehicleTypeId, string DriverName, string PassengerName)
	{

		DynamicParameters p = new DynamicParameters();
		p.Add("StatusId", StatusId);
		p.Add("VehicleTypeId", VehicleTypeId);
		p.Add("DriverName", DriverName);
		p.Add("PassengerName", PassengerName);

		var output = await _dataAccessHelper.QueryData<TripInitialModel, dynamic>("Filter_TripInitials", p);

		return output;
	}


	public async Task<int> InsertTripInitial(TripInitialModel tripInitial, LogModel logModel)
    {
        ClearCache(TripInitialCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", tripInitial.Id);
        p.Add("VehicleTypeId", tripInitial.VehicleTypeId);
        p.Add("CountryId", tripInitial.CountryId);
        p.Add("CityId", tripInitial.CityId);
        p.Add("PassengerId", tripInitial.PassengerId);
        p.Add("DriverId", tripInitial.DriverId);
        p.Add("VehicleFareId", tripInitial.VehicleFareId);

        p.Add("OriginLatitude", tripInitial.OriginLatitude);
        p.Add("OriginLongitude", tripInitial.OriginLongitude);
        p.Add("DestinationLatitude", tripInitial.DestinationLatitude);
        p.Add("DestinationLongitude", tripInitial.DestinationLongitude);
        p.Add("PickupPointLatitude", tripInitial.PickupPointLatitude);
        p.Add("PickupPointLongitude", tripInitial.PickupPointLongitude);

        p.Add("RequestTime", tripInitial.RequestTime);
        p.Add("OriginAddress", tripInitial.OriginAddress);
        p.Add("DestinationAddress", tripInitial.DestinationAddress);

        p.Add("DistanceValue", tripInitial.DistanceValue);
        p.Add("DistanceText", tripInitial.DistanceText);
        p.Add("DurationValue", tripInitial.DurationValue);
        p.Add("DurationText", tripInitial.DurationText);
        p.Add("DurationInTrafficValue", tripInitial.DurationInTrafficValue);
        p.Add("DurationInTrafficText", tripInitial.DurationInTrafficText);
        p.Add("BaseFare", tripInitial.BaseFare);
        p.Add("CostPerKm", tripInitial.CostPerKm);
        p.Add("CostPerMin", tripInitial.CostPerMin);
        p.Add("DistanceKm", tripInitial.DistanceKm);
        p.Add("DurationMinute", tripInitial.DurationMinute);
        p.Add("InitialFee", tripInitial.InitialFee);
        p.Add("CostOfTravelTime", tripInitial.CostOfTravelTime);
        p.Add("CostOfDistance", tripInitial.CostOfDistance);

        p.Add("EstimatedCost", tripInitial.EstimatedCost);
        p.Add("TaxAmount", tripInitial.TaxAmount);
        p.Add("TotalCost", tripInitial.TotalCost);


        p.Add("CreatedBy", tripInitial.CreatedBy);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_TripInitial_Insert", p);
        return p.Get<int>("Id");
    }

    public async Task UpdateTripInitial(TripInitialModel tripInitial, LogModel logModel)
    {
        ClearCache(TripInitialCache);

        try
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("Id", tripInitial.Id);
            p.Add("VehicleTypeId", tripInitial.VehicleTypeId);
            p.Add("CountryId", tripInitial.CountryId);
            p.Add("CityId", tripInitial.CityId);
            p.Add("PassengerId", tripInitial.PassengerId);
 
            p.Add("DriverId", tripInitial.DriverId);
            p.Add("StatusId", tripInitial.StatusId);
            p.Add("VehicleFareId", tripInitial.VehicleFareId);
            p.Add("OriginLatitude", tripInitial.OriginLatitude);
            p.Add("OriginLongitude", tripInitial.OriginLongitude);
            p.Add("DestinationLatitude", tripInitial.DestinationLatitude);
            p.Add("DestinationLongitude", tripInitial.DestinationLongitude);
            p.Add("PickupPointLatitude", tripInitial.PickupPointLatitude);
            p.Add("PickupPointLongitude", tripInitial.PickupPointLongitude);
            p.Add("RequestTime", tripInitial.RequestTime);
            p.Add("OriginAddress", tripInitial.OriginAddress);
            p.Add("DestinationAddress", tripInitial.DestinationAddress);
            p.Add("DistanceValue", tripInitial.DistanceValue);
            p.Add("DistanceText", tripInitial.DistanceText);
            p.Add("DurationValue", tripInitial.DurationValue);
            p.Add("DurationText", tripInitial.DurationText);
            p.Add("DurationInTrafficValue", tripInitial.DurationInTrafficValue);
            p.Add("DurationInTrafficText", tripInitial.DurationInTrafficText);
            p.Add("BaseFare", tripInitial.BaseFare);
            p.Add("CostPerKm", tripInitial.CostPerKm);
            p.Add("CostPerMin", tripInitial.CostPerMin);
            p.Add("DistanceKm", tripInitial.DistanceKm);
            p.Add("DurationMinute", tripInitial.DurationMinute);
            p.Add("InitialFee", tripInitial.InitialFee);
            p.Add("CostOfTravelTime", tripInitial.CostOfTravelTime);
            p.Add("CostOfDistance", tripInitial.CostOfDistance);

            p.Add("EstimatedCost", tripInitial.EstimatedCost);
            p.Add("TaxAmount", tripInitial.TaxAmount);
            p.Add("TotalCost", tripInitial.TotalCost);

            p.Add("ModifiedBy", tripInitial.ModifiedBy);
            p.Add("UserName", logModel.UserName);
            p.Add("UserRole", logModel.UserRole);
            p.Add("IP", logModel.IP);

            await _dataAccessHelper.ExecuteData("USP_TripInitial_Update", p);
        }
        catch(Exception ex)
        {
            throw;
        }
    }

    public async Task<string> MakePayment(TripInitialModel tripInitial, LogModel logModel)
    {
        ClearCache(TripInitialCache);

        try
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("TripInitialId", tripInitial.Id);
            p.Add("PaymentMethodId", tripInitial.PaymentMethodId);
            p.Add("PaymentTypeId", tripInitial.PaymentTypeId);
            p.Add("PaymentOptionId", tripInitial.PaymentOptionId);
            p.Add("AccountNumber", tripInitial.AccountNumber);
            p.Add("ExpireMonthYear", tripInitial.ExpireMonthYear);
            p.Add("CvvCode", tripInitial.CvvCode);            
            p.Add("TransactionAmount", tripInitial.InitialFee); 
            p.Add("BillDate", tripInitial.RequestTime);

            p.Add("ModifiedBy", tripInitial.ModifiedBy);
            p.Add("UserName", logModel.UserName);
            p.Add("UserRole", logModel.UserRole);
            p.Add("IP", logModel.IP);

            var output = await _dataAccessHelper.ExecuteData("USP_TripInitial_MakePayment", p);
        }
        catch (Exception ex)
        {
           
            return ex.Message;
        }
        return "Sucess";
    }
    public async Task DeleteTripInitial(string tripInitialId, LogModel logModel)
    {
        ClearCache(TripInitialCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", tripInitialId);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_TripInitial_Delete", p);
    }

    public async Task<List<TripInitialModel>> Export()
    {
        return await _dataAccessHelper.QueryData<TripInitialModel, dynamic>("USP_TripInitial_Export", new { });
    }
    #endregion

    #region "Helper Methods"
    private void ClearCache(string key)
    {
        switch (key)
        {
            case TripInitialCache:
                var keys = _cache.Get<List<string>>(TripInitialCache);
                if (keys is not null)
                {
                    foreach (var item in keys)
                        _cache.Remove(item);
                    _cache.Remove(TripInitialCache);
                }
                break;
            default:
                break;
        }
    }
    #endregion
}