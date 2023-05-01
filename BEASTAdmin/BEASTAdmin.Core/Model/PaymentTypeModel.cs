
using System.ComponentModel.DataAnnotations;

namespace BEASTAdmin.Core.Model;

public class PaymentTypeModel : AuditModel
{

    [Required(ErrorMessage = "Please enter 'Name'.")]
    [MinLength(3, ErrorMessage = "Minimum length of 'Name' is 3 characters.")]
    [MaxLength(150, ErrorMessage = "Maximum length of 'Name' is 150 characters.")]
    public string Name { get; set; }

    public List<PaymentTypeModel> PaymentTypes { get; } = new List<PaymentTypeModel>();
}

