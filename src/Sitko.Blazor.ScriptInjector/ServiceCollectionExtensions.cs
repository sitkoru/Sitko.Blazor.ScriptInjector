namespace Sitko.Blazor.ScriptInjector
{
    using JetBrains.Annotations;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    [PublicAPI]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddScriptInjector(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddScoped<IScriptInjector, ScriptInjector>();
            return serviceCollection;
        }
    }
}
