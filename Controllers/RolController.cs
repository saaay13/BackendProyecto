using BackendProyecto.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendProyecto.Models;

namespace BackendProyecto.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolController : ControllerBase
    {
        private readonly DBConexion dBConexion;

        public RolController(DBConexion dBConexion)
        {
            this.dBConexion = dBConexion;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Roles>>> GetRoles()
        {
            return await dBConexion.Rol.ToListAsync();
        }
        [HttpPost]
        public async Task<ActionResult<Roles>> PostRol(Roles rol)
        {
            var buscadoNombre = dBConexion.Rol.Any(p => p.NombreRol== rol.NombreRol);
            if (buscadoNombre)
            {
                return BadRequest("El Rol ya existe");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest("Datos del rol inválidos");
            }

            dBConexion.Rol.Add(rol);
            await dBConexion.SaveChangesAsync();

            return CreatedAtAction("GetRoles", new { id = rol.IdRol }, rol);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRol(int id)
        {

            var rol = await dBConexion.Rol.FindAsync(id);
            if (rol== null)
            {
                return NotFound("Rol no encontrado");
            }

            dBConexion.Rol.Remove(rol);
            await dBConexion.SaveChangesAsync();

            return Ok($"Rol con Id {id} eliminado correctamente");

        }


    }
}

