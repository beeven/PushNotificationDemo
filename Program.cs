using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<testpushnotification.Services.IVAPIDService, testpushnotification.Services.VAPIDService>();
builder.Services.AddDbContext<testpushnotification.Data.SubscriptionDbContext>(options => {
    options.UseSqlite("Data Source=subsdb.sqlite");
});
builder.Services.AddHttpClient();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using(var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetService<testpushnotification.Data.SubscriptionDbContext>();
    db.Database.Migrate();
    db.Dispose();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
