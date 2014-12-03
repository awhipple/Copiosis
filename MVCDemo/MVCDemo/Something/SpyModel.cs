/* This is the entity model that was generated via Entity Framework and doing CodeFirst generation */

namespace MVCDemo.Something
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class SpyModel : DbContext
    {
        public SpyModel()
            : base("name=Spy")
        {
        }

        public virtual DbSet<Affiliation> Affiliations { get; set; }
        public virtual DbSet<AffiliationRel> AffiliationRels { get; set; }
        public virtual DbSet<Agent> Agents { get; set; }
        public virtual DbSet<Language> Languages { get; set; }
        public virtual DbSet<Mission> Missions { get; set; }
        public virtual DbSet<SecurityClearance> SecurityClearances { get; set; }
        public virtual DbSet<Skill> Skills { get; set; }
        public virtual DbSet<Team> Teams { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Affiliation>()
                .Property(e => e.title)
                .IsUnicode(false);

            modelBuilder.Entity<Affiliation>()
                .Property(e => e.description)
                .IsUnicode(false);

            modelBuilder.Entity<Affiliation>()
                .HasMany(e => e.AffiliationRels)
                .WithRequired(e => e.Affiliation)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AffiliationRel>()
                .Property(e => e.affiliation_strength)
                .IsUnicode(false);

            modelBuilder.Entity<Agent>()
                .Property(e => e.first)
                .IsUnicode(false);

            modelBuilder.Entity<Agent>()
                .Property(e => e.middle)
                .IsUnicode(false);

            modelBuilder.Entity<Agent>()
                .Property(e => e.last)
                .IsUnicode(false);

            modelBuilder.Entity<Agent>()
                .Property(e => e.address)
                .IsUnicode(false);

            modelBuilder.Entity<Agent>()
                .Property(e => e.city)
                .IsUnicode(false);

            modelBuilder.Entity<Agent>()
                .Property(e => e.country)
                .IsUnicode(false);

            modelBuilder.Entity<Agent>()
                .HasMany(e => e.AffiliationRels)
                .WithRequired(e => e.Agent)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Agent>()
                .HasMany(e => e.Languages)
                .WithMany(e => e.Agents)
                .Map(m => m.ToTable("LanguageRel").MapLeftKey("agent_id").MapRightKey("lang_id"));

            modelBuilder.Entity<Agent>()
                .HasMany(e => e.Skills)
                .WithMany(e => e.Agents)
                .Map(m => m.ToTable("SkillRel").MapLeftKey("agent_id").MapRightKey("skill_id"));

            modelBuilder.Entity<Agent>()
                .HasMany(e => e.Teams)
                .WithMany(e => e.Agents)
                .Map(m => m.ToTable("TeamRel").MapLeftKey("agent_id").MapRightKey("team_id"));

            modelBuilder.Entity<Language>()
                .Property(e => e.language1)
                .IsUnicode(false);

            modelBuilder.Entity<Mission>()
                .Property(e => e.name)
                .IsUnicode(false);

            modelBuilder.Entity<Mission>()
                .Property(e => e.mission_status)
                .IsUnicode(false);

            modelBuilder.Entity<SecurityClearance>()
                .Property(e => e.sc_level)
                .IsUnicode(false);

            modelBuilder.Entity<SecurityClearance>()
                .Property(e => e.description)
                .IsUnicode(false);

            modelBuilder.Entity<SecurityClearance>()
                .HasMany(e => e.Agents)
                .WithOptional(e => e.SecurityClearance)
                .HasForeignKey(e => e.clearance_id);

            modelBuilder.Entity<SecurityClearance>()
                .HasMany(e => e.Missions)
                .WithOptional(e => e.SecurityClearance)
                .HasForeignKey(e => e.access_id);

            modelBuilder.Entity<Skill>()
                .Property(e => e.skill1)
                .IsUnicode(false);

            modelBuilder.Entity<Team>()
                .Property(e => e.name)
                .IsUnicode(false);

            modelBuilder.Entity<Team>()
                .Property(e => e.meeting_frequency)
                .IsUnicode(false);

            modelBuilder.Entity<User>()
                .Property(e => e.LastName)
                .IsUnicode(false);

            modelBuilder.Entity<User>()
                .Property(e => e.FirstName)
                .IsUnicode(false);

            modelBuilder.Entity<User>()
                .Property(e => e.UserName)
                .IsUnicode(false);
        }
    }
}
