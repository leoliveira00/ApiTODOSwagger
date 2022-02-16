using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiTODOSwagger.Data;

namespace ApiTODOSwagger.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        {
        }
        
        public DbSet<Pessoa> Pessoas { get; set; }
        public DbSet<Tarefa> Tarefas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            /*
             * Seta a precisão máxima para as propriedade Pessoa.Nome e Tarefa.FilePath
             **/
            modelBuilder.Entity<Pessoa>()
                .Property(p => p.Nome)
                    .HasMaxLength(100);

            modelBuilder.Entity<Tarefa>()
                .Property(p => p.FilePath)
                    .HasMaxLength(500);

            /*
             * Configura a foreigh key 
             **/
            modelBuilder.Entity<Tarefa>()
                .HasOne(t => t.Pessoa)
                    .WithMany(c => c.Tarefas);

            /*
             * Cria por default duas pessoas quando da migração
             **/
            modelBuilder.Entity<Pessoa>().HasData
            (
                new Pessoa { Id = 1, Nome = "João Silva" },
                new Pessoa { Id = 2, Nome = "Ana Silva" }
            ); 
        }
    }
}
