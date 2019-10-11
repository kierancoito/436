import sun.net.www.http.HttpClient;
import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.SocketTimeoutException;
import java.net.URL;
import java.util.HashSet;
import java.util.Set;
import java.util.regex.Pattern;
import java.util.zip.GZIPInputStream;
import java.net.URI;
import java.net.URISyntaxException;

public class WebCrawler {

    static void Main(String[] args) {
        //TODO remove when testing full usage
        String[] arguments;
        arguments = new String[]{"http://www.courses.washington.edu/css502/dimpsey", "50"};

        //verify the correct number of arguments
        if (arguments.length != 2) {
            System.out.println("Not enough valid arguments to run application");
            return;
        }

        //find out how many hops have been specified by the user
        int hops = Integer.parseInt(arguments[1]);

        //create an array to remember all visited URLs
        String[] previousURLS = new String[hops + 2];
        previousURLS[0] = arguments[0];

        //verify valid number of hops
        if (hops <= 0) {
            System.out.println("invalid number of hops specified to run application");
            return;
        }

        //extract initial base URI and the URI path
        URI uri = null;
        try {
            uri = new URI(arguments[0]);

        } catch (URISyntaxException e) {

            e.printStackTrace();
        }

        //TODO remove once application is working
        System.out.println("initial URL: " + uri.toString());

        int count = 1;
        while (hops >= count) {
            //connect to http client
            try (HttpClient client = new HttpClient()) {
                //set base address for HTTP client
                try {
                    client.BaseAddress = new URI(uri.toString());
                } catch (URISyntaxException e) {
                    e.printStackTrace();
                }

                //get response from website at the specified extension
                HttpResponseMessage response = client.GetAsync("").Result;

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
                        System.out.println(failureCode + " Bad URL: " + uri.toString() +
                                " cannot proceed any further");
                        return;
                    }

                    //3xx errors should redirect
                    if (failureCode / 300 == 1) {

                        //find redirected URL
                        String result = response.Content.ReadAsStringAsync().Result;
                        String pattern = "<a[^>]* href=\"([^\"]*)\"";
                        Pattern regexPattern = Pattern.compile(pattern);
                        MatchCollection redirected = linkParser.Matches(result);
                        String newAddress = redirected[0].Groups[1].ToString();

                        try {
                            uri = new URI(uri.GetLeftPart(System.UriPartial.Authority).ToString() + newAddress.ToString());
                        }
                        catch ( UriFormatException ) {
                            Console.Write("Bad address suggested by redirection. Cannot go any further");

                        }

                        //decrement count to account for the redirection that happened
                        count--;
                        System.out.println(failureCode + " URL redirect, new one is: " + uri.toString());
                    }
                }
            }

            System.out.println(count + " " + uri.toString());
            count++;
        }
    }

    private static URI FindNextHtml(HttpResponseMessage response, URI uri, String[] previousURLS, int count) {
        //handle success
        String result = response.Content.ReadAsStringAsync().Result;

        //setup regex
        String pattern = "<a href=(?:\"{1}|'{1})(http?://.*?)/?\"{1}|'{1}(\\s.*)?>";
        var linkParser = new Regex(pattern);

        //get all valid URLs from the current website
        MatchCollection newUrl = linkParser.Matches(result);

        int currentParseUrl = 0;
        String url;

        //ensure there is a valid next URL from the current URL
        if (newUrl.Count > 0) {
            url = newUrl[currentParseUrl].Groups[1].ToString();
        }
        else {
            System.out.println("No more valid URLs to visit");
            return null;
        }

        int checker = 0;

        //ensure URL is valid
        boolean goodURL = false;
        checker = 0;

        while (goodURL != true) {
            //get the first valid URL
            while (goodURL != true) {
                try {
                    uri = new URI(url);
                    goodURL = true;
                }
                catch (UriFormatException) {
                    goodURL = false;
                    currentParseUrl++;
                    url = newUrl[currentParseUrl].Groups[1].ToString();
                    if (url.contains(".pdf")) {
                        System.out.println("Final location is a .pdf file which does not contain html");
                        return null;
                    }
                }
            }

            //verify the URL has not been visited
            //if not find the next possible URL
            while (checker < count + 1) {

                if (url.equals(previousURLS[checker])) {

                    //if the current URL has been visited before get the next URL
                    currentParseUrl++;

                    if (newUrl.Count > currentParseUrl) {
                        url = newUrl[currentParseUrl].Groups[1].ToString();
                        goodURL = false;
                    }
                    else {
                        System.out.println("No more valid URLs to visit");
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
