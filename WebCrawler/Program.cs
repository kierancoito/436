using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Net.Http;

namespace WebCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] arguments; 
            arguments = new string[2]{"https://stackoverflow.com/questions/15641797/extract-base-url-from-a-string-in-c", "4" };
            
            //verify the correct number of arguments
            if (arguments.Length != 2) {
                Console.WriteLine("Not enough valid arguments to run application");
                return;
            }

            int hops = int.Parse(arguments[1]);
            //verify valid number of hops
            if (hops <= 0) {
                Console.WriteLine("invalid number of hops specified to run application");
                return;
            }
            
            //extract initial base URI and the URI path 
            var uri = new Uri(arguments[0]);
            var baseUri = uri.GetLeftPart(System.UriPartial.Authority);
            var extension = uri.PathAndQuery;
            
            Console.WriteLine(baseUri);
            Console.WriteLine(extension);
            Console.WriteLine(baseUri + extension);
            Console.WriteLine("making call");

            while (hops != 0) {
                
                using (var client = new HttpClient(new HttpClientHandler
                    {AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate})) {
                    
                    //set base address for HTTP client
                    client.BaseAddress = new Uri(baseUri);
                    //get response from website at the specified extension
                    HttpResponseMessage response = client.GetAsync(extension).Result;
                    
                    //check if page exists
                    if (response.IsSuccessStatusCode) {
                        //handle success
                        string result = response.Content.ReadAsStringAsync().Result;

                        //setup regex 
                        var linkParser = new Regex(@"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?", 
                            RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        
                        //find first URL
                        Console.WriteLine(linkParser.Match(result));
                        
                    } else {
                        //handle failure
                        int failureCode = (int) response.StatusCode;
                        Console.WriteLine(failureCode);
                        return;
                    }

                    Console.WriteLine(hops);
                }

                hops--;
            }
        }
    }
}