using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Тг_бот_максима;


namespace Тг_бот_максима
{
    class Program
    {
        private static readonly Dictionary<long, (string From, string To)> userSelection = new();

        static async Task Main()
        {
            string botToken = "8203191544:AAGt7dxjEgT35kQ1YC4KyHRZH23NtApnzrE";
            var bot = new TelegramBotClient(botToken);

            using var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<Telegram.Bot.Types.Enums.UpdateType>()
            };

            bot.StartReceiving(UpdateHandler, ErrorHandler, receiverOptions, cts.Token);

            var me = await bot.GetMe();
            Console.WriteLine($"Бот @{me.Username} запущен");

            Console.ReadLine();
            cts.Cancel();
        }

        // Основной метод обработки сообщений
        private static async Task UpdateHandler(ITelegramBotClient bot, Update update, CancellationToken token)
        {
            if (update.Message is not { } message || message.Text is not { } text)
                return;

            long chatId = message.Chat.Id;

            if (text == "/start")
            {
                var buttons = CurrencyKeyboard.GetKeyboard();

                await bot.SendMessage(
                    chatId: chatId,
                    text: "Выберите направление конвертации 💸 ",
                    replyMarkup: buttons,
                    cancellationToken: token
                );
                return;
            }

            if (text.Contains("→"))
            {
                string[] parts = text.Split("→", StringSplitOptions.TrimEntries);
                string fromCurrency = parts[0].Trim();
                string toCurrency = parts[1].Trim();

                userSelection[chatId] = (fromCurrency, toCurrency);

                await bot.SendMessage(
                    chatId: chatId,
                    text: $"Вы выбрали {fromCurrency} → {toCurrency}. Введите сумму для конвертации:",
                    cancellationToken: token
                );
                return;
            }

            //  Пользователь ввёл сумму
            if (userSelection.ContainsKey(chatId))
            {
                if (!decimal.TryParse(text.Replace(',', '.'), out decimal amount))
                {
                    await bot.SendMessage(
                        chatId: chatId,
                        text: "Некорректная сумма. Введите число.",
                        cancellationToken: token
                    );
                    return;
                }

                var (fromCurrency, toCurrency) = userSelection[chatId];

                decimal result = await CurrencyConverter.ConvertAsync(fromCurrency, toCurrency, amount);

                if (result == 0)
                {
                    await bot.SendMessage(
                        chatId: chatId,
                        text: "Ошибка при получении курса валют 💥",
                        cancellationToken: token
                    );
                }
                else
                {
                    await bot.SendMessage(
                        chatId: chatId,
                        text: $"{amount} {fromCurrency} → {toCurrency} = {result:F2}",
                        cancellationToken: token
                    );
                }

                userSelection.Remove(chatId);
                return;
            }

            if (text == "/help")
            {
                await bot.SendMessage(
                    chatId: chatId,
                    text: "👋 Привет! Я бот-конвертер валют.\n\n" +
                    " Я могу быстро и удобно пересчитывать любую сумму из одной валюты в другую по актуальному курсу.\n\n" +
                    "📌 Как пользоваться:\n" +
                    "1️⃣ Нажми /start и выбери направление конвертации, например: USD → KZT.\n" +
                    "2️⃣ Введи сумму, которую хочешь конвертировать, например: 100\n" +
                    "3️⃣ Я сразу верну результат в нужной валюте 💸\n\n" +
                    "💡 Совет: для выбора валют используй кнопки — это быстрее и проще.\n\n" +
                    "Если хочешь снова выбрать другую валютную пару, нажми /start и повтори процесс.",
                    cancellationToken: token
                );
            }
        }

        private static Task ErrorHandler(ITelegramBotClient bot, Exception exception, CancellationToken token)
        {
            Console.WriteLine($"Ошибка: {exception.Message}");
            return Task.CompletedTask;
        }
    }
}