using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Net.Http;

namespace WebCrawler {
    class Program {
        static void Main(string[] args) {
            
            //TODO remove when testing full usage
            string[] arguments;
            arguments = new string[2]
                {"https://stackoverflow.com/questions/15641797/extract-base-url-from-a-string-in-c", "2"};

            //verify the correct number of arguments
            if (arguments.Length != 2) {
                Console.WriteLine("Not enough valid arguments to run application");
                return;
            }

            //find out how many hops have been specified by the user
            int hops = int.Parse(arguments[1]);
            
            //verify valid number of hops
            if (hops <= 0) {
                Console.WriteLine("invalid number of hops specified to run application");
                return;
            }
            
            //create an array to keep track of previous websites visited
            string[] previousSites = new string[hops];

            //extract initial base URI and the URI path 
            var uri = new Uri(arguments[0]);
            
            //seperate parts of URL
            //var baseUri = uri.GetLeftPart(System.UriPartial.Authority);
            //var extension = uri.PathAndQuery;

            //TODO remove once application is working
            Console.WriteLine("initial URL: " + uri.ToString());

            //remember previous address to ensure it isn't gone to again
            string oldUri = uri.ToString();
            
            while (hops > 0) {

                //connect to http client 
                using (var client = new HttpClient(new HttpClientHandler
                    {AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate})) {
                    
                    //set base address for HTTP client
                    client.BaseAddress = new Uri(uri.ToString());
                    
                    //get response from website at the specified extension
                    HttpResponseMessage response = client.GetAsync("").Result;

                    //check if page exists
                    if (response.IsSuccessStatusCode) {
                        
                        //handle success
                        string result = response.Content.ReadAsStringAsync().Result;

                        //setup regex 
                        string pattern = "<a href=(?:\"{1}|'{1})(http?://.*?)/?\"{1}|'{1}(\\s.*)?>";
                        var linkParser = new Regex(pattern);
                        int filePosition = 0;
                        
                        //get next URL that is different than the current URL
                        while (uri.ToString() == oldUri) {
                            
                            //save previous URL
                            oldUri = uri.ToString();
                            
                            //find next valid
                            //TODO change to Matches instead of Match and redo how saving previous addresses and skipping others works
                            string newUrl = linkParser.Match(result, filePosition).Groups[1].ToString();
                            uri = new Uri(newUrl);

                            if (oldUri == uri.ToString()) {
                                filePosition = result.IndexOf(uri.ToString(), filePosition+1) + 1;
                                
                            }else {
                                Console.WriteLine(hops + " " + newUrl);

                            }
                        }
                    }else {
                        //handle failure
                        int failureCode = (int) response.StatusCode;
                        if (failureCode / 400 == 1) {
                            Console.WriteLine(failureCode + " Bad URL, cannot proceed any further");
                            return;
                        }
                        else if (failureCode / 300 == 1) {
                            //TODO need to deal with redirect 
                            Console.WriteLine(failureCode + " URL redirect");
                            return;
                        }
                    }
                }
                //remember previous address to ensure it isn't gone to again
                oldUri = uri.ToString();
                
                hops--;
            }
        }
    }
}