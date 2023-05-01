using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Common;

namespace BEASTAPI.Core.Contract.Persistence.Common;

public interface IPricingRepository
{
    Task<PaginatedListModel<PricingModel>> GetPricings(int pageNumber);
    Task<PricingModel> GetPricingById(string pricingId);
    Task<PricingModel> GetPricingByName(string pricingName);
    Task<string> InsertPricing(PricingModel pricing, LogModel logModel);
    Task UpdatePricing(PricingModel pricing, LogModel logModel);
    Task DeletePricing(string pricingId, LogModel logModel);
    Task<List<PricingModel>> Export();
}