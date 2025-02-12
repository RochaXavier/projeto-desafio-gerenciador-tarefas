using Data.Enum;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entidade
{
    [Table("tarefa")]
    public class Tarefa
    {
        public Guid Id { get; set; }
        public string TipoTarefa { get; set; }
        public Dictionary<string, string> Parametros { get; set; }
        public DateTime DataGravacao { get; set; } = DateTime.Now;
        public DateTime? DataUltimaAlteracao { get; set; }
        public StatusTarefa Status { get; set; } = StatusTarefa.Pendente;
    }
}
