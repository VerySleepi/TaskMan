using TaskMan.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace TaskMan.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        public DbSet<User> User { get; set; }
        public DbSet<Models.Task> Task { get; set; }
        public DbSet<UserTasks> UserTasks { get; set; }
        public DbSet<Group> Group { get; set; }
        public DbSet<GroupList> GroupList { get; set; }
        public DbSet<GroupUsers> GroupUsers { get; set; }
        public DbSet<GroupTask> GroupTask { get; set; }
    }
}
