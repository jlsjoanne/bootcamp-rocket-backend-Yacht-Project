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

        public DbSet<UploadedFile> UploadedFiles { get; set; }
        public DbSet<Contact> Contacts { get; set; }

        public DbSet<News> News { get; set; }
        public DbSet<NewsFile> NewsFiles { get; set; }
        public DbSet<NewsImage> NewsImages { get; set; }

        public DbSet<Yacht> Yachts { get; set; }
        public DbSet<YachtDownload> YachtDownloads { get; set; }
        public DbSet<YachtImage> YachtImages { get; set; }
        public DbSet<YachtInterior> YachtInteriors { get; set; }
        
        
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UploadedFile>()
                .Map(m =>
                {
                    m.ToTable("UploadedFiles");
                });

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
        }
    }
}
