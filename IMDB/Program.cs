using IMDB.Data;
using IMDB.Data.Cart;
using IMDB.Data.Services;
using IMDB.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;


namespace IMDB
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString")));

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options=> {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
               
                options.User.RequireUniqueEmail = true;
            }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

            builder.Services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
                    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

                    options.Scope.Add("profile");
                    options.Scope.Add("email");
                    options.ClaimActions.MapJsonKey("picture", "picture");

                    options.SaveTokens = true;
                })
                .AddGitHub(options =>
                {
                    options.ClientId = builder.Configuration["Authentication:GitHub:ClientId"];
                    options.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"];
                })
                .AddFacebook(options =>
                {
                    options.AppId = builder.Configuration["Authentication:Facebook:AppId"];
                    options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];

                    options.Fields.Add("picture.width(500).height(500)");
                    options.Fields.Add("name");
                    options.Fields.Add("email");

                    options.ClaimActions.MapJsonKey("picture", "picture.data.url");
                    options.SaveTokens = true;
                });

            //services configuration
            builder.Services.AddScoped<IActorsService,Actorsservice>();
            builder.Services.AddScoped<IProducersService,ProducersService>();
            builder.Services.AddScoped<ICinemasService,CinemasService>();
            builder.Services.AddScoped<IMoviesService,MoviesService>();
            builder.Services.AddScoped<IOrdersService, OrdersService>();
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

            builder.Services.AddScoped<IEmailService, EmailService>();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddMemoryCache();
            builder.Services.AddSession();

            builder.Services.AddScoped<ShoppingCart>(Sp => ShoppingCart.GetShoppingCart(Sp));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            
            app.UseRouting();
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();
           

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Movies}/{action=Index}/{id?}")
                .WithStaticAssets();

            //seed database
            AppDbIntializer.Seed(app);
            AppDbIntializer.SeedUsersAndRolesAsync(app).Wait();
            app.Run();
        }
    }
}
