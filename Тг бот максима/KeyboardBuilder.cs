using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace Тг_бот_максима
{
    internal static class CurrencyKeyboard
    {
        private static readonly string[] currencies = { "🇰🇿 KZT", "🇷🇺 RUB", "🇺🇸 USD", "🇨🇳 CNY" };

        public static ReplyKeyboardMarkup GetKeyboard()
        {
            var keyboardRows = new List<KeyboardButton[]>();

            for (int i = 0; i < currencies.Length; i++)
            {
                for (int j = i + 1; j < currencies.Length; j++)
                {
                    string from = currencies[i];
                    string to = currencies[j];

                    keyboardRows.Add(new KeyboardButton[]
                    {
                        new($"{from} → {to}"),
                        new($"{to} → {from}")
                    });
                }
            }

            return new ReplyKeyboardMarkup(keyboardRows)
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };
        }
    }
}