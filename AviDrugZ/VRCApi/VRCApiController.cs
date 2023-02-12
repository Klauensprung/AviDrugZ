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

        private Configuration Configuration;
        public void saveHeadersToConfig()
        {

        }

        private string sessionValue = "";

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
            request.UserAgent = "PostmanRuntime/7.26.10";

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
            Sucess2FA
        }
        
        private void finalizeLogin()
        {
            AuthApi = new AuthenticationApi(Configuration);
            UserApi = new UsersApi(Configuration);
            WorldApi = new WorldsApi(Configuration);
            AvatarApi = new AvatarsApi(Configuration);
            FavoritesApi = new FavoritesApi(Configuration);
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
            request.UserAgent = "PostmanRuntime/7.26.10";

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
            loginStatus status = loginToVRC(ref Config);
            Configuration = Config;

            if (status == loginStatus.LoggedIn)
            {
                finalizeLogin();
            }

            return status;
        }

        private async Task tryLogin()
        {

            try
            {
                // Calling "GetCurrentUser(Async)" logs you in if you are not already logged in.
                CurrentUser CurrentUser = await AuthApi.GetCurrentUserAsync();
                //    userID = CurrentUser.Id;
                // MessageBox.Show(String.Format("Logged in as {0}, Current Avatar {1}", CurrentUser.DisplayName, CurrentUser.CurrentAvatar));

                //User OtherUser = await UserApi.GetUserAsync("usr_c1644b5b-3ca4-45b4-97c6-a2a0de70d469");
                // MessageBox.Show(String.Format("Found user {0}, joined {1}", OtherUser.DisplayName, OtherUser.DateJoined));


            }
            catch (ApiException e)
            {
                log.Error(e.Message);
                throw e;
            }
        }
    }
}
