using LanchesMac.Context;
using LanchesMac.Models;
using LanchesMac.Repositories.Interfaces;
using LanchesMac.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using ReflectionIT.Mvc.Paging;
using Microsoft.AspNetCore.Identity;
using LanchesMac.Services;
using LanchesMac.Areas.Admin.Servicos;
using LanchesMac.Areas.Admin.Services;
using FastReport.Data;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var connection = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<AppDbContext>(options =>
                   options.UseSqlServer(connection));

        FastReport.Utils.RegisteredObjects.AddConnection(typeof(MsSqlDataConnection));

        builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                   .AddEntityFrameworkStores<AppDbContext>()
                   .AddDefaultTokenProviders();

        builder.Services.Configure<ConfigurationImagens>(builder.Configuration.GetSection("ConfigurationPastaImagens"));

        builder.Services.AddTransient<ILancheRepository, LancheRepository>();
        builder.Services.AddTransient<ICategoriaRepository, CategoriaRepository>();
        builder.Services.AddTransient<IPedidoRepository, PedidoRepository>();
        builder.Services.AddScoped<ISeedUserRoleInitial, SeedUserRoleInitial>();

        builder.Services.AddScoped<RelatorioVendasService>();
        builder.Services.AddScoped<GraficoVendasService>();

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin",
                politica =>
                {
                    politica.RequireRole("Admin");
                });
        });

        builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        builder.Services.AddScoped(sp => CarrinhoCompra.GetCarrinho(sp));

        builder.Services.AddControllersWithViews();

        builder.Services.AddPaging(options =>
        {
            options.ViewName = "Bootstrap4";
            options.PageParameterName = "pageindex";
        });

        builder.Services.AddMemoryCache();
        builder.Services.AddSession();






        var app = builder.Build();



        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            //o valor HSTS padr�o � de 30 dias
            app.UseHsts();
        }
        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseFastReport();
        app.UseRouting();


        app.UseSession();

        app.UseAuthentication();
        app.UseAuthorization();

        CriarPerfisUsuarios(app);

        ////cria os perfis
        //seedUserRoleInitial.SeedRoles();
        ////cria os us�ario e atribui ao perfil
        //seedUserRoleInitial.SeedUsers();




        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
            name: "areas",
            pattern: "{area:exists}/{controller=Admin}/{action=Index}/{id?}");

            endpoints.MapControllerRoute(
               name: "categoriaFiltro",
               pattern: "Lanche/{action}/{categoria?}",
               defaults: new { Controller = "Lanche", action = "List" });

            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        });




        app.Run();

        void CriarPerfisUsuarios(WebApplication app)
        {
            var scopedFactory = app.Services.GetService<IServiceScopeFactory>();
            using (var scope = scopedFactory.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<ISeedUserRoleInitial>();
                service.SeedUsers();
                service.SeedRoles();
            }
        }
    }
}