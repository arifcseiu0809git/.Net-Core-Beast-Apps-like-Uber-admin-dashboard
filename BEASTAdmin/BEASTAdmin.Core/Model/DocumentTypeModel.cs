using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using BEASTAdmin.Core.Enums;

namespace BEASTAdmin.Core.Model;

public class DocumentTypeModel : AuditModel
{

	public DocumentTypeModel()
	{
		Id = "";
	}
    [Required(ErrorMessage = "Please Define 'Name'.")]
	public string Name { get; set; }
	[Required(ErrorMessage = "Please Select 'Document For'.")]
	public DocumentFor DocumentFor { get; set; }

	public string ?DocumentForSt
	{
		get
		{
			return this.DocumentFor==null?"": ((DocumentFor)this.DocumentFor).ToString();

		}
	}

	}