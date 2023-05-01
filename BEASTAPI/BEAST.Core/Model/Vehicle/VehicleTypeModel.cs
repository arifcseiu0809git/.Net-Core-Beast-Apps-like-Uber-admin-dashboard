namespace BEASTAPI.Core.Model.Vehicle
{
    public class VehicleTypeModel: AuditModel
    {
        public string Name { get; set; }
        public float? UnitPricePerKm { get; set; }
        public float? WaitingTimeCostPerMin { get; set; }
        public string ImageName { get; set; }
        public string ImageUrl { get; set; }
        public string Descriptions { get; set; }
        public int? SortingPriority { get; set; }
    }
}