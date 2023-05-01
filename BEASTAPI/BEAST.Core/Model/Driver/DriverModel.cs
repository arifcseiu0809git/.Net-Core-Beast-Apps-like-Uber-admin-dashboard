namespace BEASTAPI.Core.Model.Driver
{
    public class DriverModel : AuditModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public string GenderId { get; set; }
        public string NID { get; set; }
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
}