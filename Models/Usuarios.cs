
using System.ComponentModel.DataAnnotations;

namespace BackendProyecto.Models
{
    public class Usuarios
    {
        [Key]
        public int IdUsuario { get; set; }
        //

        [Required(ErrorMessage="El nombre del usuario es obligatorio")]
        [MinLength(3,ErrorMessage ="El nombre debe tener como minimo 3 caracteres")]
        public string Nombre { get; set; } = string.Empty;
        //

        [Required(ErrorMessage="El apellido del usuario es obligatorio")]
        [MinLength(3, ErrorMessage = "El apellido debe tener como minimo 3 caracteres")]
        public string Apellido{ get; set; } = string.Empty;

        [Required(ErrorMessage="El correo es obligatorio")]
        [EmailAddress(ErrorMessage="El correo no es valido")]
        [MaxLength(150)]
        public string CorreoUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage="La contraseña es obligatoria")]
        [MinLength(6, ErrorMessage="La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; }=string.Empty;

        [Phone(ErrorMessage="El telefono no es valido")]
        [StringLength(20, ErrorMessage = "El telefono no puede superar los 20 caracteres")]
        [RegularExpression(@"^\+?[0-9]{1,4}\s?[0-9]{6,12}$", 
        ErrorMessage = "El telefono debe tener formato +código espacio numero, solo digitos")]
        public string Telefono { get; set; } = string.Empty;//+591 77875506
        [Required]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

    }
}
