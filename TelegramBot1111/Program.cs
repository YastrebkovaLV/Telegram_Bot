
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

class Program
{
    private static TelegramBotClient botClient;
    private static Dictionary<long, string> userNames = new Dictionary<long, string>();
    private static List<(string Outfit,string ImageUr1)> ridiculousOutfits = new List<(string,string)>
    {
        ("Любая вещь с надписью fashion", "https://kingboxer.ru/upload/iblock/69e/69e740686b62ccba98f5db40a85e9694.jpg"),
        ("Футболка со стразами","https://zolla.com/upload/iblock/422/9a32j585fyc91cun25rb667r38eqxo86.jpg"),
        ("Брюки ядерного цвета","https://ae04.alicdn.com/kf/S02918473fad04489962d78003b841c952.jpg_480x480.jpg"),
        ("Пуховик в полосочку", "https://ae04.alicdn.com/kf/Hcc6739c3b41a422abb6345c3451f0b23U.jpg"),
        ("Сандалии с носками","https://static2.issaplus.com/wa-data/public/photos/09/74/7409/7409.970.jpg")
    };

    static async Task Main()
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        botClient = new TelegramBotClient("7604883527:AAHOh4sWaTSFu7ZTnWNpx09aPDTINnV1Kwc", cancellationToken: cts.Token);

        var me = await botClient.GetMeAsync();
        Console.WriteLine($"Bot id: {me.Id}. Bot name: {me.FirstName}");

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = { } 
        };

        botClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cancellationToken: cts.Token
        );

        Console.WriteLine("Бот работает,откройте тг для взаимодействия с ним");

        Console.CancelKeyPress += (s, e) => {
            e.Cancel = true;
            cts.Cancel();
        };

        await Task.Delay(-1, cts.Token);
    }

    private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.Message && update.Message?.Text != null)
        {
            var chatId = update.Message.Chat.Id;
            var text = update.Message.Text.Trim();

            await HandleTextMessage(chatId, text, cancellationToken);
        }
    }

    private static async Task HandleTextMessage(long chatId, string text, CancellationToken cancellationToken)
    {
        if (text.Equals("/start", StringComparison.OrdinalIgnoreCase))
        {
            if (!userNames.ContainsKey(chatId))
            {
                await botClient.SendTextMessageAsync(chatId, "Привет! Я бот, который расскажет о нелепых нарядах. Используйте /help, чтобы узнать больше.", cancellationToken: cancellationToken);
                userNames[chatId] = null; // Инициализация
            }
        }
        else if (text.Equals("/help", StringComparison.OrdinalIgnoreCase))
        {
            await ShowHelp(chatId, cancellationToken);
        }
        else if (text.Equals("/ridiculous_outfits", StringComparison.OrdinalIgnoreCase))
        {
            await ShowRidiculousOutfits(chatId, cancellationToken);
        }
        else
        {
            await botClient.SendTextMessageAsync(chatId, "Команда не распознана. Используйте /help, чтобы узнать доступные команды.", cancellationToken: cancellationToken);
        }
    }

    private static async Task ShowHelp(long chatId, CancellationToken cancellationToken)
    {
        string helpMessage = "Доступные команды:\n" +
                             "/start - Начать общение с ботом\n" +
                             "/help - Показать это сообщение\n" +
                             "/ridiculous_outfits - Показать список нелепых нарядов";
        await botClient.SendTextMessageAsync(chatId, helpMessage, cancellationToken: cancellationToken);
    }

    private static async Task ShowRidiculousOutfits(long chatId, CancellationToken cancellationToken)
    {
        foreach (var (outfit, imageUrl) in ridiculousOutfits)
        {
            await botClient.SendTextMessageAsync(chatId, outfit, cancellationToken: cancellationToken);
            await botClient.SendPhotoAsync(chatId, imageUrl, cancellationToken: cancellationToken);
        }
    }

    private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Ошибка: {exception.Message}");
        return Task.CompletedTask;
    }
}
