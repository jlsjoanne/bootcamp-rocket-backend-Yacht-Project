using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using TayanaYachts.Models;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace TayanaYachts.DAL
{
    public partial class TayanaContext : DbContext
    {
        public TayanaContext()
            : base("name=TayanaContext")
        {
        }

        public DbSet<Country> Countries { get; set; }
        public DbSet<Area> Areas { get; set; }
        public DbSet<Dealer> Dealers { get; set; }
        public DbSet<DealerImage> DealerImages { get; set; }

        public DbSet<Contact> Contacts { get; set; }

        public DbSet<News> News { get; set; }
        public DbSet<NewsFile> NewsFiles { get; set; }
        public DbSet<NewsImage> NewsImages { get; set; }

        public DbSet<Yacht> Yachts { get; set; }
        public DbSet<YachtDownload> YachtDownloads { get; set; }
        public DbSet<YachtImage> YachtImages { get; set; }
        public DbSet<YachtInterior> YachtInteriors { get; set; }
        public DbSet<YachtEditorImage> YachtEditorImages { get; set; }

        public DbSet<YachtHeroImage> YachtHeroImages { get; set; }

        public DbSet<Member> Members { get; set; }
        
        
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<News>()
                .HasMany(n => n.Images)
                .WithRequired(i => i.News)
                .HasForeignKey(i => i.NewsId)
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<News>()
                .HasOptional(n => n.ThumbnailImage)
                .WithMany()
                .HasForeignKey(n => n.ThumbnailImageId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<NewsFile>()
                .Map(m =>
                {
                    m.MapInheritedProperties();
                    m.ToTable("NewsFiles");
                });
            modelBuilder.Entity<NewsImage>()
                .Map(m =>
                {
                    m.MapInheritedProperties();
                    m.ToTable("NewsImages");
                });
            modelBuilder.Entity<YachtDownload>()
                .Map(m =>
                {
                    m.MapInheritedProperties();
                    m.ToTable("YachtDownloads");
                });
            modelBuilder.Entity<YachtImage>()
                .Map(m =>
                {
                    m.MapInheritedProperties();
                    m.ToTable("YachtImages");
                });
            modelBuilder.Entity<YachtInterior>()
                .Map(m =>
                {
                    m.MapInheritedProperties();
                    m.ToTable("YachtInteriors");
                });
            modelBuilder.Entity<YachtEditorImage>()
                .Map(m =>
                {
                    m.MapInheritedProperties();
                    m.ToTable("YachtEditorImages");
                });
            modelBuilder.Entity<Dealer>()
                .HasRequired(d => d.Image)
                .WithRequiredPrincipal(i => i.Dealer)
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<YachtHeroImage>()
                .Map(m =>
                {
                    m.MapInheritedProperties();
                    m.ToTable("YachtHeroImages");
                });
            modelBuilder.Entity<YachtHeroImage>()
                .HasRequired(h => h.Yacht)
                .WithMany()
                .HasForeignKey(h => h.YachtId)
                .WillCascadeOnDelete(true);
            base.OnModelCreating(modelBuilder);
        }
    }
}
