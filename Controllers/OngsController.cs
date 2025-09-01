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

        // =================== LECTURA (ADMIN/COORD) ===================

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<IEnumerable<Ongs>>> GetOngs()
        {
            return await dBConexion.Ong.ToListAsync();
        }

        [HttpGet("{idOng:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<Ongs>> GetOng(int idOng)
        {
            var ong = await dBConexion.Ong.FindAsync(idOng);
            if (ong == null) return NotFound("ONG no encontrada");
            return ong;
        }

     
        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<object>>> GetOngsPublic()
        {
            var ongs = await dBConexion.Ong
                .Select(o => new
                {
                    o.IdOng,
                    o.NombreOng,
                    o.Descripcion,
                    o.Direccion,
                    o.Telefono,
                })
                .ToListAsync();

            return Ok(ongs);
        }

        // =================== CREAR (ADMIN) ===================
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<Ongs>> PostOng([FromBody] Ongs ong)
        {
            if (!ModelState.IsValid)
                return BadRequest("Datos inválidos");

            var existe = await dBConexion.Ong.AnyAsync(p => p.NombreOng == ong.NombreOng);
            if (existe) return BadRequest("La ONG ya existe");

            dBConexion.Ong.Add(ong);
            await dBConexion.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOng), new { idOng = ong.IdOng }, ong);
        }

        // =================== ACTUALIZAR (ADMIN/COORD) ===================
        [HttpPut("{idOng:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> PutOng(int idOng, [FromBody] Ongs input)
        {
            if (idOng != input.IdOng)
                return BadRequest("El ID de la URL no coincide con el del cuerpo");

            if (!ModelState.IsValid)
                return BadRequest("Datos inválidos");


            var nombreDuplicado = await dBConexion.Ong
                .AnyAsync(o => o.IdOng != idOng && o.NombreOng == input.NombreOng);
            if (nombreDuplicado)
                return BadRequest("Ya existe otra ONG con ese nombre");

            var ong = await dBConexion.Ong.FindAsync(idOng);
            if (ong == null) return NotFound("ONG no encontrada");

            ong.NombreOng = input.NombreOng;
            ong.Descripcion = input.Descripcion;
            ong.Direccion = input.Direccion;
            ong.Telefono = input.Telefono;

            await dBConexion.SaveChangesAsync();
            return NoContent();
        }

        // =================== ELIMINAR (ADMIN) ===================
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteOng(int id)
        {
            var ong = await dBConexion.Ong.FindAsync(id);
            if (ong == null)
                return NotFound("ONG no encontrada");

            dBConexion.Ong.Remove(ong);
            await dBConexion.SaveChangesAsync();

            return Ok($"ONG con Id {id} eliminada correctamente");
        }
    }
}
