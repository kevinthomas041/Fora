using Fora.BusinessRules;
using Fora.Interfaces;
using Fora.Settings;

namespace Fora
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddSingleton(builder.Configuration.GetSection("EDGAR_Settings").Get<EDGAR_Settings>());
            builder.Services.AddTransient<IFundableAmountBuilder, FundableAmountBuilder>();

            var clientSettings = builder.Configuration.GetSection("HttpClientSettings").Get<HttpClientSettings>();
            builder.Services.AddHttpClient("", client =>
            {
                client.BaseAddress = new Uri(clientSettings.EDGAR_API);
                client.DefaultRequestHeaders.Add("Accept", clientSettings.Accept);
                client.DefaultRequestHeaders.Add("User-Agent", clientSettings.UserAgent);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            builder.Services.AddControllers();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            
            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}