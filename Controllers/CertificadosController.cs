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
    public class CertificadosController : ControllerBase
    {
        private readonly DBConexion dBConexion;

        public CertificadosController(DBConexion dBConexion)
        {
            this.dBConexion = dBConexion;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Certificados>>> GetCertificados()
        {
            var certificados = await dBConexion.Certificado
                    .Include(c => c.Usuario)
                    .Include(c => c.Actividad)
                        .ThenInclude(a => a.Proyecto)
                            .ThenInclude(p => p.Responsable)
                    .Include(c => c.Actividad)
                        .ThenInclude(a => a.Proyecto)
                            .ThenInclude(p => p.Ong)
                    .ToListAsync();

            return certificados;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Certificados>> GetCertificado(int id)
        {
            var certificado = await dBConexion.Certificado
                .Include(c => c.Usuario)
                .Include(c => c.Actividad)
                    .ThenInclude(a => a.Proyecto)
                        .ThenInclude(p => p.Responsable)
                .Include(c => c.Actividad)
                    .ThenInclude(a => a.Proyecto)
                        .ThenInclude(p => p.Ong)
                .FirstOrDefaultAsync(c => c.IdCertificado == id);

            if (certificado == null) return NotFound();
            return certificado;
        }
        [HttpGet("mis-certificados/{idUsuario}")]
        //[Authorize(Roles = "Voluntario,Coordinador,Administrador")]
        public async Task<ActionResult<IEnumerable<object>>> GetMisCertificados(int idUsuario)
        {
            var certificados = await dBConexion.Certificado
                .Include(c => c.Actividad)
                .Where(c => c.IdUsuario == idUsuario)
                .Select(c => new
                {
                    c.IdCertificado,
                    c.Actividad.NombreActividad,
                    c.Actividad.FechaActividad,
                    c.Actividad.Lugar
                })
                .ToListAsync();

            return Ok(certificados);//cambios
        }

        [HttpPost]
        public async Task<ActionResult<Certificados>> PostCertificado(Certificados certificado)
        {
            if (!ModelState.IsValid) return BadRequest("Datos invalidos");

            var usuario = await dBConexion.Usuario.FindAsync(certificado.IdUsuario);
            if (usuario is null) return BadRequest("El usuario no existe");

            var actividad = await dBConexion.Actividad
                .Include(a => a.Proyecto)
                .FirstOrDefaultAsync(a => a.IdActividad == certificado.IdActividad);
            if (actividad is null) return BadRequest("La actividad no existe");

            var inscripcion = await dBConexion.Inscripcion
                .FirstOrDefaultAsync(i => i.IdUsuario == certificado.IdUsuario
                                       && i.IdActividad == certificado.IdActividad
                                       && i.EstadoInscripcion == Inscripciones.EstadoInscripcionEnum.Confirmada);
            if (inscripcion is null)
                return BadRequest("El usuario no tiene inscripción confirmada en esta actividad.");

            if (actividad.FechaActividad.Date > DateTime.UtcNow.Date)
                return BadRequest("La actividad aún no ha finalizado.");

            var totalRegistros = await dBConexion.Asistencia
                .CountAsync(a => a.IdInscripcion == inscripcion.IdInscripcion);
            var presentes = await dBConexion.Asistencia
                .CountAsync(a => a.IdInscripcion == inscripcion.IdInscripcion && a.Asistio);


            int porcentaje = totalRegistros == 0 ? 0 : (int)Math.Round((presentes * 100.0) / totalRegistros);
            if (porcentaje < 70)
                return BadRequest($"Asistencia insuficiente ({porcentaje}%). Requisito mínimo: 70%.");

            var yaGenerado = await dBConexion.Certificado
                .AnyAsync(c => c.IdActividad == certificado.IdActividad && c.IdUsuario == certificado.IdUsuario);
            if (yaGenerado)
                return BadRequest("El certificado de ese usuario en esa actividad ya fue generado");

            if (string.IsNullOrWhiteSpace(certificado.CodigoVerificacion))
                certificado.CodigoVerificacion = Guid.NewGuid().ToString("N")[..12].ToUpper();

            certificado.FechaEmision = certificado.FechaEmision == default
                ? DateTime.UtcNow
                : certificado.FechaEmision;

            dBConexion.Certificado.Add(certificado);
            await dBConexion.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCertificado), new { id = certificado.IdCertificado }, certificado);
        }

        [HttpDelete("{id}")]
       [Authorize(Roles = "Administrador,Coordinador")]
        public async Task<IActionResult> DeleteCertificado(int id)
        {

            var certiicado = await dBConexion.Certificado.FindAsync(id);
            if (certiicado == null)
            {
                return NotFound("Certificado no encontrado");
            }

            dBConexion.Certificado.Remove(certiicado);
            await dBConexion.SaveChangesAsync();

            return Ok($"Certificado con Id {id} eliminado correctamente");
        }
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Administrador,Coordinador")]
        public async Task<IActionResult> PutCertificado(int id, [FromBody] Certificados dto)
        {
            var entity = await dBConexion.Certificado.FindAsync(id);
            if (entity is null) return NotFound("Certificado no encontrado.");

            if (entity.IdUsuario != dto.IdUsuario || entity.IdActividad != dto.IdActividad)
            {
                var usuario = await dBConexion.Usuario.FindAsync(dto.IdUsuario);
                if (usuario is null) return BadRequest("El usuario no existe.");
                var actividad = await dBConexion.Actividad.FindAsync(dto.IdActividad);
                if (actividad is null) return BadRequest("La actividad no existe.");

                var yaGenerado = await dBConexion.Certificado
                    .AnyAsync(c => c.IdActividad == dto.IdActividad && c.IdUsuario == dto.IdUsuario && c.IdCertificado != id);
                if (yaGenerado) return BadRequest("Ya existe un certificado para ese usuario en esa actividad.");
            }

            entity.IdUsuario = dto.IdUsuario;
            entity.IdActividad = dto.IdActividad;
            entity.FechaEmision = dto.FechaEmision;

            await dBConexion.SaveChangesAsync();
            return NoContent();
        }
        [HttpGet("{id:int}/pdf")]
        [Authorize(Roles = "Voluntario,Administrador")]
        public async Task<IActionResult> GetCertificadoPdf(int id, [FromServices] IWebHostEnvironment env)
        {
            var cert = await dBConexion.Certificado
                .Include(c => c.Usuario)
                .Include(c => c.Actividad).ThenInclude(a => a.Proyecto).ThenInclude(p => p.Ong)
                .FirstOrDefaultAsync(c => c.IdCertificado == id);

            if (cert is null) return NotFound("Certificado no encontrado.");

            var isAdmin = User.IsInRole("Administrador");
            var userId = GetCurrentUserId();

            
            if (!isAdmin)
            {
                if (userId is null || cert.IdUsuario != userId.Value)
                    return Forbid("No puedes descargar certificados de otros usuarios.");
            }

            var webroot = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var logoPath = Path.Combine(webroot, "img", "LogoSinFondo.png");
            byte[]? logoBytes = System.IO.File.Exists(logoPath)
                ? await System.IO.File.ReadAllBytesAsync(logoPath)
                : null;

            var bytes = PdfCertificadoBuilder.BuildCertificado(cert, logoBytes);
            return File(bytes, "application/pdf", $"certificado-{id}.pdf");
        }

        [HttpGet("mios")]
        [Authorize(Roles = "Voluntario,Coordinador,Administrador")]
        public async Task<ActionResult<IEnumerable<Certificados>>> GetMisCertificados()
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Forbid();

            var res = await dBConexion.Certificado
                .Include(c => c.Usuario)
                .Include(c => c.Actividad).ThenInclude(a => a.Proyecto).ThenInclude(p => p.Ong)
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
