using System;
using System.Collections.Generic;
using System.Text;
using Endevrian.Models;
using Endevrian.Models.MapModels;
using Endevrian.Models.TagModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Endevrian.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            SqliteConnectionStringBuilder connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = "ApplicationDbContext.db" };
            string connectionString = connectionStringBuilder.ToString();
            SqliteConnection connection = new SqliteConnection(connectionString);

            optionsBuilder.UseSqlite(connection);
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
