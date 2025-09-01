using BackendProyecto.Data;
using BackendProyecto.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendProyecto.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    public class AsistenciasController : ControllerBase
    {
        private readonly DBConexion dBConexion;
        public AsistenciasController(DBConexion dBConexion) => this.dBConexion = dBConexion;

        // ===== DTO =====
        public class AsistenciaInput
        {
            public int IdInscripcion { get; set; }
            public DateTime? HoraResgistro { get; set; }
            public string? Observacion { get; set; }
        }

        // ===== GET: api/Asistencias =====
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Asistencias>>> GetAsistencias()
        {
            var asistencias = await dBConexion.Asistencia
                .Include(a => a.Inscripcion)!.ThenInclude(i => i!.Usuario)
                .Include(a => a.Inscripcion)!.ThenInclude(i => i!.Actividad)!.ThenInclude(act => act!.Proyecto)!.ThenInclude(p => p!.Responsable)
                .Include(a => a.Inscripcion)!.ThenInclude(i => i!.Actividad)!.ThenInclude(act => act!.Proyecto)!.ThenInclude(p => p!.Ong)
                .ToListAsync();

            return asistencias;
        }

        // ===== GET: api/Asistencias/5 =====
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Asistencias>> GetAsistencia(int id)
        {
            var asistencia = await dBConexion.Asistencia
                .Include(a => a.Inscripcion)!.ThenInclude(i => i!.Usuario)
                .Include(a => a.Inscripcion)!.ThenInclude(i => i!.Actividad)!.ThenInclude(act => act!.Proyecto)!.ThenInclude(p => p!.Responsable)
                .Include(a => a.Inscripcion)!.ThenInclude(i => i!.Actividad)!.ThenInclude(act => act!.Proyecto)!.ThenInclude(p => p!.Ong)
                .FirstOrDefaultAsync(a => a.IdAsistencia == id);

            if (asistencia is null) return NotFound();
            return asistencia;
        }

        // ===== GET: api/Asistencias/mis-asistencias/123 =====
        [HttpGet("mis-asistencias/{idUsuario:int}")]
        public async Task<ActionResult<IEnumerable<object>>> GetMisAsistencias(int idUsuario)
        {
            var asistencias = await dBConexion.Asistencia
                .Include(a => a.Inscripcion)!.ThenInclude(i => i!.Actividad)
                .Where(a => a.Inscripcion!.IdUsuario == idUsuario)
                .Select(a => new
                {
                    a.Inscripcion!.Actividad!.NombreActividad,
                    a.Inscripcion!.Actividad!.FechaActividad,
                    a.Inscripcion!.Actividad!.HoraInicio,
                    a.Inscripcion!.Actividad!.HoraFin,
                    a.Inscripcion!.Actividad!.Lugar,
                    a.IdAsistencia
                })
                .ToListAsync();

            return Ok(asistencias);
        }

        // ===== POST: api/Asistencias  (REGISTRAR) =====
        [HttpPost] 
        public async Task<ActionResult> RegistrarAsistencia([FromBody] AsistenciaInput input)
        {
            // 1) Inscripción -> Actividad -> Proyecto
            var ins = await dBConexion.Inscripcion
                .Include(i => i.Actividad)!.ThenInclude(a => a!.Proyecto)
                .FirstOrDefaultAsync(i => i.IdInscripcion == input.IdInscripcion);

            if (ins is null) return NotFound("La inscripción no existe.");
            if (ins.Actividad is null || ins.Actividad.Proyecto is null)
                return BadRequest("La inscripción no tiene actividad o proyecto válidos.");

            // 2) Día a registrar
            var hora = input.HoraResgistro ?? DateTime.Now;
            var dia = hora.Date;

            // 3) Validar rango [inicio actividad, fin proyecto] (INCLUYE)
            var inicio = ins.Actividad.FechaActividad.Date;
            var fin = ins.Actividad.Proyecto.FechaFin.Date;
            if (dia < inicio || dia > fin)
                return BadRequest($"Solo puedes registrar asistencia entre {inicio:yyyy-MM-dd} y {fin:yyyy-MM-dd}.");

            // 4) 1 por día por inscripción
            bool existeMismoDia = await dBConexion.Asistencia
                .AnyAsync(a => a.IdInscripcion == ins.IdInscripcion && a.HoraResgistro.Date == dia);
            if (existeMismoDia)
                return Conflict("Ya registraste asistencia para este día.");

           
            var nueva = new Asistencias
            {
                IdInscripcion = ins.IdInscripcion,
                HoraResgistro = hora,
                Observacion = string.IsNullOrWhiteSpace(input.Observacion) ? null : input.Observacion.Trim(),
                Asistio = false
            };

            dBConexion.Asistencia.Add(nueva);
            await dBConexion.SaveChangesAsync();

            // 6) Recalcular bandera
            await RecalcularBanderaAsistio(ins.IdInscripcion);

            return Created("", null);
        }

        // ===== helper: recalcula bandera Asistio =====
        private async Task<bool> RecalcularBanderaAsistio(int idInscripcion)
        {
            var ins = await dBConexion.Inscripcion
                .Include(i => i.Actividad)!.ThenInclude(a => a!.Proyecto)
                .FirstOrDefaultAsync(i => i.IdInscripcion == idInscripcion);

            if (ins is null || ins.Actividad is null || ins.Actividad.Proyecto is null)
                return false;

            var inicio = ins.Actividad.FechaActividad.Date;
            var fin = ins.Actividad.Proyecto.FechaFin.Date;
            if (fin < inicio) fin = inicio;

            int diasRequeridos = (fin - inicio).Days + 1;

            var diasRegistrados = await dBConexion.Asistencia
                .Where(a => a.IdInscripcion == idInscripcion &&
                            a.HoraResgistro.Date >= inicio &&
                            a.HoraResgistro.Date <= fin)
                .Select(a => a.HoraResgistro.Date)
                .Distinct()
                .CountAsync();

            bool completo = diasRegistrados >= diasRequeridos;

            var filas = await dBConexion.Asistencia
                .Where(a => a.IdInscripcion == idInscripcion)
                .ToListAsync();

            foreach (var f in filas)
                f.Asistio = completo;

            await dBConexion.SaveChangesAsync();
            return completo;
        }

        // ===== DELETE: api/Asistencias/123 =====
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsistencia(int id)
        {
            var asistencia = await dBConexion.Asistencia.FindAsync(id);
            if (asistencia is null) return NotFound("Asistencia no encontrada");

            dBConexion.Asistencia.Remove(asistencia);
            await dBConexion.SaveChangesAsync();

            return Ok($"Asistencia con Id {id} eliminada correctamente");
        }

        // ===== GET: api/Asistencias/por-inscripcion/999 =====
        [HttpGet("por-inscripcion/{idInscripcion:int}")]
        public async Task<ActionResult<IEnumerable<object>>> GetPorInscripcion(int idInscripcion)
        {
            var existe = await dBConexion.Inscripcion.AnyAsync(i => i.IdInscripcion == idInscripcion);
            if (!existe) return NotFound("Inscripción no existe.");

            var lista = await dBConexion.Asistencia
                .Where(a => a.IdInscripcion == idInscripcion)
                .OrderBy(a => a.HoraResgistro)
                .Select(a => new
                {
                    a.IdAsistencia,
                    a.IdInscripcion,
                    a.Asistio,
                    a.Observacion,
                    a.HoraResgistro
                })
                .ToListAsync();

            return Ok(lista);
        }

        // ===== GET: api/Asistencias/estado/999 =====
        [HttpGet("estado/{idInscripcion:int}")] 
        public async Task<ActionResult<object>> EstadoAsistencia(int idInscripcion)
        {
            var ins = await dBConexion.Inscripcion
                .Include(i => i.Actividad)!.ThenInclude(a => a!.Proyecto)
                .FirstOrDefaultAsync(i => i.IdInscripcion == idInscripcion);

            if (ins is null || ins.Actividad is null || ins.Actividad.Proyecto is null)
                return NotFound();

            var inicio = ins.Actividad.FechaActividad.Date;
            var fin = ins.Actividad.Proyecto.FechaFin.Date;
            if (fin < inicio) fin = inicio;

            int diasReq = (fin - inicio).Days + 1;

            var registradas = await dBConexion.Asistencia
                .Where(a => a.IdInscripcion == idInscripcion &&
                            a.HoraResgistro.Date >= inicio &&
                            a.HoraResgistro.Date <= fin)
                .Select(a => a.HoraResgistro.Date)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();

            var todas = Enumerable.Range(0, diasReq).Select(o => inicio.AddDays(o)).ToList();
            var pendientes = todas.Except(registradas).OrderBy(d => d).ToList();

            return Ok(new
            {
                Rango = new { Inicio = inicio, Fin = fin, DiasRequeridos = diasReq },
                DiasRegistrados = registradas,
                DiasPendientes = pendientes,
                Completado = registradas.Count >= diasReq
            });
        }
    }
}
