using Interpretarr.Clients.FlareSolver;
using Interpretarr.Clients.QBittorrent;
using Interpretarr.Clients.Sonarr;
using Interpretarr.Config;
using Interpretarr.Model;
using Interpretarr.Services.F1;

namespace Interpretarr
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.AddEnvironmentVariables();

            builder.Services.Configure<FlareSolverConfig>(builder.Configuration.GetSection("FlareSolverConfig"));
            builder.Services.Configure<QBittorrentConfig>(builder.Configuration.GetSection("QBittorrentConfig"));
            builder.Services.Configure<SonarrConfig>(builder.Configuration.GetSection("SonarrConfig"));

            builder.Services.AddSingleton<SonarrClient>();
            builder.Services.AddSingleton<FlareSolverClient>();
            builder.Services.AddSingleton<QBittorrentClient>();
            builder.Services.AddSingleton<HttpClient>();

            builder.Services.AddSingleton<F1Helper1337x>();
            builder.Services.AddSingleton<IMiddlemanHelper>(sp => sp.GetRequiredService<F1Helper1337x>());
            builder.Services.AddSingleton<F1HelperTorrentLeech>();
            builder.Services.AddSingleton<IMiddlemanHelper>(sp => sp.GetRequiredService<F1HelperTorrentLeech>());

            builder.Services.AddSingleton<SonarrService>();
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            Configure(app);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }

        public static void Configure(IApplicationBuilder app)
        {
            app.ApplicationServices.GetRequiredService<SonarrService>();
        }
    }
}