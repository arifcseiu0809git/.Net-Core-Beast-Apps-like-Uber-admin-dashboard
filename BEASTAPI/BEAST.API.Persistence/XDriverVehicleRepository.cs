using BEASTAPI.Core.Contract.Persistence;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Common;
using BEASTAPI.Core.Model.Vehicle;
using BEASTAPI.Core.ViewModel;
using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEASTAPI.Persistence
{
    public class XDriverVehicleRepository : IXDriverVehicleRepository
    {
        private readonly IDataAccessHelper _dataAccessHelper;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;
        private const string XDriverVehicleCache = "XDriverVehicleData";

        public XDriverVehicleRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
        {
            _dataAccessHelper = dataAccessHelper;
            _config = config;
            _cache = cache;
        }

        #region DataAccessHelper Methods
        public async Task DeleteXDriverVehicleModel(string xDriverVehicleModelId, LogModel logModel)
        {
            ClearCache(XDriverVehicleCache);

            DynamicParameters p = new DynamicParameters();
            p.Add("Id", xDriverVehicleModelId);
            p.Add("UserName", logModel.UserName);
            p.Add("UserRole", logModel.UserRole);
            p.Add("IP", logModel.IP);

            await _dataAccessHelper.ExecuteData("USP_XDriverVehicle_Delete", p);
        }

        public async Task<List<XDriverVehicleViewModel>> Export()
        {
            return await _dataAccessHelper.QueryData<XDriverVehicleViewModel, dynamic>("USP_XDriverVehicle_Export", new { });
        }

        public async Task<XDriverVehicleReadViewModel> GetXDriverVehicleModelById(string xDriverVehicleModelId)
        {
            return (await _dataAccessHelper.QueryData<XDriverVehicleReadViewModel, dynamic>("USP_XDriverVehicle_GetById", new { Id = xDriverVehicleModelId })).FirstOrDefault();
        }

		public async Task<List<XDriverVehicleReadViewModel>> GetDriversBySearchPrefix(string prefix)
		{
			return (await _dataAccessHelper.QueryData<XDriverVehicleReadViewModel, dynamic>("USP_Driver_GetBySearchPrefix", new { prefix = prefix }));
		}

		public async Task<List<XDriverVehicleReadViewModel>> GetVehiclesBySearchPrefix(string prefix)
		{
			return (await _dataAccessHelper.QueryData<XDriverVehicleReadViewModel, dynamic>("USP_Vehicle_GetBySearchPrefix", new { prefix = prefix }));
		}

		public async Task<PaginatedListModel<XDriverVehicleViewModel>> GetXDriverVehicleModels(int pageNumber)
        {
            PaginatedListModel<XDriverVehicleViewModel> output = _cache.Get<PaginatedListModel<XDriverVehicleViewModel>>(XDriverVehicleCache + pageNumber);

            if (output is null)
            {
                DynamicParameters p = new DynamicParameters();
                p.Add("PageNumber", pageNumber);
                p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
                p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

                var result = await _dataAccessHelper.QueryData<XDriverVehicleViewModel, dynamic>("USP_XDriverVehicle_GetAll", p);
                int TotalRecords = p.Get<int>("TotalRecords");
                int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

                output = new PaginatedListModel<XDriverVehicleViewModel>
                {
                    PageIndex = pageNumber,
                    TotalRecords = TotalRecords,
                    TotalPages = totalPages,
                    HasPreviousPage = pageNumber > 1,
                    HasNextPage = pageNumber < totalPages,
                    Items = result.ToList()
                };

                _cache.Set(XDriverVehicleCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

                List<string> keys = _cache.Get<List<string>>(XDriverVehicleCache);
                if (keys is null)
                    keys = new List<string> { XDriverVehicleCache + pageNumber };
                else
                    keys.Add(XDriverVehicleCache + pageNumber);
                _cache.Set(XDriverVehicleCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
            }

            return output;
        }

        public async Task<string> InsertXDriverVehicleModel(XDriverVehicleModel xDriverVehicleModel, LogModel logModel)
        {
            ClearCache(XDriverVehicleCache);

            DynamicParameters p = new DynamicParameters();
            p.Add("Id", xDriverVehicleModel.Id);
            p.Add("UserId", xDriverVehicleModel.UserId);
            p.Add("VehicleId", xDriverVehicleModel.VehicleId);
			p.Add("IsActive", xDriverVehicleModel.IsActive = true);
			p.Add("IsDeleted", xDriverVehicleModel.IsDeleted = false);
			p.Add("CreatedBy", logModel.UserName);

            p.Add("UserName", logModel.UserName);
            p.Add("UserRole", logModel.UserRole);
            p.Add("IP", logModel.IP);

            await _dataAccessHelper.ExecuteData("USP_XDriverVehicle_Insert", p);
            return xDriverVehicleModel.Id;
        }

        public async Task UpdateXDriverVehicleModel(XDriverVehicleModel xDriverVehicleModel, LogModel logModel)
        {
            ClearCache(XDriverVehicleCache);

            DynamicParameters p = new DynamicParameters();
            p.Add("Id", xDriverVehicleModel.Id);
            p.Add("UserId", xDriverVehicleModel.UserId);
            p.Add("VehicleId", xDriverVehicleModel.VehicleId);
			p.Add("LastModifiedBy", logModel.UserName);

            p.Add("UserName", logModel.UserName);
            p.Add("UserRole", logModel.UserRole);
            p.Add("IP", logModel.IP);

            await _dataAccessHelper.ExecuteData("USP_XDriverVehicle_Update", p);
        }
        #endregion


        #region "Helper Methods"
        private void ClearCache(string key)
        {
            switch (key)
            {
                case XDriverVehicleCache:
                    var keys = _cache.Get<List<string>>(XDriverVehicleCache);
                    if (keys is not null)
                    {
                        foreach (var item in keys)
                            _cache.Remove(item);
                        _cache.Remove(XDriverVehicleCache);
                    }
                    break;
                default:
                    break;
            }
        }
        #endregion
    }
}
