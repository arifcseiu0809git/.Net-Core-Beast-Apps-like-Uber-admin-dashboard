using BEASTAdmin.Core.Infrastructure;
using BEASTAdmin.Core.Model.Vehicle;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Service.Base;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;

namespace BEASTAdmin.Service.Vehicle
{
    public class VehicleFareService : BaseService
    {
        private readonly ILogger<VehicleFareService> _logger;
        private readonly SecurityHelper _securityHelper;
        private readonly HttpClient _httpClient;

        public VehicleFareService(
            SecurityHelper securityHelper,
            IHttpClientFactory httpClientFactory,
            IContextAccessor contextAccessor,
            ILogger<VehicleFareService> logger
            ) : base(securityHelper, httpClientFactory, contextAccessor)
        {
            this._securityHelper = securityHelper;
            this._httpClient = ConfigureClient().GetAwaiter().GetResult();
            this._logger = logger;
        }

        public async Task<PaginatedListModel<VehicleFareModel>> GetVehicleFares(int pageNumber)
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
                        await _httpClient.GetAsync($"v1/VehicleFare?pagenumber={pageNumber}")
                    );

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return await response.Content.ReadFromJsonAsync<PaginatedListModel<VehicleFareModel>>();
                default:
                    throw new Exception(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task<VehicleFareModel> GetVehicleFareById(string vehicleFareId)
        {
            _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(vehicleFareId.ToString()));

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
                        await _httpClient.GetAsync($"v1/VehicleFare/{vehicleFareId}")
                    );

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return await response.Content.ReadFromJsonAsync<VehicleFareModel>();
                default:
                    throw new Exception(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task<VehicleFareModel> InsertVehicleFare(VehicleFareModel vehicleFare, LogModel logModel)
        
        {
            _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash());

            Dictionary<string, object> PostData = new Dictionary<string, object> {
            {"Data", vehicleFare},
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
                        await _httpClient.PostAsJsonAsync($"v1/VehicleFare", PostData)
                    );

            switch (response.StatusCode)
            {
                case HttpStatusCode.Created:
                    return await response.Content.ReadFromJsonAsync<VehicleFareModel>();
                default:
                    throw new Exception(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task UpdateVehicleFare(string vehicleFareId, VehicleFareModel vehicleFare, LogModel logModel)
        {
            _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(vehicleFareId.ToString()));

            Dictionary<string, object> PostData = new Dictionary<string, object> {
            {"Data", vehicleFare},
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
                        await _httpClient.PutAsJsonAsync($"v1/VehicleFare/Update/{vehicleFareId}", PostData)
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

        public async Task DeleteVehicleFare(string vehicleFareId, LogModel logModel)
        {
            _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(vehicleFareId.ToString()));

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
                        await _httpClient.PutAsJsonAsync($"v1/VehicleFare/Delete/{vehicleFareId}", logModel)
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
                    await _httpClient.GetAsync($"v1/VehicleFare/Export")
                );

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return await response.Content.ReadFromJsonAsync<ExportFileModel>();
                default:
                    throw new Exception(await response.Content.ReadAsStringAsync());
            }
        }
    }
}
