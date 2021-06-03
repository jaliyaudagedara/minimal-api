WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// ConfigureServices
builder.Services.AddDbContext<EmployeeContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

await using WebApplication app = builder.Build();

// Configure
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.MapGet("/employees", async ([FromServices] EmployeeContext dbContext) =>
{
    return await dbContext.Employees.ToListAsync();
});

app.MapGet("/employees/{id}", async ([FromServices] EmployeeContext dbContext, int id) =>
{
    Employee employee = await dbContext.Employees.FindAsync(id);
    if (employee is null)
    {
        return NotFound();
    }

    return Ok(employee);
});

app.MapPost("/employees", async ([FromServices] EmployeeContext dbContext, Employee employee) =>
{
    await dbContext.Employees.AddAsync(employee);
    await dbContext.SaveChangesAsync();

    return Ok(employee);
});

app.MapPut("/employees/{id}", async ([FromServices] EmployeeContext dbContext, int id, Employee employee) =>
{
    if (id != employee.Id)
    {
        return BadRequest();
    }

    if (!await dbContext.Employees.AnyAsync(x => x.Id == id))
    {
        return NotFound();
    }

    dbContext.Entry(employee).State = EntityState.Modified;
    await dbContext.SaveChangesAsync();

    return NoContent();
});

app.MapDelete("/employees/{id}", async ([FromServices] EmployeeContext dbContext, int id) =>
{
    Employee employee = await dbContext.Employees.FindAsync(id);
    if (employee is null)
    {
        return NotFound();
    }

    dbContext.Employees.Remove(employee);
    await dbContext.SaveChangesAsync();

    return NoContent();
});

await app.RunAsync();
