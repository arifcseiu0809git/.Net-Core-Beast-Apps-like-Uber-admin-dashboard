using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BEASTAdmin.Core.Model;

public class DriverModel : AuditModel
{
	public DriverModel()
    {
		Id = "";
         UserId = "";
        FirstName = "";
        MiddleName = "";
        LastName = "";
        
        Email = "";
        MobileNumber = "";
        GenderId = "";
        NID = "";
        DrivingLicenseNo = "";
        StatusId = "";
        StatusName = "";
        VehicleTypeId = "";
        VehicleFareId = "";
        BaseFare = 0;
        IsApproved = false;
        ApprovedBy = "";
        
        
        
    }

    public string UserId { get; set; }

	public string UserFullName 
	{
		get
		{
			var fullname = $"{FirstName} {LastName}";
			fullname = !String.IsNullOrWhiteSpace(MiddleName) ?
						$"{FirstName} {MiddleName} {LastName}" : fullname;
			return fullname; 
		}
	}
	[Required(ErrorMessage = "Please Type 'FirstName'.")]
	[MinLength(1, ErrorMessage = "Minimum length of 'Name' is 1 characters.")]
	public string FirstName { get; set; }
	
	public string MiddleName { get; set; }
	[Required(ErrorMessage = "Please Type 'LastName'.")]
	public string LastName { get; set; }

	[DisplayName("Date Of Birth")]
	[DisplayFormat(DataFormatString = "{0:dd/MMMM/yyyy}", ApplyFormatInEditMode = true)]
	public DateTime DateOfBirth { get; set; } = DateTime.Now.AddYears(-10);
	[Required(ErrorMessage = "Please Type 'Email'.")]
	[EmailAddress]
	public string Email { get; set; }
	[Required(ErrorMessage = "Please Type 'Mobile Number'.")]
	[MinLength(10, ErrorMessage = "Minimum length of 'Mobile Number' is 10 characters.")]
	public string MobileNumber { get; set; }

	[Required(ErrorMessage = "Please Select 'Gender'.")]
	public string GenderId { get; set; }
	[Required(ErrorMessage = "Please Type 'NID'.")]
	[MinLength(13, ErrorMessage = "Minimum length of 'NID' is 13 characters.")]
	public string NID { get; set; }
	[Required(ErrorMessage = "Please Select 'Driver License No'.")]
	[MinLength(5, ErrorMessage = "Minimum length of 'Driving License No' is 5 characters.")]
	public string DrivingLicenseNo { get; set; }
	public string StatusId { get; set; }
	public string StatusName { get; set; }
    public string? VehicleTypeId { get; set; }
    public string? VehicleFareId { get; set; }
    public double? BaseFare { get; set; }

    public bool? IsApproved { get; set; }
	public string ApprovedBy { get; set; }
	public DateTime? ApprovedDate { get; set; }
	public string UserType { get; set; }
	public string OtpCode { get; set; }
	public DateTime StartTime { get; set; }
	public DateTime EndTime { get; set; }
	public Int32 TimeInMinutes { get; set; }

	

}