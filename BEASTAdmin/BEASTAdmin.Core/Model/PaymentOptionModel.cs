using System.ComponentModel.DataAnnotations;

namespace BEASTAdmin.Core.Model;

public class PaymentOptionModel : AuditModel
{

    [Required(ErrorMessage = "Please enter 'Payment Type'.")]
    public string PaymentType { get; set; }

    public string PaymentTypeName { get; set; }

    [Required(ErrorMessage = "Please enter 'Name'.")]
    [MinLength(3, ErrorMessage = "Minimum length of 'Name' is 3 characters.")]
    [MaxLength(150, ErrorMessage = "Maximum length of 'Name' is 150 characters.")]
    public string Name { get; set; }

    public List<PaymentOptionModel> PaymentOptions { get; } = new List<PaymentOptionModel>();
}

