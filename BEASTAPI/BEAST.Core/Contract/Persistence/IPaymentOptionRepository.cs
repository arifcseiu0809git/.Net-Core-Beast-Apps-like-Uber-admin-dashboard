using BEASTAPI.Core.Model;

namespace BEASTAPI.Core.Contract.Persistence
{
    public interface IPaymentOptionRepository
    {
        Task<PaginatedListModel<PaymentOptionModel>> GetPaymentOption(int pageNumber);
        Task<List<PaymentOptionModel>> GetDistinctPaymentOptions();
        Task<PaymentOptionModel> GetPaymentOptionById(string paymentOptionId);
        Task<PaymentOptionModel> GetPaymentOptionByName(string paymentOptionName);
        Task<string> InsertPaymentOption(PaymentOptionModel paymentOption, LogModel logModel);
        Task UpdatePaymentOption(PaymentOptionModel paymentOption, LogModel logModel);
        Task DeletePaymentOption(string paymentOptionId, LogModel logModel);
        Task<List<PaymentOptionModel>> Export();
        Task<List<PaymentOptionModel>> GetPaymentOptionsByPaymentTypeId(string paymentTypeId);
    }
}
