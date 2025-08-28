using BackendProyecto.Data;
using BackendProyecto.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendProyecto.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActividadesController : ControllerBase
    {
        private readonly DBConexion dBConexion;

        public ActividadesController(DBConexion dBConexion)
        {
            this.dBConexion = dBConexion;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Coordinador")]
        public async Task<ActionResult<IEnumerable<Actividades>>> GetActividades()
        {
            return await dBConexion.Actividad
                                    .Include(a => a.Proyecto)
                                        .ThenInclude(p => p.Responsable)
                                    .Include(a => a.Proyecto)
                                        .ThenInclude(p => p.Ong)
                                    .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Actividades>> GetActividad(int id)
        {
            var actividad = await dBConexion.Actividad.FindAsync(id);
            if (actividad == null) return NotFound();
            return actividad;
        }
        [HttpGet("public")]
        [Authorize(Roles = "Administrador,Coordinador,Voluntario")]

        public async Task<ActionResult<IEnumerable<object>>> GetActividadesPublic()
        {
            var actividades = await dBConexion.Actividad
                                  .Include(a => a.Proyecto)
                                      .ThenInclude(p => p.Responsable)
                                  .Include(a => a.Proyecto)
                                      .ThenInclude(p => p.Ong)
                                  .OrderBy(a => a.FechaActividad)
                                  .ToListAsync();


            var resultado = actividades.Select(a => new
            {
                a.NombreActividad,
                a.FechaActividad,
                a.HoraInicio,
                a.HoraFin,
                a.Lugar,
                a.CupoMaximo,
                Proyecto = a.Proyecto != null ? new
                {
                    a.Proyecto.NombreProyecto,
                    Responsable = a.Proyecto.Responsable != null ? new
                    {
                        a.Proyecto.Responsable.Nombre,
                    } : null,
                    Ong = a.Proyecto.Ong != null ? new
                    {
                        a.Proyecto.Ong.NombreOng
                    } : null
                } : null
            });

            return Ok(resultado);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Coordinador")]
        public async Task<ActionResult<Actividades>> PostActividad(Actividades actividad)
        {
            var buscadoNombre = dBConexion.Actividad.Any(p => p.NombreActividad == actividad.NombreActividad);
            if (buscadoNombre)
            {
                return BadRequest("La Actividad ya existe");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest("Datos invalidos");
            }

            var proyecto = await dBConexion.Proyecto.FindAsync(actividad.IdProyecto);
            if (proyecto == null)
            {
                return BadRequest("El proyecto no existe");
            }

            dBConexion.Actividad.Add(actividad);
            await dBConexion.SaveChangesAsync();

            return CreatedAtAction("GetActividad", new { id = actividad.IdActividad }, actividad);
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador,Coordinador")]
        public async Task<IActionResult> DeleteActividad(int id)
        {

            var actividad = await dBConexion.Actividad.FindAsync(id);
            if (actividad == null)
            {
                return NotFound("Actividad no encontrado");
            }

            dBConexion.Actividad.Remove(actividad);
            await dBConexion.SaveChangesAsync();

            return Ok($"Actividad con Id {id} eliminado correctamente");
        }
    }
}
