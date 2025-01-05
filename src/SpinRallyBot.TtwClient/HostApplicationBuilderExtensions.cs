using AngleSharp.Html.Parser;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SpinRallyBot;

public static class HostApplicationBuilderExtensions {
    public static HostApplicationBuilder AddTtwClient(this HostApplicationBuilder builder) {
        builder.Services.AddSingleton<IHtmlParser, HtmlParser>();

        builder.Services.AddHttpClient<ITtwClient, TtwClient>()
            .ConfigureHttpClient(client => {
                client.BaseAddress = new Uri(Constants.RttwUrl);
                client.Timeout = TimeSpan.FromMinutes(5);
            });

        return builder;
    }
}
