using AtradeI.Services;
using Quartz;

namespace QuartzJobs
{
    public class TradeJob : IJob
    {
        private readonly DataAggregatorService _aggregator;
        private readonly OpenAIClient _ai;
        private readonly TradeExecutor _executor;

        public TradeJob(DataAggregatorService aggregator, OpenAIClient ai, TradeExecutor executor)
        {
            _aggregator = aggregator;
            _ai = ai;
            _executor = executor;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var data = await _aggregator.BuildContextAsync("BTCUSDT");
            var decision = await _ai.GetDecisionAsync(data);


            Console.WriteLine($"[{DateTime.UtcNow}]" +
                $" Pair: {decision.Pair}," +
                $" AI Decision: {decision.Decision}," +
                $" Reason: {decision.Reason}," +
                $" Analyzed data: {decision.Analyzed_Data}," +
                $" Confidence: {decision.Confidence}," +
                $" Time (UTC): {decision.Time}," +
                $" Current price: {decision.Current_Price}"
                );

            //if ((decision == "BUY" || decision == "SELL") && data.Positions.All(p => p.Side != decision))
            //{
            //    var success = await _executor.ExecuteOrderAsync(data.Symbol, decision, 0.01m);
            //    Console.WriteLine(success ? "Order executed." : "Order execution failed.");
            //}
        }
    }
}
