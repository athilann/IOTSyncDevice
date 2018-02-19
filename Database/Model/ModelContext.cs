using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Database.Model
{
    public class ModelContext : DbContext
    {
        public DbSet<Registre> Registres { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=registre.db");
        }
    }

    public class Registre
    {
        public int Id { get; set; }
        public DateTime RegistreDate { get; set; }

    }
}
