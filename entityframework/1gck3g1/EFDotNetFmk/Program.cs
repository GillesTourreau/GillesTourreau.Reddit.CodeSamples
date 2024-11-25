using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFDotNetFmk
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (var ctx = new MyDbContext())
            {
                var entity1 = new MyEntity();

                ctx.Entities.Add(entity1);

                Console.WriteLine(entity1.Id);

                var entity2 = new MyEntity();

                ctx.Entities.Add(entity2);

                Console.WriteLine(entity2.Id);
                Console.ReadLine();
            }
        }

        public class MyDbContext : DbContext
        {
            public DbSet<MyEntity> Entities => this.Set<MyEntity>();

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MyEntity>()
                    .Property(e => e.Id);
            }
        }

        public class MyEntity
        {
            public int Id { get; set; }
        }
    }
}
