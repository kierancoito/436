using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Authentication;
using Newtonsoft.Json;

namespace WeatherTeller {
    class Program {
        static string key = "2699707ffc88a1ed7db778705f819feb";

        static void Main(string[] args) {
            //TODO remove when testing is done
            args = new string[] {"Seattle"};
            
            //creating end of address from user specified information
            string city = args[0];
            string call = "weather?q=" + city + "&APPID=" + key;

            Console.WriteLine("Making API Call...");

            using (var client = new HttpClient()) {
                
                client.BaseAddress = new Uri("http://api.openweathermap.org/data/2.5/");
                HttpResponseMessage response = client.GetAsync(call).Result;

                //verify the http request is valid
                try {
                    var responseCode = response.EnsureSuccessStatusCode().StatusCode;
                }
                catch ( HttpRequestException ) {
                    
                    Console.WriteLine("The city you specified does not exist. Please try again");
                    return;
                }
                
                //get results and convert them into workable information
                string result = response.Content.ReadAsStringAsync().Result;
                RootObject weatherDetails = JsonConvert.DeserializeObject<RootObject>(result);

                //get weather info
                //conditions
                string weatherCondition = weatherDetails.weather[1].description;
                Console.WriteLine("Weather conditions in " + city + " are " + weatherCondition);
                
                
                //temperature
                int temp = (int)((weatherDetails.main.temp - 273.15) * 1.8 + 32);
                Console.WriteLine("The temperature is " + temp + " degrees fahrenheit");
                
                //wind speed
                double windSpeed = weatherDetails.wind.speed;
                double windDirection = weatherDetails.wind.deg;


                Console.WriteLine("Wind is at a speed of " + (int)(windSpeed * 2.237) + 
                                  " miles per hour in the " + DegreesToCardinal(windDirection) + " direction");

                //clouds

                //humidity

                //conditions
            }
        }
        
        static string DegreesToCardinal(double degrees){
    
            string[] cardinals = { "N", "NE", "E", "SE", "S", "SW", "W", "NW", "N" };
            int index = (int) Math.Round(((double) degrees % 360) / 45);
            return cardinals[index];
        }
    }
    
    public class Coord {
        public double lon { get; set; }
        public double lat { get; set; }
    }

    public class Weather {
        public int id { get; set; }
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }

    public class Main {
        public double temp { get; set; }
        public int pressure { get; set; }
        public int humidity { get; set; }
        public double temp_min { get; set; }
        public double temp_max { get; set; }
    }

    public class Wind {
        public double speed { get; set; }
        public int deg { get; set; }
    }

    public class Clouds {
        public int all { get; set; }
    }

    public class Sys {
        public int type { get; set; }
        public int id { get; set; }
        public double message { get; set; }
        public string country { get; set; }
        public int sunrise { get; set; }
        public int sunset { get; set; }
    }

    public class RootObject {
        public Coord coord { get; set; }
        public List<Weather> weather { get; set; }
        public string @base { get; set; }
        public Main main { get; set; }
        public int visibility { get; set; }
        public Wind wind { get; set; }
        public Clouds clouds { get; set; }
        public int dt { get; set; }
        public Sys sys { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public int cod { get; set; }
    }
}