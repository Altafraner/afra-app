using Afra_App.Otium.Domain.Contracts.Rules;
using Afra_App.Otium.Domain.Contracts.Services;
using Afra_App.Otium.Services;
using Afra_App.Otium.Services.Rules;

namespace Afra_App.Otium.Extensions;

/// <summary>
/// Contains extension methods to add built-in rules to the service collection.
/// </summary>
public static class AddRulesExtension
{
    /// <summary>
    /// Adds all built-in rules to the service collection.
    /// </summary>
    public static void AddRules(this IServiceCollection services)
    {
        services.AddScoped<IBlockRule, AlwaysAttendedRule>();
        services.AddScoped<IBlockRule, MustEnrollRule>();
        services.AddScoped<IBlockRule, ParallelEnrollmentRule>();

        services.AddScoped<IIndependentRule, EnrollmentTimeframeRule>();
        services.AddScoped<IIndependentRule, MaxEnrollmentsRule>();
        services.AddScoped<IIndependentRule, MustBeStudentRule>();
        services.AddScoped<IIndependentRule, NotCancelledRule>();

        services.AddScoped<IWeekRule, RequiredKategorienRule>();

        services.AddScoped<IRulesFactory, ServiceProviderRulesFactory>();
        services.AddScoped<RulesValidationService>();
    }
}
