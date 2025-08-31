using BackendProyecto.Data;
using BackendProyecto.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace BackendProyecto.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AsistenciasController : ControllerBase
    {
        private readonly DBConexion dBConexion;

        public AsistenciasController(DBConexion dBConexion)
        {
            this.dBConexion = dBConexion;
        }


        [HttpGet]
       // [Authorize(Roles = "Administrador,Coordinador")]
        public async Task<ActionResult<IEnumerable<Asistencias>>> GetAsitencias()
        {
            var asistencias = await dBConexion.Asistencia
                  .Include(a => a.Inscripcion)
                      .ThenInclude(i => i.Usuario)
                  .Include(a => a.Inscripcion)
                      .ThenInclude(i => i.Actividad)
                          .ThenInclude(act => act.Proyecto)
                              .ThenInclude(p => p.Responsable)
                  .Include(a => a.Inscripcion)
                      .ThenInclude(i => i.Actividad)
                          .ThenInclude(act => act.Proyecto)
                              .ThenInclude(p => p.Ong)
                  .ToListAsync();

            return asistencias;
        }
        [HttpGet("{id}")]
        //[Authorize(Roles = "Administrador,Coordinador")]
        public async Task<ActionResult<Asistencias>> GetAsistencia(int id)
        {
            var asistencia = await dBConexion.Asistencia
                .Include(a => a.Inscripcion)
                    .ThenInclude(i => i.Usuario)
                .Include(a => a.Inscripcion)
                    .ThenInclude(i => i.Actividad)
                        .ThenInclude(act => act.Proyecto)
                            .ThenInclude(p => p.Responsable)
                .Include(a => a.Inscripcion)
                    .ThenInclude(i => i.Actividad)
                        .ThenInclude(act => act.Proyecto)
                            .ThenInclude(p => p.Ong)
                .FirstOrDefaultAsync(a => a.IdAsistencia == id);
            if (asistencia == null) return NotFound();
            return asistencia;
        }
        [HttpGet("mis-asistencias/{idUsuario}")]
        //[Authorize(Roles = "Voluntario")]
        public async Task<ActionResult<IEnumerable<object>>> GetMisAsistencias(int idUsuario)
        {
            var asistencias = await dBConexion.Asistencia
                .Include(a => a.Inscripcion)
                    .ThenInclude(i => i.Actividad)
                .Where(a => a.Inscripcion.IdUsuario == idUsuario)
                .Select(a => new
                {
                    a.Inscripcion.Actividad.NombreActividad,
                    a.Inscripcion.Actividad.FechaActividad,
                    a.Inscripcion.Actividad.HoraInicio,
                    a.Inscripcion.Actividad.HoraFin,
                    a.Inscripcion.Actividad.Lugar,
                    a.IdAsistencia
                })
                .ToListAsync();

            return Ok(asistencias);
        }


        [HttpPost]
        //[Authorize(Roles = "Administrador,Coordinador")]
        public async Task<ActionResult<Asistencias>> PostAsistencia(Asistencias asistencia)
        {
            if (!ModelState.IsValid) return BadRequest("Datos inválidos");

            var inscripcion = await dBConexion.Inscripcion.FindAsync(asistencia.IdInscripcion);
            if (inscripcion == null) return BadRequest("La inscripción no existe");

            // Registrar (permitiendo múltiples registros)
            asistencia.HoraResgistro = asistencia.HoraResgistro == default ? DateTime.Now : asistencia.HoraResgistro;

            dBConexion.Asistencia.Add(asistencia);
            await dBConexion.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAsistencia), new { id = asistencia.IdAsistencia }, asistencia);
        }


        [HttpDelete("{id}")]
       // [Authorize(Roles = "Administrador,Coordinador")]
        public async Task<IActionResult> DeleteAsistencia(int id)
        {

            var asistencia = await dBConexion.Asistencia.FindAsync(id);
            if (asistencia == null)
            {
                return NotFound("Asistencia no encontrada");
            }

            dBConexion.Asistencia.Remove(asistencia);
            await dBConexion.SaveChangesAsync();

            return Ok($"Asistencia con Id {id} eliminada correctamente");



        }
        // GET: api/Asistencias/por-inscripcion/999
        [HttpGet("por-inscripcion/{idInscripcion:int}")]
        //[Authorize(Roles = "Administrador,Coordinador")]
        public async Task<ActionResult<IEnumerable<object>>> GetPorInscripcion(int idInscripcion)
        {
            var existe = await dBConexion.Inscripcion.AnyAsync(i => i.IdInscripcion == idInscripcion);
            if (!existe) return NotFound("Inscripción no existe.");

            var lista = await dBConexion.Asistencia
                .Where(a => a.IdInscripcion == idInscripcion)
                .OrderBy(a => a.HoraResgistro)
                .Select(a => new
                {
                    a.IdAsistencia,
                    a.IdInscripcion,
                    a.Asistio,
                    a.Observacion,
                    a.HoraResgistro
                })
                .ToListAsync();

            return Ok(lista);
        }
        // PUT: api/Asistencias/123
        [HttpPut("{id:int}")]
        //[Authorize(Roles = "Administrador,Coordinador")]
        public async Task<IActionResult> PutAsistencia(int id, [FromBody] Asistencias dto)
        {
            var entity = await dBConexion.Asistencia.FindAsync(id);
            if (entity is null) return NotFound("Asistencia no encontrada.");

            // Solo campos editables
            entity.Asistio = dto.Asistio;
            entity.Observacion = dto.Observacion;
            // entity.HoraResgistro = dto.HoraResgistro; // si quieres permitirlo

            await dBConexion.SaveChangesAsync();
            return NoContent();
        }


    }
}