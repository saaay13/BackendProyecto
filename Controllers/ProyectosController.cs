using BackendProyecto.Data;
using BackendProyecto.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendProyecto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProyectosController : ControllerBase
    {
        private readonly DBConexion _db;
        public ProyectosController(DBConexion db) => _db = db;

        // GET: api/Proyectos
        [HttpGet]
        //[Authorize(Roles = "Administrador,Coordinador")]
        public async Task<ActionResult<IEnumerable<Proyectos>>> GetAll()
        {
            var proyectos = await _db.Proyecto
                .Include(p => p.Ong)
                .Include(p => p.Responsable)
                .AsNoTracking()
                .ToListAsync();
            return Ok(proyectos);
        }

        // GET: api/Proyectos/5
        [HttpGet("{id:int}")]
       // [Authorize(Roles = "Administrador,Coordinador")]
        public async Task<ActionResult<Proyectos>> GetById(int id)
        {
            var proyecto = await _db.Proyecto
                .Include(p => p.Ong)
                .Include(p => p.Responsable)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdProyecto == id);

            if (proyecto is null) return NotFound();
            return Ok(proyecto);
        }

        // GET: api/Proyectos/public
        [HttpGet("public")]
     
        public async Task<ActionResult<IEnumerable<object>>> GetPublic()
        {
            var data = await _db.Proyecto
                .Include(p => p.Ong)
                .Include(p => p.Responsable)
                .Select(p => new
                {
                    p.NombreProyecto,
                    p.Descripcion,
                    p.FechaInicio,
                    p.FechaFin,
                    Ong = p.Ong != null ? p.Ong.NombreOng : null,
                    Responsable = p.Responsable != null ? p.Responsable.Nombre : null
                })
                .ToListAsync();

            return Ok(data);
        }

        // POST: api/Proyectos
        [HttpPost]
        [Authorize(Roles = "Administrador,Coordinador")]
        public async Task<ActionResult<Proyectos>> Create([FromBody] Proyectos proyecto)
        {
            if (!ModelState.IsValid) return BadRequest("Datos inválidos");

            var nombreRepetido = await _db.Proyecto.AnyAsync(p => p.NombreProyecto == proyecto.NombreProyecto);
            if (nombreRepetido) return BadRequest("El proyecto ya existe");

            var ong = await _db.Ong.FindAsync(proyecto.IdOng);
            if (ong is null) return BadRequest("La ONG no existe");

            var responsable = await _db.Usuario.FindAsync(proyecto.IdResponsable);
            if (responsable is null) return BadRequest("El responsable no existe");

            _db.Proyecto.Add(proyecto);
            await _db.SaveChangesAsync();

            // apúntalo a GetById explícitamente
            return CreatedAtAction(nameof(GetById), new { id = proyecto.IdProyecto }, proyecto);
        }

        // DELETE: api/Proyectos/5
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrador,Coordinador")]
        public async Task<IActionResult> Delete(int id)
        {
            var proyecto = await _db.Proyecto.FindAsync(id);
            if (proyecto is null) return NotFound("Proyecto no encontrado");

            _db.Proyecto.Remove(proyecto);
            await _db.SaveChangesAsync();
            return Ok($"Proyecto con Id {id} eliminado correctamente");
        }
        // PUT: api/Proyectos/5
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Administrador,Coordinador")]
        public async Task<IActionResult> Update(int id, [FromBody] Proyectos input)
        {
            if (id != input.IdProyecto)
                return BadRequest("El Id de la URL no coincide con el del cuerpo.");


            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);


            var proyecto = await _db.Proyecto.FirstOrDefaultAsync(p => p.IdProyecto == id);
            if (proyecto is null) return NotFound("Proyecto no encontrado");

            var nombreRepetido = await _db.Proyecto
                .AnyAsync(p => p.NombreProyecto == input.NombreProyecto && p.IdProyecto != id);
            if (nombreRepetido) return BadRequest("Ya existe un proyecto con ese nombre.");

            var ongExiste = await _db.Ong.AnyAsync(o => o.IdOng == input.IdOng);
            if (!ongExiste) return BadRequest("La ONG no existe.");

            var respExiste = await _db.Usuario.AnyAsync(u => u.IdUsuario == input.IdResponsable);
            if (!respExiste) return BadRequest("El responsable no existe.");

            if (input.FechaFin < input.FechaInicio)
                return BadRequest("La fecha de fin no puede ser anterior a la de inicio.");

         
            proyecto.IdOng = input.IdOng;
            proyecto.NombreProyecto = input.NombreProyecto;
            proyecto.Descripcion = input.Descripcion;
            proyecto.FechaInicio = input.FechaInicio;
            proyecto.FechaFin = input.FechaFin;
            proyecto.EstadoProyecto = input.EstadoProyecto; // enum
            proyecto.IdResponsable = input.IdResponsable;

            await _db.SaveChangesAsync();
            return NoContent();
        }

    }
}
