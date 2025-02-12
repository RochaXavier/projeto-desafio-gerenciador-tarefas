using Data.Enum;

namespace Data.Contratos
{
    public class Tarefa
    {
        public Guid Id { get; set; }
        public TipoTarefas TipoTarefa { get; set; }
        public Dictionary<string, string> Parametros { get; set; }

    }
}
