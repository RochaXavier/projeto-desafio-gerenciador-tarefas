using Data.Contratos;

namespace GerenciamentoTarefas.Services
{
    public interface ISenderMessageService
    {
        Task<bool> EnqueueAsync(Tarefa tarefa);
    }
}
