using System;
using FilRouge.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Win32;

namespace FilRouge.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityUserLogin<string>>().HasKey(l => new { l.LoginProvider, l.ProviderKey });

            modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys())
                .ToList()
                .ForEach(fk => fk.DeleteBehavior = DeleteBehavior.Restrict);

            modelBuilder.Entity<Register>()
                .HasOne(r => r.Dog)
                .WithMany() // ← ici on ne précise PAS de propriété côté Dog
                .HasForeignKey(r => r.DogId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Register>()
                .HasOne(r => r.Course)
                .WithMany()
                .HasForeignKey(r => r.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Participate>()
                .HasOne(p => p.Dog)
                .WithMany()
                .HasForeignKey(p => p.DogId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Participate>()
                .HasOne(p => p.Session)
                .WithMany()
                .HasForeignKey(p => p.SessionId)
                .OnDelete(DeleteBehavior.NoAction);
        }


        public DbSet<Person> Persons { get; set; }

        public DbSet<Dog> Dogs { get; set; }

        public DbSet<Course> Courses { get; set; }

        public DbSet<Session> Sessions { get; set; }

        public DbSet<Breed> Breeds { get; set; }

        public DbSet<Register> Registers { get; set; }

        public DbSet<Participate> Participations { get; set; }

        public DbSet<Concerns> Concerns { get; set; }

    }
}

