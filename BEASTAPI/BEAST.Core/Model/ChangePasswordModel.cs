namespace BEASTAPI.Core.Model;

public class ChangePasswordModel
{
    public string UserId { get; set; }
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
    public string ConfirmNewPassword { get; set; }
}
