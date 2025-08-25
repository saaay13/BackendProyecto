using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BackendProyecto.Models
{
    public class Actividades
    {
        [Key]
        public int IdActividad { get; set; }
        //
        public int IdProyecto { get; set; }
        [ForeignKey(nameof(IdProyecto))]
        public Proyectos? Proyecto { get; set; }
        //
        [Required(ErrorMessage ="El nombre del proyecto es obligatorio ")]
        [MaxLength(150)]
        public string? NombreActividad { get; set; }
        // 
        [Required(ErrorMessage = "La fecha de la actividad es obligatoria")]
        [DataType(DataType.Date)]
        public DateTime FechaActividad { get; set; }
        //

        [Required(ErrorMessage = "La hora de inicio es obligatoria")]
        [DataType(DataType.Time)]
        public TimeSpan HoraInicio { get; set; }
        //

        [Required(ErrorMessage = "La hora de finalizacion es obligatoria")]
        [DataType(DataType.Time)]
        public TimeSpan HoraFin { get; set; }
        //
        [Required(ErrorMessage ="El Lugar de realizacion de la actividad es obligatoria")]
        [MaxLength(200)]
        public string? Lugar {  get; set; }
        //
        [Required(ErrorMessage ="El limite de cupos es obligatorio")]
        [Range(1,int.MaxValue,ErrorMessage ="El limite de cupos debe ser mayor que 0")]
        public int CupoMaximo { get; set; }
        //
        public enum EstadoActividadEnum { Planificada, EnCurso, Completada, Cancelada }
        public EstadoActividadEnum EstadoActividad { get; set; } = EstadoActividadEnum.EnCurso;



    }
}
