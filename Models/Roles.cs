using System.ComponentModel.DataAnnotations;

namespace BackendProyecto.Models
{
    public class Roles
    {
        [Key]
        public int IdRol { get; set; }

        [Required(ErrorMessage = "El nombre del rol es obligatorio")]
        [MaxLength(60, ErrorMessage = "El rol no puede tener mas de 60 caracteres")]
        public string NombreRol { get; set; } = string.Empty;
    }
}
