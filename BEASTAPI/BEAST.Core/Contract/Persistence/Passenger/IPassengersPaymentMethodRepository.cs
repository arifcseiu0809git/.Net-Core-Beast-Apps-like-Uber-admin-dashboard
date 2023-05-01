using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Passenger;

namespace BEASTAPI.Core.Contract.Persistence.Passenger;

public interface IPassengersPaymentMethodRepository
{
    Task<PaginatedListModel<PassengersPaymentMethodModel>> GetPassengersPaymentMethods(int pageNumber);
    Task<PassengersPaymentMethodModel> GetPassengersPaymentMethodById(string passengersPaymentMethodId);
    Task<PassengersPaymentMethodModel> GetPassengersPaymentMethodByName(string passengersPaymentMethodName);
    Task<string> InsertPassengersPaymentMethod(PassengersPaymentMethodModel passengersPaymentMethod, LogModel logModel);
    Task UpdatePassengersPaymentMethod(PassengersPaymentMethodModel passengersPaymentMethod, LogModel logModel);
    Task DeletePassengersPaymentMethod(string passengersPaymentMethodId, LogModel logModel);
    Task<List<PassengersPaymentMethodModel>> Export();
}