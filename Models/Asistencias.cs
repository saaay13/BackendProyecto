using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendProyecto.Models
{
    public class Asistencias
    {
        [Key]
        public int IdAsistencia { get; set; }
        //
        public int IdInscripcion { get; set; }
        [ForeignKey(nameof(IdInscripcion))]
        public Inscripciones? Inscripcion { get; set; }
        //
        [Required]
        public bool Asistio {  get; set; }
        //
        public string? Observacion { get; set; }
        //
        [Required]
        public DateTime HoraResgistro { get; set; } = DateTime.Now;
    }
}
