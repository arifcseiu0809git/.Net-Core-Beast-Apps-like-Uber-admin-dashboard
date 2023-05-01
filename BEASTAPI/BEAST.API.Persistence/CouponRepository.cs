using Dapper;
using BEASTAPI.Core.Contract.Persistence;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEASTAPI.Core.Model;
using System.Data;

namespace BEASTAPI.Persistence
{
    public class CouponRepository: ICouponRepository
    {
        private readonly IDataAccessHelper _dataAccessHelper;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;
        private const string CouponCache = "CouponData";

        public CouponRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
        {
            this._dataAccessHelper = dataAccessHelper;
            this._config = config;
            this._cache = cache;
        }

        public async Task<PaginatedListModel<CouponModel>> GetCoupon(int pageNumber)
        {
            PaginatedListModel<CouponModel> output = _cache.Get<PaginatedListModel<CouponModel>>(CouponCache + pageNumber);

            if (output is null)
            {
                DynamicParameters p = new DynamicParameters();
                p.Add("PageNumber", pageNumber);
                p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
                p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

                var result = await _dataAccessHelper.QueryData<CouponModel, dynamic>("USP_Coupon_GetAll", p);
                int TotalRecords = p.Get<int>("TotalRecords");
                int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

                output = new PaginatedListModel<CouponModel>
                {
                    PageIndex = pageNumber,
                    TotalRecords = TotalRecords,
                    TotalPages = totalPages,
                    HasPreviousPage = pageNumber > 1,
                    HasNextPage = pageNumber < totalPages,
                    Items = result.ToList()
                };

                _cache.Set(CouponCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

                List<string> keys = _cache.Get<List<string>>(CouponCache);
                if (keys is null)
                    keys = new List<string> { CouponCache + pageNumber };
                else
                    keys.Add(CouponCache + pageNumber);
                _cache.Set(CouponCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
            }

            return output;
        }

        public async Task<CouponModel> GetCouponById(string couponId)
        {
            return (await _dataAccessHelper.QueryData<CouponModel, dynamic>("USP_Coupon_GetById", new { Id = couponId })).FirstOrDefault();
        }


        public async Task<string> InsertCoupon(CouponModel coupon, LogModel logModel)
        {
            try
            {
                ClearCache(CouponCache);

                DynamicParameters p = new DynamicParameters();
                p.Add("Id", coupon.Id);
                p.Add("UserId", coupon.UserId);
                p.Add("CouponCode", coupon.CouponCode);
                p.Add("StartTime", coupon.StartTime);
                p.Add("EndTime", coupon.EndTime);
                p.Add("DiscountOnFare", coupon.DiscountOnFare);
                p.Add("DiscountPercentage", coupon.DiscountPercentage);
                p.Add("IsAppliedCoupon", coupon.IsAppliedCoupon);
                p.Add("IsActive", coupon.IsActive);
                p.Add("IsDeleted", coupon.IsDeleted);
                p.Add("CreatedBy", coupon.CreatedBy);
                p.Add("UserName", logModel.UserName);
                p.Add("UserRole", logModel.UserRole);
                p.Add("IP", logModel.IP);

                await _dataAccessHelper.ExecuteData("USP_Coupon_Insert", p);
                return coupon.Id;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task UpdateCoupon(CouponModel coupon, LogModel logModel)
        {
            try
            {
                ClearCache(CouponCache);

                DynamicParameters p = new DynamicParameters();
                p.Add("Id", coupon.Id);
                p.Add("UserId", coupon.UserId);
                p.Add("CouponCode", coupon.CouponCode);
                p.Add("StartTime", coupon.StartTime);
                p.Add("EndTime", coupon.EndTime);
                p.Add("DiscountOnFare", coupon.DiscountOnFare);
                p.Add("DiscountPercentage", coupon.DiscountPercentage);
                p.Add("IsAppliedCoupon", coupon.IsAppliedCoupon);
                p.Add("IsActive", coupon.IsActive);
                p.Add("IsDeleted", coupon.IsDeleted); 
                p.Add("ModifiedBy", coupon.ModifiedBy);
                p.Add("UserName", logModel.UserName);
                p.Add("UserRole", logModel.UserRole);
                p.Add("IP", logModel.IP);

                await _dataAccessHelper.ExecuteData("USP_Coupon_Update", p);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task DeleteCoupon(string CouponId, LogModel logModel)
        {
            try
            {
                ClearCache(CouponCache);

                DynamicParameters p = new DynamicParameters();
                p.Add("Id", CouponId);
                p.Add("UserName", logModel.UserName);
                p.Add("UserRole", logModel.UserRole);
                p.Add("IP", logModel.IP);

                await _dataAccessHelper.ExecuteData("USP_Coupon_Delete", p);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<CouponModel>> Export()
        {
            return await _dataAccessHelper.QueryData<CouponModel, dynamic>("USP_Coupon_Export", new { });
        }

        #region "Helper Methods"
        private void ClearCache(string key)
        {
            switch (key)
            {
                case CouponCache:
                    var keys = _cache.Get<List<string>>(CouponCache);
                    if (keys is not null)
                    {
                        foreach (var item in keys)
                            _cache.Remove(item);
                        _cache.Remove(CouponCache);
                    }
                    break;
                default:
                    break;
            }
        }
        #endregion

    }
}
