using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace CascadeRelationShip
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer("Data Source=(localdb)\\DemoApp; Initial Catalog=CascadeRelationShip; Integrated Security=True");

            var dbContext = new ApplicationDbContext(options.Options);

            dbContext.Database.EnsureCreated();

            Console.WriteLine("Hello, World!");
        }
    }

    public class Course
    {
        [Key]
        public int CourseId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        // [ForeignKey("Member")]
        public int MemberId { get; set; }

        public Member Member { get; set; }
        public List<Session> Sessions { get; set; }
        public bool isStandAlone { get; set; }


        public Course()
        {
            Sessions = new List<Session>();
        }
    }

    public class Session
    {
        [Key]
        public int SessionId { get; set; }
        public DateTime Date { get; set; }

        public List<Enrollment> EnrolledMembers { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; }

        public SessionStatus Status { get; set; }

        public Session()
        {

            EnrolledMembers = new List<Enrollment>();
        }

    }

    public class OnlineSession : Session
    {
        public string OnlineLink { get; set; }
        public string? MeetingCode { get; set; }
        public string AdditionalDetails { get; set; }
    }



    public class OnPremSession : Session
    {
        public string StreetNumber { get; set; }
        public string StreetName { get; set; }
        public string Suburb { get; set; }


    }

    public enum SessionStatus
    {
        Cancelled,
        Scheduled,
        Draft,
        Complete,
        Closed

    }

    public class Enrollment
    {
        [Key]
        public int EnrollmentId { get; set; }
        public int MemberId { get; set; }

        public Member Member { get; set; }

        public int SessionId { get; set; }
        public Session Session { get; set; }
        public DateTime EnrolledDate = DateTime.Now;
    }

    public class Member
    {
        [Key]
        public int MemberId { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        //public List<Course> EnrolledCourses { get; set; }

        public List<Course>? CreatedCourses { get; set; }
        public MemberType Type { get; set; }

        public DateTime StartDate { get; set; }
        public List<Enrollment> Enrollments { get; set; }
        public Member()
        {

            Enrollments = new List<Enrollment>();
        }
    }

    public enum MemberType
    {
        Pro,
        Standard,
        Honourary
    }

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Session>()
                .HasOne(s => s.Course)
                .WithMany(c => c.Sessions)
                .HasForeignKey(s => s.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<Enrollment>()
            //    .HasOne(e => e.Session)
            //    .WithMany(s => s.EnrolledMembers)
            //    .HasForeignKey(e=>e.SessionId)
            //    .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Session>()
                .HasDiscriminator<string>("session_type")
                .HasValue<Session>("session_base")
                .HasValue<OnPremSession>("session_onprem")
                .HasValue<OnlineSession>("session_online");

            //modelBuilder.Entity<Member>()
            //    .HasMany(m => m.CreatedCourses)
            //    .WithOne(c => c.Member)
            //    .HasForeignKey(s => s.CourseId)
            //    .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Member>()
                .HasMany(m => m.Enrollments)
                .WithOne(e => e.Member)
                .HasForeignKey(e => e.MemberId)
                .OnDelete(DeleteBehavior.NoAction);

            //modelBuilder.Entity<Member>().HasData(
            //    new Member
            //    {
            //        MemberId = 1,
            //        Name = "Bob",
            //        LastName = "Dylan",
            //        Type = MemberType.Pro,
            //        StartDate = DateTime.Now,
            //    },
            //    new Member
            //    {
            //        MemberId = 2,
            //        Name = "Ringo",
            //        LastName = "Starr",
            //        Type = MemberType.Standard,
            //        StartDate = DateTime.Now,
            //    });
            //modelBuilder.Entity<Course>().HasData(
            //    new Course
            //    {
            //        CourseId = 1,
            //        Name = "Gardening",
            //        Description = "Beginner's guide to gardening",
            //        MemberId = 1,
            //        isStandAlone = true
            //    });
            //modelBuilder.Entity<Enrollment>().HasData(
            //    new Enrollment
            //    {
            //        EnrollmentId = 1,
            //        MemberId = 2,
            //        SessionId = 1,

            //    },
            //    new Enrollment
            //    {
            //        EnrollmentId = 2,
            //        MemberId = 2,
            //        SessionId = 2
            //    }
            //   );
            //modelBuilder.Entity<OnlineSession>().HasData(
            //    new OnlineSession
            //    {
            //        SessionId = 1,
            //        Date = DateTime.Now,
            //        CourseId = 1,
            //        Status = SessionStatus.Scheduled,
            //        OnlineLink = "microsoft.com",
            //        AdditionalDetails = "Cameras are required"

            //    });
            //modelBuilder.Entity<OnPremSession>().HasData(
            //    new OnPremSession
            //    {
            //        SessionId = 2,
            //        Date = DateTime.Now,
            //        CourseId = 2,
            //        Status = SessionStatus.Scheduled,
            //        StreetNumber = "25A",
            //        StreetName = "Pitt Street",
            //        Suburb = "Sydney"

            //    });
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
    }
}
