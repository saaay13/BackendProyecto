using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendProyecto.Models
{
    public class Inscripciones
    {
        [Key]
        public int IdInscripcion {  get; set; }
        //
        public int IdUsuario { get; set; }
        [ForeignKey(nameof(IdUsuario))]
        public Usuarios? Usuario { get; set; }
        //
        public int IdActividad { get; set; }
        [ForeignKey(nameof(IdActividad))]
        public Actividades? Actividad { get; set; }
        //
        public DateTime FechaInscripcion { get; set; } = DateTime.Now;
        //
        public enum EstadoInscripcionEnum { Pendiente, Confirmada, Cancelada }
        public EstadoInscripcionEnum EstadoInscripcion { get; set; } = EstadoInscripcionEnum.Pendiente;


    }
}
