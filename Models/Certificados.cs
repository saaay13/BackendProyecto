using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendProyecto.Models
{
    public class Certificados
    {
        [Key]
        public int IdCertificado {  get; set; }
        //
        public int IdUsuario { get; set; }
        [ForeignKey(nameof(IdUsuario))]
        public Usuarios? Usuario { get; set; }
        //
        public int IdActividad { get; set; }
        [ForeignKey(nameof(IdActividad))]
        public Actividades? Actividad { get; set; }
        //
        [Required]
        public DateTime FechaEmision { get; set; }=DateTime.Now;
        //
        [Required]
        [MaxLength(36)]
        public string CodigoVerificacion { get; set; }=Guid.NewGuid().ToString();
        [MaxLength(500)]
        public string? UrlCertificado { get; set; }
    }
}
