using Microsoft.Extensions.Logging;
using BEASTAdmin.Service.Base;
using BEASTAdmin.Core.Infrastructure;
using BEASTAdmin.Core.Model.Vehicle;
using BEASTAdmin.Core.Model;
using Polly;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace BEASTAdmin.Service.Vehicle;

public class VehiclesService : BaseService
{
    private readonly ILogger<VehiclesService> _logger;
    private readonly SecurityHelper _securityHelper;
    private readonly HttpClient _httpClient;

    public VehiclesService(ILogger<VehiclesService> logger, SecurityHelper securityHelper, IHttpClientFactory httpClientFactory, IContextAccessor contextAccessor) : base(securityHelper, httpClientFactory, contextAccessor)
    {
        this._logger = logger;
        this._securityHelper = securityHelper;
        this._httpClient = ConfigureClient().GetAwaiter().GetResult();
    }

    public async Task<PaginatedListModel<VehiclesList>> GetVehicles(int pageNumber)
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
                    await _httpClient.GetAsync($"v1/Vehicle?pagenumber={pageNumber}")
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<PaginatedListModel<VehiclesList>>();
                break;
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task<VehiclesList> GetVehicleById(string vehicleId)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(vehicleId.ToString()));

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
                    await _httpClient.GetAsync($"v1/Vehicle/{vehicleId}")
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<VehiclesList>();
                break;
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }
	
    public async Task<List<VehiclesList>> GetVehicleByTypeId(int vehicleTypeId)
	{
		_httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(vehicleTypeId.ToString()));

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
					await _httpClient.GetAsync($"v1/Vehicle/GetVehicleByTypeId/{vehicleTypeId}")
				);

		switch (response.StatusCode)
		{
			case HttpStatusCode.OK:
				return await response.Content.ReadFromJsonAsync<List<VehiclesList>>();
				break;
			default:
				throw new Exception(await response.Content.ReadAsStringAsync());
		}
	}

	public async Task<VehiclesList> InsertVehicle(VehiclesList vehicle, LogModel logModel)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(vehicle.RegistrationNo));

        Dictionary<string, object> PostData = new Dictionary<string, object> {
            {"Data", vehicle},
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
                    await _httpClient.PostAsJsonAsync($"v1/Vehicle", PostData)
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.Created:
                return await response.Content.ReadFromJsonAsync<VehiclesList>();
                break;
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }

	//public async Task<List<Vehicles>> GetDistinctVehicleModelIncludingBrand(string vehicleId)
	//{
	//	_httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash());

	//	var response = await Policy
	//			.Handle<HttpRequestException>(ex =>
	//			{
	//				_ = Task.Run(() => { _logger.LogError(ex, ex.Message); });
	//				return true;
	//			})
	//			.WaitAndRetryAsync
	//			(
	//				1, retryAttempt => TimeSpan.FromSeconds(2)
	//			)
	//			.ExecuteAsync(async () =>
	//				await _httpClient.GetAsync($"v1/VehicleCurrentLocation/GetDistinctVehicleCurrentLocationModel/{vehicleCurrentLocationId}")
	//			);

	//	switch (response.StatusCode)
	//	{
	//		case HttpStatusCode.OK:
	//			return await response.Content.ReadFromJsonAsync<List<VehicleCurrentLocationModel>>();
	//		default:
	//			throw new Exception(await response.Content.ReadAsStringAsync());
	//	}
	//}
	public async Task UpdateVehicle(string vehicleId, VehiclesList vehicle, LogModel logModel)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(vehicleId.ToString()));

        Dictionary<string, object> PostData = new Dictionary<string, object> {
            {"Data", vehicle},
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
                    await _httpClient.PutAsJsonAsync($"v1/Vehicle/Update/{vehicleId}", PostData)
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

    public async Task DeleteVehicle(string vehicleId, LogModel logModel)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(vehicleId.ToString()));

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
                    await _httpClient.PutAsJsonAsync($"v1/Vehicle/Delete/{vehicleId}", logModel)
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

    public async Task<ExportFileModel> Export()
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
                await _httpClient.GetAsync($"v1/Vehicle/Export")
            );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<ExportFileModel>();
                break;
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }
}