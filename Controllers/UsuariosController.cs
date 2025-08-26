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
        //[Authorize(Roles ="Administrador")]
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
            var buscadoNombre = dBConexion.Usuario.Any(p => p.Nombre == usuario.Nombre);
            if (buscadoNombre)
            {
                return BadRequest("El usuario ya existe");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest("Datos del usuario invalidos");
            }

            dBConexion.Usuario.Add(usuario);
            await dBConexion.SaveChangesAsync();

            return CreatedAtAction("GetUsuario", new { id = usuario.IdUsuario }, usuario);
        }
        [HttpDelete("{id}")]
        //[Authorize(Roles ="Administrador , Coordinador")]
        public async Task<IActionResult> DeleteUsuario(int id)
        { 

            var usuario = await dBConexion.Usuario.FindAsync(id);
            if (usuario == null)
            {
                return NotFound("Usuario no encontrado");
            }

            dBConexion.Usuario.Remove(usuario);
            await dBConexion.SaveChangesAsync();

            return Ok($"Usuario con Id {id} eliminado correctamente");

        }





    }
}
