using BSEUK.Models;
using Microsoft.EntityFrameworkCore;

namespace BSEUK.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<Paper> Papers { get; set; }
        public DbSet<Semester> Semesters { get; set; }
        public DbSet<StudentsMarksObtained> StudentsMarksObtaineds { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserAuth> UserAuths { get; set; }
        public DbSet<PaperType> PaperTypes{ get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

    }
}
