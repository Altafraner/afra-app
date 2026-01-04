using Altafraner.AfraApp.Profundum.Services;
using Quartz;

namespace Altafraner.AfraApp.Profundum.Jobs;

///
internal sealed class MatchingJob : IJob
{
    private readonly ProfundumMatchingService _matchingService;

    ///
    public MatchingJob(ProfundumMatchingService matchingService)
    {
        _matchingService = matchingService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine("Matching job start -------------------------------");
        var id = context.MergedJobDataMap.GetGuidValueFromString("matchingid");
        Console.WriteLine("Matching job begin   -------------------------------");
        var result = await _matchingService.PerformMatching();
        Console.WriteLine("Matching job end   -------------------------------");
        _matchingService.RegisterMatching(id, result);
        Console.WriteLine("Matching job reg   -------------------------------");
    }
}
