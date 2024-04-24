using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace XWFC
{
    public class ServerInteraction
    {
        private HttpClient _client;
        public ServerInteraction(string url="http://127.0.0.1:5000/")
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri(url);

        }

        public async Task Request()
        {
            var x = await RequestAction(_client, "predict");
            
        }
        public static async Task<string> RequestAction(HttpClient client, string path)
        {
            Debug.Log("Starting request....");
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(client.BaseAddress + path);
            
            request.Content = new StringContent("{state: 123}", Encoding.UTF8, "application/json");
            
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                Debug.Log("Response: " + response.Content.ReadAsStringAsync().Result);
                return response.Content.ReadAsStringAsync().Result;
            }
        }

        public static async void PostAction(string content)
        {
            WebRequest request = WebRequest.Create("123");
            using (var response = await request.GetResponseAsync())
            {
                var output = response.GetResponseStream();
            }
        }
    }
}