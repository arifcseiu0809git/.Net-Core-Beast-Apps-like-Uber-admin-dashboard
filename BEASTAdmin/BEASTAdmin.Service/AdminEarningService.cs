using BEASTAdmin.Core.Infrastructure;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Service.Base;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace BEASTAdmin.Service
{
    public class AdminEarningService : BaseService
    {

        private readonly ILogger<AdminEarningService> _logger;
        private readonly SecurityHelper _securityHelper;
        private readonly HttpClient _httpClient;
        public AdminEarningService(
            SecurityHelper securityHelper,
            IHttpClientFactory httpClientFactory,
            IContextAccessor contextAccessor,
            ILogger<AdminEarningService> logger)
            : base(securityHelper, httpClientFactory, contextAccessor)
        {
            _securityHelper = securityHelper;
            _logger = logger;
            _httpClient = ConfigureClient().GetAwaiter().GetResult();
        }

        public async Task<PaginatedListModel<AdminEarningModel>> GetEarnings(int pageNumber)
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
                        await _httpClient.GetAsync($"v1/AdminEarning?pagenumber={pageNumber}")
                    );

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return await response.Content.ReadFromJsonAsync<PaginatedListModel<AdminEarningModel>>();
                default:
                    throw new Exception(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task<PaginatedListModel<AdminEarningModel>> GetEarningByDriverId(string driverId, int pageNumber)
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
                        await _httpClient.GetAsync($"v1/AdminEarning/GetEarningByDriverId?driverId={driverId}&pagenumber={pageNumber}")
                    );

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return await response.Content.ReadFromJsonAsync<PaginatedListModel<AdminEarningModel>>();
                default:
                    throw new Exception(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task<object> AddDriverCommissions(AdminEarningInsertModel adminEarningModel, LogModel logModel)
        {
            _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(adminEarningModel.DriverId));

            Dictionary<string, object> PostData = new Dictionary<string, object> {
            {"Data", adminEarningModel},
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
                        await _httpClient.PostAsJsonAsync($"v1/AdminEarning", PostData)
                    );

            switch (response.StatusCode)
            {
                case HttpStatusCode.Created:
                    return await response.Content.ReadFromJsonAsync<object>();
                case HttpStatusCode.OK:
                    return await response.Content.ReadFromJsonAsync<object>();
                default:
                    throw new Exception(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task<ExportFileModel> Export(string driverId)
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
                    await _httpClient.GetAsync($"v1/AdminEarning/Export?driverId={driverId}")
                );

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return await response.Content.ReadFromJsonAsync<ExportFileModel>();
                default:
                    throw new Exception(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task<decimal> GetDueCommissionByDriverId(string driverId, DateTime fromDate, DateTime toDate)
        {
            _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(driverId));

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
                        await _httpClient.GetAsync($"v1/AdminEarning/GetDueCommission?driverId={driverId}&fromDate={fromDate}&toDate={toDate}")
                    );

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return await response.Content.ReadFromJsonAsync<decimal>();
                default:
                    throw new Exception(await response.Content.ReadAsStringAsync());
            }
        }
    }
}
