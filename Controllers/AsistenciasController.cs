using BackendProyecto.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using  BackendProyecto.Models;


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
        //Get de Asistencias
        [HttpGet]
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
        public async Task<ActionResult<Asistencias>> GetAsistencia(int id)
        {
            var asistencia = await dBConexion.Asistencia.FindAsync(id);
            if (asistencia == null) return NotFound();
            return asistencia;
        }

        [HttpPost]
        public async Task<ActionResult<Asistencias>> PostAsistencia(Asistencias asistencia)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Datos invalidos");
            }

            var inscripcion = await dBConexion.Inscripcion.FindAsync(asistencia.IdInscripcion);
            if (inscripcion == null)
            {
                return BadRequest("La inscripcion no existe");
            }

            dBConexion.Asistencia.Add(asistencia);
            await dBConexion.SaveChangesAsync();

            return CreatedAtAction("GetAsistencia", new { id = asistencia.IdAsistencia }, asistencia);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsistencia(int id)
        {

            var asistencia = await dBConexion.Asistencia.FindAsync(id);
            if (asistencia== null)
            {
                return NotFound("Asistencia no encontrada");
            }

            dBConexion.Asistencia.Remove(asistencia);
            await dBConexion.SaveChangesAsync();

            return Ok($"Asistencia con Id {id} eliminada correctamente");



        }

    }
}