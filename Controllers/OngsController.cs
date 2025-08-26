using BackendProyecto.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendProyecto.Models;
using Microsoft.AspNetCore.Authorization;

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
        //[Authorize(Roles ="Administrador")]
        public async Task<ActionResult<Ongs>> PostOng(Ongs ong)
        {
            var buscadoNombre = dBConexion.Ong.Any(p => p.NombreOng == ong.NombreOng);
            if (buscadoNombre)
            {
                return BadRequest("La ONG ya existe");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest("Datos invalidos");
            }

            dBConexion.Ong.Add(ong);
            await dBConexion.SaveChangesAsync();

            return CreatedAtAction("GetOng", new { idOng = ong.IdOng }, ong);
        }
        [HttpDelete("{id}")]
        //[Authorize(Roles ="Administrador")]
        public async Task<IActionResult> DeleteOng(int id)
        {

            var ong = await dBConexion.Ong.FindAsync(id);
            if (ong == null)
            {
                return NotFound("Usuario no encontrado");
            }

            dBConexion.Ong.Remove(ong);
            await dBConexion.SaveChangesAsync();

            return Ok($"Ong con Id {id} eliminado correctamente");

        }

    }
}
