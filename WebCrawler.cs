using System;
using System.Net;
using System.Net.Http;

namespace HTTPGet
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Making API Call...");
            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                client.BaseAddress = new Uri("http://courses.washington.edu/");
                HttpResponseMessage response = client.GetAsync("css342/dimpsey").Result;
                response.EnsureSuccessStatusCode();
                string result = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine("Result: " + result);
            }
        }
    }
}