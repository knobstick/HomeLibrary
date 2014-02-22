﻿using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using KesselRun.HomeLibrary.EF.Repositories;
using KesselRun.HomeLibrary.Model;

namespace KesselRun.HomeLibrary.EF.Db
{
    public class HomeLibraryContext : EntitiesContext
    {
        //  DbSets go here
        public DbSet<Book> Books { get; set; }
        public DbSet<BookCover> BookCovers { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Lending> Lendings { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<Publisher> Publishers { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //  set up the Publisher's table
            modelBuilder.Entity<Publisher>().HasKey(p => p.Id);
            modelBuilder.Entity<Publisher>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<Publisher>().Property(p => p.Name).IsRequired().IsVariableLength();
            
            //  set up the People table
            modelBuilder.Entity<Person>().HasKey(p => p.Id);
            modelBuilder.Entity<Person>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<Person>().Property(p => p.Email).IsRequired().IsVariableLength();
            modelBuilder.Entity<Person>().Property(p => p.FirstName).IsRequired().IsVariableLength();
            modelBuilder.Entity<Person>().Property(p => p.LastName).IsRequired().IsVariableLength();
            modelBuilder.Entity<Person>().Property(p => p.Sobriquet).IsOptional().IsVariableLength();

            //  set up the Comment table
            modelBuilder.Entity<Comment>().HasKey(p => p.Id);
            modelBuilder.Entity<Comment>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<Comment>().Property(c => c.CommentText).IsRequired().IsVariableLength();

            //  set up the Book table
            modelBuilder.Entity<Book>().HasKey(p => p.Id);
            modelBuilder.Entity<Book>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<Book>().Property(c => c.Title).IsRequired().IsVariableLength();

            modelBuilder.Entity<BookCover>().HasKey(p => p.Id);
            modelBuilder.Entity<BookCover>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<Lending>().HasKey(p => p.Id);
            modelBuilder.Entity<Lending>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);


            base.OnModelCreating(modelBuilder);
        }
    }
}
