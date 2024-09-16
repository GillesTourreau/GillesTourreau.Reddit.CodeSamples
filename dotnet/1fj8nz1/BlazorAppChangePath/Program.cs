using BlazorAppChangePath.Components;

namespace BlazorAppChangePath
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // Create a branch for "/admin" request.
            app.Map("/admin", adminApp =>
            {
                adminApp.UseRouting();

                adminApp.UseStaticFiles();
                adminApp.UseAntiforgery();

                adminApp.UseEndpoints(endPoints =>
                {
                    endPoints.MapRazorComponents<App>()
                        .AddInteractiveServerRenderMode();
                });
            });

            app.Run();
        }
    }
}
