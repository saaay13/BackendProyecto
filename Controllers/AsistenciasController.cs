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
        [Authorize(Roles = "Administrador,Coordinador")]
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
        [Authorize(Roles = "Administrador,Coordinador")]
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
        [Authorize(Roles = "Voluntario")]
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
        [Authorize(Roles = "Administrador,Coordinador")]
        public async Task<ActionResult<Asistencias>> PostAsistencia(Asistencias asistencia)
        {
            if (!ModelState.IsValid)
                return BadRequest("Datos inválidos");

            // Verificar que la inscripción exista
            var inscripcion = await dBConexion.Inscripcion.FindAsync(asistencia.IdInscripcion);
            if (inscripcion == null)
                return BadRequest("La inscripción no existe");

            // Verificar si ya existe una asistencia para esta inscripción
            var yaRegistrada = await dBConexion.Asistencia
                .AnyAsync(a => a.IdInscripcion == asistencia.IdInscripcion);

            if (yaRegistrada)
                return BadRequest("La asistencia para esta inscripción ya fue registrada");

            // Agregar la asistencia
            dBConexion.Asistencia.Add(asistencia);
            await dBConexion.SaveChangesAsync();

            return CreatedAtAction("GetAsistencia", new { id = asistencia.IdAsistencia }, asistencia);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador,Coordinador")]
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

    }
}