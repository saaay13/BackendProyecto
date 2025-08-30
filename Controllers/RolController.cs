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

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<IEnumerable<object>>> Get()
        {
            var roles = await _db.Rol
                .Select(r => new { r.IdRol, r.NombreRol })
                .ToListAsync();

            return Ok(roles);
        }
    }
}
