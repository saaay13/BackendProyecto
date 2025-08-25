using BackendProyecto.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendProyecto.Models;

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
        public async Task<ActionResult<Carnets>> PostCarnet(Carnets carnet)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Datos invalidos");
            }

            var usuario = await dBConexion.Usuario.FindAsync(carnet.IdUsuario);
            if (usuario == null)
            {
                return BadRequest("El usuario no existe");
            }

            var ong = await dBConexion.Ong.FindAsync(carnet.IdOng);
            if (ong == null)
            {
                return BadRequest("La ong no existe");
            }

            dBConexion.Carnet.Add(carnet);
            await dBConexion.SaveChangesAsync();

            return CreatedAtAction("GetCarnets", new { idCarnet = carnet.IdCarnet }, carnet);
        }

    }
}
