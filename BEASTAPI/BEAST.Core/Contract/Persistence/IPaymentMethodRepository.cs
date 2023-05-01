using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Map;

namespace BEASTAPI.Core.Contract.Persistence
{
    public interface IPaymentMethodRepository
    {
        Task<PaginatedListModel<PaymentMethodModel>> GetPaymentMethods(int pageNumber);
        Task<PaymentMethodModel> GetPaymentMethodById(string paymentMethodId);
        Task<string> InsertPaymentMethod(PaymentMethodModel paymentMethod, LogModel logModel);
        Task<string> UpdatePaymentMethod(PaymentMethodModel paymentMethod, LogModel logModel);
        Task DeletePaymentMethod(string paymentMethodId, LogModel logModel);
        Task<List<PaymentMethodModel>> Filter(string PaymentType, string PaymentOption, string ContactNo, string AccountNo);
        Task<List<PaymentMethodModel>> Export();
        Task<List<PaymentMethodModel>> GetPaymentMethodsByPaymentTypeAndPaymentOption(string paymentTypeId, string paymentOptionId);
        Task<List<PaymentMethodModel>> GetPaymentMethodByuserId(string paymentTypeId, string paymentOptionId, string userId);
    }
}
