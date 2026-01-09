using Altafraner.AfraApp.Profundum.Domain.Contracts.Rules;
using Altafraner.AfraApp.Profundum.Domain.Contracts.Services;
using Altafraner.AfraApp.Profundum.Services.Rules;

namespace Altafraner.AfraApp.Profundum.Extensions;

///
public static class AddRulesExtension
{
    /// <summary>
    /// Adds all built-in rules to the service collection.
    /// </summary>
    public static void AddRules(this IServiceCollection services)
    {
        services.AddScoped<IProfundumAggregateRule, MaxEinschreibungenRule>();
        services.AddScoped<IProfundumIndividualRule, ProfilRule>();
        services.AddScoped<IProfundumIndividualRule, NotMultipleInstancesOfSameProfundumRule>();
        services.AddScoped<IProfundumIndividualRule, KlassenLimitsRule>();
        services.AddScoped<IProfundumIndividualRule, DependencyRule>();
        services.AddScoped<IRulesFactory, ServiceProviderRulesFactory>();
    }
}
