using System.Web;

namespace SpinRallyBot.Events.CommandReceivedConsumers;

public class FindCommandReceivedConsumer : CommandReceivedConsumerBase {
    private readonly IScopedMediator _mediator;

    public FindCommandReceivedConsumer(ITelegramBotClient botClient, IMemoryCache memoryCache,
        IScopedMediator mediator) :
        base(Command.Find, botClient, memoryCache, mediator) {
        _mediator = mediator;
    }

    protected override async Task ConsumeAndGetReply(long userId, long chatId, string[] args,
        CancellationToken cancellationToken) {
        while (true)
            switch (args) {
                case [{ } url, ..] when TryGetPlayerUrlAndId(url, out var playerUrl, out var playerId):
                    var result = await _mediator
                        .CreateRequestClient<GetCachedPlayerInfo>()
                        .GetResponse<PlayerInfo, PlayerInfoNotFound>(new GetCachedPlayerInfo(playerUrl),
                            cancellationToken);
                    if (result.Is<PlayerInfo>(out var playerInfoResponse) &&
                        playerInfoResponse.Message is { } player1) {
                        await PlayerInfo(chatId, player1, cancellationToken);
                        return;
                    }

                    if (result.Is<PlayerInfoNotFound>(out _)) {
                        Text = $"Участник с идентификатором {playerId} не найден".ToEscapedMarkdownV2();
                        return;
                    }

                    throw new UnreachableException();
                case [{ } search, ..]:
                    var players = await GetPlayersByName(search, cancellationToken);
                    switch (players) {
                        case []:
                            Text = "Участники не найдены".ToEscapedMarkdownV2();
                            return;
                        case [var player2]:
                            args = new[] { player2.PlayerUrl };
                            continue;
                        default:
                            //todo: pagination
                            var playerButtons = players
                                .Select(p => {
                                    var data = JsonSerializer.Serialize(
                                        new NavigationData.CommandData(Command.Find, p.PlayerUrl));
                                    return new InlineKeyboardButton(p.Fio) {
                                        CallbackData = data
                                    };
                                })
                                .ToArray();
                            Text = "Выберите участника";
                            InlineKeyboard = playerButtons.Split(1);
                            return;
                    }

                default:
                    Text = CommandHelpers.HelpByCommand[Command.Find];
                    return;
            }
    }

    private async Task PlayerInfo(long chatId, PlayerInfo player, CancellationToken cancellationToken) {
        var buttons = new List<InlineKeyboardButton>();

        var findSubscriptionResponse = await _mediator
            .CreateRequestClient<FindSubscription>()
            .GetResponse<SubscriptionResult, SubscriptionNotFound>(new FindSubscription(chatId, player.PlayerUrl),
                cancellationToken);

        if (findSubscriptionResponse.Is<SubscriptionResult>(out _)) {
            buttons.Add(new InlineKeyboardButton("Отписаться") {
                CallbackData =
                    JsonSerializer.Serialize(new NavigationData.ActionData(Actions.Unsubscribe, player.PlayerUrl))
            });
        }

        if (findSubscriptionResponse.Is<SubscriptionNotFound>(out _)) {
            buttons.Add(new InlineKeyboardButton("Подписаться") {
                CallbackData =
                    JsonSerializer.Serialize(new NavigationData.ActionData(Actions.Subscribe, player.PlayerUrl))
            });
        }

        Text =
            $"{player.Fio}\n".ToEscapedMarkdownV2() +
            $"Рейтинг: {player.Rating:F}\n".ToEscapedMarkdownV2() +
            $"Позиция: {player.Position}".ToEscapedMarkdownV2();
        InlineKeyboard = buttons.Split(1);
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