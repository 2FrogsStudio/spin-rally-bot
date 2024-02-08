using SpinRallyBot.Attributes;

namespace SpinRallyBot;

public enum Command {
    Unknown,

    [Command(
        Text = "/start",
        Description = "Главное меню"
    )]
    Start,

    [Command(
        Text = "/find",
        InlineName = "🔎 Поиск",
        Pipeline = Pipeline.Find,
        Description = "Найти игрока по ФИО или ссылке на r.ttw.ru"
    )]
    [CommandArg("[Запрос]",
                "Имя для поиска или ссылка на игрока(https://r.ttw.ru/players/?id=52a31ad)")]
    Find,

    [Command(Text = "/update",
             Description = "Обновление",
             IsInitCommand = false,
             IsAdminCommand = true)]
    Update,

    [Command(Text = "/reschedule_job",
             Description = "Обновление джобы",
             IsInitCommand = false,
             IsAdminCommand = true)]
    RescheduleJob,

    [Command(
        Text = "/help",
        Description = "Список доступных команд"
    )]
    Help
}
