﻿using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Security.Policy;
/****
 *
 *
 *
 *
 * 
 ****/
namespace WebCrawler {
    class Program {
        static void Main(string[] args) {
            
            //verify the correct number of arguments
            if (args.Length != 2) {
                Console.WriteLine("Not enough valid arguments to run application");
                return;
            }

            //find out how many hops have been specified by the user
            int hops = int.Parse(args[1]);

            //create an array to remember all visited URLs
            string[] previousURLS = new string[hops + 2];
            previousURLS[0] = args[0];

            //verify valid number of hops
            if (hops <= 0) {
                Console.WriteLine("invalid number of hops specified to run application");
                return;
            }

            //extract initial base URI and the URI path
            Uri uri;
            try {
                uri = new Uri(args[0]);
            }
            catch ( UriFormatException ) {
                Console.Write( args[0] + " is not a valid address, please inspect and retry");
                return;
            }

            

            //TODO remove once application is working
            Console.WriteLine("initial URL: " + uri.ToString());
            
            int count = 1;
            while (hops >= count) {
                //connect to http client 
                using (var client = new System.Net.Http.HttpClient(new System.Net.Http.HttpClientHandler {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                })) {
                    //set base address for HTTP client
                    client.BaseAddress = new Uri(uri.ToString());

                    //get response from website at the specified extension
                    System.Net.Http.HttpResponseMessage response = client.GetAsync("").Result;

                    //check if page exists
                    if (response.IsSuccessStatusCode) {
                        
                        uri = FindNextHtml(response, uri, previousURLS, count);
                        if (uri == null) {
                            return;
                        }
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
                            
                            //find redirected URL
                            string result = response.Content.ReadAsStringAsync().Result;
                            string pattern = "<a[^>]* href=\"([^\"]*)\"";
                            var linkParser = new Regex(pattern);
                            MatchCollection redirected = linkParser.Matches(result);
                            string newAddress = redirected[0].Groups[1].ToString();
                            Uri oldUri = uri;

                            try {
                                uri = new Uri(uri.GetLeftPart(System.UriPartial.Authority).ToString() + newAddress.ToString());
                            }
                            catch ( UriFormatException ) {
                                Console.Write("Bad address suggested by redirection. Cannot go any further");

                            }
                            
                            //decrement count to account for the redirection that happened
                            count--;
                            Console.WriteLine(failureCode + " URL redirect, new one is: " + uri.ToString());
                        }
                    }
                }

                Console.WriteLine(count + " " + uri.ToString());
                count++;
            }
        }

        private static Uri FindNextHtml(System.Net.Http.HttpResponseMessage response, Uri uri, string[] previousURLS, int count) {
            //handle success
            string result = response.Content.ReadAsStringAsync().Result;

            //setup regex 
            string pattern = "<a href=(?:\"{1}|'{1})(http?://.*?)/?\"{1}|'{1}(\\s.*)?>";
            var linkParser = new Regex(pattern);

            //get all valid URLs from the current website
            MatchCollection newUrl = linkParser.Matches(result);

            int currentParseUrl = 0;
            string url;
            
            //ensure there is a valid next URL from the current URL
            if (newUrl.Count > 0) {
                url = newUrl[currentParseUrl].Groups[1].ToString();
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
                        currentParseUrl++;
                        url = newUrl[currentParseUrl].Groups[1].ToString();
                        if (url.Contains(".pdf")) {
                            Console.WriteLine("Final location is a .pdf file which does not contain html");
                            return null;
                        }
                    }
                }

                //verify the URL has not been visited
                //if not find the next possible URL
                while (checker < count + 1) {
                    
                    if (url.Equals(previousURLS[checker])) {
                        
                        //if the current URL has been visited before get the next URL
                        currentParseUrl++;

                        if (newUrl.Count > currentParseUrl) {
                            url = newUrl[currentParseUrl].Groups[1].ToString();
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