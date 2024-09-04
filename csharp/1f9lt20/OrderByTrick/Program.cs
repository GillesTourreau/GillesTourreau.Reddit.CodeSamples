using Microsoft.EntityFrameworkCore;

namespace OrderByTrick
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var countries = new Country[]
            {
                new Country() { Name = "France" },
                new Country() { Name = "United States of America" },
                new Country() { Name = "Argentina" },
                new Country() { Name = "Philippines" },
                new Country() { Name = "Canada" }
            };

            var b = new DbContextOptionsBuilder<MyDbContext>();
            b.UseSqlServer("Data Source=(localdb)\\demoapp; Initial Catalog=Countries; Integrated Security=True");

            var c = new MyDbContext(b.Options);

            var sql = c.Set<Country>().OrderByDescending(c => c.Name == "Canada" || c.Name == "United States of America").ThenBy(c => c.Name).ToArray();

            var order = countries.OrderByDescending(c => c.Name == "Canada" || c.Name == "United States of America").ThenBy(c => c.Name).ToArray();

            Console.WriteLine(order);
        }

        public class MyDbContext : DbContext
        {
            public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Country>();
            }
        }

        private sealed class Country
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public override string ToString()
            {
                return this.Name;
            }
        }
    }
}
