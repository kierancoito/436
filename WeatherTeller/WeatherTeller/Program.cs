using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using Newtonsoft.Json;
/**
 * Created by Kieran Coito
 * CSS 436
 * October 18th 2019
 *
 * This program will take in a city name and then give back the current weather information for that city
 * It uses openweathermap.org to do this
 */

namespace WeatherTeller {
    class Program {
        
        static string key = "2699707ffc88a1ed7db778705f819feb";

        /**
         * 
         */
        static void Main(string[] args) {
            
            //verify the user has entered a city
            if (args.Length < 1) {
                Console.WriteLine("You must enter a city to get its weather details");
                return;
            }

            //creating end of address from user specified information
            string city = args[0];
            for (int i = 1; i < args.Length; i++) {
                city += "+" + args[i];
            }
            
            //construct the specific weather call
            string call = "weather?q=" + city + "&APPID=" + key;

            using (var client = new HttpClient()) {
                
                //construct the URI within the client
                client.BaseAddress = new Uri("http://api.openweathermap.org/data/2.5/");
                HttpResponseMessage response = client.GetAsync(call).Result;

                //verify the http request is valid
                //if it is not a valid request than the page doesn't exist which means
                //the city does not exist or input was bad, so the application will terminate
                try {
                    var responseCode = response.EnsureSuccessStatusCode().StatusCode;
                }
                catch ( HttpRequestException ) {
                    
                    Console.WriteLine("The city you specified does not exist. Please try again");
                    return;
                }
                
                //get results and convert them into workable information
                //if conversion of json file fails alert user
                string result = response.Content.ReadAsStringAsync().Result;
                RootObject weatherDetails;
                try {
                    weatherDetails = JsonConvert.DeserializeObject<RootObject>(result);
                }
                catch ( Exception ) {
                    Console.WriteLine("Converting the JSON object failed. " +
                                      "This means one of the values in the JSON value were not correct. " +
                                      "Try Again");
                    return;
                }
                //print weather results to user
                GetWeatherInfo(weatherDetails);
            }
        }
        
        /**
         * This method will convert a cardinal direction that is represented by degrees into words
         *
         * @param the double value which represents the cardinal direction in degrees 
         */
        private static string DegreesToCardinal(double degrees){
            string[] cardinals = { "North", "Northeast", "East", 
                                   "Southeast", "South", "Southwest", 
                                   "West", "Northwest", "North" };
            int index = (int) Math.Round( degrees % 360 / 45 );
            return cardinals[index];
        }

        /**
         * This method will display to the console all information about a cities weather
         * Specifically temperature, humidity, conditions, and wind
         *
         * @param all weather details for the city being queried 
         */
        private static void GetWeatherInfo( RootObject weatherDetails ) {

            //conditions
            string weatherCondition = weatherDetails.weather[0].description;
            Console.WriteLine("Weather conditions in " + weatherDetails.name + " are " + weatherCondition);
            
            //temperature & humidity
            int temp = (int)((weatherDetails.main.temp - 273.15) * 1.8 + 32);
            double humidity = weatherDetails.main.humidity;
            Console.WriteLine("The temperature is " + temp + " degrees fahrenheit, with " + humidity
                              + "% humidity");
            //wind speed
            double windSpeed = weatherDetails.wind.speed;
            double windDirection = weatherDetails.wind.deg;
            Console.WriteLine("Wind is at a speed of " + (int)(windSpeed * 2.237) + 
                              " miles per hour in the " + DegreesToCardinal(windDirection) + " direction");
            //clouds
            double cloudCover = weatherDetails.clouds.all;
            Console.WriteLine("There is " + cloudCover + "% cloud cover");
        }
    }
    
    /*
     * All of the following classes represent all of the information that is retrieved from
     * openweathermap.org for a weather class. These classes are used to convert the json
     * file that openweathermap returns into classes that can then be used in a easier form
     */
    public class Coord
    {
        public double lon { get; set; }
        public double lat { get; set; }
    }

    public class Weather
    {
        public int id { get; set; }
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }

    public class Main
    {
        public double temp { get; set; }
        public double pressure { get; set; }
        public int humidity { get; set; }
        public double temp_min { get; set; }
        public double temp_max { get; set; }
        public double sea_level { get; set; }
        public double grnd_level { get; set; }
    }

    public class Wind
    {
        public double speed { get; set; }
        public double deg { get; set; }
    }

    public class Clouds
    {
        public int all { get; set; }
    }

    public class Sys
    {
        public double message { get; set; }
        public string country { get; set; }
        public double sunrise { get; set; }
        public double sunset { get; set; }
    }

    public class RootObject
    {
        public Coord coord { get; set; }
        public List<Weather> weather { get; set; }
        public string @base { get; set; }
        public Main main { get; set; }
        public Wind wind { get; set; }
        public Clouds clouds { get; set; }
        public int dt { get; set; }
        public Sys sys { get; set; }
        public int timezone { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public int cod { get; set; }
    }
}