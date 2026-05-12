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

        public DbSet<Area> Areas { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Dealer> Dealers { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<UploadedFile> UploadedFiles { get; set; }
        public DbSet<Yacht> Yachts { get; set; }
        
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();


        }
    }
}
