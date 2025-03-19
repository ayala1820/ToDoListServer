using Microsoft.EntityFrameworkCore;
using TodoApi;
using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

IServiceCollection serviceCollection = builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("ToDoDB"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("ToDoDB"))
    ));
builder.Services.AddCors(option =>
{

    option.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader());

});
var app = builder.Build();
app.UseCors("AllowAllOrigins");

app.UseSwagger(options =>
{
options.SerializeAsV2 = true;
});
app.UseSwaggerUI(options => // UseSwaggerUI is called only in Development.
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = "swagger";
});


app.MapGet("/", () => "Hello World!");
app.MapGet("/items", async (ToDoDbContext db) =>
{
    return await db.Items.ToListAsync();
});
// הוספת משימה חדשה
app.MapPost("/items", async (ToDoDbContext db, Item item) =>
{
    db.Items.Add(item);
    await db.SaveChangesAsync();
    return item;
});
app.MapPost("/items/creatItem{name}", async (string name, ToDoDbContext db) =>
{
    Item item = new Item() { Name = name, IsComplete = false };
    db.Items.Add(item);
    await db.SaveChangesAsync();
    return item;
});
// עדכון משימה
app.MapPut("/items/{id}", async (int id, bool IsComplete, ToDoDbContext db) =>
{
    var item2 = await db.Items.FindAsync(id);
    if (item2 != null)
    {
        item2.IsComplete = IsComplete;
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    return Results.NoContent();
});

// מחיקת משימה
app.MapDelete("/items/{id}", async (int id, ToDoDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    if (item == null)
    {
        return Results.NotFound();  // אם לא נמצא פריט עם ה-ID הספציפי
    }

    db.Items.Remove(item);  // מסירה את הפריט מהמסד
    await db.SaveChangesAsync();  // שמירת  השינויים במסד הנתונים
    return Results.NoContent();  // מחזיר תגובה של "אין תוכן" לאחר המחיקה
});
app.UseCors("AllowAllOrigins");

app.Run();