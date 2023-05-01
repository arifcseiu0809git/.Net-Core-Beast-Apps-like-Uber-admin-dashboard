using BEASTAdmin.Core.Infrastructure;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Model.Vehicle;
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
	public class XDriverVehicleService : BaseService
	{
		private readonly ILogger<XDriverVehicleService> _logger;
		private readonly SecurityHelper _securityHelper;
		private readonly HttpClient _httpClient;
		public XDriverVehicleService(
			SecurityHelper securityHelper,
			IHttpClientFactory httpClientFactory, 
			IContextAccessor contextAccessor,
			ILogger<XDriverVehicleService> logger) : 
			base(securityHelper, httpClientFactory, contextAccessor)
		{
            this._logger = logger;
            this._securityHelper = securityHelper;
            this._httpClient = ConfigureClient().GetAwaiter().GetResult();
        }

		public async Task<PaginatedListModel<XDriverVehicleModel>> GetDriverVehicles(int pageNumber)
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
						await _httpClient.GetAsync($"v1/XDriverVehicle?pagenumber={pageNumber}")
					);

			switch (response.StatusCode)
			{
				case HttpStatusCode.OK:
					return await response.Content.ReadFromJsonAsync<PaginatedListModel<XDriverVehicleModel>>();
				default:
					throw new Exception(await response.Content.ReadAsStringAsync());
			}
		}

		
		public async Task<XDriverVehicleUpsertModel> GetDriverVehicleById(string id)
		{
			_httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(id.ToString()));

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
						await _httpClient.GetAsync($"v1/XDriverVehicle/{id}")
					);

			switch (response.StatusCode)
			{
				case HttpStatusCode.OK:
					return await response.Content.ReadFromJsonAsync<XDriverVehicleUpsertModel>();
				default:
					throw new Exception(await response.Content.ReadAsStringAsync());
			}
		}

		
		public async Task<XDriverVehicleUpsertModel> InsertDriverVehicle(XDriverVehicleUpsertModel xDriverVehicleModel, LogModel logModel)
		{
			try
			{
				_httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(xDriverVehicleModel.UserId));

				Dictionary<string, object> PostData = new Dictionary<string, object> {
			{"Data", xDriverVehicleModel},
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
							await _httpClient.PostAsJsonAsync($"v1/XDriverVehicle", PostData)
						);

				switch (response.StatusCode)
				{
					case HttpStatusCode.Created:
						return await response.Content.ReadFromJsonAsync<XDriverVehicleUpsertModel>();
					default:
						throw new Exception(await response.Content.ReadAsStringAsync());
				}
			}
			catch (Exception)
			{
				throw;
			}
		}

		public async Task Update(string id, XDriverVehicleUpsertModel xDriverVehicleModel, LogModel logModel)
		{
			try
			{
				_httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(id.ToString()));

				Dictionary<string, object> PostData = new Dictionary<string, object> {
				{"Data", xDriverVehicleModel},
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
							await _httpClient.PutAsJsonAsync($"v1/XDriverVehicle/Update/{id}", PostData)
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

		public async Task DeletexDriverVehicle(string id, LogModel logModel)
		{
			try
			{
				_httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(id.ToString()));

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
							await _httpClient.PutAsJsonAsync($"v1/XDriverVehicle/Delete/{id}", logModel)
						);

				switch (response.StatusCode)
				{
					case HttpStatusCode.NoContent:
						// Success
						break;
					case HttpStatusCode.OK:
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
						await _httpClient.GetAsync($"v1/XDriverVehicle/Export")
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

		public async Task<List<XDriverVehicleUpsertModel>> GetDriverBySearchPrefix(string prefix)
		{
			_httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(prefix.ToString()));

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
						await _httpClient.GetAsync($"v1/XDriverVehicle/Drivers/{prefix}")
					);

			switch (response.StatusCode)
			{
				case HttpStatusCode.OK:
					return await response.Content.ReadFromJsonAsync<List<XDriverVehicleUpsertModel>>();
				default:
					throw new Exception(await response.Content.ReadAsStringAsync());
			}
		}

		public async Task<List<XDriverVehicleUpsertModel>> GetVehiclesBySearchPrefix(string prefix)
		{
			_httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(prefix.ToString()));

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
						await _httpClient.GetAsync($"v1/XDriverVehicle/Vehicles/{prefix}")
					);

			switch (response.StatusCode)
			{
				case HttpStatusCode.OK:
					return await response.Content.ReadFromJsonAsync<List<XDriverVehicleUpsertModel>>();
				default:
					throw new Exception(await response.Content.ReadAsStringAsync());
			}
		}
	}
}
