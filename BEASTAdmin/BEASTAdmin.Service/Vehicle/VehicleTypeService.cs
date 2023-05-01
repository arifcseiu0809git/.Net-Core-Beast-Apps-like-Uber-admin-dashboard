using Microsoft.Extensions.Logging;
using BEASTAdmin.Service.Base;
using BEASTAdmin.Core.Infrastructure;
using BEASTAdmin.Core.Model;
using Polly;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BEASTAdmin.Core.Model.Vehicle;

namespace BEASTAdmin.Service.Vehicle;

public class VehicleTypeService : BaseService
{
    private readonly ILogger<VehicleTypeService> _logger;
    private readonly SecurityHelper _securityHelper;
    private readonly HttpClient _httpClient;

    public VehicleTypeService(ILogger<VehicleTypeService> logger, SecurityHelper securityHelper, IHttpClientFactory httpClientFactory, IContextAccessor contextAccessor) : base(securityHelper, httpClientFactory, contextAccessor)
    {
        _logger = logger;
        _securityHelper = securityHelper;
        _httpClient = ConfigureClient().GetAwaiter().GetResult();
    }

    public async Task<PaginatedListModel<VehicleTypeModel>> GetVehicleTypes(int pageNumber)
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
                    await _httpClient.GetAsync($"v1/VehicleType?pagenumber={pageNumber}")
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<PaginatedListModel<VehicleTypeModel>>();
                break;
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task<List<VehicleTypeModel>> GetDistinctVehicleTypes()
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
                    await _httpClient.GetAsync($"v1/VehicleType/GetDistinctVehicleTypes")
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<List<VehicleTypeModel>>();
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task<VehicleTypeModel> GetVehicleTypeById(string vehicleId)
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
                    await _httpClient.GetAsync($"v1/VehicleType/{vehicleId}")
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<VehicleTypeModel>();
                break;
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }
    public async Task<List<VehicleTypeModel>> GetDistinctVehicleType()
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
                    await _httpClient.GetAsync($"v1/VehicleType/GetDistinctVehicleTypes")
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<List<VehicleTypeModel>>();
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }
    //public async Task<List<VehicleTypeModel>> GetVehicleTypeByCategoryId(int categoryId)
    //{
    //    _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(categoryId.ToString()));

    //    var response = await Policy
    //            .Handle<HttpRequestException>(ex =>
    //            {
    //                _ = Task.Run(() => { _logger.LogError(ex, ex.Message); });
    //                return true;
    //            })
    //            .WaitAndRetryAsync
    //            (
    //                1, retryAttempt => TimeSpan.FromSeconds(2)
    //            )
    //            .ExecuteAsync(async () =>
    //                await _httpClient.GetAsync($"v1/Pie/GetVehicleTypeByCategoryId/{categoryId}")
    //            );

    //    switch (response.StatusCode)
    //    {
    //        case HttpStatusCode.OK:
    //            return await response.Content.ReadFromJsonAsync<List<VehicleTypeModel>>();
    //            break;
    //        default:
    //            throw new Exception(await response.Content.ReadAsStringAsync());
    //    }
    //}

    public async Task<VehicleTypeModel> InsertVehicleType(VehicleTypeModel vehicle, LogModel logModel)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(vehicle.Name));

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
                    await _httpClient.PostAsJsonAsync($"v1/VehicleType", PostData)
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.Created:
                return await response.Content.ReadFromJsonAsync<VehicleTypeModel>();
                break;
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task UpdateVehicleType(string vehicleId, VehicleTypeModel vehicle, LogModel logModel)
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
                    await _httpClient.PutAsJsonAsync($"v1/VehicleType/Update/{vehicleId}", PostData)
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

    public async Task DeleteVehicleType(string vehicleId, LogModel logModel)
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
                    await _httpClient.PutAsJsonAsync($"v1/VehicleType/Delete/{vehicleId}", logModel)
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
                await _httpClient.GetAsync($"v1/VehicleType/Export")
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