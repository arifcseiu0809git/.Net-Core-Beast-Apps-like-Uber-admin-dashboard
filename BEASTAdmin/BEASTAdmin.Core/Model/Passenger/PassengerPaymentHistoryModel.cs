namespace BEASTAdmin.Core.Model.Passenger;
public class PassengerPaymentHistoryModel : AuditModel
{
	public string PassengerName { get; set; }
	public string OriginAddress { get; set; }
	public string DestinationAddress { get; set; }
	public decimal TransactionAmount { get; set; }
	public string VehicleTypeName { get; set; }
	public string TripCancelledBy { get; set; }
	public DateTime? TripStartTime { get; set; }
	public DateTime? TripEndTime { get; set; }
	public string PaymentTypeName { get; set; }
	public string PaymentOptionName { get; set; }
	public string StatusName { get; set; }
}

