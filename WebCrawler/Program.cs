using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Security.Policy;

namespace WebCrawler {
    class Program {
        static void Main(string[] args) {
            //TODO remove when testing full usage
            string[] arguments;
            arguments = new string[2]
                {"http://courses.washington.edu/css502/dimpsey", "50"};

            //verify the correct number of arguments
            if (arguments.Length != 2) {
                Console.WriteLine("Not enough valid arguments to run application");
                return;
            }

            //find out how many hops have been specified by the user
            int hops = int.Parse(arguments[1]);

            //create an array to remember all visited URLs
            string[] previousURLS = new string[hops + 2];
            previousURLS[0] = arguments[0];

            //verify valid number of hops
            if (hops <= 0) {
                Console.WriteLine("invalid number of hops specified to run application");
                return;
            }

            //extract initial base URI and the URI path 
            var uri = new Uri(arguments[0]);

            //TODO remove once application is working
            Console.WriteLine("initial URL: " + uri.ToString());


            int count = 1;
            while (hops >= count) {
                //connect to http client 
                using (var client = new HttpClient(new HttpClientHandler {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                })) {
                    //set base address for HTTP client
                    client.BaseAddress = new Uri(uri.ToString());

                    //get response from website at the specified extension
                    HttpResponseMessage response = client.GetAsync("").Result;

                    //check if page exists
                    if (response.IsSuccessStatusCode) {
                        uri = FindNextHtml(response, uri, previousURLS, count);
                    }
                    else {
                        //extract failure code
                        int failureCode = (int) response.StatusCode;

                        //4xx errors will be a dead link
                        if (failureCode / 400 == 1) {
                            Console.WriteLine(failureCode + " Bad URL: " + uri.ToString() +
                                              " cannot proceed any further");
                            return;
                        }

                        //3xx errors should redirect
                        if (failureCode / 300 == 1) {
                            string result = response.Content.ReadAsStringAsync().Result;

                            //TODO need to deal with redirect 
                            Console.WriteLine(failureCode + " URL redirect: " + uri.ToString());
                            return;
                        }
                    }
                }

                Console.WriteLine(count + " " + uri.ToString());
                count++;
            }
        }

        private static Uri FindNextHtml(HttpResponseMessage response, Uri uri, string[] previousURLS, int count) {
            //handle success
            string result = response.Content.ReadAsStringAsync().Result;

            //setup regex 
            string pattern = "<a href=(?:\"{1}|'{1})(http?://.*?)/?\"{1}|'{1}(\\s.*)?>";
            //"(?:<a href=\"{1}|'{1})(http?://.*?)/?\"{1}|'{1}(\\s.*)?>";

            var linkParser = new Regex(pattern);

            //get all valid URLs from the current website
            MatchCollection newUrl = linkParser.Matches(result);

            int curerntParseUrl = 0;
            string url;
            //ensure there is a valid next URL from the current URL
            if (newUrl.Count > 0) {
                url = newUrl[curerntParseUrl].Groups[1].ToString();
            }
            else {
                Console.WriteLine("No more valid URLs to visit");
                return null;
            }

            int checker = 0;
            //ensure URL is valid
            bool goodURL = false;
            checker = 0;

            while (goodURL != true) {
                //get the first valid URL 
                while (goodURL != true) {
                    try {
                        uri = new Uri(url);
                        goodURL = true;
                    }
                    catch (UriFormatException) {
                        goodURL = false;
                        curerntParseUrl++;
                        url = newUrl[curerntParseUrl].Groups[1].ToString();
                    }
                }

                //verify the URL has not been visited
                //if not find the next possible URL
                while (checker < count + 1) {
                    if (url.Equals(previousURLS[checker])) {
                        //if the current URL has been visited before get the next URL
                        curerntParseUrl++;

                        if (newUrl.Count > curerntParseUrl) {
                            url = newUrl[curerntParseUrl].Groups[1].ToString();
                            goodURL = false;
                        }
                        else {
                            Console.WriteLine("No more valid URLs to visit");
                            return null;
                        }

                        continue;
                    }

                    checker++;
                }

                checker = 0;
            }

            //add current URL to list of visited URLs
            previousURLS[count + 1] = url;
            
            return uri;
        }
    }
}