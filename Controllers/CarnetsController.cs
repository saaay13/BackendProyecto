using BackendProyecto.Data;
using BackendProyecto.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BackendProyecto.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarnetsController : ControllerBase
    {
        private readonly DBConexion dBConexion;

        public CarnetsController(DBConexion dBConexion)
        {
            this.dBConexion = dBConexion;
        }

        [HttpGet]

        public async Task<ActionResult<IEnumerable<Carnets>>> GetCarnets()
        {
            var carnets = await dBConexion.Carnet
                    .Include(c => c.Usuario)
                    .Include(c => c.Ong)
                    .ToListAsync();

            return carnets;
        }
        [HttpPost]
        [Authorize(Roles = "Administrador,Coordinador")]
        public async Task<ActionResult<Carnets>> PostCarnet(Carnets carnet)
        {
            if (!ModelState.IsValid) return BadRequest("Datos invalidos");

            var usuario = await dBConexion.Usuario
                .FirstOrDefaultAsync(u => u.IdUsuario == carnet.IdUsuario);
            if (usuario is null) return BadRequest("El usuario no existe");

            var ong = await dBConexion.Ong.FindAsync(carnet.IdOng);
            if (ong is null) return BadRequest("La ong no existe");

            var rolesUsuario = await dBConexion.UsuarioRol
                .Include(ur => ur.Rol)
                .Where(ur => ur.IdUsuario == carnet.IdUsuario)
                .Select(ur => ur.Rol.NombreRol)
                .ToListAsync();

            var esElegibleRol = rolesUsuario.Any(r =>
                string.Equals(r, "Voluntario", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(r, "Coordinador", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(r, "Administrador", StringComparison.OrdinalIgnoreCase));

            if (!esElegibleRol)
                return BadRequest("El usuario no tiene rol de Voluntario (o superior).");

            bool tieneInscripcion = await dBConexion.Inscripcion
                .AnyAsync(i => i.IdUsuario == carnet.IdUsuario
                            && i.EstadoInscripcion == Inscripciones.EstadoInscripcionEnum.Confirmada);
            if (!tieneInscripcion) return BadRequest("El usuario no tiene inscripciones confirmadas.");

            var hoy = DateTime.UtcNow.Date;
            var carnetVigente = await dBConexion.Carnet
                .AnyAsync(c => c.IdUsuario == carnet.IdUsuario && c.FechaVencimiento >= hoy);
            if (carnetVigente) return BadRequest("El usuario ya tiene un carnet vigente.");

            var duplicadoPorOng = await dBConexion.Carnet
                .AnyAsync(c => c.IdOng == carnet.IdOng && c.IdUsuario == carnet.IdUsuario && c.FechaVencimiento >= hoy);
            if (duplicadoPorOng) return BadRequest("Ya existe un carnet vigente para ese usuario en esa ONG.");

            if (carnet.FechaEmision == default) carnet.FechaEmision = DateTime.UtcNow;
            if (carnet.FechaVencimiento == default) carnet.FechaVencimiento = DateTime.UtcNow.AddYears(1);
            if (carnet.CodigoVerificacion == default) carnet.CodigoVerificacion = Guid.NewGuid();

            dBConexion.Carnet.Add(carnet);
            await dBConexion.SaveChangesAsync();  // ⚡ ya tiene IdCarnet

            // 🔗 Construir URL absoluta al PDF
            var pdfUrl = Url.Action(nameof(GetCarnetPdf), "Carnets", new { id = carnet.IdCarnet },
                                     Request.Scheme, Request.Host.ToString());

            carnet.UrlCarnet = pdfUrl;
            await dBConexion.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCarnetById), new { id = carnet.IdCarnet }, carnet);
        }
        [HttpGet("{id:int}/pdf")]
        [Authorize(Roles = "Voluntario,Administrador")]
        public async Task<IActionResult> GetCarnetPdf(int id, [FromServices] IWebHostEnvironment env)
        {
            var carnet = await dBConexion.Carnet
                .Include(c => c.Usuario)
                .Include(c => c.Ong)
                .FirstOrDefaultAsync(c => c.IdCarnet == id);

            if (carnet is null) return NotFound("Carnet no encontrado.");

            var isAdmin = User.IsInRole("Administrador");
            var userId = GetCurrentUserId();

           
            if (!isAdmin)
            {
                if (userId is null || carnet.IdUsuario != userId.Value)
                    return Forbid("No puedes descargar carnets de otros usuarios.");
            }

            var webroot = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var logoPath = Path.Combine(webroot, "img", "LogoSinFondo.png");
            byte[]? logoBytes = System.IO.File.Exists(logoPath)
                ? await System.IO.File.ReadAllBytesAsync(logoPath)
                : null;

            var pdfBytes = PdfCarnetBuilder.BuildCarnet(carnet, logoBytes);
            return File(pdfBytes, "application/pdf", $"carnet-{id}.pdf");
        }




        [HttpDelete("{id}")]
        [Authorize(Roles ="Administrador")]
        public async Task<IActionResult> DeleteCarnet(int id)
        {

            var carnet = await dBConexion.Carnet.FindAsync(id);
            if (carnet == null)
            {
                return NotFound("Carnet no encontrado");
            }

            dBConexion.Carnet.Remove(carnet);
            await dBConexion.SaveChangesAsync();

            return Ok($"Carnet con Id {id} eliminado correctamente");



        }
        // GET: api/Carnets/5
        [HttpGet("{id:int}")]
        //[Authorize(Roles = "Administrador,Coordinador")]
        public async Task<ActionResult<Carnets>> GetCarnetById(int id)
        {
            var carnet = await dBConexion.Carnet
                .Include(c => c.Usuario)
                .Include(c => c.Ong)
                .FirstOrDefaultAsync(c => c.IdCarnet == id);

            return carnet is null ? NotFound("Carnet no encontrado") : Ok(carnet);
        }

        // PUT: api/Carnets/5
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Administrador,Coordinador")]
        public async Task<IActionResult> PutCarnet(int id, [FromBody] Carnets dto)
        {
            var entity = await dBConexion.Carnet.FindAsync(id);
            if (entity is null) return NotFound("Carnet no encontrado.");

            // Validaciones básicas
            var usuario = await dBConexion.Usuario.FindAsync(dto.IdUsuario);
            if (usuario is null) return BadRequest("El usuario no existe.");

            var ong = await dBConexion.Ong.FindAsync(dto.IdOng);
            if (ong is null) return BadRequest("La ong no existe.");

            entity.IdUsuario = dto.IdUsuario;
            entity.IdOng = dto.IdOng;
            entity.FechaEmision = dto.FechaEmision;
            entity.FechaVencimiento = dto.FechaVencimiento;
            entity.Beneficios = dto.Beneficios;
            entity.CodigoVerificacion = dto.CodigoVerificacion;
            entity.EstadoInscripcion = dto.EstadoInscripcion;
            await dBConexion.SaveChangesAsync();
            return NoContent();
        }
        [HttpGet("mios")]
        [Authorize(Roles = "Voluntario,Coordinador,Administrador")]
        public async Task<ActionResult<IEnumerable<Carnets>>> GetMisCarnets()
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Forbid();

            var res = await dBConexion.Carnet
                .Include(c => c.Usuario)
                .Include(c => c.Ong)
                .Where(c => c.IdUsuario == userId.Value)
                .ToListAsync();

            return res;
        }
        private int? GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : null;
        }

    }
}
