using Microsoft.Extensions.Logging;
using BEASTAdmin.Service.Base;
using BEASTAdmin.Core.Infrastructure;
using BEASTAdmin.Core.Model;
using Polly;
using System.Net;
using System.Net.Http.Json;
using BEASTAdmin.Core.Model.Passenger;
using System.Security.Cryptography;

namespace BEASTAdmin.Service;

public class PassengerService : BaseService
{
    private readonly ILogger<PassengerService> _logger;
    private readonly SecurityHelper _securityHelper;
    private readonly HttpClient _httpClient;

    public PassengerService(ILogger<PassengerService> logger, SecurityHelper securityHelper, IHttpClientFactory httpClientFactory, IContextAccessor contextAccessor) : base(securityHelper, httpClientFactory, contextAccessor)
    {
        this._logger = logger;
        this._securityHelper = securityHelper;
        this._httpClient = ConfigureClient().GetAwaiter().GetResult();
    }

    public async Task<PaginatedListModel<PassengerModel>> GetPassengers(int pageNumber)
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
                    await _httpClient.GetAsync($"v1/PassengerAuth?pagenumber={pageNumber}")
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<PaginatedListModel<PassengerModel>>();
                break;
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task<PaginatedListModel<PassengerRideHistoryModel>> GetPassengerRideHistoriesById(string passengerId, int pageNumber)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(passengerId));

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
                    await _httpClient.GetAsync($"v1/PassengerAuth/GetPassengerRideHistoriesById/{passengerId}/{pageNumber}")

				);

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<PaginatedListModel<PassengerRideHistoryModel>>();
                break;
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }
	public async Task<PaginatedListModel<PassengerPaymentHistoryModel>> GetPassengerPaymentHistoriesById(string passengerId, int pageNumber)
	{
		_httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(passengerId));

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
					await _httpClient.GetAsync($"v1/PassengerAuth/GetPassengerPaymentHistoriesById/{passengerId}/{pageNumber}")

				);

		switch (response.StatusCode)
		{
			case HttpStatusCode.OK:
				return await response.Content.ReadFromJsonAsync<PaginatedListModel<PassengerPaymentHistoryModel>>();
				break;
			default:
				throw new Exception(await response.Content.ReadAsStringAsync());
		}
	}
	public async Task<PassengerModel> GetPassengerById(string passengerId)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(passengerId));

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
                    await _httpClient.GetAsync($"v1/PassengerAuth/{passengerId}")
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<PassengerModel>();
                break;
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task<List<PassengerModel>> GetDistinctPassengers()
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
                    await _httpClient.GetAsync($"v1/PassengerAuth/GetDistinctPassengers")
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<List<PassengerModel>>();
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }
    public async Task<PassengerModel> InsertPassenger(PassengerModel passenger, LogModel logModel)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(passenger.FirstName+" "+passenger.LastName));

        Dictionary<string, object> PostData = new Dictionary<string, object> {
            {"Data", passenger},
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
                    await _httpClient.PostAsJsonAsync($"v1/PassengerAuth/Register", PostData)
                );

        switch (response.StatusCode)
        {
            //case HttpStatusCode.Created:
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<PassengerModel>();
                break;
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task UpdatePassenger(string passengerId, PassengerModel passenger, LogModel logModel)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(passengerId));

        Dictionary<string, object> PostData = new Dictionary<string, object> {
            {"Data", passenger},
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
                    await _httpClient.PutAsJsonAsync($"v1/PassengerAuth/Update/{passengerId}", PostData)
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

    public async Task DeletePassenger(string passengerId, LogModel logModel)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(passengerId));

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
                    await _httpClient.PutAsJsonAsync($"v1/PassengerAuth/Delete/{passengerId}", logModel)
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

    public async Task<ExportFileModel> Export(string StatusId,string City,string ContactNo)
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
                await _httpClient.GetAsync($"v1/PassengerAuth/Export/{StatusId}/{City}/{ContactNo}")
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
	
    public async Task<ExportFileModel> ExportPassengerRideHistory(string passengerId)
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
				await _httpClient.GetAsync($"v1/PassengerAuth/ExportPassengerRideHistory/{passengerId}")
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
	public async Task<ExportFileModel> ExportPassengerPaymentHistory(string passengerId)
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
				await _httpClient.GetAsync($"v1/PassengerAuth/ExportPassengerPaymentHistory/{passengerId}")
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
	public async Task<List<PassengerModel>> Filter(string StatusId, string City, string ContactNo)
	{
		_httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(StatusId + City + ContactNo ));

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
					await _httpClient.GetAsync($"v1/PassengerAuth/Filter/{StatusId}/{City}/{ContactNo}")
				);

		switch (response.StatusCode)
		{
			case HttpStatusCode.OK:
				return await response.Content.ReadFromJsonAsync<List<PassengerModel>>();
				break;
			default:
				throw new Exception(await response.Content.ReadAsStringAsync());
		}
	}

}