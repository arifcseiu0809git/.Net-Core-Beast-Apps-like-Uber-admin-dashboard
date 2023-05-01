using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Driver;
using BEASTAPI.Core.ViewModel;

namespace BEASTAPI.Core.Contract.Driver;

public interface IDriverRepository
{
    Task<RegisterResponseModel> Register(DriverModel driverModel, LogModel logModel);
    Task<RegisterResponseModel> SendOTP(string id, DriverModel driverModel, string generatedOTP);
    Task<DriverModel> ValidateDriverOTP(DriverModel driverModel);
    Task<PaginatedListModel<DriverModel>> GetDrivers(int pageNumber);
    Task<PaginatedListModel<DriverModel>> GetDrivers(int pageNumber, bool IsApproved,string StatusId, string NID, string DrivingLicenseNo);
    Task<DriverModel> GetDriverById(string DriverId);
    Task<List<DriverModel>> GetDistinctDrivers();
    Task<List<DriverModel>> GetActiveDrivers();
    Task<List<DriverModel>> Filter(bool IsApproved,string StatusId, string NID, string DrivingLicenseNo);
    Task<string> InsertDriver(DriverModel driver, LogModel logModel);
    Task UpdateDriver(DriverModel driver, LogModel logModel);
    Task UpdateStatus(DriverModel driver, LogModel logModel);
    Task DeleteDriver(string DriverId, LogModel logModel);
    Task<List<DriverModel>> Export(string StatusId, string NID, string DrivingLicenseNo, bool IsApproved);
    Task<PaginatedListModel<DriverCommissionModel>> GetDriverCommissions(string driverId, int pageNumber);
}