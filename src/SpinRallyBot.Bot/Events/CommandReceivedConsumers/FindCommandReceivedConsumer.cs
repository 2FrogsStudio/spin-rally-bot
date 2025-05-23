namespace SpinRallyBot.Events.CommandReceivedConsumers;

public class FindCommandReceivedConsumer(ITelegramBotClient botClient, IScopedMediator mediator)
    : CommandReceivedConsumerBase(Command.Find,
        botClient, mediator) {
    private readonly IScopedMediator _mediator = mediator;

    protected override async Task ConsumeAndGetReply(long userId, long chatId, int? replyToMessageId, string[] args,
        bool isBotAdmin,
        CancellationToken cancellationToken) {
        while (true) {
            switch (args) {
                case [{ } url, ..] when TryGetPlayerUrlAndId(url, out string? playerUrl, out string? playerId):
                    await ComposePlayerInfo(chatId, playerUrl ?? throw new InvalidOperationException(),
                        playerId ?? throw new InvalidOperationException(), cancellationToken);
                    return;
                case [{ } search, ..]:
                    (string Fio, string PlayerUrl)[] players = await SearchPlayers(search, cancellationToken);
                    switch (players) {
                        case []:
                            Text = "Участники не найдены".ToEscapedMarkdownV2();
                            return;
                        case [var player]:
                            args = [player.PlayerUrl];
                            continue;
                        default:
                            //todo: pagination
                            InlineKeyboardButton[] playerButtons = players
                                .Select(p => {
                                    string data = JsonSerializer.Serialize(
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
    }

    private async Task ComposePlayerInfo(long chatId, string playerUrl, string playerId,
        CancellationToken cancellationToken) {
        await _mediator.Send(new UpdatePlayer(playerUrl), cancellationToken);

        Response<GetPlayerResult, GetPlayerNotFoundResult> result = await _mediator
            .CreateRequestClient<GetPlayer>()
            .GetResponse<GetPlayerResult, GetPlayerNotFoundResult>(new GetPlayer(playerUrl),
                cancellationToken);
        if (result.Is<GetPlayerResult>(out Response<GetPlayerResult>? playerResponse) &&
            playerResponse.Message is { } player) {
            var buttons = new List<InlineKeyboardButton>();

            Response<SubscriptionFound, SubscriptionNotFound> findSubscriptionResponse = await _mediator
                .CreateRequestClient<FindSubscription>()
                .GetResponse<SubscriptionFound, SubscriptionNotFound>(new FindSubscription(chatId, player.PlayerUrl),
                    cancellationToken);
            if (findSubscriptionResponse.Is<SubscriptionFound>(out _)) {
                buttons.Add(new InlineKeyboardButton("Отписаться") {
                    CallbackData =
                        JsonSerializer.Serialize(
                            new NavigationData.ActionData(Actions.Unsubscribe, player.PlayerUrl))
                });
            }

            if (findSubscriptionResponse.Is<SubscriptionNotFound>(out _)) {
                buttons.Add(new InlineKeyboardButton("Подписаться") {
                    CallbackData =
                        JsonSerializer.Serialize(new NavigationData.ActionData(Actions.Subscribe, player.PlayerUrl))
                });
            }

            Text =
                $"{player.Fio}".ToEscapedMarkdownV2() + "\n" +
                $"Рейтинг: {player.Rating:F2}".ToEscapedMarkdownV2() + "\n" +
                $"Позиция: {player.Position}".ToEscapedMarkdownV2() + "\n" +
                $"Подписчиков: {player.Subscribers}".ToEscapedMarkdownV2() + "\n" +
                $"Обновлено: {player.Updated:dd.MM.yyyy H:mm} (МСК)".ToEscapedMarkdownV2() + "\n" +
                $"{Constants.RttwUrl}{player.PlayerUrl}".ToEscapedMarkdownV2();
            InlineKeyboard = buttons.Split(1);
            return;
        }

        if (result.Is<GetPlayerNotFoundResult>(out _)) {
            Text = $"Участник с идентификатором {playerId} не найден".ToEscapedMarkdownV2();
            return;
        }

        throw new UnreachableException();
    }

    private async Task<(string Fio, string PlayerUrl)[]> SearchPlayers(string search,
        CancellationToken cancellationToken) {
        Response<SearchPlayersResult> response = await _mediator
            .CreateRequestClient<SearchPlayers>()
            .GetResponse<SearchPlayersResult>(new SearchPlayers(search), cancellationToken);
        return response.Message.Players;
    }

    private static bool TryGetPlayerUrlAndId(string url, out string? playerUrl, out string? playerId) {
        var baseUri = new Uri(Constants.RttwUrl);
        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri)
            || string.IsNullOrWhiteSpace(uri.Host)) {
            uri = new Uri(baseUri, url);
        }

        if (uri.Host == baseUri.Host
            && uri is { AbsolutePath: "/players/" }) {
            playerId = HttpUtility.ParseQueryString(uri.Query).Get("id");
            if (!string.IsNullOrWhiteSpace(playerId)) {
                playerUrl = uri.PathAndQuery;
                return true;
            }
        }

        playerId = null;
        playerUrl = null;
        return false;
    }
}
