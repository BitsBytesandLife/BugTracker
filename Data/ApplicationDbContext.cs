using BugTracker.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace BugTracker.Data
{
    public class ApplicationDbContext : IdentityDbContext<BTUser>
    {

        private readonly IConfiguration Configuration;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration)
            : base(options)
        {
            Configuration = configuration;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseNpgsql(
                    DataUtility.GetConnectionString(Configuration),
            o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
        }
        public DbSet<BugTracker.Models.Company> Company { get; set; }
        public DbSet<BugTracker.Models.Invite> Invite { get; set; }
        public DbSet<BugTracker.Models.Notification> Notification { get; set; }
        public DbSet<BugTracker.Models.Project> Project { get; set; }
        public DbSet<BugTracker.Models.ProjectPriority> ProjectPriority { get; set; }
        public DbSet<BugTracker.Models.Ticket> Ticket { get; set; }
        public DbSet<BugTracker.Models.TicketAttachment> TicketAttachment { get; set; }
        public DbSet<BugTracker.Models.TicketComment> TicketComment { get; set; }
        public DbSet<BugTracker.Models.TicketHistory> TicketHistory { get; set; }
        public DbSet<BugTracker.Models.TicketPriority> TicketPriority { get; set; }
        public DbSet<BugTracker.Models.TicketStatus> TicketStatus { get; set; }
        public DbSet<BugTracker.Models.TicketType> TicketType { get; set; }
        public IEnumerable<object> Companies { get; internal set; }
    }
}
