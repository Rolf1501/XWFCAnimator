using System;
using System.Collections.Generic;
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

        public async Task Request(Grid<string> state, Vector3 coordinate)
        {
            var x = await RequestAction(_client, "predict", state, coordinate);
            
        }
        public static async Task<string> RequestAction(HttpClient client, string path, Grid<string> state, Vector3 coordinate)
        {
            Debug.Log("Starting request....");
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(client.BaseAddress + path);
            var content = new StringBuilder();
            content.Append(TrainingDataFormatter.FormatState(state) + ",");
            content.Append(TrainingDataFormatter.FormatCoordinate(coordinate));
            
            request.Content = new StringContent(content.ToString(), Encoding.UTF8, "text/csv");
            // request.Content = new StringContent(content.ToString(), Encoding.UTF8, "application/json");
            
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