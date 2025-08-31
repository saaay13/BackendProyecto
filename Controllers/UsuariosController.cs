using BackendProyecto.Data;
using BackendProyecto.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BackendProyecto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
  //  [Authorize(Roles = "Administrador,Coordinador")] 
    public class UsuariosController : ControllerBase
    {
        private readonly DBConexion dbConexion;
        public UsuariosController(DBConexion db) => dbConexion = db;

        // DTO de lectura (no incluye Password)
        public class UsuarioReadDto
        {
            public int IdUsuario { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public string Apellido { get; set; } = string.Empty;
            public string CorreoUsuario { get; set; } = string.Empty;
            public string? Telefono { get; set; }

            public DateTime FechaRegistro { get; set; }
        }

        public class UsuarioUpdateRequest
        {
            [Required] public string Nombre { get; set; } = string.Empty;
            [Required] public string Apellido { get; set; } = string.Empty;
            [Required, EmailAddress] public string CorreoUsuario { get; set; } = string.Empty;
            [Phone] public string? Telefono { get; set; }
           
        }

        // GET: api/Usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioReadDto>>> GetUsuarios()
        {
            var lista = await dbConexion.Usuario
                .Select(u => new UsuarioReadDto
                {
                    IdUsuario = u.IdUsuario,
                    Nombre = u.Nombre,
                    Apellido = u.Apellido,
                    CorreoUsuario = u.CorreoUsuario,
                    Telefono = u.Telefono,
                    FechaRegistro = u.FechaRegistro
                })
                .ToListAsync();

            return Ok(lista);
        }

        // GET: api/Usuarios/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UsuarioReadDto>> GetUsuario(int id)
        {
            var u = await dbConexion.Usuario
                .Where(x => x.IdUsuario == id)
                .Select(u => new UsuarioReadDto
                {
                    IdUsuario = u.IdUsuario,
                    Nombre = u.Nombre,
                    Apellido = u.Apellido,
                    CorreoUsuario = u.CorreoUsuario,
                    Telefono = u.Telefono,
                    FechaRegistro = u.FechaRegistro
                })
                .FirstOrDefaultAsync();

            if (u is null)
                return NotFound("Usuario no encontrado.");

            return Ok(u);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateUsuario(int id, [FromBody] UsuarioUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuario = await dbConexion.Usuario.FindAsync(id);
            if (usuario is null)
                return NotFound("Usuario no encontrado.");

            // Email único (excluyendo al mismo usuario)
            var emailOcupado = await dbConexion.Usuario
                .AnyAsync(u => u.CorreoUsuario == request.CorreoUsuario && u.IdUsuario != id);
            if (emailOcupado)
                return BadRequest("El correo ya está en uso por otro usuario.");

            usuario.Nombre = request.Nombre;
            usuario.Apellido = request.Apellido;
            usuario.CorreoUsuario = request.CorreoUsuario;
            usuario.Telefono = request.Telefono;

            await dbConexion.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Usuarios/5
        // Solo Admin puede eliminar usuarios
        [HttpDelete("{id:int}")]
       // [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await dbConexion.Usuario.FindAsync(id);
            if (usuario is null)
                return NotFound("Usuario no encontrado.");

            // Quitar asignaciones de rol primero
            var asignaciones = await dbConexion.UsuarioRol
                .Where(ur => ur.IdUsuario == id)
                .ToListAsync();

            if (asignaciones.Count > 0)
                dbConexion.UsuarioRol.RemoveRange(asignaciones);

            // Luego eliminar el usuario
            dbConexion.Usuario.Remove(usuario);
            await dbConexion.SaveChangesAsync();

            return Ok($"Usuario con Id {id} eliminado y roles desvinculados.");
        }
        // DTO solo para contraseña
        public class PasswordUpdateRequest
        {
            [Required(ErrorMessage = "La contraseña es obligatoria")]
            [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
            public string NewPassword { get; set; } = string.Empty;
        }

        // PUT: api/Usuarios/{id}/password
        [HttpPut("{id:int}/password")]
       // [Authorize(Roles = "Administrador,Coordinador")]

        public async Task<IActionResult> UpdatePassword(int id, [FromBody] PasswordUpdateRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuario = await dbConexion.Usuario.FindAsync(id);
            if (usuario is null)
                return NotFound("Usuario no encontrado.");

            // Actualizar contraseña con BCrypt
            usuario.Password = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);

            await dbConexion.SaveChangesAsync();
            return Ok(new { mensaje = "Contraseña actualizada correctamente." });
        }


    }
}
