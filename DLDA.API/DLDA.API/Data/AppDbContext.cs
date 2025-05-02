using DLDA.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DLDA.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Assessment> Assessments { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<AssessmentItem> AssessmentItems { get; set; }
    }
}
