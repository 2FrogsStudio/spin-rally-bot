using System.Diagnostics;
using System.Web;
using SpinRallyBot.Events.CommandReceivedConsumers.Base;
using SpinRallyBot.Models;
using SpinRallyBot.Queries;
using SpinRallyBot.Subscriptions;
using SpinRallyBot.Utils;

namespace SpinRallyBot.Events.CommandReceivedConsumers;

public class FindCommandReceivedConsumer : CommandReceivedConsumerBase {
    private readonly ITelegramBotClient _botClient;
    private readonly IScopedMediator _mediator;

    public FindCommandReceivedConsumer(ITelegramBotClient botClient, IMemoryCache memoryCache, IScopedMediator mediator) :
        base(Command.Find, botClient, memoryCache) {
        _botClient = botClient;
        _mediator = mediator;
    }

    protected override async Task<string?> ConsumeAndGetReply(long chatId, string[] args, CancellationToken cancellationToken) {
        while (true)
            switch (args) {
                case [{ } url, ..] when TryGetPlayerUrlAndId(url, out var playerUrl, out var playerId):
                    var result = await _mediator.CreateRequestClient<GetCachedPlayerInfo>()
                        .GetResponse<PlayerInfo, PlayerInfoNotFound>(new GetCachedPlayerInfo(playerUrl), cancellationToken);

                    if (result.Is<PlayerInfo>(out var playerInfoResponse) && playerInfoResponse.Message is { } player) {
                        var buttons = new List<InlineKeyboardButton>();

                        var findSubscriptionResponse = await _mediator.CreateRequestClient<FindSubscription>()
                            .GetResponse<Subscription, SubscriptionNotFound>(new FindSubscription(chatId, player.PlayerUrl), cancellationToken);

                        if (findSubscriptionResponse.Is<Subscription>(out _)) {
                            buttons.Add(new InlineKeyboardButton("Отписаться") {
                                CallbackData = JsonSerializer.Serialize(new CallbackData.ActionData(Actions.Unsubscribe, player.PlayerUrl))
                            });
                        }
                        if (findSubscriptionResponse.Is<SubscriptionNotFound>(out _)) {
                            buttons.Add(new InlineKeyboardButton("Подписаться") {
                                CallbackData = JsonSerializer.Serialize(new CallbackData.ActionData(Actions.Subscribe, player.PlayerUrl))
                            });
                        }

                        var text =
                            $"{player.Fio}\n" +
                            $"Рейтинг: {player.Rating:F}\n" +
                            $"Позиция: {player.Position}";
                        await _botClient.SendTextMessageAsync(
                            chatId,
                            text.ToEscapedMarkdownV2(),
                            parseMode: ParseMode.MarkdownV2,
                            disableNotification: true,
                            replyMarkup: new InlineKeyboardMarkup(buttons),
                            cancellationToken: cancellationToken);

                        text.ToEscapedMarkdownV2();
                    }

                    if (result.Is<PlayerInfoNotFound>(out _)) {
                        return $"Участник с идентификатором {playerId} не найден".ToEscapedMarkdownV2();
                    }

                    throw new UnreachableException();
                case [{ } search, ..]:
                    var players = await GetPlayersByName(search, cancellationToken);
                    switch (players.Length) {
                        case 0:
                            return "Участники не найдены".ToEscapedMarkdownV2();
                        case 1:
                            args = new[] { players[0].PlayerUrl };
                            continue;
                        default:
                            var playerButtons = players
                                .Select(p => {
                                    var data = JsonSerializer.Serialize(new CallbackData.CommandData(Command.Find, p.PlayerUrl));
                                    return new InlineKeyboardButton(p.Fio) { CallbackData = data };
                                })
                                .ToArray();
                            await _botClient.SendTextMessageAsync(chatId, "Выберите участника", replyMarkup: new InlineKeyboardMarkup(playerButtons.Split(1)),
                                cancellationToken: cancellationToken);
                            return null;
                    }

                default:
                    return CommandHelpers.HelpByCommand[Command.Find];
            }
    }

    private async Task<Player[]> GetPlayersByName(string search, CancellationToken cancellationToken) {
        var response = await _mediator
            .CreateRequestClient<GetPlayersByName>()
            .GetResponse<Player[]>(new GetPlayersByName(search), cancellationToken);

        return response.Message;
    }

    private static bool TryGetPlayerUrlAndId(string url, out string playerUrl, out string playerId) {
        var baseUri = new Uri(Constants.RttwUrl);
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)
            || string.IsNullOrWhiteSpace(uri.Host)) {
            uri = new Uri(baseUri, url);
        }

        if (uri.Host == baseUri.Host
            && uri is { AbsolutePath: "/players/" }) {
            playerId = HttpUtility.ParseQueryString(uri.Query).Get("id")!;
            if (!string.IsNullOrWhiteSpace(playerId)) {
                playerUrl = uri.PathAndQuery;
                return true;
            }
        }

        playerId = null!;
        playerUrl = null!;
        return false;
    }
}