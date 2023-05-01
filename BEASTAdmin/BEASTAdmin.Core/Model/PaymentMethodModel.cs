
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace BEASTAdmin.Core.Model;

public class PaymentMethodModel : AuditModel
{
    public PaymentMethodModel()
    {
        Message = "";

	}
	public string UserId { get; set; }

    [DisplayName("Payment Type")]
    [Required(ErrorMessage = "Please select a 'Payment Type'.")]
    public string PaymentType { get; set; }

    public string PaymentTypeName { get; set; }

    [DisplayName("Payment Option")]
    [Required(ErrorMessage = "Please select a 'Payment Option'.")]
    public string PaymentOption { get; set; }

    public string PaymentOptionName { get; set; }
    public string UserFullName { get; set; }
    public string ContactNo{ get; set; }

    public string AccountNumber { get; set; }
	public string ExpireMonthYear { get; set; }
	public string CvvCode { get; set; }
	public string Message { get; set; }


	public List<PaymentMethodModel> PaymentMethods { get; } = new List<PaymentMethodModel>();
}

