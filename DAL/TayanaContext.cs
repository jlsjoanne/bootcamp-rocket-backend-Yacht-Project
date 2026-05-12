using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace TayanaYachts.DAL
{
    public partial class TayanaContext : DbContext
    {
        public TayanaContext()
            : base("name=TayanaContext")
        {
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
