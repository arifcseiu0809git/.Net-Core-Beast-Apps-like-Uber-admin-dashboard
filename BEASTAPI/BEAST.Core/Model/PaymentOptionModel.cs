
namespace BEASTAPI.Core.Model
{
    public class PaymentOptionModel : AuditModel
    { 
        public string PaymentType { get; set; }
        public string PaymentTypeName { get; set; }
        public string Name { get; set; } 
    }
}
