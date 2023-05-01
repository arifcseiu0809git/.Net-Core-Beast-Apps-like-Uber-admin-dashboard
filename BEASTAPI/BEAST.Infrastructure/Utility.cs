using Microsoft.AspNetCore.Http;

namespace BEASTAPI.Infrastructure;

public class Utility
{
    public static string GetIPAddress(HttpRequest request)
    {
        return request.HttpContext.Connection.RemoteIpAddress.ToString();
    }
}