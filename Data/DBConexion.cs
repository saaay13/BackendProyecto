using Microsoft.EntityFrameworkCore;
using BackendProyecto.Models;
namespace BackendProyecto.Data


{
    public class DBConexion : DbContext
    {
        public DBConexion(DbContextOptions options) : base(options) { }
        public DbSet<Usuarios> Usuario { get; set; }
        public DbSet<Roles> Rol { get; set; }
        public DbSet<UsuarioRol> UsuarioRol { get; set; }
        public DbSet<Ongs> Ong { get; set; }
        public DbSet<Proyectos> Proyecto { get; set; }
        public DbSet<Actividades> Actividad { get; set; }
        public DbSet<Inscripciones> Inscripcion { get; set; }
        public DbSet<Asistencias> Asistencia { get; set; }
        public DbSet<Certificados> Certificado { get; set; }
        public DbSet<Carnets> Carnet { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Certificados>()
                .HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Certificados>()
                .HasOne(c => c.Actividad)
                .WithMany()
                .HasForeignKey(c => c.IdActividad)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Inscripciones>()
                .HasOne(i => i.Usuario)
                .WithMany()
                .HasForeignKey(i => i.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inscripciones>()
                .HasOne(i => i.Actividad)
                .WithMany()
                .HasForeignKey(i => i.IdActividad)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<UsuarioRol>()
                .HasOne(ur => ur.Usuario)
                .WithMany() 
                .HasForeignKey(ur => ur.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UsuarioRol>()
                .HasOne(ur => ur.Rol)
                .WithMany() 
                .HasForeignKey(ur => ur.IdRol)
                .OnDelete(DeleteBehavior.Restrict);

        }

    }
}
