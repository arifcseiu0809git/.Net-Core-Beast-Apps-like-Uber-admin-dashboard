using Microsoft.Extensions.Logging;
using BEASTAdmin.Service.Base;
using BEASTAdmin.Core.Infrastructure;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Model.Common;
using Polly;
using System.Net;
using System.Net.Http.Json;

namespace BEASTAdmin.Service;

public class TripInitialService : BaseService
{
    private readonly ILogger<TripInitialService> _logger;
    private readonly SecurityHelper _securityHelper;
    private readonly HttpClient _httpClient;

    public TripInitialService(ILogger<TripInitialService> logger, SecurityHelper securityHelper, IHttpClientFactory httpClientFactory, IContextAccessor contextAccessor) : base(securityHelper, httpClientFactory, contextAccessor)
    {
        this._logger = logger;
        this._securityHelper = securityHelper;
        this._httpClient = ConfigureClient().GetAwaiter().GetResult();
    }

    public async Task<PaginatedListModel<TripInitialModel>> GetTripInitials(int pageNumber)
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
                    await _httpClient.GetAsync($"v1/TripInitial?pagenumber={pageNumber}")
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<PaginatedListModel<TripInitialModel>>();
                break;
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }

	public async Task<TripInitialModel> GetTripInitialByName(string name)
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
                    await _httpClient.GetAsync($"v1/TripInitial/{name}")
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<TripInitialModel>();
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }
    public async Task<TripInitialModel> GetTripInitialById(string tripInitialId)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(tripInitialId.ToString()));

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
                    await _httpClient.GetAsync($"v1/TripInitial/{tripInitialId}")
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<TripInitialModel>();
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task<List<TripInitialModel>> GetDistinctTripInitials()
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
                    await _httpClient.GetAsync($"v1/TripInitial/GetDistinctTripInitials")
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<List<TripInitialModel>>();
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }
    public async Task<TripInitialModel> InsertTripInitial(TripInitialModel tripInitial, LogModel logModel)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(tripInitial.OriginAddress));

            Dictionary<string, object> PostData = new Dictionary<string, object> {
            {"Data", tripInitial},
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
                        await _httpClient.PostAsJsonAsync($"v1/TripInitial", PostData)
                    );

            switch (response.StatusCode)
            {
                case HttpStatusCode.Created:
                    return await response.Content.ReadFromJsonAsync<TripInitialModel>();
                default:
                    throw new Exception(await response.Content.ReadAsStringAsync());
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task UpdateTripInitial(string tripInitialId, TripInitialModel tripInitial, LogModel logModel)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(tripInitialId.ToString()));

            Dictionary<string, object> PostData = new Dictionary<string, object> {
            {"Data", tripInitial},
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
                        await _httpClient.PutAsJsonAsync($"v1/TripInitial/Update/{tripInitialId}", PostData)
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
    
    public async Task<string> MakePayment(string tripInitialId, TripInitialModel tripInitial, LogModel logModel)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(tripInitialId.ToString()));

            Dictionary<string, object> PostData = new Dictionary<string, object> {
            {"Data", tripInitial},
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
                        await _httpClient.PutAsJsonAsync($"v1/TripInitial/MakePayment/{tripInitialId}", PostData)
                    );

            switch (response.StatusCode)
            {
                case HttpStatusCode.NoContent:
                    return "Success";
                default:
                    throw new Exception(await response.Content.ReadAsStringAsync());
            }
        }
        catch (Exception ex)
        {
            return  ex.Message;
        }
    }

    public async Task DeleteTripInitial(string tripInitialId, LogModel logModel)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(tripInitialId.ToString()));

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
                        await _httpClient.PutAsJsonAsync($"v1/TripInitial/Delete/{tripInitialId}", logModel)
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

	public async Task<List<TripInitialModel>> Filter(string StatusId, string VehicleTypId, string DriverName, string PassengerName)
	{
		_httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(StatusId + VehicleTypId + DriverName + PassengerName.ToString().ToString()));

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
					await _httpClient.GetAsync($"v1/TripInitial/Filter/{StatusId}/{VehicleTypId}/{DriverName}/{PassengerName}")
				);

		switch (response.StatusCode)
		{
			case HttpStatusCode.OK:
				return await response.Content.ReadFromJsonAsync<List<TripInitialModel>>();
				break;
			default:
				throw new Exception(await response.Content.ReadAsStringAsync());
		}
	}

	public async Task<ExportFileModel> Export(string StatusId, string VehicleTypId, string DriverName, string PassengerName)
	{
        try
        {
			_httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(StatusId + VehicleTypId + DriverName + PassengerName.ToString().ToString()));
			//_httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash());

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
					await _httpClient.GetAsync($"v1/TripInitial/Export/{StatusId}/{VehicleTypId}/{DriverName}/{PassengerName}")
					//await _httpClient.GetAsync($"v1/TripInitial/Filter/{StatusId}/{VehicleTypId}/{DriverName}/{PassengerName}")
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