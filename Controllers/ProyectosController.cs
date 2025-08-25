using BackendProyecto.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendProyecto.Models;

namespace BackendProyecto.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProyectosController : ControllerBase
    {
        private readonly DBConexion dBConexion;

        public ProyectosController(DBConexion dBConexion)
        {
            this.dBConexion = dBConexion;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Proyectos>>> GetProyectos()
        {
            var proyectos = await dBConexion.Proyecto
                    .Include(p => p.Ong)
                    .Include(p => p.Responsable)
                    .ToListAsync();
            return proyectos;
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Proyectos>> GetProyectos(int id)
        {
            var proyecto = await dBConexion.Proyecto.FindAsync(id);
            if (proyecto == null)
                return NotFound();
            return proyecto;
        }

        [HttpPost]
        public async Task<ActionResult<Proyectos>> PostProyecto(Proyectos proyecto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Datos invalidos");
            }

            var ong=await dBConexion.Ong.FindAsync(proyecto.IdOng);
            if (ong == null)
            {
                return BadRequest("La ong no existe");
            }

            var responsable = await dBConexion.Usuario.FindAsync(proyecto.IdResponsable);
            if (responsable==null)
            {
                return BadRequest("El responsable no existe");
            }

            dBConexion.Proyecto.Add(proyecto);
            await dBConexion.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProyectos), new { id = proyecto.IdProyecto }, proyecto);
        }

    }
}
