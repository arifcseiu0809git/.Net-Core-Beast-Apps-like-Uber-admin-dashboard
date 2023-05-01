using BEASTAPI.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BEASTAPI.Core.Contract.Persistence
{
    public interface ICouponRepository
    {
        Task<PaginatedListModel<CouponModel>> GetCoupon(int pageNumber);
        Task<CouponModel> GetCouponById(string CouponId);
        Task<string> InsertCoupon(CouponModel coupon, LogModel logModel);
        Task UpdateCoupon(CouponModel coupon, LogModel logModel);
        Task DeleteCoupon(string couponId, LogModel logModel);
        Task<List<CouponModel>> Export();
    }
}
