using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Тг_бот_максима
{
    internal class CurrencyConverter
    {
        private static readonly HttpClient http = new();

        private static readonly Dictionary<string, string> currencyMap = new()
    {
        { "🇰🇿 KZT", "KZT" },
        { "🇷🇺 RUB", "RUB" },
        { "🇺🇸 USD", "USD" },
        { "🇨🇳 CNY", "CNY" }
    };

        public static async Task<decimal> ConvertAsync(string fromCurrency, string toCurrency, decimal amount)
        {
            try
            {

                fromCurrency = currencyMap.ContainsKey(fromCurrency) ? currencyMap[fromCurrency] : fromCurrency;
                toCurrency = currencyMap.ContainsKey(toCurrency) ? currencyMap[toCurrency] : toCurrency;

                const string apiKey = "644ba5b0b5002a39f8c29f06";
                string url = $"https://v6.exchangerate-api.com/v6/{apiKey}/latest/{fromCurrency}";
                string response = await http.GetStringAsync(url);

                using var doc = JsonDocument.Parse(response);
                var root = doc.RootElement;

                
                if (!root.TryGetProperty("result", out var resultProp) || resultProp.GetString() != "success")
                    throw new Exception("Ошибка при получении данных от API.");

                var rates = root.GetProperty("conversion_rates");
                decimal rate = rates.GetProperty(toCurrency).GetDecimal();

                return amount * rate;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка конвертации: {ex.Message}");
                return 0;
            }

        }
    }
}

