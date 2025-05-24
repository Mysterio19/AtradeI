using AtradeI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuartzJobs;
using System.Threading.Tasks;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        // HttpClient factory
        services.AddHttpClient();

        // Модульные сервисы
        services.AddSingleton<CandleService>();
        services.AddSingleton<IndicatorCalculator>();
        services.AddSingleton<NewsFetcher>();
        services.AddSingleton<GlobalMetricsService>();
        services.AddSingleton<DataAggregatorService>();
        services.AddSingleton<MultiTimeframeService>();

        // API ключи (рекомендуется вынести в appsettings.json)
        var bybitApiKey = "your_bybit_api_key";
        var bybitApiSecret = "your_bybit_api_secret";
        var openAiKey = "*";

        // Сервисы с зависимостью от ключей
        services.AddSingleton(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            return new BybitPositionService(httpClientFactory.CreateClient(), bybitApiKey, bybitApiSecret);
        });
        services.AddSingleton(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            return new TradeExecutor(httpClientFactory.CreateClient(), bybitApiKey, bybitApiSecret);
        });
        services.AddSingleton(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            return new OpenAIClient(httpClientFactory.CreateClient(), openAiKey);
        });

        // TradeJob для ручного запуска
        services.AddSingleton<TradeJob>();

        //// Quartz задача (если понадобится позже)
        //services.AddQuartz(q =>
        //{
        //    var jobKey = new JobKey("TradeJob");
        //    q.AddJob<TradeJob>(opts => opts.WithIdentity(jobKey));
        //    q.AddTrigger(opts => opts
        //        .ForJob(jobKey)
        //        .WithIdentity("TradeTrigger")
        //        .WithCronSchedule("0 0/15 * * * ?")); // каждые 15 минут
        //});
        //
        //services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
    });

var host = builder.Build();

// Прямой вызов логики TradeJob для теста
using (var scope = host.Services.CreateScope())
{
    var job = scope.ServiceProvider.GetRequiredService<TradeJob>();
    await job.Execute(null); // context не используется
}

await host.RunAsync();
