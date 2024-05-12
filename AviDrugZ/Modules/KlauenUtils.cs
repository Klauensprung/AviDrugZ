using AviDrugZ.Models;
using AviDrugZ.Models.WebModels;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;
using aviDrug;
using AviDrugZ.Views;
using AviDrugZ.Models.VRC;

namespace AviDrugZ
{
    public static class KlauenUtils
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(KlauenUtils));

        public static AvatarVRC? exportToVRCDB(string avatarID)
        {

            //Create a POST request
            string liveUrl = "https://search.bs002.de/api/Avatar/putavatar";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(liveUrl);
            request.Method = "PUT";
            request.UserAgent = "AviDrugZ v2 | " + VRCApiController.instance.userID;
            request.ContentType = "application/json";

            //Make a custom json with avatarid (Id) and user (userId)
            //Create new json object with those fields, not with serialized object
            string json = "{\"Id\":\"" + "avtr_"+avatarID + "\",\"UserId\":\"" + VRCApiController.instance.userID + "\"}";

            //Send the json string as the body
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            try
            {
                var httpResponse = (HttpWebResponse)request.GetResponse();

                //Check for success
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    //parse to AvatarVRC
                    AvatarVRC avatarVRC = JsonConvert.DeserializeObject<AvatarVRC>(result);
                    return avatarVRC;

                    if (result.EndsWith("Not Created.\"}"))
                    {
                        Log.Info("Avatar " + avatarID + " not created");

                    }
                    else
                    {
                        Log.Info("Avatar " + avatarID + " created");
                    }
                }

            }
            catch (WebException e)
            {
                Log.Error(e.Message);
                return null;
            }









        }

        private static Regex alphanumeric = new Regex("^[a-zA-Z0-9\\-]*$");
        public static string ReadUntilFieldValue_avtr(string filePath, string UntilText)
        {
            try
            {
                BinaryReader reader = new BinaryReader(System.IO.File.Open(filePath, FileMode.Open), Encoding.ASCII);
                int bufferLimit = UntilText.Length;


                var buffer = new StringBuilder();
                bool foundEndOfLine = false;
                char ch;
                while (!foundEndOfLine)
                {
                    try
                    {
                        ch = reader.ReadChar();
                        buffer.Append(ch);
                        if (buffer.Length > UntilText.Length)
                        {
                            buffer.Remove(0, 1);
                        }

                        if (buffer.ToString() == UntilText)
                        {
                            bool endRead = false;
                            var avatarIDBuilder = new StringBuilder();
                            int underscores = 0;

                            int idnr = 0;
                            while (!endRead)
                            {
                                idnr++;
                                ch = reader.ReadChar();

                                if ((idnr == 9||idnr==14||idnr==19||idnr==24) && ch != '-') endRead = true;
                                avatarIDBuilder.Append(ch);
                                if (idnr == 36)
                                {
                                    string avatarId = avatarIDBuilder.ToString();
                                    if (!alphanumeric.IsMatch(avatarId))
                                    {
                                        endRead = true;
                                        break;
                                    }
                                    
                                    underscores++;
                                    reader.Close();
                                    return avatarId;
                                }
                            }
                            return null;
                        }
                    }
                    catch (EndOfStreamException ex)
                    {
                        reader.Close();
                        return null;
                    }


                }

                return null;
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                return null;
            }
        }

        public static CacheResult ReadUntilFieldValue(string filePath, string UntilText,NyanLoading? loadingView = null)
        {
            try
            {

                BinaryReader reader = new BinaryReader(System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.ASCII);
                int bufferLimit = UntilText.Length;

                

                var buffer = new StringBuilder();
                bool foundEndOfLine = false;
                char ch;
                while (!foundEndOfLine)
                {
                    try
                    {
                        ch = reader.ReadChar();
                        buffer.Append(ch);
                        if (buffer.Length > UntilText.Length)
                        {
                            buffer.Remove(0, 1);
                        }

                        if (buffer.ToString() == UntilText)
                        {
                            bool endRead = false;
                            var avatarIDBuilder = new StringBuilder();
                            int underscores = 0;
                            while (!endRead)
                            {
                                ch = reader.ReadChar();
                                avatarIDBuilder.Append(ch);
                                if (ch == '_')
                                {
                                    underscores++;
                                    if (underscores >= 3)
                                    {
                                        //Read avatar ID sucessfully, return ID
                                        string aviID = avatarIDBuilder.ToString().Split('_')[2];
                                        reader.Close();
                                        
                                        loadingView?.addProgress(1f);

                                        return new CacheResult() { AvatarID = aviID, CacheLocation = filePath };
                                    }
                                }
                            }
                            //Until 3 underscores are read


                            reader.Close();

                            loadingView?.addProgress(1f);
                            return null;
                        }
                    }
                    catch (EndOfStreamException ex)
                    {
                        loadingView?.addProgress(1f);
                        reader.Close();
                        return null;
                    }


                }

                return null;
            }
            catch (Exception e)
            {
                loadingView?.addProgress(1f);
                Log.Error(e.Message);
                return null;
            }
        }

    }
}
