using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json.Linq;

namespace FunctionAppAgregacaoContagem;

public static class TestesObjeto
{
    private const string ENTITY_KEY = "objeto";

    [FunctionName("IncrementarObjeto")]
    public static async Task<HttpResponseMessage> Incrementar(
    [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "objeto/incrementar")] HttpRequestMessage req,
    [DurableClient] IDurableEntityClient client)
    {
        await client.SignalEntityAsync(new EntityId(nameof(ContadorObjeto), entityKey: ENTITY_KEY),
            operationName: "incrementar", operationInput: 1);
        return req.CreateResponse(HttpStatusCode.OK);
    }

    [FunctionName("ConsultarStatusObjeto")]
    public static async Task<IActionResult> Consultar(
    [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "objeto")] HttpRequestMessage req,
    [DurableClient] IDurableEntityClient client)
    {
        var stateResponse = await client.ReadEntityStateAsync<JToken>(
            new EntityId(nameof(ContadorObjeto), entityKey: ENTITY_KEY));
        return new OkObjectResult(stateResponse);
    }


    [FunctionName("ResetarStatusObjeto")]
    public static async Task<HttpResponseMessage> Resetar(
    [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "objeto/reset")] HttpRequestMessage req,
    [DurableClient] IDurableEntityClient client)
    {
        await client.SignalEntityAsync(new EntityId(nameof(ContadorObjeto), entityKey: ENTITY_KEY),
            operationName: "reset");
        return req.CreateResponse(HttpStatusCode.OK);
    }

    [FunctionName(nameof(ContadorObjeto))]
    public static void ContadorObjeto([EntityTrigger] IDurableEntityContext context)
    {
        switch (context.OperationName.ToLowerInvariant())
        {
            case "incrementar":
                var resultado = context.GetState<ResultadoContador>() ?? new ResultadoContador();
                resultado.ValorAtual += context.GetInput<int>();
                resultado.UltimaAtualizacao = DateTime.Now;
                context.SetState(resultado);
                break;
            case "reset":
                context.SetState(new ResultadoContador());
                break;
            case "get":
                context.Return(context.GetState<ResultadoContador>());
                break;
        }
    }

    public class ResultadoContador
    {
        public DateTime HorarioInicializacao { get; init; } = DateTime.Now;
        public DateTime? UltimaAtualizacao { get; set; } = null;
        public int ValorAtual { get; set; }
    }
}