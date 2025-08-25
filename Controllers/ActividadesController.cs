using BackendProyecto.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendProyecto.Models;

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

        [HttpPost]
        public async Task<ActionResult<Actividades>> PostActividad(Actividades actividad)
        {
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

    }
}
