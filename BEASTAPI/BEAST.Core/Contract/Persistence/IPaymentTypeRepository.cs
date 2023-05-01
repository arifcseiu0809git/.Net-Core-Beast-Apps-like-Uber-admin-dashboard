using BEASTAPI.Core.Model;

namespace BEASTAPI.Core.Contract.Persistence
{
    public interface IPaymentTypeRepository
    {
        Task<PaginatedListModel<PaymentTypeModel>> GetPaymentTypes(int pageNumber);
        Task<List<PaymentTypeModel>> GetDistinctPaymentTypes();
        Task<PaymentTypeModel> GetPaymentTypeById(string paymentTypeId);
        Task<PaymentTypeModel> GetPaymentTypeByName(string paymentTypeName);
        Task<string> InsertPaymentType(PaymentTypeModel paymentType, LogModel logModel);
        Task UpdatePaymentType(PaymentTypeModel paymentType, LogModel logModel);
        Task DeletePaymentType(string paymentTypeId, LogModel logModel);
        Task<List<PaymentTypeModel>> Export();
        Task<bool> CheckIfDuplicateExists(string id, string paymentTypeName);
    }
}
