namespace BEASTAPI.Core.Model.Vehicle
{
    public class VehicleFareModel : AuditModel
    {
        public string VehicleTypeId { get; set; }
        public string VehicleTypeName { get; set; }
        public decimal? BaseFare { get; set; }
        public decimal? CostPerMinInServiceArea { get; set; }
        public decimal? WaitingCostPerMinInServiceArea { get; set; }
        public decimal? CostPerKmInServiceArea { get; set; }
        public decimal? BookingFee { get; set; }
        public decimal? CostPerMinOutsideServiceArea { get; set; }
        public decimal? WaitingCostPerMinOutsideServiceArea { get; set; }
        public decimal? CostPerKmOutsideServiceArea { get; set; }
        public decimal? CancelFee { get; set; }       
    }
}