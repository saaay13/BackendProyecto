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
            if (!ModelState.IsValid)
            {
                return BadRequest("Datos del rol inválidos");
            }

            dBConexion.Rol.Add(rol);
            await dBConexion.SaveChangesAsync();

            return CreatedAtAction("GetRoles", new { id = rol.IdRol }, rol);
        }

    }
}

