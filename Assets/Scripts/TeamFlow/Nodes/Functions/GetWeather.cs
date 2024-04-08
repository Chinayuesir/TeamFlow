using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using OpenAI;

namespace TeamFlow.Nodes
{
    public class GetWeather:FunctionNode
    {
        protected override void Init()
        {
            ToolFromFunc = Tool.GetOrCreateTool(typeof(GetWeather), nameof(GetCurrentWeatherAsync));
        }
        public enum WeatherUnit
        {
            Celsius,
            Fahrenheit
        }

        [Function("Get the current weather in a given location")]
        public static async Task<string> GetCurrentWeatherAsync(
            [FunctionParameter("The location the user is currently in.")] string location,
            [FunctionParameter("The units the user has requested temperature in." +
                               " Typically this is based on the users location.")] WeatherUnit unit)
        {
            var temp = new Random().Next(-10, 40);

            temp = unit switch
            {
                WeatherUnit.Fahrenheit => CelsiusToFahrenheit(temp),
                _ => temp
            };

            return await Task.FromResult($"The current weather in {location} is {temp}\u00b0 {unit}");
        }

        public static int CelsiusToFahrenheit(int celsius) => (celsius * 9 / 5) + 32;
    }
}