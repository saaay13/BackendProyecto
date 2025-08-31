using BackendProyecto.Data;
using BackendProyecto.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using static BackendProyecto.Models.Actividades;

namespace BackendProyecto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActividadesController : ControllerBase
    {
        private readonly DBConexion dBConexion;
        public ActividadesController(DBConexion dBConexion) => this.dBConexion = dBConexion;

        // ===================== DTOs =====================
        public record ActividadListDto(
            int IdActividad,
            int IdProyecto,
            string NombreProyecto,
            string NombreActividad,
            DateTime FechaActividad,
            TimeSpan HoraInicio,
            TimeSpan HoraFin,
            string Lugar,
            int CupoMaximo,
            EstadoActividadEnum EstadoActividad
        );

        public record ActividadDetalleDto(
            int IdActividad,
            int IdProyecto,
            string NombreActividad,
            DateTime FechaActividad,
            TimeSpan HoraInicio,
            TimeSpan HoraFin,
            string Lugar,
            int CupoMaximo,
            EstadoActividadEnum EstadoActividad
        );

        public class ActividadInput
        {
            [Required] public int IdProyecto { get; set; }

            [Required, MaxLength(150, ErrorMessage = "Máximo 150 caracteres")]
            public string? NombreActividad { get; set; }

            [Required, DataType(DataType.Date)]
            public DateTime FechaActividad { get; set; }

            [Required, DataType(DataType.Time)]
            public TimeSpan HoraInicio { get; set; }

            [Required, DataType(DataType.Time)]
            public TimeSpan HoraFin { get; set; }

            [Required, MaxLength(200)]
            public string? Lugar { get; set; }

            [Required, Range(1, int.MaxValue)]
            public int CupoMaximo { get; set; }

            [Required]
            public EstadoActividadEnum EstadoActividad { get; set; } = EstadoActividadEnum.EnCurso;
        }

        // ===================== LISTA (Admin/Coord) =====================
        [HttpGet]
        [Authorize(Roles = "Administrador,Coordinador")]
        public async Task<ActionResult<IEnumerable<ActividadListDto>>> GetActividades()
        {
            var lista = await dBConexion.Actividad
                .Include(a => a.Proyecto)
                .OrderBy(a => a.FechaActividad)
                .Select(a => new ActividadListDto(
                    a.IdActividad,
                    a.IdProyecto,
                    a.Proyecto != null ? a.Proyecto.NombreProyecto : string.Empty,
                    a.NombreActividad ?? string.Empty,
                    a.FechaActividad,
                    a.HoraInicio,
                    a.HoraFin,
                    a.Lugar ?? string.Empty,
                    a.CupoMaximo,
                    a.EstadoActividad
                ))
                .ToListAsync();

            return Ok(lista);
        }

        // ===================== DETALLE (Admin/Coord) =====================
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Administrador,Coordinador")]
        public async Task<ActionResult<ActividadDetalleDto>> GetActividad(int id)
        {
            var a = await dBConexion.Actividad
                .Where(x => x.IdActividad == id)
                .Select(x => new ActividadDetalleDto(
                    x.IdActividad,
                    x.IdProyecto,
                    x.NombreActividad ?? string.Empty,
                    x.FechaActividad,
                    x.HoraInicio,
                    x.HoraFin,
                    x.Lugar ?? string.Empty,
                    x.CupoMaximo,
                    x.EstadoActividad
                ))
                .FirstOrDefaultAsync();

            if (a is null) return NotFound("Actividad no encontrada.");
            return Ok(a);
        }

        // ===================== LISTA PÚBLICA (visitantes) =====================
        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<object>>> GetActividadesPublic()
        {
            var actividades = await dBConexion.Actividad
                .Include(a => a.Proyecto).ThenInclude(p => p.Responsable)
                .Include(a => a.Proyecto).ThenInclude(p => p.Ong)
                .OrderBy(a => a.FechaActividad)
                .ToListAsync();

            var resultado = actividades.Select(a => new
            {
                a.IdActividad,
                a.NombreActividad,
                a.FechaActividad,
                a.HoraInicio,
                a.HoraFin,
                a.Lugar,
                a.CupoMaximo,
                a.EstadoActividad,
                Proyecto = a.Proyecto == null ? null : new
                {
                    a.Proyecto.IdProyecto,
                    a.Proyecto.NombreProyecto,
                    Responsable = a.Proyecto.Responsable == null ? null : new
                    {
                        a.Proyecto.Responsable.Nombre
                    },
                    Ong = a.Proyecto.Ong == null ? null : new
                    {
                        a.Proyecto.Ong.NombreOng
                    }
                }
            });

            return Ok(resultado);
        }

        // ===================== CREAR =====================
        [HttpPost]
        [Authorize(Roles = "Administrador,Coordinador")]
        public async Task<ActionResult> PostActividad([FromBody] ActividadInput input)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Validar proyecto
            var proyectoExiste = await dBConexion.Proyecto.AnyAsync(p => p.IdProyecto == input.IdProyecto);
            if (!proyectoExiste) return BadRequest("El proyecto no existe.");

            // Reglas de negocio
            if (input.HoraFin <= input.HoraInicio)
                return BadRequest("La hora de fin debe ser mayor a la hora de inicio.");

            var duplicado = await dBConexion.Actividad.AnyAsync(p =>
                p.IdProyecto == input.IdProyecto &&
                p.NombreActividad == input.NombreActividad);
            if (duplicado)
                return BadRequest("Ya existe una actividad con ese nombre en el mismo proyecto.");

            var entidad = new Actividades
            {
                IdProyecto = input.IdProyecto,
                NombreActividad = input.NombreActividad,
                FechaActividad = input.FechaActividad.Date,
                HoraInicio = input.HoraInicio,
                HoraFin = input.HoraFin,
                Lugar = input.Lugar,
                CupoMaximo = input.CupoMaximo,
                EstadoActividad = input.EstadoActividad
            };

            dBConexion.Actividad.Add(entidad);
            await dBConexion.SaveChangesAsync();

            return CreatedAtAction(nameof(GetActividad), new { id = entidad.IdActividad }, new { entidad.IdActividad });
        }

        // ===================== EDITAR =====================
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Administrador,Coordinador")]
        public async Task<IActionResult> PutActividad(int id, [FromBody] ActividadInput input)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var entidad = await dBConexion.Actividad.FindAsync(id);
            if (entidad is null) return NotFound("Actividad no encontrada.");

            var proyectoExiste = await dBConexion.Proyecto.AnyAsync(p => p.IdProyecto == input.IdProyecto);
            if (!proyectoExiste) return BadRequest("El proyecto no existe.");

            if (input.HoraFin <= input.HoraInicio)
                return BadRequest("La hora de fin debe ser mayor a la hora de inicio.");

            var nombreDuplicado = await dBConexion.Actividad.AnyAsync(p =>
                p.IdActividad != id &&
                p.IdProyecto == input.IdProyecto &&
                p.NombreActividad == input.NombreActividad);
            if (nombreDuplicado)
                return BadRequest("Ya existe otra actividad con ese nombre en el mismo proyecto.");

            entidad.IdProyecto = input.IdProyecto;
            entidad.NombreActividad = input.NombreActividad;
            entidad.FechaActividad = input.FechaActividad.Date;
            entidad.HoraInicio = input.HoraInicio;
            entidad.HoraFin = input.HoraFin;
            entidad.Lugar = input.Lugar;
            entidad.CupoMaximo = input.CupoMaximo;
            entidad.EstadoActividad = input.EstadoActividad;

            await dBConexion.SaveChangesAsync();
            return NoContent();
        }

        // ===================== ELIMINAR =====================
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrador,Coordinador")]
        public async Task<IActionResult> DeleteActividad(int id)
        {
            var actividad = await dBConexion.Actividad.FindAsync(id);
            if (actividad is null) return NotFound("Actividad no encontrada.");

            dBConexion.Actividad.Remove(actividad);
            await dBConexion.SaveChangesAsync();

            return Ok($"Actividad con Id {id} eliminada correctamente.");
        }
    }
}
