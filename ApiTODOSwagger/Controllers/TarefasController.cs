using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiTODOSwagger.Data;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace ApiTODOSwagger.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TarefasController : ControllerBase
    {
        private readonly AppDbContext _context;

        private IWebHostEnvironment _hostingEnvironment;

        Dictionary<int, string> DictFlagStatus = new Dictionary<int, string>()
        {
            {1,"Agendado"},
            {2, "Em Andamento"},
            {3, "Finalizado"}
        };

        public TarefasController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _hostingEnvironment = environment;
        }

        // GET: api/Tarefas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tarefa>>> GetTarefa()
        {
            return await _context.Tarefas.Include(p => p.Pessoa).ToListAsync();
        }

        // GET: api/Tarefas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Tarefa>> GetTarefa(int id)
        {
            var tarefa = await _context.Tarefas
                .Include(p => p.Pessoa)
                    .Where(t => t.Id == id).FirstOrDefaultAsync();


            if (tarefa == null)
            {
                return NotFound();
            }

            return tarefa;
        }

        // PUT: api/Tarefas/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTarefa(int id, Tarefa tarefa)
        {
            if (id != tarefa.Id)
            {
                return BadRequest();
            }

            _context.Entry(tarefa).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TarefaExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Tarefas
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Tarefa>> PostTarefa(Tarefa tarefa)
        {
            _context.Tarefas.Add(tarefa);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTarefa", new { id = tarefa.Id }, tarefa);
        }

        // DELETE: api/Tarefas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTarefa(int id)
        {
            var tarefa = await _context.Tarefas.FindAsync(id);
            if (tarefa == null)
            {
                return NotFound();
            }

            _context.Tarefas.Remove(tarefa);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TarefaExists(int id)
        {
            return _context.Tarefas.Any(e => e.Id == id);
        }


        /*
         * Cria uma tarefa associada a uma pessoa e retorna a tarefa criada
         **/
        [HttpPost("CriarTarefaPessoa/{pessoaId}/{dataInicio}")]
        public async Task<ActionResult<Tarefa>> CriarTarefaPessoa(int pessoaId, DateTime dataInicio)
        {
            Tarefa tarefa = new Tarefa();
            tarefa.PessoaId = pessoaId;
            tarefa.DataInicio = dataInicio;
            tarefa.FlagStatus = DictFlagStatus[1];

            _context.Tarefas.Add(tarefa);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTarefa", new { id = tarefa.Id }, tarefa);
        }

        /*
         * Finaliza a tarefa e retorna mensagem 
         **/
        [HttpPut("FinalizarTarefa/{id}")]
        public async Task<string> FinalizarTarefa(int id)
        {
            if (!TarefaExists(id))
            {
                return "Registro não encontrado.";
            }

            var tarefa = await _context.Tarefas.FindAsync(id);

            /*
             * Retorna mensagem se a tarefa já estiver finalizada 
             **/
            if (tarefa.DataFinalizacao is not null)
            {
                return "Esta tarefa já foi finalizada. Data da finalização: " + tarefa.DataFinalizacao + ".";
            }

            /*
             * Finaliza a tarefa setando a duração estimada (em dias)
             **/
            var now = DateTime.Now;
            tarefa.DataFinalizacao = now;
            tarefa.DuracaoEstimada = (now - tarefa.DataInicio).Days;
            tarefa.FlagStatus = DictFlagStatus[3];

            _context.Entry(tarefa).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return e.Message;
            }

            return "Tarefa finalizada com sucesso!";
        }

        /*
         * Inicia uma tarefa
         **/
        [HttpPut("IniciarTarefa/{id}")]
        public async Task<string> IniciarTarefa(int id)
        {
            if (!TarefaExists(id))
            {
                return "Registro não encontrado.";
            }

            var tarefa = await _context.Tarefas.FindAsync(id);
                        
            if (tarefa.FlagStatus != DictFlagStatus[1])
            {
                return "Status da tarefa não permite esta alteração. Status atual: " + tarefa.FlagStatus + ".";
            }

            /*
             * Inicializa a tarefa (altera status para "em andamento")
             **/
            tarefa.FlagStatus = DictFlagStatus[2];
            tarefa.DataInicioAndamento = DateTime.Now;

            _context.Entry(tarefa).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "Inicializaçao da tarefa efetuada com sucesso!";
        }

        /*
         * Retorna o período de tempo em dias em que a tarefa esteve em andamento 
         * ou mensagem com status atual caso não esteja finalizada.
         **/
        [HttpGet("GetTempoAtivoTarefa/{id}")]
        public async Task<ActionResult<string>> GetTempoAndamentoTarefa(int id)
        {
            var tarefa = await _context.Tarefas.FindAsync(id);
            var duracao = "";

            if (tarefa == null)
                return "Tarefa não encontrada.";

            if (tarefa.DataFinalizacao is null)
            {
                duracao = (DateTime.Now - tarefa.DataInicioAndamento).ToString();
                return "Esta tarefa encontra-se em andamento. Status atual: " + tarefa.FlagStatus + ". Tempo decorrido: " + duracao;
            }
            else
            {
                duracao = (tarefa.DataFinalizacao - tarefa.DataInicioAndamento).ToString();
                return "Esta farefa encontra-se encerrada. Tempo decorrido: " + duracao;
            }
        }

        [HttpPost("single-file")]
        public async Task<string> Upload(IFormFile file, int id)
        {
            if (!TarefaExists(id))
            {
                return "Tarefa não encontrada.";
            }

            if (file != null && file.Length > 0)
            {
                /*
                 * Grava o arquivo no diretório "uploads" 
                 **/
                var pasta = "uploads";
                var path = Path.Combine(_hostingEnvironment.ContentRootPath, pasta);
                string filePath = Path.Combine(path, file.FileName);

                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                /*
                 * Salva o caminho do arquivo na tarefa 
                 **/
                var tarefa = await _context.Tarefas.FindAsync(id);
                tarefa.FilePath = Path.Combine(pasta, file.FileName);
                _context.Entry(tarefa).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }

                return "Ficheiro anexado com sucesso!";
            }
            else 
            {
                return "Nenhum ficheiro carregado.";
            }
        }
    }
}
