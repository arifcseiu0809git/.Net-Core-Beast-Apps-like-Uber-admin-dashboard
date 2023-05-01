using Microsoft.Extensions.Logging;
using BEASTAdmin.Service.Base;
using BEASTAdmin.Core.Infrastructure;
using BEASTAdmin.Core.Model;
using Polly;
using System.Net;
using System.Net.Http.Json;

namespace BEASTAdmin.Service;

public class SavedAddressService : BaseService
{
    private readonly ILogger<SavedAddressService> _logger;
    private readonly SecurityHelper _securityHelper;
    private readonly HttpClient _httpClient;

    public SavedAddressService(ILogger<SavedAddressService> logger, SecurityHelper securityHelper, IHttpClientFactory httpClientFactory, IContextAccessor contextAccessor) : base(securityHelper, httpClientFactory, contextAccessor)
    {
        this._logger = logger;
        this._securityHelper = securityHelper;
        this._httpClient = ConfigureClient().GetAwaiter().GetResult();
    }

    public async Task<PaginatedListModel<SavedAddressModel>> GetSavedAddresses(int pageNumber)
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
                    await _httpClient.GetAsync($"v1/SavedAddress?pagenumber={pageNumber}")
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<PaginatedListModel<SavedAddressModel>>();
                break;
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }
    

	public async Task<SavedAddressModel> GetSavedAddressById(string savedAddressId)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(savedAddressId.ToString()));

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
                    await _httpClient.GetAsync($"v1/SavedAddress{savedAddressId}")
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<SavedAddressModel>();
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }	
    public async Task<List<SavedAddressModel>> GetSavedAddressByName(string addressname)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(addressname.ToString()));

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
                    await _httpClient.GetAsync($"v1/SavedAddress/GetSavedAddressByName/{addressname}")
                );
        //await _httpClient.GetAsync($"v1/Driver/GetDriverStatusWise/{pageNumber}/{IsApproved}")
        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<List<SavedAddressModel>>();
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task<SavedAddressModel> InsertSavedAddress(SavedAddressModel savedAddress, LogModel logModel)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(savedAddress.HomeAddress));

            Dictionary<string, object> PostData = new Dictionary<string, object> {
            {"Data", savedAddress},
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
                        await _httpClient.PostAsJsonAsync($"v1/SavedAddress/InsertSavedAddress", PostData)
                    );

            switch (response.StatusCode)
            {
                case HttpStatusCode.Created:
                    return await response.Content.ReadFromJsonAsync<SavedAddressModel>();
                default:
                    throw new Exception(await response.Content.ReadAsStringAsync());
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task UpdateSavedAddress(string savedAddressId, SavedAddressModel savedAddress, LogModel logModel)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(savedAddressId.ToString()));

            Dictionary<string, object> PostData = new Dictionary<string, object> {
            {"Data", savedAddress},
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
                        await _httpClient.PutAsJsonAsync($"v1/SavedAddress/Update/{savedAddressId}", PostData)
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

    public async Task DeleteSavedAddress(string savedAddressId, LogModel logModel)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(savedAddressId.ToString()));

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
                        await _httpClient.PutAsJsonAsync($"v1/SavedAddress/Delete/{savedAddressId}", logModel)
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

    public async Task<ExportFileModel> Export()
    {
        try
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
                    await _httpClient.GetAsync($"v1/SavedAddress/Export")
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