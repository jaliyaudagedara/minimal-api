namespace MinimalApi
{
    public class EmployeeContext : DbContext
    {
        public EmployeeContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
    }

    public class Employee
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
