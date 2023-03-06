using log4net;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VRChat.API.Api;
using VRChat.API.Client;
using VRChat.API.Model;

namespace aviDrug
{
    class VRCApiController
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(VRCApiController));

        public bool isPremium;
        public static VRCApiController instance;
        public UsersApi UserApi;
        public WorldsApi WorldApi;
        public AuthenticationApi AuthApi;
        public AvatarsApi AvatarApi;
        public FavoritesApi FavoritesApi;
        public string userID;
        public loginStatus currentLoginStatus;

        private Configuration Configuration;
        private string sessionValue = "";

        public loginStatus finishPhoneFA(string otpCode)
        {
            var config = Configuration;
            //Make HTTPGETRequest POST  "https://api.vrchat.cloud/api/1/auth/twofactor"
            string cookieValue = sessionValue;

            var url = "https://api.vrchat.cloud/api/1/auth/twofactorauth/totp/verify";
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";

            //Set auth cookie
            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(new Cookie("auth", cookieValue, "/", "api.vrchat.cloud"));

            //add json body with 2FA code
            string jsonBody = "{\"code\":\"" + otpCode + "\"}";

            //set content type to application/json
            request.ContentType = "application/json";

            //set content length to json body length
            request.ContentLength = jsonBody.Length;

            //set connection keep alive
            request.KeepAlive = true;

            //set encodign to gzip, deflate, br
            request.Headers["Accept-Encoding"] = "gzip, deflate, br";

            //set accept to */*
            request.Accept = "*/*";

            //set user agent to PostmanRuntime/7.26.10
            request.UserAgent = "SearchWorld/V3/Nocturn9992@proton.me";

            //set cache control to no-cache
            request.Headers["Cache-Control"] = "no-cache";

            //get request stream
            Stream dataStream = request.GetRequestStream();

            //write json body to request stream
            dataStream.Write(Encoding.UTF8.GetBytes(jsonBody), 0, jsonBody.Length);

            //get response
            var response = (HttpWebResponse)request.GetResponse();
            string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            Console.WriteLine(responseString);

            //get json result from responseString and read 'verified' field into string
            dynamic json = JsonConvert.DeserializeObject(responseString);
            string verified = json["verified"].ToString();

            if (verified == "True")
            {
                log.Debug("Sucessfully verified via 2FA !");

                //Get cookie sent from server
                string twoFACookie = response.Headers["Set-Cookie"];
                //Get cookie name and value from set cookie header
                string cookieN = twoFACookie.Split(';')[0].Split('=')[0];
                //get cookie value
                string cookieV = twoFACookie.Split(';')[0].Split('=')[1];

                config.AddApiKey(cookieN, cookieV);
                Configuration = config;

                finalizeLogin();
                return loginStatus.Sucess2FA;
            }
            else
            {
                return loginStatus.NotLoggedIn;
            }
        }

        public loginStatus finish2FAAuth(string code)
        {
            if(currentLoginStatus==loginStatus.Requires2FA)
            {
                return finish2FA(code);
            }
            else if(currentLoginStatus == loginStatus.RequiresPhone2FA)
            {
                return finishPhoneFA(code);
            }
            return loginStatus.NotLoggedIn;
        }

        public loginStatus finish2FA(string twoFactorCode)
        {
            var config = Configuration;
            //Make HTTPGETRequest POST  "https://api.vrchat.cloud/api/1/auth/twofactor"
            string cookieValue = sessionValue;
            
            var url = "https://api.vrchat.cloud/api/1/auth/twofactorauth/emailotp/verify";
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";

            //Set auth cookie
            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(new Cookie("auth", cookieValue, "/", "api.vrchat.cloud"));

            //add json body with 2FA code
            string jsonBody = "{\"code\":\"" + twoFactorCode + "\"}";

            //set content type to application/json
            request.ContentType = "application/json";

            //set content length to json body length
            request.ContentLength = jsonBody.Length;

            //set connection keep alive
            request.KeepAlive = true;

            //set encodign to gzip, deflate, br
            request.Headers["Accept-Encoding"] = "gzip, deflate, br";

            //set accept to */*
            request.Accept = "*/*";

            //set user agent to PostmanRuntime/7.26.10
            request.UserAgent = "SearchWorld/V3/Nocturn9992@proton.me";

            //set cache control to no-cache
            request.Headers["Cache-Control"] = "no-cache";

            //get request stream
            Stream dataStream = request.GetRequestStream();

            //write json body to request stream
            dataStream.Write(Encoding.UTF8.GetBytes(jsonBody), 0, jsonBody.Length);

            //get response
            var response = (HttpWebResponse)request.GetResponse();
            string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            Console.WriteLine(responseString);

            //get json result from responseString and read 'verified' field into string
            dynamic json = JsonConvert.DeserializeObject(responseString);
            string verified = json["verified"].ToString();

            if (verified == "True")
            {
                log.Debug("Sucessfully verified via 2FA !");

                //Get cookie sent from server
                string twoFACookie = response.Headers["Set-Cookie"];
                //Get cookie name and value from set cookie header
                string cookieN = twoFACookie.Split(';')[0].Split('=')[0];
                //get cookie value
                string cookieV = twoFACookie.Split(';')[0].Split('=')[1];

                config.AddApiKey(cookieN, cookieV);
                Configuration = config;

                finalizeLogin();
                return loginStatus.Sucess2FA;
            }
            else
            {
                return loginStatus.NotLoggedIn;
            }
        }

        public enum loginStatus
        {
            LoggedIn,
            NotLoggedIn,
            Requires2FA,
            Sucess2FA,
            RequiresPhone2FA
        }
        
        private void finalizeLogin()
        {
            AuthApi = new AuthenticationApi(Configuration);
            UserApi = new UsersApi(Configuration);
            WorldApi = new WorldsApi(Configuration);
            AvatarApi = new AvatarsApi(Configuration);
            FavoritesApi = new FavoritesApi(Configuration);

            userID = AuthApi.GetCurrentUser().Id;
        }
        private loginStatus loginToVRC(ref Configuration config)
        {
            //Make HTTPGetRequest to curl -X GET "https://api.vrchat.cloud/api/1/auth/user" \
            //-H "Authorization: Basic {string}"

            string url = "https://api.vrchat.cloud/api/1/auth/user";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";

            //add Authorization basic header
            string authInfo = config.Username + ":" + config.Password;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            request.Headers["Authorization"] = "Basic " + authInfo;

            //add api key header
            request.Headers["apiKey"] = "JlE5Jldo5Jibnk5O5hTx6XVqsJu4WJ26";

            config.AddApiKey("apiKey", "JlE5Jldo5Jibnk5O5hTx6XVqsJu4WJ26");
            //set connection keep alive
            request.KeepAlive = true;

            //set encodign to gzip, deflate, br
            request.Headers["Accept-Encoding"] = "gzip, deflate, br";

            //set accept to */*
            request.Accept = "*/*";

            //set user agent to PostmanRuntime/7.26.10
            request.UserAgent = "SearchWorld/V3/Nocturn9992@proton.me";

            //set cache control to no-cache
            request.Headers["Cache-Control"] = "no-cache";

            //get response
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return loginStatus.NotLoggedIn;
            }
            string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            Console.WriteLine(responseString);

            //Decode response to json and get requiresTwoFactorAuth field value
            //  dynamic json = JsonConvert.DeserializeObject(responseString);

            //Get cookie sent from server
            string cookie = response.Headers["Set-Cookie"];
            //Get cookie name and value from set cookie header
            string cookieName = cookie.Split(';')[0].Split('=')[0];
            //get cookie value
            string cookieValue = cookie.Split(';')[0].Split('=')[1];
            //add cookie to config = "JlE5Jldo5Jibnk5O5hTx6XVqsJu4WJ26";
            //   config.AddApiKey("apiKey", "JlE5Jldo5Jibnk5O5hTx6XVqsJu4WJ26");
            config.AddApiKey(cookieName, cookieValue);
            //get response code
            int responseCode = (int)response.StatusCode;

            if (responseCode == 200)
            {
                if (responseString.Contains("requiresTwoFactorAuth"))
                {
                    if (responseString.Contains("emailOtp"))
                    {
                        Configuration = config;
                        sessionValue = cookieValue;
                        return loginStatus.Requires2FA;
                    }
                    else if(responseString.Contains("totp"))
                    {
                        Configuration = config;
                        sessionValue = cookieValue;
                        return loginStatus.RequiresPhone2FA;
                    }
                }
                else
                {
                    return loginStatus.LoggedIn;
                }
            }
            else if (responseCode == 401)
            {
                log.Error("Login details are wrong");
                return loginStatus.NotLoggedIn;
            }

            return loginStatus.NotLoggedIn;

        }

        public VRCApiController()
        {
            //ooo singleton pattern 
            instance = this;
        }

        public loginStatus login(string user, string pass)
        {
            Configuration Config = new Configuration();
            Config.Username = user;
            Config.Password = pass;
            currentLoginStatus = loginToVRC(ref Config);
            Configuration = Config;

            if (currentLoginStatus == loginStatus.LoggedIn)
            {
                finalizeLogin();
            }

            return currentLoginStatus;
        }
    }
}
