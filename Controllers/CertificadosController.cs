using BackendProyecto.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendProyecto.Models;

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

        [HttpPost]
        //[Authorize(Roles ="Administrador")]
        public async Task<ActionResult<Certificados>> PostCertificado(Certificados certificado)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Datos invalidos");
            }
            if (certificado.IdActividad == certificado.IdUsuario)
            { 
                return BadRequest("El certificado de ese usuario en esa actividad ya se genero ");
            }

            var usuario = await dBConexion.Usuario.FindAsync(certificado.IdUsuario);
            if (usuario == null)
            {
                return BadRequest("El usuario no existe");
            }

            var actividad = await dBConexion.Actividad.FindAsync(certificado.IdActividad);
            if (actividad == null)
            {
                return BadRequest("La actividad no existe");
            }

            dBConexion.Certificado.Add(certificado);
            await dBConexion.SaveChangesAsync();

            return CreatedAtAction("GetCertificado", new { id = certificado.IdCertificado }, certificado);
        }
        [HttpDelete("{id}")]
        //[Authorize(Roles ="Administrador")]
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
    }
}
