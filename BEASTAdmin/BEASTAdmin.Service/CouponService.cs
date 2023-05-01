using Microsoft.Extensions.Logging;
using BEASTAdmin.Service.Base;
using BEASTAdmin.Core.Infrastructure;
using BEASTAdmin.Core.Model;
using Polly;
using System.Net;
using System.Net.Http.Json;

namespace BEASTAdmin.Service;

public class CouponService : BaseService
{
    private readonly ILogger<CouponService> _logger;
    private readonly SecurityHelper _securityHelper;
    private readonly HttpClient _httpClient;

    public CouponService(ILogger<CouponService> logger, SecurityHelper securityHelper, IHttpClientFactory httpClientFactory, IContextAccessor contextAccessor) : base(securityHelper, httpClientFactory, contextAccessor)
    {
        this._logger = logger;
        this._securityHelper = securityHelper;
        this._httpClient = ConfigureClient().GetAwaiter().GetResult();
    }

    public async Task<PaginatedListModel<CouponModel>> GetCoupons(int pageNumber)
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
                    await _httpClient.GetAsync($"v1/Coupon?pagenumber={pageNumber}")
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<PaginatedListModel<CouponModel>>();
                break;
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task<CouponModel> GetCouponById(string couponId)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(couponId.ToString()));

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
                    await _httpClient.GetAsync($"v1/Coupon/{couponId}")
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<CouponModel>();
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task<CouponModel> InsertCoupon(CouponModel coupon, LogModel logModel)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(coupon.CouponCode));

            Dictionary<string, object> PostData = new Dictionary<string, object> {
            {"Data", coupon},
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
                        await _httpClient.PostAsJsonAsync($"v1/Coupon/InsertCoupon", PostData)
                    );

            switch (response.StatusCode)
            {
                case HttpStatusCode.Created:
                    return await response.Content.ReadFromJsonAsync<CouponModel>();
                default:
                    throw new Exception(await response.Content.ReadAsStringAsync());
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task UpdateCoupon(string couponId, CouponModel coupon, LogModel logModel)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(couponId.ToString()));

            Dictionary<string, object> PostData = new Dictionary<string, object> {
            {"Data", coupon},
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
                        await _httpClient.PutAsJsonAsync($"v1/Coupon/Update/{couponId}", PostData)
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

    public async Task DeleteCoupon(string couponId, LogModel logModel)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(couponId.ToString()));

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
                        await _httpClient.PutAsJsonAsync($"v1/Coupon/Delete/{couponId}", logModel)
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
                    await _httpClient.GetAsync($"v1/Coupon/Export")
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