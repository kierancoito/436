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
            arguments = new string[2]{"https://stackoverflow.com/questions/15641797/extract-base-url-from-a-string-in-c", "1" };
            
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
            
            //TODO remove once application is working
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
                    //TODO need to deal with if there is no extension for URL
                    HttpResponseMessage response = client.GetAsync(extension).Result;
                    
                    //check if page exists
                    if (response.IsSuccessStatusCode) {
                        //handle success
                        string result = response.Content.ReadAsStringAsync().Result;

                        //setup regex 
                        string pattern = "<a href=(?:\"{1}|'{1})(https?://.*?)/?\"{1}|'{1}(\\s.*)?>";
                        var linkParser = new Regex(pattern);
                        
                        //find first URL
                        string newUrl = linkParser.Match(result).Groups[1].ToString();
                        Console.WriteLine(hops + " " + newUrl);
                        
                        uri = new Uri(newUrl);
                        baseUri = uri.GetLeftPart(System.UriPartial.Authority);
                        extension = uri.PathAndQuery;
                        
                    } else {
                        //handle failure
                        int failureCode = (int) response.StatusCode;
                        if (failureCode/400 == 1) {
                            Console.WriteLine(failureCode + " Bad URL, cannot proceed any further");
                            return;
                            
                        }else if (failureCode/300 == 1) {
                            //TODO need to deal with redirect 
                            Console.WriteLine(failureCode + " URL redirect");
                            return;
                            
                        }
                    }
                }
                hops--;
            }
        }
    }
}