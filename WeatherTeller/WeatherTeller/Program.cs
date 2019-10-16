using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace WeatherTeller {
    
    class Program{
        
        static void Main(string[] args) {
            
            Console.WriteLine("Making API Call...");
            
            using (var client = new HttpClient()) {
                
                client.BaseAddress = new Uri("http://api.openweathermap.org/data/2.5/");

                HttpResponseMessage response = client.GetAsync("weather?q=Seattle&APPID=MYAPPID").Result;
                response.EnsureSuccessStatusCode();
                string result = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine("Result: " + result);
                Console.WriteLine("Deserializing JSON");
                RootObject weatherDetails = JsonConvert.DeserializeObject<RootObject>(result);

                double kelvinTemp = weatherDetails.main.temp;
                int temp = (int) ((kelvinTemp - 273.15) * 1.8 + 32);
                Console.WriteLine("Temperature in seattle:" + temp);
            }
        }
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
