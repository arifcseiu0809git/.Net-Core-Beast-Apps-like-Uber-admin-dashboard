using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BEASTAdmin.Core.Model;
public class PassengerModel : AuditModel
{
    public string UserId { get; set; }

    [Required(ErrorMessage = "Please enter 'FirstName'.")]
    [MinLength(3, ErrorMessage = "Minimum length of 'FirstName' is 3 characters.")]
    [MaxLength(150, ErrorMessage = "Maximum length of 'FirstName' is 150 characters.")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Please enter 'LastName'.")]
    [MinLength(3, ErrorMessage = "Minimum length of 'LastName' is 3 characters.")]
    [MaxLength(150, ErrorMessage = "Maximum length of 'LastName' is 150 characters.")]
    public string LastName { get; set; }

    [DisplayName("System Status")]
    [Required(ErrorMessage = "Please select a 'System Status'.")]
    public string SystemStatusId { get; set; }
    public string SystemStatusName { get; set; }

    [Required(ErrorMessage = "Please enter 'Email'.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Please enter 'DateOfBirth'.")]
    public DateTime? DateOfBirth { get; set; }

    [Required(ErrorMessage = "Please enter 'Gender'.")]
    public string Gender { get; set; }

    [Required(ErrorMessage = "Please enter 'MobileNumber'.")]
    public string MobileNumber { get; set; }

    [Required(ErrorMessage = "Please enter 'EnrollDate'.")]
    public DateTime? EnrollDate { get; set; }

    [Required(ErrorMessage = "Please enter 'NID'.")]
    public string NID { get; set; }
    public string Password { get; set; }

    public string UserType { get; set; }
    public string OtpCode { get; set; }
    public string City { get; set; }
    public float Ratings { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public Int32 TimeInMinutes { get; set; }
}

