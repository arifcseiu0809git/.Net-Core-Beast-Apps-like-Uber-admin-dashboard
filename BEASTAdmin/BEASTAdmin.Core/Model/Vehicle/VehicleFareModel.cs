using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEASTAdmin.Core.Model.Vehicle
{
    public class VehicleFareModel : AuditModel
    {
        [DisplayName("Vehicle Type")]
        [Required(ErrorMessage = "Please Select a Vehicle Type.")]
        public string VehicleTypeId { get; set; }
        public string VehicleTypeName { get; set; }
        [DisplayName("Base Fare")]
		[Required(ErrorMessage = "Please Enter Value.")]
		public decimal? BaseFare { get; set; }
        [DisplayName("Cost Per Minute in Service Area")]
		[Required(ErrorMessage = "Please Enter Value.")]
		public decimal? CostPerMinInServiceArea { get; set; }
        [DisplayName("Waiting Cost Per Minute in Service Area")]
		[Required(ErrorMessage = "Please Enter Value.")]
		public decimal? WaitingCostPerMinInServiceArea { get; set; }
        [DisplayName("Cost Per Km in Service Area")]
		[Required(ErrorMessage = "Please Enter Value.")]
		public decimal? CostPerKmInServiceArea { get; set; }
        [DisplayName("Booking Fare")]
		[Required(ErrorMessage = "Please Enter Value.")]
		public decimal? BookingFee { get; set; }
        [DisplayName("Cost Per Minute Outside Service Area")]
		[Required(ErrorMessage = "Please Enter Value.")]
		public decimal? CostPerMinOutsideServiceArea { get; set; }
        [DisplayName("Waiting Cost Per Minute Outside Service Area")]
		[Required(ErrorMessage = "Please Enter Value.")]
		public decimal? WaitingCostPerMinOutsideServiceArea { get; set; }
        [DisplayName("Cost Per Km Outside Service Area")]
		[Required(ErrorMessage = "Please Enter Value.")]
		public decimal? CostPerKmOutsideServiceArea { get; set; }
        [DisplayName("Cancellation Fee")]
		[Required(ErrorMessage = "Please Enter Value.")]
		public decimal? CancelFee { get; set; }
    }
}
