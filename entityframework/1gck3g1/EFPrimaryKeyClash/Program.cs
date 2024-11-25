
namespace EFPrimaryKeyClash
{
    using Microsoft.EntityFrameworkCore;

    internal class Program
    {
        static void Main(string[] args)
        {
            using (var ctx = new MyDbContext())
            {
                ctx.Database.EnsureDeleted();
                ctx.Database.EnsureCreated();
            }

            using (var ctx = new MyDbContext())
            {
                var entity1 = new MyEntity();

                ctx.Add(entity1);

                Console.WriteLine($"After ctx.Add(): Entity1: {entity1.Id}");

                var entity2 = new MyEntity();

                ctx.Add(entity2);

                Console.WriteLine($"After ctx.Add(): Entity2: {entity2.Id}");

                ctx.SaveChanges();

                Console.WriteLine($"After ctx.SaveChanges(): Entity1: {entity1.Id}");
                Console.WriteLine($"After ctx.SaveChanges(): Entity2: {entity2.Id}");
            }
        }
    }

    public class MyDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=(localdb)\\reddit; Initial Catalog=1gck3g1; Integrated Security=true");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MyEntity>()
                .Property(e => e.Id)
                .UseIdentityColumn();
        }
    }

    public class MyEntity
    {
        public int Id { get; set; }
    }
}
