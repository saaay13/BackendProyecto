using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendProyecto.Models
{
    public class Carnets
    {
        [Key]
        public int IdCarnet { get; set; }
        //

        [Required(ErrorMessage = "El usuario es obligatorio")]
        public int IdUsuario { get; set; }

        [ForeignKey(nameof(IdUsuario))]
        public Usuarios? Usuario { get; set; }
        //

        [Required(ErrorMessage = "La ONG es obligatoria")]
        public int IdOng { get; set; }

        [ForeignKey(nameof(IdOng))]
        public Ongs? Ong { get; set; }
        //

        [Required(ErrorMessage = "La fecha de emisión es obligatoria")]
        public DateTime FechaEmision { get; set; } = DateTime.Now;
        //

        [Required(ErrorMessage = "La fecha de vencimiento es obligatoria")]
        public DateTime FechaVencimiento { get; set; }
        //

        [Required(ErrorMessage = "Los beneficios son obligatorios")]
        [StringLength(500, ErrorMessage = "Los beneficios no pueden superar los 500 caracteres.")]
        public string Beneficios { get; set; } = string.Empty;
        //
        [Required(ErrorMessage = "El código de verificación es obligatorio.")]
        public Guid CodigoVerificacion { get; set; }
        //
       

        [Required(ErrorMessage = "El estado del carnet es obligatorio.")]
        public EstadoCarnetEnum EstadoInscripcion { get; set; } = EstadoCarnetEnum.Activo;

        public enum EstadoCarnetEnum { Activo, Suspendido, Vencido }
        [MaxLength(500)]
        public string? UrlCarnet { get; set; }
    }
}
