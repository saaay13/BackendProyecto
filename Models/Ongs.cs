using System.ComponentModel.DataAnnotations;

namespace BackendProyecto.Models
{
    public class Ongs
    {
        [Key]
        public int IdOng { get; set; }
        //
        [Required(ErrorMessage ="El nombre de la ONG es obligatorio")]
        [MaxLength(150)]
        public string NombreOng { get; set; }=string.Empty;
        //
        [MaxLength(300)]
        public string Descripcion { get; set; } = string.Empty;
        //
        [Required(ErrorMessage ="La direccion de la ONG es obligatoria")]
        [MaxLength(200)]
        public string Direccion { get; set; } = string.Empty;
        //
        [Phone(ErrorMessage = "El telefono no es valido")]
        [StringLength(20, ErrorMessage = "El telefono no puede superar los 20 caracteres")]
        [RegularExpression(@"^\+?[0-9]{1,4}\s?[0-9]{6,12}$",
        ErrorMessage = "El telefono debe tener formato +código espacio numero, solo digitos")]
        public string Telefono { get; set; } = string.Empty;//+591 77875506

    }
}
