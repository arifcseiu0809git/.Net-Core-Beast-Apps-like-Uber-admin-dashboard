using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Driver;
using BEASTAPI.Core.Model.Passenger;

namespace BEASTAPI.Core.Contract.Persistence.Passenger;

public interface IPassengerAuthRepository
{
    Task<RegisterResponseModel> Register(PassengerModel passengerInfo, LogModel logModel);
    Task<int> InsertOTP(PassengerModel passenger, LogModel logModel);
    Task<PassengerModel> ValidatePassengerAuthOTP(PassengerModel passenger);

    Task<PaginatedListModel<PassengerModel>> GetPassengers(int pageNumber);
    Task<PaginatedListModel<PassengerRideHistoriesModel>> GetPassengerRideHistories(string passengerId, int pageNumber);
    Task<PaginatedListModel<PassengerPaymentHistoriesModel>> GetPassengerPaymentHistories(string passengerId, int pageNumber);

	Task<PassengerModel> GetPassengerById(string passengerId);
    Task<PassengerModel> GetPassengerByName(string passengerName);
    Task<List<PassengerModel>> GetDistinctPassengers();
    Task<string> InsertPassenger(PassengerModel passenger, LogModel logModel);
    Task UpdatePassenger(PassengerModel passenger, LogModel logModel);
    Task DeletePassenger(string passengerId, LogModel logModel);
    Task<List<PassengerExportModel>> Export(string StatusId);
    Task<List<PassengerExportModel>> ExportWithContactNo(string StatusId, string ContactNo);
    Task<List<PassengerExportModel>> ExportWithCity_N_ContactNo(string StatusId,string City,string ContactNo);
    Task<List<PassengerExportModel>> ExportWithCity(string StatusId,string City);
    Task<List<PassengerRideHistoriesModel>> ExportPassengerRideHistory(string passengerId);
    Task<List<PassengerPaymentHistoriesModel>> ExportPassengerPaymentHistory(string passengerId);
    Task<UserInfoModel> Login(UserLoginModel userLogin);
    Task<UserInfoModel> ChangePassword(ChangePasswordModel changePassword);
    Task UpdateRefreshToken(string userId, TokenModel token);
    Task<TokenModel> GetRefreshToken(string userId);
    Task<UserInfoModel> GetCurrentUser(string userId);
	Task<List<PassengerModel>> Filter(string StatusId);
	Task<List<PassengerModel>> FilterWithCity_N_ContactNo(string StatusId, string City, string ContactNo);
	Task<List<PassengerModel>> FilterWithCity(string StatusId, string City);
	Task<List<PassengerModel>> FilterWithContactNo(string StatusId, string ContactNo);
}