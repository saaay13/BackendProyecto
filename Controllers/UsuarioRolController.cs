using BackendProyecto.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendProyecto.Models;

namespace BackendProyecto.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioRolController : ControllerBase
    {
        private readonly DBConexion dBConexion;

        public UsuarioRolController(DBConexion dBConexion)
        {
            this.dBConexion = dBConexion;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioRol>>> GetUsuarioRoles()
        {
            var usuarioRoles = await dBConexion.UsuarioRol
                            .Include(ur => ur.Usuario)
                            .Include(ur => ur.Rol)
                            .ToListAsync();

            return usuarioRoles;
        }
        [HttpPost]
        public async Task<ActionResult<UsuarioRol>> PostUsuarioRol(UsuarioRol usuarioRol)
        {

            if (!ModelState.IsValid)
                return BadRequest("Datos invalidos");
            if (usuarioRol.IdRol == usuarioRol.IdUsuario)
            {
                return BadRequest("A ese usuario ya se le asigno ese rol");
            }    

            var usuario = await dBConexion.Usuario.FindAsync(usuarioRol.IdUsuario);
            if (usuario == null)
                return BadRequest("El usuario no existe");

            var rol = await dBConexion.Rol.FindAsync(usuarioRol.IdRol);
            if (rol == null)
                return BadRequest("El rol no existe");

            usuarioRol.Usuario = usuario;
            usuarioRol.Rol = rol;
            usuarioRol.FechaAsignacion = DateTime.Now;

            dBConexion.UsuarioRol.Add(usuarioRol);
            await dBConexion.SaveChangesAsync();

            return Ok(usuarioRol);


        }
    }
}
