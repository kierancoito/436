using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Security;
using System.Security.Policy;
/****
 * Created by Kieran Coito
 * CSS436 Fall 2019
 * Started October 6th 2019
 * Finished October 10th 2019
 *
 * 
 * This program will take in two arguments a valid URL and a number of hops
 * It will start on the supplied URL, and parse its HTML for the next
 * http URL that is within a <a href >, it will continue to do this until
 * it hits an invalid website, hits a website that doesn't contain
 * another website to hop to, or it goes through all of the hops
 *
 *
 * This program will only parse HTTP websites and not HTTPS websites
 * 
 ****/
namespace WebCrawler {
    class Program {
        /**
         * Main method of this program that will take in the arguments from the user and then execute the program
         *
         * Parameters:
         *     string[] args - arguments supplied by the user via the command line
         */
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
                Console.WriteLine("Invalid number of hops specified to run application");
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

            int count = 1;
            HttpResponseMessage response = null;
            while (hops >= count) {
                //connect to http client 
                using (var client = new HttpClient(new HttpClientHandler {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate})) {
                    //set base address for HTTP client
                    client.BaseAddress = uri;

                    //get response from website at the specified extension
                    response = client.GetAsync("").Result;

                    //check if page exists
                    if (response.IsSuccessStatusCode) {
                        //find next valid url
                        uri = FindNextHtml(response, uri, previousURLS, count);
                        //if no valid url is returned than exit out of the program as that means there is no
                        //next url and the program has hit a deadend
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

                        //3xx errors is a redirect
                        if (failureCode / 300 == 1) {
                            
                            //find redirected URL
                            //this is slightly different than the other HTML regex expression used in FindNextHtml
                            //this is to account for redirects that are just the final part of a website 
                            // i.e /CSS to /css
                            string result = response.Content.ReadAsStringAsync().Result;
                            string pattern = "<a[^>]* href=\"([^\"]*)\"";
                            var linkParser = new Regex(pattern);
                            MatchCollection redirected = linkParser.Matches(result);
                            string newAddress = redirected[0].Groups[1].ToString();

                            try {
                                uri = new Uri(uri.GetLeftPart(System.UriPartial.Authority).ToString() + newAddress.ToString());
                            }
                            catch ( UriFormatException ) {
                                Console.Write("Bad address suggested by redirection. Cannot go any further");
                            }
                            
                            //decrement count to account for the redirection that happened
                            count--;
                        }
                    }
                }
                count++;
            }
            //print out final destination and the html contents of that destination
            Console.WriteLine(" Final destination happened at hop " + count);
            Console.WriteLine(" The final URL is " + uri.ToString());
            Console.WriteLine(response.Content.ReadAsStringAsync().Result);
        }
        
        /**
         * This function will find the next valid URL from the supplied website
         *
         * Parameters
         *     HttpResponseMessage response - the response message that was received from the currently visited
         *                                    URL website.
         *     Uri uri - The uri object that is used for the http connection
         *     string[] previousURLs - An array of all URLs that have been visited by the program as it operates
         *     int count - the current hop that the program
         */
        private static Uri FindNextHtml( HttpResponseMessage response, Uri uri, string[] previousURLS, int count ) {
            //handle success
            string result = response.Content.ReadAsStringAsync().Result;

            //setup regex 
            string pattern = "<a href=(?:\"{1}|'{1})(http?://.*?)/?\"{1}|'{1}(\\s.*)?>";
            var linkParser = new Regex(pattern);

            //get all valid URLs from the current website
            MatchCollection newUrl = linkParser.Matches(result);

            //create a iterator to parse through all regex matches in the newUrl collection
            int currentParseUrl = 0;
            string url;
            string previousUrl = uri.ToString();
            
            //ensure there is a valid URL to visit 
            //if there isn't terminate the program as the current URL
            //cannot be hopped from
            if (newUrl.Count > 0){
                url = newUrl[currentParseUrl].Groups[1].ToString();
            }
            else {
                Console.WriteLine("No more valid URLs to visit");
                Console.WriteLine(previousUrl + " was the last url visited");
                Console.WriteLine(result);
                return null;
            }

            //ensure URL is valid
            bool goodURL = false;
            int checker = 0;

            //loop as long as the currently URL is not a valid URL
            while (goodURL != true) {
                //get the first valid URL 
                while (goodURL != true) {
                    try {
                        // attempt use the new url
                        uri = new Uri(url);
                        goodURL = true;
                    }
                    catch (UriFormatException) {
                        
                        goodURL = false;
                        currentParseUrl++;
                        url = newUrl[currentParseUrl].Groups[1].ToString();
                        
                        //if the new url ends in a pdf it cannot proceed further
                        if (url.Contains(".pdf")) {
                            Console.WriteLine("Final location is a .pdf file which does not contain html");
                            return null;
                        }
                    }
                }

                //verify the URL has not been visited
                //if it has been found find the next valid URL and begin the checking again
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
                //reset checker in case goodURL is not true
                checker = 0;
            }
            //add current URL to list of visited URLs
            previousURLS[count + 1] = url;
            
            //return new 
            return uri;
        }
    }
}