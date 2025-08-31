using BackendProyecto.Data;
using BackendProyecto.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendProyecto.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolController : ControllerBase
    {
        private readonly DBConexion _db;
        public RolController(DBConexion db) => _db = db;

        // GET: api/Rol
        [HttpGet]
        //[Authorize(Roles = "Administrador")]
        public async Task<ActionResult<IEnumerable<object>>> Get()
        {
            var roles = await _db.Rol
                .Select(r => new { r.IdRol, r.NombreRol })
                .ToListAsync();

            return Ok(roles);
        }

        // POST: api/Rol
        [HttpPost]
        //[Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Post([FromBody] RolCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NombreRol))
                return BadRequest("El nombre del rol es obligatorio.");

            var existe = await _db.Rol.AnyAsync(r => r.NombreRol == dto.NombreRol);
            if (existe) return Conflict("Ya existe un rol con ese nombre.");

            var rol = new Roles { NombreRol = dto.NombreRol.Trim() };
            _db.Rol.Add(rol);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = rol.IdRol }, new { rol.IdRol, rol.NombreRol });
        }

        // GET: api/Rol/5
        [HttpGet("{id:int}")]
        //[Authorize(Roles = "Administrador")]
        public async Task<IActionResult> GetById(int id)
        {
            var r = await _db.Rol.FindAsync(id);
            if (r is null) return NotFound();
            return Ok(new { r.IdRol, r.NombreRol });
        }

        // PUT: api/Rol/5
        [HttpPut("{id:int}")]
       // [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Put(int id, [FromBody] RolUpdateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NombreRol))
                return BadRequest("El nombre del rol es obligatorio.");

            var rol = await _db.Rol.FindAsync(id);
            if (rol is null) return NotFound("Rol no encontrado.");

            var existe = await _db.Rol.AnyAsync(r => r.NombreRol == dto.NombreRol && r.IdRol != id);
            if (existe) return Conflict("Ya existe otro rol con ese nombre.");

            rol.NombreRol = dto.NombreRol.Trim();
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Rol/5
        [HttpDelete("{id:int}")]
        //[Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int id)
        {
            var rol = await _db.Rol.FindAsync(id);
            if (rol is null) return NotFound("Rol no encontrado.");

            // ¿Tiene asignaciones?
            var asignado = await _db.UsuarioRol.AnyAsync(ur => ur.IdRol == id);
            if (asignado) return BadRequest("No se puede eliminar: el rol tiene asignaciones.");

            _db.Rol.Remove(rol);
            await _db.SaveChangesAsync();
            return Ok($"Rol {id} eliminado.");
        }
    }

    public record RolCreateDto(string NombreRol);
    public record RolUpdateDto(string NombreRol);
}
