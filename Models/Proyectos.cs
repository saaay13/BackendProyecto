using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendProyecto.Models
{
    public class Proyectos
    {
        [Key]
        public int IdProyecto { get; set; }
        //

        public int IdOng {  get; set; }
        [ForeignKey(nameof(IdOng))]
        public Ongs? Ong { get; set; }
        //
        [Required(ErrorMessage ="El nombre del proyecto es obligatorio ")]
        [MaxLength(150)]
        public string? NombreProyecto { get; set; }
        //
        [Required(ErrorMessage = "La descripcion del proyecto es obligatoria ")]
        [MaxLength(500)]
        public string? Descripcion { get; set; }
        //
        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        [DataType(DataType.Date)]
        public DateTime FechaInicio { get; set; }
        //
        [Required(ErrorMessage ="La fecha de finalizcion es obligatoria")]
        [DataType(DataType.Date)]
        public DateTime FechaFin {  get; set; }
        //
        public enum EstadoProyectoEnum { Activo, Finalizado, Cancelado }
        public EstadoProyectoEnum EstadoProyecto { get; set; } = EstadoProyectoEnum.Activo;

        //
        public int IdResponsable { get; set; }
        [ForeignKey(nameof(IdResponsable))]//Condicion que el responsale tenga un rol de coordinador para arriba
        public Usuarios? Responsable { get; set; }
    }
}
