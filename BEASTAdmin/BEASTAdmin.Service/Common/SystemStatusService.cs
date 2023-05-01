using Microsoft.Extensions.Logging;
using BEASTAdmin.Service.Base;
using BEASTAdmin.Core.Infrastructure;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Model.Common;
using Polly;
using System.Net;
using System.Net.Http.Json;
using BEASTAdmin.Core.Model.Vehicle;

namespace BEASTAdmin.Service;

public class SystemStatusService : BaseService
{
    private readonly ILogger<SystemStatusService> _logger;
    private readonly SecurityHelper _securityHelper;
    private readonly HttpClient _httpClient;

    public SystemStatusService(ILogger<SystemStatusService> logger, SecurityHelper securityHelper, IHttpClientFactory httpClientFactory, IContextAccessor contextAccessor) : base(securityHelper, httpClientFactory, contextAccessor)
    {
        this._logger = logger;
        this._securityHelper = securityHelper;
        this._httpClient = ConfigureClient().GetAwaiter().GetResult();
    }

    public async Task<PaginatedListModel<SystemStatusModel>> GetSystemStatus(int pageNumber)
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
                    await _httpClient.GetAsync($"v1/SystemStatus?pagenumber={pageNumber}")
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<PaginatedListModel<SystemStatusModel>>();
                break;
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }
	public async Task<List<SystemStatusModel>> GetDistinctSystemStatus()
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
					await _httpClient.GetAsync($"v1/SystemStatus/GetDistinctSystemStatus")
				);

		switch (response.StatusCode)
		{
			case HttpStatusCode.OK:
				return await response.Content.ReadFromJsonAsync<List<SystemStatusModel>>();
			default:
				throw new Exception(await response.Content.ReadAsStringAsync());
		}
	}
	public async Task<SystemStatusModel> GetSystemStatusById(string SystemStatusId)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(SystemStatusId.ToString()));

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
                    await _httpClient.GetAsync($"v1/SystemStatus/{SystemStatusId}")
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<SystemStatusModel>();
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }
    public async Task<SystemStatusModel> InsertSystemStatus(SystemStatusModel SystemStatus, LogModel logModel)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(SystemStatus.Name));

            Dictionary<string, object> PostData = new Dictionary<string, object> {
            {"Data", SystemStatus},
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
                        await _httpClient.PostAsJsonAsync($"v1/SystemStatus", PostData)
                    );

            switch (response.StatusCode)
            {
                case HttpStatusCode.Created:
                    return await response.Content.ReadFromJsonAsync<SystemStatusModel>();
                default:
                    throw new Exception(await response.Content.ReadAsStringAsync());
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task UpdateSystemStatus(string SystemStatusId, SystemStatusModel SystemStatus, LogModel logModel)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(SystemStatusId.ToString()));

            Dictionary<string, object> PostData = new Dictionary<string, object> {
            {"Data", SystemStatus},
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
                        await _httpClient.PutAsJsonAsync($"v1/SystemStatus/Update/{SystemStatusId}", PostData)
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

    public async Task DeleteSystemStatus(string SystemStatusId, LogModel logModel)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(SystemStatusId.ToString()));

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
                        await _httpClient.PutAsJsonAsync($"v1/SystemStatus/Delete/{SystemStatusId}", logModel)
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
                    await _httpClient.GetAsync($"v1/SystemStatus/Export")
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