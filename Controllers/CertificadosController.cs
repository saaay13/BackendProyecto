using BackendProyecto.Data;
using BackendProyecto.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        //[Authorize(Roles = "Administrador,Coordinador")]
        public async Task<ActionResult<Certificados>> PostCertificado(Certificados certificado)
        {
            if (!ModelState.IsValid) return BadRequest("Datos invalidos");

            var usuario = await dBConexion.Usuario.FindAsync(certificado.IdUsuario);
            if (usuario is null) return BadRequest("El usuario no existe");

            var actividad = await dBConexion.Actividad
                .Include(a => a.Proyecto)
                .FirstOrDefaultAsync(a => a.IdActividad == certificado.IdActividad);
            if (actividad is null) return BadRequest("La actividad no existe");

            // 1) Debe tener inscripción CONFIRMADA en esa actividad
            var inscripcion = await dBConexion.Inscripcion
                .FirstOrDefaultAsync(i => i.IdUsuario == certificado.IdUsuario
                                       && i.IdActividad == certificado.IdActividad
                                       && i.EstadoInscripcion == Inscripciones.EstadoInscripcionEnum.Confirmada);
            if (inscripcion is null)
                return BadRequest("El usuario no tiene inscripción confirmada en esta actividad.");

            // 2) La actividad debe haber finalizado (usa tu propio criterio/fecha)
            if (actividad.FechaActividad.Date > DateTime.UtcNow.Date)
                return BadRequest("La actividad aún no ha finalizado.");

            // 3) Asistencia mínima por IdInscripcion (70% por ejemplo)
            var totalRegistros = await dBConexion.Asistencia
                .CountAsync(a => a.IdInscripcion == inscripcion.IdInscripcion);
            var presentes = await dBConexion.Asistencia
                .CountAsync(a => a.IdInscripcion == inscripcion.IdInscripcion && a.Asistio);

            // Si no registras faltas (solo presentes), puedes exigir al menos 1 presente:
            // if (presentes < 1) return BadRequest("No se registran asistencias del usuario.");

            int porcentaje = totalRegistros == 0 ? 0 : (int)Math.Round((presentes * 100.0) / totalRegistros);
            if (porcentaje < 70)
                return BadRequest($"Asistencia insuficiente ({porcentaje}%). Requisito mínimo: 70%.");

            // 4) Evitar duplicado usuario-actividad
            var yaGenerado = await dBConexion.Certificado
                .AnyAsync(c => c.IdActividad == certificado.IdActividad && c.IdUsuario == certificado.IdUsuario);
            if (yaGenerado)
                return BadRequest("El certificado de ese usuario en esa actividad ya fue generado");

            // 5) Emitir certificado
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
       // [Authorize(Roles = "Administrador,Coordinador")]
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
       // [Authorize(Roles = "Administrador,Coordinador")]
        public async Task<IActionResult> PutCertificado(int id, [FromBody] Certificados dto)
        {
            var entity = await dBConexion.Certificado.FindAsync(id);
            if (entity is null) return NotFound("Certificado no encontrado.");

            // Validaciones básicas si cambias usuario/actividad:
            if (entity.IdUsuario != dto.IdUsuario || entity.IdActividad != dto.IdActividad)
            {
                var usuario = await dBConexion.Usuario.FindAsync(dto.IdUsuario);
                if (usuario is null) return BadRequest("El usuario no existe.");
                var actividad = await dBConexion.Actividad.FindAsync(dto.IdActividad);
                if (actividad is null) return BadRequest("La actividad no existe.");

                // Evitar duplicados usuario-actividad
                var yaGenerado = await dBConexion.Certificado
                    .AnyAsync(c => c.IdActividad == dto.IdActividad && c.IdUsuario == dto.IdUsuario && c.IdCertificado != id);
                if (yaGenerado) return BadRequest("Ya existe un certificado para ese usuario en esa actividad.");
            }

            // Actualizar campos principales
            entity.IdUsuario = dto.IdUsuario;
            entity.IdActividad = dto.IdActividad;
            entity.FechaEmision = dto.FechaEmision;

            // Normalmente NO cambiamos CodigoVerificacion en edición.
            await dBConexion.SaveChangesAsync();
            return NoContent();
        }


    }
}
