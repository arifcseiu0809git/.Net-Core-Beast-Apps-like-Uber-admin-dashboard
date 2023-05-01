using BEASTAPI.Core.Model;

namespace BEASTAPI.Core.Contract.Infrastructure;

public interface ISecurityHelper
{
    string GenerateHash(string payload = "Default Payload");
    string GenerateJSONWebToken(UserInfoModel userInfo);
    string GenerateRefreshToken();
    bool IsValidHash(string senderHash, string payLoad = "Default Payload");
    string GenerateOTP(string mobileNumber);
}