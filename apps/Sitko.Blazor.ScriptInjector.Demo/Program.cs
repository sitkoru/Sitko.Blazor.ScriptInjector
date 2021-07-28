using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Sitko.Blazor.ScriptInjector.Demo
{
    using JetBrains.Annotations;

    public class Program
    {
        public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        [PublicAPI]
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
