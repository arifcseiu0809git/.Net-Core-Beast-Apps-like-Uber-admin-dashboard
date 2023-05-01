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
    public class AddressRepository : IAddressRepository
    {
        private readonly IDataAccessHelper _dataAccessHelper;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;
        private const string AddressCache = "AddressData";

        public AddressRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
        {
            this._dataAccessHelper = dataAccessHelper;
            this._config = config;
            this._cache = cache;
        }

        public async Task<PaginatedListModel<AddressModel>> GetAddress(int pageNumber)
        {
            PaginatedListModel<AddressModel> output = _cache.Get<PaginatedListModel<AddressModel>>(AddressCache + pageNumber);

            if (output is null)
            {
                DynamicParameters p = new DynamicParameters();
                p.Add("PageNumber", pageNumber);
                p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
                p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

                var result = await _dataAccessHelper.QueryData<AddressModel, dynamic>("USP_Address_GetAll", p);
                int TotalRecords = p.Get<int>("TotalRecords");
                int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

                output = new PaginatedListModel<AddressModel>
                {
                    PageIndex = pageNumber,
                    TotalRecords = TotalRecords,
                    TotalPages = totalPages,
                    HasPreviousPage = pageNumber > 1,
                    HasNextPage = pageNumber < totalPages,
                    Items = result.ToList()
                };

                _cache.Set(AddressCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

                List<string> keys = _cache.Get<List<string>>(AddressCache);
                if (keys is null)
                    keys = new List<string> { AddressCache + pageNumber };
                else
                    keys.Add(AddressCache + pageNumber);
                _cache.Set(AddressCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
            }

            return output;
        }

        public async Task<AddressModel> GetAddressById(string addressId)
        {
            return (await _dataAccessHelper.QueryData<AddressModel, dynamic>("USP_Address_GetById", new { Id = addressId })).FirstOrDefault();
        }


        public async Task<string> InsertAddress(AddressModel address, LogModel logModel)
        {
            try
            {
                ClearCache(AddressCache);

                DynamicParameters p = new DynamicParameters();
                p.Add("Id",address.Id);
                p.Add("UserId", address.UserId);
                p.Add("City", address.City);
                p.Add("Zip", address.Zip);
                p.Add("AddressLine1", address.AddressLine1);
                p.Add("AddressLine2", address.AddressLine2);
                p.Add("IsActive", address.IsActive);
                p.Add("IsDeleted", address.IsDeleted);
                p.Add("CreatedBy", address.CreatedBy);
                p.Add("UserName", logModel.UserName);
                p.Add("UserRole", logModel.UserRole);
                p.Add("IP", logModel.IP);

                await _dataAccessHelper.ExecuteData("USP_Address_Insert", p);
                return address.Id;
            }
            catch (Exception ex)
            { 
                throw;
            }
        }

        public async Task UpdateAddress(AddressModel address, LogModel logModel)
        {
            try
            {
                ClearCache(AddressCache);

                DynamicParameters p = new DynamicParameters();
                p.Add("Id", address.Id);
                p.Add("UserId", address.UserId);
                p.Add("City", address.City);
                p.Add("Zip", address.Zip);
                p.Add("AddressLine1", address.AddressLine1);
                p.Add("AddressLine2", address.AddressLine2);
                p.Add("IsActive", address.IsActive);
                p.Add("IsDeleted", address.IsDeleted);
                p.Add("LastModifiedBy", address.ModifiedBy);
                p.Add("UserName", logModel.UserName);
                p.Add("UserRole", logModel.UserRole);
                p.Add("IP", logModel.IP);

                await _dataAccessHelper.ExecuteData("USP_Address_Update", p);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task DeleteAddress(string addressId, LogModel logModel)
        {
            try
            {
                ClearCache(AddressCache);

                DynamicParameters p = new DynamicParameters();
                p.Add("Id", addressId);
                p.Add("UserName", logModel.UserName);
                p.Add("UserRole", logModel.UserRole);
                p.Add("IP", logModel.IP);

                await _dataAccessHelper.ExecuteData("USP_Address_Delete", p);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<AddressModel>> Export()
        {
            return await _dataAccessHelper.QueryData<AddressModel, dynamic>("USP_Address_Export", new { });
        }


        #region "Helper Methods"
        private void ClearCache(string key)
        {
            switch (key)
            {
                case AddressCache:
                    var keys = _cache.Get<List<string>>(AddressCache);
                    if (keys is not null)
                    {
                        foreach (var item in keys)
                            _cache.Remove(item);
                        _cache.Remove(AddressCache);
                    }
                    break;
                default:
                    break;
            }
        }
        #endregion
    }
}
