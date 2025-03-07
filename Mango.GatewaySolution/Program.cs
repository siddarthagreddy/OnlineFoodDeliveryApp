using Mango.GatewaySolution.Extensions;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace Mango.GatewaySolution
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.AddAppAuthetication();
            builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
            builder.Services.AddOcelot(builder.Configuration);
            var app = builder.Build();


            app.UseOcelot().GetAwaiter().GetResult();
            app.MapGet("/", () => "Hello World!");

            app.Run();
        }
    }
}
