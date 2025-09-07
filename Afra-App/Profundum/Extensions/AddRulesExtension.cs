using Afra_App.Profundum.Domain.Contracts.Rules;
using Afra_App.Profundum.Domain.Contracts.Services;
using Afra_App.Profundum.Services.Rules;

namespace Afra_App.Profundum.Extensions;

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

        services.AddScoped<IRulesFactory, ServiceProviderRulesFactory>();
    }
}
