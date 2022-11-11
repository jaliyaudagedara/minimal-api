WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// ConfigureServices
builder.Services.AddDbContext<EmployeeContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Minimal API", Description = "OpenAPI specification for Minimal API", Version = "v1" });
});

WebApplication app = builder.Build();

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
    return TypedResults.Ok(await dbContext.Employees.ToListAsync());
});

app.MapGet("/employees/{id}", async Task<Results<Ok<Employee>, NotFound>> (int id, EmployeeContext dbContext) =>
{
    Employee employee = await dbContext.Employees.FindAsync(id);
    if (employee is null)
    {
        return TypedResults.NotFound();
    }

    return TypedResults.Ok(employee);
});

app.MapPost("/employees", async (Employee employee, EmployeeContext dbContext) =>
{
    await dbContext.Employees.AddAsync(employee);
    await dbContext.SaveChangesAsync();

    return TypedResults.Created($"/employees/{employee.Id}", employee);
});

app.MapPut("/employees/{id}", async Task<Results<NoContent, BadRequest, NotFound>> (int id, Employee employee, EmployeeContext dbContext) =>
{
    if (id != employee.Id)
    {
        return TypedResults.BadRequest();
    }

    if (!await dbContext.Employees.AnyAsync(x => x.Id == id))
    {
        return TypedResults.NotFound();
    }

    dbContext.Entry(employee).State = EntityState.Modified;
    await dbContext.SaveChangesAsync();

    return TypedResults.NoContent();
});

app.MapDelete("/employees/{id}", async Task<Results<NoContent, NotFound>> (int id, EmployeeContext dbContext) =>
{
    Employee employee = await dbContext.Employees.FindAsync(id);
    if (employee is null)
    {
        return TypedResults.NotFound();
    }

    dbContext.Employees.Remove(employee);
    await dbContext.SaveChangesAsync();

    return TypedResults.NoContent();
});

await app.RunAsync();
