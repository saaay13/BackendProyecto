using BackendProyecto.Data;
using BackendProyecto.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BackendProyecto.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly DBConexion dBConexion;
        private readonly IConfiguration _configuration;

        public AuthController(DBConexion dBConexion, IConfiguration configuration)
        {
            this.dBConexion = dBConexion;
            _configuration = configuration;
        }

        // ------------------- REGISTER -------------------
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verificar si ya existe el email
            if (await dBConexion.Usuario.AnyAsync(u => u.CorreoUsuario == request.CorreoUsuario))
                return BadRequest(new { mensaje = "El email ya está registrado" });

            // Crear usuario con contraseña hasheada
            var usuario = new Usuarios
            {
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                CorreoUsuario = request.CorreoUsuario,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Telefono = request.Telefono
            };

            dBConexion.Usuario.Add(usuario);
            await dBConexion.SaveChangesAsync();

            return Ok(new { mensaje = "Usuario registrado exitosamente", usuario.CorreoUsuario });
        }

        // ------------------- LOGIN -------------------
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuario = await dBConexion.Usuario.FirstOrDefaultAsync(u => u.CorreoUsuario == request.CorreoUsuario);
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Password, usuario.Password))
                return Unauthorized(new { mensaje = "Credenciales inválidas" });

            var token = await GenerateJwtToken(usuario);

            return Ok(new
            {
                mensaje = "Usuario logueado exitosamente",
                token
            });
        }

        // ------------------- GENERAR TOKEN -------------------
        private async Task<string> GenerateJwtToken(Usuarios usuario)
        {
            var rolUsuario = await dBConexion.UsuarioRol
                                    .Include(ur => ur.Rol)
                                    .FirstOrDefaultAsync(ur => ur.IdUsuario == usuario.IdUsuario);

            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
        new Claim(ClaimTypes.Email, usuario.CorreoUsuario),
        new Claim(ClaimTypes.Role, rolUsuario?.Rol?.NombreRol ?? "Voluntario")
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        // ------------------- REQUESTS -------------------
        public class RegisterRequest
        {
            [Required(ErrorMessage = "El nombre es obligatorio")]
            public string Nombre { get; set; }=string.Empty;

            [Required(ErrorMessage = "El apellido es obligatorio")]
            public string Apellido { get; set; }=string.Empty;

            [Required(ErrorMessage = "El email es obligatorio")]
            [EmailAddress(ErrorMessage = "El formato del correo es inválido")]
            public string CorreoUsuario { get; set; }=string.Empty;

            [Required(ErrorMessage = "La contraseña es obligatoria")]
            [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
            public string Password { get; set; } = string.Empty;

            [Phone(ErrorMessage = "El teléfono no es válido")]
            public string Telefono { get; set; }=string.Empty;
        }

        public class LoginRequest
        {
            [Required(ErrorMessage = "El email es obligatorio")]
            [EmailAddress(ErrorMessage = "El formato del correo es inválido")]
            public string CorreoUsuario { get; set; }=string.Empty;

            [Required(ErrorMessage = "La contraseña es obligatoria")]
            [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
            public string Password { get; set; }=string.Empty ;
        }

        // ------------------- CRUD -------------------
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
                return NotFound("Usuario no encontrado");

            return usuario;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await dBConexion.Usuario.FindAsync(id);
            if (usuario == null)
                return NotFound("Usuario no encontrado");

            dBConexion.Usuario.Remove(usuario);
            await dBConexion.SaveChangesAsync();

            return Ok($"Usuario con Id {id} eliminado correctamente");
        }
    }
}
