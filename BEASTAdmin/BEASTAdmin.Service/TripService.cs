using Microsoft.Extensions.Logging;
using BEASTAdmin.Service.Base;
using BEASTAdmin.Core.Infrastructure;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Model.Common;
using Polly;
using System.Net;
using System.Net.Http.Json;

namespace BEASTAdmin.Service;

public class TripService : BaseService
{
    private readonly ILogger<TripService> _logger;
    private readonly SecurityHelper _securityHelper;
    private readonly HttpClient _httpClient;

    public TripService(ILogger<TripService> logger, SecurityHelper securityHelper, IHttpClientFactory httpClientFactory, IContextAccessor contextAccessor) : base(securityHelper, httpClientFactory, contextAccessor)
    {
        this._logger = logger;
        this._securityHelper = securityHelper;
        this._httpClient = ConfigureClient().GetAwaiter().GetResult();
    }

    public async Task<PaginatedListModel<TripModel>> GetTrips(string statusId, int pageNumber)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(pageNumber.ToString()));

        var response = await Policy
                .Handle<HttpRequestException>(ex =>
                {
                    _ = Task.Run(() => { _logger.LogError(ex, ex.Message); });
                    return true;
                })
                .WaitAndRetryAsync
                (
                    1, retryAttempt => TimeSpan.FromSeconds(2)
                )
                .ExecuteAsync(async () =>
                    await _httpClient.GetAsync($"v1/Trip/GetTrips/{statusId}/{pageNumber}")
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<PaginatedListModel<TripModel>>();
                break;
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }


	public async Task<PaginatedListModel<TripModel>> GetTripsByDriverId(string driverId, int PageNumber)
	{
		_httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(driverId.ToString()));

		var response = await Policy
				.Handle<HttpRequestException>(ex =>
				{
					_ = Task.Run(() => { _logger.LogError(ex, ex.Message); });
					return true;
				})
				.WaitAndRetryAsync
				(
					1, retryAttempt => TimeSpan.FromSeconds(2)
				)
				.ExecuteAsync(async () =>
					await _httpClient.GetAsync($"v1/Trip/GetTripsByDriverId/{driverId}/{PageNumber}")
				);

		switch (response.StatusCode)
		{
			case HttpStatusCode.OK:
				return await response.Content.ReadFromJsonAsync<PaginatedListModel<TripModel>>();
				break;
			default:
				throw new Exception(await response.Content.ReadAsStringAsync());
		}
	}
	public async Task<TripModel> GetTripByName(string name)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(name));

        var response = await Policy
                .Handle<HttpRequestException>(ex =>
                {
                    _ = Task.Run(() => { _logger.LogError(ex, ex.Message); });
                    return true;
                })
                .WaitAndRetryAsync
                (
                    1, retryAttempt => TimeSpan.FromSeconds(2)
                )
                .ExecuteAsync(async () =>
                    await _httpClient.GetAsync($"v1/Trip/{name}")
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<TripModel>();
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }
    public async Task<TripModel> GetTripById(string tripId)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(tripId.ToString()));

        var response = await Policy
                .Handle<HttpRequestException>(ex =>
                {
                    _ = Task.Run(() => { _logger.LogError(ex, ex.Message); });
                    return true;
                })
                .WaitAndRetryAsync
                (
                    1, retryAttempt => TimeSpan.FromSeconds(2)
                )
                .ExecuteAsync(async () =>
                    await _httpClient.GetAsync($"v1/Trip/{tripId}")
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<TripModel>();
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task<List<TripModel>> Filter(string StatusId, string VehicleTypId, string DriverName, string PassengerName, string ContactNo)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(StatusId + VehicleTypId + DriverName + PassengerName.ToString()+ContactNo));

        var response = await Policy
                .Handle<HttpRequestException>(ex =>
                {
                    _ = Task.Run(() => { _logger.LogError(ex, ex.Message); });
                    return true;
                })
                .WaitAndRetryAsync
                (
                    1, retryAttempt => TimeSpan.FromSeconds(2)
                )
                .ExecuteAsync(async () =>
                    await _httpClient.GetAsync($"v1/Trip/Filter/{StatusId}/{VehicleTypId}/{DriverName}/{PassengerName}/{ContactNo}")
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<List<TripModel>>();
                break;
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }


    public async Task<List<TripModel>> GetDistinctTrips()
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash());

        var response = await Policy
                .Handle<HttpRequestException>(ex =>
                {
                    _ = Task.Run(() => { _logger.LogError(ex, ex.Message); });
                    return true;
                })
                .WaitAndRetryAsync
                (
                    1, retryAttempt => TimeSpan.FromSeconds(2)
                )
                .ExecuteAsync(async () =>
                    await _httpClient.GetAsync($"v1/Trip/GetDistinctTrips")
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<List<TripModel>>();
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }
    public async Task<TripModel> InsertTrip(TripModel trip, LogModel logModel)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(trip.StartLocationName));

            Dictionary<string, object> PostData = new Dictionary<string, object> {
            {"Data", trip},
            {"Log", logModel}
        };

            var response = await Policy
                    .Handle<HttpRequestException>(ex =>
                    {
                        _ = Task.Run(() => { _logger.LogError(ex, ex.Message); });
                        return true;
                    })
                    .WaitAndRetryAsync
                    (
                        1, retryAttempt => TimeSpan.FromSeconds(2)
                    )
                    .ExecuteAsync(async () =>
                        await _httpClient.PostAsJsonAsync($"v1/Trip", PostData)
                    );

            switch (response.StatusCode)
            {
                case HttpStatusCode.Created:
                    return await response.Content.ReadFromJsonAsync<TripModel>();
                default:
                    throw new Exception(await response.Content.ReadAsStringAsync());
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task UpdateTrip(string tripId, TripModel trip, LogModel logModel)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(tripId.ToString()));

            Dictionary<string, object> PostData = new Dictionary<string, object> {
            {"Data", trip},
            {"Log", logModel}
        };

            var response = await Policy
                    .Handle<HttpRequestException>(ex =>
                    {
                        _ = Task.Run(() => { _logger.LogError(ex, ex.Message); });
                        return true;
                    })
                    .WaitAndRetryAsync
                    (
                        1, retryAttempt => TimeSpan.FromSeconds(2)
                    )
                    .ExecuteAsync(async () =>
                        await _httpClient.PutAsJsonAsync($"v1/Trip/Update/{tripId}", PostData)
                    );

            switch (response.StatusCode)
            {
                case HttpStatusCode.NoContent:
                    break;
                default:
                    throw new Exception(await response.Content.ReadAsStringAsync());
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task DeleteTrip(string tripId, LogModel logModel)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(tripId.ToString()));

            var response = await Policy
                    .Handle<HttpRequestException>(ex =>
                    {
                        _ = Task.Run(() => { _logger.LogError(ex, ex.Message); });
                        return true;
                    })
                    .WaitAndRetryAsync
                    (
                        1, retryAttempt => TimeSpan.FromSeconds(2)
                    )
                    .ExecuteAsync(async () =>
                        await _httpClient.PutAsJsonAsync($"v1/Trip/Delete/{tripId}", logModel)
                    );

            switch (response.StatusCode)
            {
                case HttpStatusCode.NoContent:
                    // Success
                    break;
                default:
                    throw new Exception(await response.Content.ReadAsStringAsync());
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<ExportFileModel> Export(string StatusId, string VehicleTypId, string DriverName, string PassengerName, string ContactNo)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(StatusId + VehicleTypId + DriverName + PassengerName.ToString()+ ContactNo));

            var response = await Policy
                .Handle<HttpRequestException>(ex =>
                {
                    _ = Task.Run(() => { _logger.LogError(ex, ex.Message); });
                    return true;
                })
                .WaitAndRetryAsync
                (
                    1, retryAttempt => TimeSpan.FromSeconds(2)
                )
                .ExecuteAsync(async () =>
                    await _httpClient.GetAsync($"v1/Trip/Export/{StatusId}/{VehicleTypId}/{DriverName}/{PassengerName}/{ContactNo}")
                
                );

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return await response.Content.ReadFromJsonAsync<ExportFileModel>();
                default:
                    throw new Exception(await response.Content.ReadAsStringAsync());
            }
        }
        catch (Exception)
        {
            throw;
        }
    }
}