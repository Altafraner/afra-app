using Altafraner.AfraApp.Otium.Domain.Contracts.Rules;
using Altafraner.AfraApp.Otium.Domain.Contracts.Services;
using Altafraner.AfraApp.Otium.Services;
using Altafraner.AfraApp.Otium.Services.Rules;

namespace Altafraner.AfraApp.Otium.Extensions;

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
        services.AddScoped<IIndependentRule, KlassenLimitsRule>();

        services.AddScoped<IWeekRule, RequiredKategorienRule>();

        services.AddScoped<IRulesFactory, ServiceProviderRulesFactory>();
        services.AddScoped<RulesValidationService>();
    }
}
