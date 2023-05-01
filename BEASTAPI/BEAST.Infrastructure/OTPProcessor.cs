using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BEASTAPI.Core.Contract.Infrastructure;
using BEASTAPI.Core.Model;
using Polly;
using System.Net;
using System.Net.Http.Headers;
using System.Web;
using BEASTAPI.Core.Model.Passenger;

namespace BEASTAPI.Infrastructure;

public class OTPProcessor 
{
    private readonly ILogger<SMSSenderAlpha> _logger;
    private readonly SMSSettingsModel _smsSettings;
    private readonly HttpClient _httpClient;

    public OTPProcessor(ILogger<SMSSenderAlpha> logger, IOptions<SMSSettingsModel> smsSettings, IHttpClientFactory httpClientFactory)
    {
        this._logger = logger;
        this._smsSettings = smsSettings.Value;
        this._httpClient = httpClientFactory.CreateClient("SMSAPI");
        this._httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task SendSMSAsync(SMSModel sms)
    {
        try
        {
            var response = await Policy
            .Handle<HttpRequestException>(ex =>
            {
                _ = Task.Run(() => { _logger.LogError(ex, ex.Message); });
                return true;
            })
            .WaitAndRetryAsync
            (
                1, retryAttempt => TimeSpan.FromSeconds(2)
            )
            .ExecuteAsync(async () =>
                await _httpClient.GetAsync($"sendsms?api_key={_smsSettings.ApiKey}&msg={sms.Content}&to={string.Join(",", sms.To)}")
            );

            if (response.StatusCode != HttpStatusCode.OK)
                _ = Task.Run(() => { _logger.LogError(response.StatusCode.ToString()); });
        }
        catch (Exception ex)
        {
            _ = Task.Run(() => { _logger.LogError(ex, ex.Message); });
        }
    }




    // Start OTP Generation function
    public string GenerateOTP(PassengerModel passenger)
    {
        char[] charArr =passenger.MobileNumber.ToCharArray();
        string strrandom = string.Empty;
        Random objran = new Random();
        for (int i = 0; i < 4; i++)
        {
            //It will not allow Repetation of Characters
            int pos = objran.Next(1, charArr.Length);
            if (!strrandom.Contains(charArr.GetValue(pos).ToString())) strrandom += charArr.GetValue(pos);
            else i--;
        }
        return strrandom;
    }

    public static string SendSMS(string MblNo, string Msg)
    {
        string MainUrl = "SMSAPIURL"; //Here need to give SMS API URL
        string UserName = "username"; //Here need to give username
        string Password = "Password"; //Here need to give Password
        string SenderId = "SenderId";
        string strMobileno = MblNo;
        string URL = "";
        URL = MainUrl + "username=" + UserName + "&msg_token=" + Password + "&sender_id=" + SenderId + "&message=" + HttpUtility.UrlEncode(Msg).Trim() + "&mobile=" + strMobileno.Trim() + "";
        string strResponce = GetResponse(URL);
        string msg = "";
        if (strResponce.Equals("Fail"))
        {
            msg = "Fail";
        }
        else
        {
            msg = strResponce;
        }
        return msg;
    }

    public static string GetResponse(string smsURL)
    {
        try
        {
            WebClient objWebClient = new WebClient();
            System.IO.StreamReader reader = new System.IO.StreamReader(objWebClient.OpenRead(smsURL));
            string ResultHTML = reader.ReadToEnd();
            return ResultHTML;
        }
        catch (Exception)
        {
            return "Fail";
        }
    }
}