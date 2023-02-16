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
using static System.Net.Mime.MediaTypeNames;

namespace AviDrugZ
{
    public static class KlauenUtils
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(KlauenUtils));
        public static void exportToKlauentec(List<AvatarWeb> allAvatars)
        {
            int updated = 0;
            int failed = 0;
            foreach (AvatarWeb avi in allAvatars)
            {

                //Make a POST json request to https://avatars.bs002.de/avatars/avatars.php
                //with the json string as the body
                //and the header "Content-Type: application/json"

                //Create a POST request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://avatars.bs002.de/avatars/avatars.php");
                request.Method = "POST";
                request.ContentType = "application/json";

                string json = "";
                try
                {
                    json = JsonConvert.SerializeObject(avi);
                }
                catch (Exception e)
                {
                    continue;
                   //yikes cross thread stuff
                }

                //Send the json string as the body
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                //Get the response
                var httpResponse = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    if (result.EndsWith("Not Created.\"}"))
                    {
                        Log.Info("Avatar " + avi.AvatarName + " not created");
                        failed++;
                    }
                    else
                    {
                        updated++;
                        Log.Info("Avatar " + avi.AvatarName + " created");
                    }
                }

            }

            Log.Info("Uploaded " + updated + " avatars");
            Log.Info("Failed to upload " + failed + " avatars");
        }

        public static string ReadUntilFieldValue(string filePath, string UntilText)
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
                                        return aviID;
                                    }
                                }
                            }
                            //Until 3 underscores are read


                            reader.Close();

                            return null;
                        }
                    }
                    catch (EndOfStreamException ex)
                    {
                        reader.Close();
                        return null;
                        //  if (result.Length == 0) return null;
                        // else break;
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
    }
}
