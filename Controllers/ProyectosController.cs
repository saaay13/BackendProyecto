using BackendProyecto.Data;
using BackendProyecto.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

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
        [Authorize(Roles = "Administrador,Coordinador")]
        public async Task<ActionResult<IEnumerable<Proyectos>>> GetProyectos()
        {
            var proyectos = await dBConexion.Proyecto
                    .Include(p => p.Ong)
                    .Include(p => p.Responsable)
                    .ToListAsync();
            return proyectos;
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador,Coordinador")]
        public async Task<ActionResult<Proyectos>> GetProyectos(int id)
        {
            var proyecto = await dBConexion.Proyecto.FindAsync(id);
            if (proyecto == null)
                return NotFound();
            return proyecto;
        }
        [HttpGet("public")]
        [Authorize(Roles = "Voluntario")]
        public async Task<ActionResult<IEnumerable<object>>> GetProyectosPublic()
        {
            var proyectos = await dBConexion.Proyecto
                .Include(p => p.Ong)
                .Include(p => p.Responsable)
                .Select(p => new
                {
                    p.NombreProyecto,
                    p.Descripcion,
                    p.FechaInicio,
                    p.FechaFin,
                    Ong = p.Ong != null ? p.Ong.NombreOng : null,
                    Responsable = p.Responsable != null ? p.Responsable.Nombre : null
                })
                .ToListAsync();

            return Ok(proyectos);
        }
        [HttpPost]
        [Authorize(Roles = "Administrador,Coordinador")]
        public async Task<ActionResult<Proyectos>> PostProyecto(Proyectos proyecto)
        {
            var buscadoNombre = dBConexion.Proyecto.Any(p => p.NombreProyecto == proyecto.NombreProyecto);
            if (buscadoNombre)
            {
                return BadRequest("El proyecto ya existe");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest("Datos invalidos");
            }

            var ong = await dBConexion.Ong.FindAsync(proyecto.IdOng);
            if (ong == null)
            {
                return BadRequest("La ong no existe");
            }

            var responsable = await dBConexion.Usuario.FindAsync(proyecto.IdResponsable);
            if (responsable == null)
            {
                return BadRequest("El responsable no existe");
            }

            dBConexion.Proyecto.Add(proyecto);
            await dBConexion.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProyectos), new { id = proyecto.IdProyecto }, proyecto);
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador , Coordinador")]
        public async Task<IActionResult> DeleteProyecto(int id)
        {

            var proyecto = await dBConexion.Proyecto.FindAsync(id);
            if (proyecto == null)
            {
                return NotFound("Proyecto no encontrado");
            }

            dBConexion.Proyecto.Remove(proyecto);
            await dBConexion.SaveChangesAsync();

            return Ok($"Proyecto con Id {id} eliminado correctamente");


        }
        [HttpPut("{idProyecto}")]
        public async Task<IActionResult> UpdateUsuarioRol(int idUsuario, int idRolActual, [FromBody] int nuevoRolId)
        {
            var usuarioRol = await dBConexion.UsuarioRol
                .FirstOrDefaultAsync(ur => ur.IdUsuario == idUsuario && ur.IdRol == idRolActual);

            if (usuarioRol == null)
            {
                return NotFound("La relacion usuario-rol no existe");
            }

            // Eliminar la relacion vieja
            dBConexion.UsuarioRol.Remove(usuarioRol);
            // Crear la nueva relación
            var nuevoUsuarioRol = new UsuarioRol
            {
                IdUsuario = idUsuario,
                IdRol = nuevoRolId,
                FechaAsignacion = DateTime.Now
            };

            await dBConexion.UsuarioRol.AddAsync(nuevoUsuarioRol);
            await dBConexion.SaveChangesAsync();

            return Ok($"El usuario {idUsuario} cambió del rol {idRolActual} al rol {nuevoRolId}");
        }
    }
}
