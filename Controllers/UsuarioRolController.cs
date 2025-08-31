using BackendProyecto.Data;
using BackendProyecto.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendProyecto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioRolController : ControllerBase
    {
        private readonly DBConexion dBConexion;
        public UsuarioRolController(DBConexion dBConexion) => this.dBConexion = dBConexion;

        // GET: api/UsuarioRol
        [HttpGet]
        //[Authorize(Roles = "Administrador,Coordinador")]
        public async Task<ActionResult<IEnumerable<UsuarioRol>>> GetUsuarioRoles()
        {
            var lista = await dBConexion.UsuarioRol
                .Include(ur => ur.Usuario)
                .Include(ur => ur.Rol)
                .ToListAsync();

            return Ok(lista);
        }

        // POST: api/UsuarioRol
        // Asigna un rol a un usuario (no duplica relaciones)
        [HttpPost]
       // [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> PostUsuarioRol([FromBody] UsuarioRol usuarioRol)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Validaciones básicas
            var usuarioExiste = await dBConexion.Usuario.AnyAsync(u => u.IdUsuario == usuarioRol.IdUsuario);
            if (!usuarioExiste) return BadRequest("El usuario no existe.");

            var rolExiste = await dBConexion.Rol.AnyAsync(r => r.IdRol == usuarioRol.IdRol);
            if (!rolExiste) return BadRequest("El rol no existe.");

            var yaAsignado = await dBConexion.UsuarioRol
                .AnyAsync(ur => ur.IdUsuario == usuarioRol.IdUsuario && ur.IdRol == usuarioRol.IdRol);
            if (yaAsignado) return Conflict("Ese usuario ya tiene asignado ese rol.");

            usuarioRol.FechaAsignacion = DateTime.Now;

            dBConexion.UsuarioRol.Add(usuarioRol);
            await dBConexion.SaveChangesAsync();

            // No hay PK simple para Location; devolvemos 200 con la entidad incluida
            return Ok(usuarioRol);
        }

        // PUT: api/UsuarioRol/{idUsuario}/{idRolActual}
        // Cambia el rol asignado (no toca tabla Roles)
        [HttpPut("{idUsuario:int}/{idRolActual:int}")]
        //[Authorize(Roles = "Administrador")]
        public async Task<IActionResult> UpdateUsuarioRol(
            int idUsuario, int idRolActual, [FromBody] int nuevoRolId)
        {
            // Relación actual existe?
            var actual = await dBConexion.UsuarioRol
                .FirstOrDefaultAsync(ur => ur.IdUsuario == idUsuario && ur.IdRol == idRolActual);
            if (actual is null)
                return NotFound("La relación usuario-rol no existe.");

            // Rol nuevo existe?
            var rolNuevoExiste = await dBConexion.Rol.AnyAsync(r => r.IdRol == nuevoRolId);
            if (!rolNuevoExiste)
                return BadRequest("El nuevo rol no existe.");

            // Es el mismo rol?
            if (idRolActual == nuevoRolId)
                return BadRequest("El usuario ya tiene ese rol.");

            // Ya existe la relación con el nuevo rol?
            var yaAsignado = await dBConexion.UsuarioRol
                .AnyAsync(ur => ur.IdUsuario == idUsuario && ur.IdRol == nuevoRolId);
            if (yaAsignado)
                return Conflict("El usuario ya tiene asignado ese nuevo rol.");

            // Cambiar relación: quitar la vieja y crear la nueva
            dBConexion.UsuarioRol.Remove(actual);
            await dBConexion.UsuarioRol.AddAsync(new UsuarioRol
            {
                IdUsuario = idUsuario,
                IdRol = nuevoRolId,
                FechaAsignacion = DateTime.Now
            });

            await dBConexion.SaveChangesAsync();
            return Ok($"Se cambió el rol del usuario {idUsuario} de {idRolActual} a {nuevoRolId}.");
        }

        // DELETE: api/UsuarioRol/{idUsuario}/{idRol}
        // Quita la asignación (NO elimina el rol)
        [HttpDelete("{idUsuario:int}/{idRol:int}")]
        //[Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteUsuarioRol(int idUsuario, int idRol)
        {
            var entity = await dBConexion.UsuarioRol
                .FirstOrDefaultAsync(ur => ur.IdUsuario == idUsuario && ur.IdRol == idRol);

            if (entity is null)
                return NotFound("La relación usuario-rol no existe.");

            dBConexion.UsuarioRol.Remove(entity);
            await dBConexion.SaveChangesAsync();

            return Ok($"Se quitó el rol {idRol} al usuario {idUsuario}.");
        }
    }
}
