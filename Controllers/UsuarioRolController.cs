using BackendProyecto.Data;
using BackendProyecto.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        [Authorize(Roles = "Administrador,Coordinador")]
        public async Task<ActionResult<IEnumerable<UsuarioRol>>> GetUsuarioRoles()
        {
            var usuarioRoles = await dBConexion.UsuarioRol
                            .Include(ur => ur.Usuario)
                            .Include(ur => ur.Rol)
                            .ToListAsync();

            return usuarioRoles;
        }
        [HttpPost]
        [Authorize(Roles = "Administrador")]
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
        [HttpPut("{idUsuario}/{idRolActual}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> UpdateUsuarioRol(int idUsuario, int idRolActual, [FromBody] int nuevoRolId)
        {
            var usuarioRol = await dBConexion.UsuarioRol
                .FirstOrDefaultAsync(ur => ur.IdUsuario == idUsuario && ur.IdRol == idRolActual);

            if (usuarioRol == null)
            {
                return NotFound("La relacion usuario-rol no existe");
            }

            // Eliminar la relacion vieja
            dBConexion.UsuarioRol.Remove(usuarioRol);
            // Crear la nueva relación
            var nuevoUsuarioRol = new UsuarioRol
            {
                IdUsuario = idUsuario,
                IdRol = nuevoRolId,
                FechaAsignacion = DateTime.Now
            };

            await dBConexion.UsuarioRol.AddAsync(nuevoUsuarioRol);
            await dBConexion.SaveChangesAsync();

            return Ok($"El usuario {idUsuario} cambió del rol {idRolActual} al rol {nuevoRolId}");
        }

    }
}
