using BackendProyecto.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendProyecto.Models;

namespace BackendProyecto.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OngsController : ControllerBase
    {
        private readonly DBConexion dBConexion;

        public OngsController(DBConexion dBConexion)
        {
            this.dBConexion = dBConexion;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ongs>>> GetOngs()
        {
            return await dBConexion.Ong.ToListAsync();
        }
        [HttpGet("{idOng}")]
        public async Task<ActionResult<Ongs>> GetOng(int idOng)
        {
            var ong = await dBConexion.Ong.FindAsync(idOng);
            if (ong == null)
                return NotFound();
            return ong;
        }

        [HttpPost]
        public async Task<ActionResult<Ongs>> PostOng(Ongs ong)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Datos invalidos");
            }

            dBConexion.Ong.Add(ong);
            await dBConexion.SaveChangesAsync();

            return CreatedAtAction("GetOng", new { idOng = ong.IdOng }, ong);
        }

    }
}
