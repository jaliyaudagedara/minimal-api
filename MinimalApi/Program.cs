using Microsoft.OpenApi.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// ConfigureServices
builder.Services.AddDbContext<EmployeeContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Minimal API", Description = "OpenAPI specification for Minimal API", Version = "v1" });
});

await using WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Minimal API V1");
});

// Configure
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.MapGet("/employees", async (EmployeeContext dbContext) =>
{
    return await dbContext.Employees.ToListAsync();
});

app.MapGet("/employees/{id}", async (EmployeeContext dbContext, int id) =>
{
    Employee employee = await dbContext.Employees.FindAsync(id);
    if (employee is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(employee);
});

app.MapPost("/employees", async (EmployeeContext dbContext, Employee employee) =>
{
    await dbContext.Employees.AddAsync(employee);
    await dbContext.SaveChangesAsync();

    return Results.Ok(employee);
});

app.MapPut("/employees/{id}", async (EmployeeContext dbContext, int id, Employee employee) =>
{
    if (id != employee.Id)
    {
        return Results.BadRequest();
    }

    if (!await dbContext.Employees.AnyAsync(x => x.Id == id))
    {
        return Results.NotFound();
    }

    dbContext.Entry(employee).State = EntityState.Modified;
    await dbContext.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/employees/{id}", async (EmployeeContext dbContext, int id) =>
{
    Employee employee = await dbContext.Employees.FindAsync(id);
    if (employee is null)
    {
        return Results.NotFound();
    }

    dbContext.Employees.Remove(employee);
    await dbContext.SaveChangesAsync();

    return Results.NoContent();
});

await app.RunAsync();
