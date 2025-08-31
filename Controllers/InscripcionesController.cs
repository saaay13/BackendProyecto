using BackendProyecto.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendProyecto.Models;
using Microsoft.AspNetCore.Authorization;

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
        //[Authorize(Roles = "Administrador,Coordinador")]
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
        //[Authorize(Roles = "Administrador")]
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
        [HttpGet("actividad/{idActividad}/compañeros/{idUsuario}")]
        //[Authorize(Roles = "Administrador,Coordinador,Voluntario")]
        public async Task<ActionResult<IEnumerable<object>>> GetCompanerosActividad(int idActividad, int idUsuario)
        {

            var actividad = await dBConexion.Actividad.FindAsync(idActividad);
            if (actividad == null)
                return NotFound("La actividad no existe");


            var companeros = await dBConexion.Inscripcion
                .Where(i => i.IdActividad == idActividad && i.IdUsuario != idUsuario)
                .Include(i => i.Usuario)
                .Select(i => new
                {
                    i.Usuario.Nombre,
                    i.Usuario.Apellido
                })
                .ToListAsync();

            return Ok(companeros);
        }
        [HttpGet("actividad/{idActividad}/usuarios")]
       // [Authorize(Roles = "Administrador,Coordinador")]
        public async Task<ActionResult<IEnumerable<object>>> GetUsuariosPorActividad(int idActividad)
        {

            var actividad = await dBConexion.Actividad.FindAsync(idActividad);
            if (actividad == null)
                return NotFound("La actividad no existe");

            var usuarios = await dBConexion.Inscripcion
                .Where(i => i.IdActividad == idActividad)
                .Include(i => i.Usuario)
                .Select(i => new
                {
                    i.Usuario.Nombre,
                    i.Usuario.Apellido,
                    i.Usuario.CorreoUsuario,
                    i.EstadoInscripcion
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        [HttpPost]
        //[Authorize(Roles = "Administrador,Coordinador,Voluntario")]
        public async Task<ActionResult<Inscripciones>> PostInscripcion(Inscripciones inscripcion)
        {
            if (!ModelState.IsValid)
                return BadRequest("Datos invalidos");


            var usuario = await dBConexion.Usuario.FindAsync(inscripcion.IdUsuario);
            if (usuario == null)
                return BadRequest("El usuario no existe");

            var actividad = await dBConexion.Actividad.FindAsync(inscripcion.IdActividad);
            if (actividad == null)
                return BadRequest("La actividad no existe");
            var yaInscrito = await dBConexion.Inscripcion
             .AnyAsync(i => i.IdActividad == inscripcion.IdActividad && i.IdUsuario == inscripcion.IdUsuario);
            if (yaInscrito)
                return BadRequest("El Usuario ya se registró en esa actividad");

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
        //[Authorize(Roles = "Administrador,Coordinador")]
        public async Task<IActionResult> DeleteIncripcion(int id)
        {

            var inscripcion = await dBConexion.Inscripcion.FindAsync(id);
            if (inscripcion == null)
            {
                return NotFound("Inscripcion no encontrada");
            }

            dBConexion.Inscripcion.Remove(inscripcion);
            await dBConexion.SaveChangesAsync();

            return Ok($"Inscripcion con Id {id} eliminado correctamente");


        }

    }
}
