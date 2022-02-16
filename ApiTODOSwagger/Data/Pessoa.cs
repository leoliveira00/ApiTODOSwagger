using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiTODOSwagger.Data
{
    public class Pessoa
    {
        public int Id { get; set; }

        public string Nome { get; set; }

        public ICollection<Tarefa> Tarefas { get; set; }
    }
}
