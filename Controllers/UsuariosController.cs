using BackendProyecto.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendProyecto.Models;

namespace BackendProyecto.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly DBConexion dBConexion;

        public UsuariosController(DBConexion dBConexion)
        {
            this.dBConexion = dBConexion;
        }

        // Get de todos los usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuarios>>> GetUsuarios()
        {
            return await dBConexion.Usuario.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Usuarios>> GetUsuario(int id)
        {
            var usuario = await dBConexion.Usuario.FindAsync(id);
            if (usuario == null)
            {
                return NotFound("Usuario no encontrado");
            }
            return usuario;
        }

        [HttpPost]
        public async Task<ActionResult<Usuarios>> PostUsuario(Usuarios usuario)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Datos del usuario invalidos");
            }

            dBConexion.Usuario.Add(usuario);
            await dBConexion.SaveChangesAsync();

            return CreatedAtAction("GetUsuario", new { id = usuario.IdUsuario }, usuario);
        }



    }
}
