using System;
using System.Collections.Generic;
using System.Text;
using Endevrian.Models;
using Endevrian.Models.MapModels;
using Endevrian.Models.TagModels;
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

        public DbSet<SessionNote> SessionNotes { get; set; }

        public DbSet<SessionSection> SessionSections { get; set; }

        public DbSet<Map> Maps { get; set; }

        public DbSet<Tag> Tags { get; set; }

        public DbSet<TagRelation> TagRelations { get; set; }
    }
}
