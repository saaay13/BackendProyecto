using BackendProyecto.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendProyecto.Models;

namespace BackendProyecto.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InscripcionesController : ControllerBase
    {
        private readonly DBConexion dBConexion;

        public InscripcionesController(DBConexion dBConexion)
        {
            this.dBConexion = dBConexion;
        }

        // GET: api/Inscripciones
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Inscripciones>>> GetInscripciones()
        {
            var inscripciones = await dBConexion.Inscripcion
                .Include(i => i.Usuario)
                .Include(i => i.Actividad)
                    .ThenInclude(a => a.Proyecto)
                        .ThenInclude(p => p.Responsable)
                .Include(i => i.Actividad)
                    .ThenInclude(a => a.Proyecto)
                        .ThenInclude(p => p.Ong)
                .ToListAsync();

            return Ok(inscripciones);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Inscripciones>> GetInscripcion(int id)
        {
            var inscripcion = await dBConexion.Inscripcion
                .Include(i => i.Usuario)
                .Include(i => i.Actividad)
                    .ThenInclude(a => a.Proyecto)
                        .ThenInclude(p => p.Responsable)
                .Include(i => i.Actividad)
                    .ThenInclude(a => a.Proyecto)
                        .ThenInclude(p => p.Ong)
                .FirstOrDefaultAsync(i => i.IdInscripcion == id);

            if (inscripcion == null)
                return NotFound();

            return Ok(inscripcion);
        }
        [HttpPost]
        public async Task<ActionResult<Inscripciones>> PostInscripcion(Inscripciones inscripcion)
        {
            if (!ModelState.IsValid)
                return BadRequest("Datos invalidos");
            if (inscripcion.IdActividad == inscripcion.IdUsuario)
            {
                return BadRequest("El Usuario ya se registro en esa actividad");
            }

            var usuario = await dBConexion.Usuario.FindAsync(inscripcion.IdUsuario);
            if (usuario == null)
                return BadRequest("El usuario no existe");

            var actividad = await dBConexion.Actividad.FindAsync(inscripcion.IdActividad);
            if (actividad == null)
                return BadRequest("La actividad no existe");

            var inscritos = await dBConexion.Inscripcion
                .CountAsync(i => i.IdActividad == inscripcion.IdActividad
                                && i.EstadoInscripcion == Inscripciones.EstadoInscripcionEnum.Confirmada);

            if (inscritos >= actividad.CupoMaximo)
                return BadRequest("No hay cupos disponibles para esta actividad");

            inscripcion.EstadoInscripcion = Inscripciones.EstadoInscripcionEnum.Confirmada;

            dBConexion.Inscripcion.Add(inscripcion);

            
            actividad.CupoMaximo--; //Reduccion
            dBConexion.Actividad.Update(actividad);//Actualizacion

            await dBConexion.SaveChangesAsync();

            return CreatedAtAction("GetInscripcion", new { id = inscripcion.IdInscripcion }, inscripcion);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIncripcion(int id)
        {

            var incripcion = await dBConexion.Inscripcion.FindAsync(id);
            if (incripcion == null)
            {
                return NotFound("Inscripcion no encontrada");
            }

            dBConexion.Inscripcion.Remove(incripcion);
            await dBConexion.SaveChangesAsync();

            return Ok($"Inscripcion con Id {id} eliminado correctamente");


        }

    }
}
