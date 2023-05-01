using BEASTAPI.Core.Contract.Persistence;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.ViewModel;
using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEASTAPI.Persistence
{
    public class AdminEarningRepository : IAdminEarningRepository
    {
        private readonly IDataAccessHelper _dataAccessHelper;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;
        private const string AdminEarningCache = "AdminEarningData";
        public AdminEarningRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
        {
            _dataAccessHelper = dataAccessHelper;
            _config = config;
            _cache = cache;
        }

        public Task DeleteEarning(string id, LogModel logModel)
        {
            throw new NotImplementedException();
        }

        public async Task<List<AdminEarning>> Export(string driverId = null)
        {
            return new List<AdminEarning>();
        }

        public async Task<PaginatedListModel<AdminEarning>> GetEarning(int pageNumber)
        {
            return new PaginatedListModel<AdminEarning>();
        }

        public async Task<PaginatedListModel<AdminEarning>> GetEarningByDriverId(string driverId, int pageNumber)
        {
            return new PaginatedListModel<AdminEarning>();
        }

        public Task<AdminEarning> GetEarningByTripId(string tripId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> InsertDriverCommissionsByDateRange(AdminEarningInsertViewModel model, LogModel logModel)
        {

            DynamicParameters p = new DynamicParameters();
            p.Add("id", model.Id);
            p.Add("driverId", model.DriverId);
            p.Add("fromDate", model.FromDate);
            p.Add("toDate", model.ToDate);
            p.Add("isCommisionReceived", model.IsCommisionReceived);
            p.Add("transactionId", model.TransactionId);
            p.Add("isActive", model.IsActive);
            p.Add("isDeleted", model.IsDeleted);
            p.Add("createdBy", model.CreatedBy);
            p.Add("createdDate", model.CreatedDate);
            p.Add("modifiedBy", model.ModifiedBy);
            p.Add("modifiedDate", model.ModifiedDate);
            p.Add("commissionRate", model.CommissionRate);
            p.Add("commissionReceiveDate", model.CommissionReceiveDate);
            p.Add("paymentTypeId", model.PaymentTypeId);
            p.Add("paymentOptionId", model.PaymentOptionId);
            p.Add("paymentMethodId", model.PaymentMethodId);
            p.Add("userName", logModel.UserName);
            p.Add("userRole", logModel.UserRole);
            p.Add("iP", logModel.IP);

			await _dataAccessHelper.ExecuteData("USP_AdminEarning_Insert", p);
			return true;
		}

        public Task UpdateEarning(AdminEarning adminEarning, LogModel logModel)
        {
            throw new NotImplementedException();
        }

        public async Task<decimal> GetDueCommissionByDriverId(string driverId, DateTime fromDate, DateTime toDate)
        {
            var p = new DynamicParameters();
            p.Add("driverId", driverId);
            p.Add("fromDate", fromDate);
            p.Add("toDate", toDate);
            var dueCommission = (await _dataAccessHelper.QueryData<decimal, dynamic>("[USP_Driver_GetDueCommission]", p)).FirstOrDefault();
            return dueCommission;
        }

        #region "Helper Methods"
        private void ClearCache(string key)
        {
            switch (key)
            {
                case AdminEarningCache:
                    var keys = _cache.Get<List<string>>(AdminEarningCache);
                    if (keys is not null)
                    {
                        foreach (var item in keys)
                            _cache.Remove(item);
                        _cache.Remove(AdminEarningCache);
                    }
                    break;
                default:
                    break;
            }
        }
        #endregion
    }
}
