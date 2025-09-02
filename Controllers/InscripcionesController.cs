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
        public record InscripcionListItem(
            int IdInscripcion,
            int IdUsuario,
            string NombreUsuario,
            string EstadoInscripcion,
            DateTime FechaInscripcion
        );

        public class InscripcionUpdateDto
        {
            public int IdInscripcion { get; set; }
            public string EstadoInscripcion { get; set; } = "Pendiente"; // Pendiente|Confirmada|Cancelada
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
        [Authorize(Roles = "Administrador,Coordinador")]
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
        
        [HttpGet("por-actividad/{idActividad:int}")]
        //[Authorize(Roles = "Administrador,Coordinador")]
        public async Task<ActionResult<IEnumerable<InscripcionListItem>>> GetPorActividad(
            int idActividad, bool soloConfirmadas = false)
        {
            var existeAct = await dBConexion.Actividad.AnyAsync(a => a.IdActividad == idActividad);
            if (!existeAct) return NotFound("La actividad no existe.");

            var q = dBConexion.Inscripcion
                .Include(i => i.Usuario)
                .Where(i => i.IdActividad == idActividad);

            if (soloConfirmadas)
                q = q.Where(i => i.EstadoInscripcion == Inscripciones.EstadoInscripcionEnum.Confirmada);

            var lista = await q
                .OrderBy(i => i.FechaInscripcion)
                .Select(i => new InscripcionListItem(
                    i.IdInscripcion,
                    i.IdUsuario,
                    i.Usuario != null ? (i.Usuario.Nombre + " " + i.Usuario.Apellido) : "Usuario",
                    i.EstadoInscripcion.ToString(),
                    i.FechaInscripcion
                ))
                .ToListAsync();

            return Ok(lista);
        }
        // GET: api/Inscripciones/por-usuario/5
        [HttpGet("por-usuario/{idUsuario:int}")]
        //[Authorize(Roles = "Administrador,Coordinador")]
        public async Task<ActionResult<IEnumerable<object>>> GetPorUsuario(int idUsuario)
        {
            var lista = await dBConexion.Inscripcion
                .Include(i => i.Actividad)
                .Where(i => i.IdUsuario == idUsuario)
                .OrderByDescending(i => i.FechaInscripcion)
                .Select(i => new
                {
                    i.IdInscripcion,
                    i.IdActividad,
                    NombreActividad = i.Actividad != null ? i.Actividad.NombreActividad : "",
                    FechaActividad = i.Actividad != null ? i.Actividad.FechaActividad : DateTime.MinValue,
                    Lugar = i.Actividad != null ? i.Actividad.Lugar : null,
                    EstadoInscripcion = i.EstadoInscripcion.ToString(),
                    i.FechaInscripcion
                })
                .ToListAsync();

            return Ok(lista);
        }
        // PUT: api/Inscripciones/123
        [HttpPut("{id:int}")]
        //[Authorize(Roles = "Administrador,Coordinador")]
        public async Task<IActionResult> PutInscripcion(int id, [FromBody] InscripcionUpdateDto dto)
        {
            var insc = await dBConexion.Inscripcion
                .Include(i => i.Actividad)
                .FirstOrDefaultAsync(i => i.IdInscripcion == id);

            if (insc is null) return NotFound("Inscripción no encontrada.");

            if (!Enum.TryParse(dto.EstadoInscripcion, ignoreCase: true,
                out Inscripciones.EstadoInscripcionEnum nuevo))
                return BadRequest("EstadoInscripcion inválido.");

            if (nuevo == Inscripciones.EstadoInscripcionEnum.Confirmada)
            {
                var confirmadas = await dBConexion.Inscripcion
                    .CountAsync(i => i.IdActividad == insc.IdActividad
                                  && i.EstadoInscripcion == Inscripciones.EstadoInscripcionEnum.Confirmada
                                  && i.IdInscripcion != id);

                if (insc.Actividad != null && confirmadas >= insc.Actividad.CupoMaximo)
                    return BadRequest("No se puede confirmar: cupo máximo alcanzado.");
            }

            insc.EstadoInscripcion = nuevo;
            await dBConexion.SaveChangesAsync();
            return NoContent();
        }

    }
}
