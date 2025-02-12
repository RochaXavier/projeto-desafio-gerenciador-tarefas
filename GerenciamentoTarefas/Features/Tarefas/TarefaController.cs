using GerenciamentoTarefas.Features.Tarefas.Command;
using GerenciamentoTarefas.Features.Tarefas.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GerenciamentoTarefas.Features.Tarefas
{
    [ApiController]
    [Route("[controller]")]
    public class TarefaController : ControllerBase
    {
        private readonly IMediator mediator;

        public TarefaController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        /// <summary>
        /// Endpoint que possibilita a criação de uma nova tarefa
        /// </summary>
        [HttpPost("CriarTarefa")]
        public async Task<ActionResult<CriarTarefa.Result>> CriarTarefa([FromBody] CriarTarefa.Command request) => Ok(await mediator.Send(request));

        /// <summary>
        /// Endpoint para consulta de status de tarefas
        /// </summary>
        [HttpGet("ConsultarTarefa")]
        public async Task<ActionResult<ConsultarTarefa.Result>> ConsultarTarefa([FromQuery] ConsultarTarefa.Query request) => Ok(await mediator.Send(request));


    }
}
