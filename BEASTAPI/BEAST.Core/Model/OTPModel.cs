namespace BEASTAPI.Core.Model;

public class OTPModel
{
    public string MobileNo { get; set; }
    public string OTP { get; set; }
    public bool Status { get; set; }    
    public DateTime Expires { get; set; }
    public string Message { get; set; }
}