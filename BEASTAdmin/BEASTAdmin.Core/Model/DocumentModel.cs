using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BEASTAdmin.Core.Model;

public class DocumentModel : AuditModel
{

	[Required(ErrorMessage = "Please Define 'User'.")]
	public string UserId { get; set; }
	[Required(ErrorMessage = "Please Define 'Document Type'.")]
	public string DocumentType { get; set; }
	[Required(ErrorMessage = "Please enter 'url'.")]
	public string DocumentUrl { get; set; }
	public string Name { get; set; }

}