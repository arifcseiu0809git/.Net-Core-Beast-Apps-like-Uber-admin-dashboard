

namespace BEASTAPI.Core.Model
{
    public class SavedAddressModel : AuditModel
    {
        public string UserId { get; set; }
        public string HomeAddress { get; set; }
        public decimal HomeLatiitude { get; set; }
        public decimal HomeLongitude { get; set; }
        public string OfficeAddress { get; set; }
        public decimal OfficeLatiitude { get; set; }
        public decimal OfficeLongitude { get; set; }
        public decimal OtherSavedPlace { get; set; }
        public decimal OtherLatiitude { get; set; }
        public decimal OtherLongitude { get; set; }
    }
}
