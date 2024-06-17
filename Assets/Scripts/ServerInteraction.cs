using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace XWFC
{
    public class ServerInteraction
    {
        public static async Task<string> RequestAction(string state)
        {
            Debug.Log("Starting request....");
            var url = "http://127.0.0.1:5000/predict";

            WebRequest myWebRequest = WebRequest.Create(url);
            myWebRequest.ContentType = "application/json";
            myWebRequest.Method = "POST";
            await using (var streamWriter = new StreamWriter(await myWebRequest.GetRequestStreamAsync()))
            {
                var json = $"{{\"state\":\"{state}\"}}";

                await streamWriter.WriteAsync(json);
            }

            using (var myWebResponse = await myWebRequest.GetResponseAsync())
            {
                var stream = myWebResponse.GetResponseStream();
                if (stream == null) return "";
                var streamReader = new StreamReader(stream);
                var output = streamReader.ReadToEnd();
                Debug.Log("REQUEST OUTPUT " + output);
                return output;
            }
        }
    }
}