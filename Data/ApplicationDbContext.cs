using System;
using System.Collections.Generic;
using System.Text;
using Endevrian.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Endevrian.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<AdventureLog> AdventureLogs { get; set; }

        public DbSet<HistoricalAdventureLogCount> HistoricalAdventureLogCounts { get; set; }

        public DbSet<SystemLog> SystemLogs { get; set; }

        public DbSet<Campaign> Campaigns { get; set; }
    }
}
