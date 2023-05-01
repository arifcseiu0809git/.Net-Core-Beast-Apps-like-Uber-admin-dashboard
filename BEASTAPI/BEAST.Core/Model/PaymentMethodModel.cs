
namespace BEASTAPI.Core.Model
{
    public class PaymentMethodModel : AuditModel
    {
        public string UserId { get; set; }
        public string PaymentType { get; set; }
        public string PaymentTypeName { get; set; }
        public string ContactNo { get; set; }
        public string UserFullName { get; set; }
        public string PaymentOption { get; set; }
        public string PaymentOptionName { get; set; }
        public string AccountNumber { get; set; }
        public string ExpireMonthYear { get; set; }
        public string CvvCode { get; set; }
    }
}
