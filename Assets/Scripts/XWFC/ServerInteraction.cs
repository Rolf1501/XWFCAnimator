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
        public static async Task<Stream> RequestAction()
        {
            Debug.Log("Starting request....");
            var url = "http://127.0.0.1:5000/test";
            // // using var webRequest = UnityWebRequest.Get("https://duckduckgo.com/");
            // using var webRequest = UnityWebRequest.Get(url);
            // yield return webRequest.SendWebRequest();
            //
            // // using var webPost = UnityWebRequest.Post(url, );
            //
            // if (webRequest.result != UnityWebRequest.Result.Success)
            // {
            //     Debug.Log($"Encountered error: {webRequest.error}");
            // }
            // else
            // {
            //     Debug.Log($"Got result {webRequest.result}");
            // }
            WebRequest myWebRequest = WebRequest.Create(url);
            using (var myWebResponse = await myWebRequest.GetResponseAsync())
            {
                var output = myWebResponse.GetResponseStream();
                Debug.Log(output);
                return output;
            }
        }
    }
}