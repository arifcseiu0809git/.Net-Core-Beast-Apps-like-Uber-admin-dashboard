using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEASTAPI.Core.Model.Passenger
{
	public class PassengerExportModel
	{
		public string FirstName { get; set; }
		public string Email { get; set; }
		public string MobileNumber { get; set; }		
		public string SystemStatusName { get; set; }		
		public string City { get; set; }
		public string Ratings { get; set; }
	}
}
