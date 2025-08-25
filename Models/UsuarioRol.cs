using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendProyecto.Models
{
    [PrimaryKey(nameof(IdUsuario), nameof(IdRol))]
    public class UsuarioRol
    {
        public int IdUsuario { get; set; }
        [ForeignKey(nameof(IdUsuario))]
        public Usuarios? Usuario { get; set; }

        public int IdRol { get; set; }
        [ForeignKey(nameof(IdRol))]
        public Roles? Rol { get; set; } 

        public DateTime FechaAsignacion { get; set; } = DateTime.Now;
    }
}
