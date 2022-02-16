using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiTODOSwagger.Data
{
    public class Tarefa
    {
        public int Id { get; set; }

        public DateTime DataInicio { get; set; }

        public DateTime DataInicioAndamento { get; set; }

        /*
         * Duração estimada em horas 
         **/
        public int? DuracaoEstimada { get; set; }

        public DateTime? DataFinalizacao { get; set; }

        /*
         * Status: "Agendado", "Em andamento" ou "Finalizado"
         **/
        public string FlagStatus { get; set; }

        public string FilePath { get; set; }

        /*
         * Foreign Key para Pessoa
         **/
        public int PessoaId { get; set; }
        public Pessoa Pessoa { get; set; }
    }
}
