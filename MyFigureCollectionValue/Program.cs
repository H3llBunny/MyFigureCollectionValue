using AngleSharp;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyFigureCollectionValue.Data;
using MyFigureCollectionValue.Hubs;
using MyFigureCollectionValue.Models;
using MyFigureCollectionValue.Services;

namespace MyFigureCollectionValue
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddControllersWithViews();

            builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

            builder.Services.AddScoped<IBrowsingContext>(serviceProvider =>
            {
                var config = Configuration.Default.WithDefaultLoader().WithCookies();
                return BrowsingContext.New(config);
            });

            builder.Services.AddSignalR();
            builder.Services.Configure<ScraperSettings>(builder.Configuration.GetSection("ScraperSettings"));
            builder.Services.Configure<CurrencyFreaksSettings>(builder.Configuration.GetSection("CurrencyFreaks"));

            builder.Services.AddHttpClient<IScraperService, ScraperService>();
            builder.Services.AddScoped<IFigureService, FigureService>();
            builder.Services.AddScoped<ICurrencyConverterService, CurrencyConverterService>();

            builder.Services.AddHttpClient<DownloadExchangeRates>();
            builder.Services.AddHostedService<DownloadExchangeRates>();
            builder.Services.AddHostedService<UpdateAftermarketPrices>();
            builder.Services.AddHostedService<UpdateFiguresAndRetailPrices>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<ApplicationDbContext>();

                context.Database.Migrate();
                await ApplicationDbContextSeed.SeedAsync(context);
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }


            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.MapHub<ScraperProgressHub>("/scraperProgressHub");

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}
