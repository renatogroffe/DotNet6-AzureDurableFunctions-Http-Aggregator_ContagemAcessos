using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json.Linq;

namespace FunctionAppAgregacaoContagem;

public static class TestesInteger
{
    private const string ENTITY_KEY = "contador";

    [FunctionName("IncrementarInteger")]
    public static async Task<HttpResponseMessage> Incrementar(
    [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "contador/incrementar")] HttpRequestMessage req,
    [DurableClient] IDurableEntityClient client)
    {
        await client.SignalEntityAsync(new EntityId(nameof(ContadorInteger), entityKey: ENTITY_KEY),
            operationName: "incrementar", operationInput: 1);
        return req.CreateResponse(HttpStatusCode.OK);
    }

    [FunctionName("ConsultarStatusInteger")]
    public static async Task<IActionResult> Consultar(
    [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "contador")] HttpRequestMessage req,
    [DurableClient] IDurableEntityClient client)
    {
        var stateResponse = await client.ReadEntityStateAsync<JToken>(
            new EntityId(nameof(ContadorInteger), entityKey: ENTITY_KEY));
        return new OkObjectResult(stateResponse);
    }


    [FunctionName("ResetarStatusInteger")]
    public static async Task<HttpResponseMessage> Resetar(
    [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "contador/reset")] HttpRequestMessage req,
    [DurableClient] IDurableEntityClient client)
    {
        await client.SignalEntityAsync(new EntityId(nameof(ContadorInteger), entityKey: ENTITY_KEY),
            operationName: "reset");
        return req.CreateResponse(HttpStatusCode.OK);
    }

    [FunctionName(nameof(ContadorInteger))]
    public static void ContadorInteger([EntityTrigger] IDurableEntityContext context)
    {
        switch (context.OperationName.ToLowerInvariant())
        {
            case "incrementar":
                context.SetState(context.GetState<int>() + context.GetInput<int>());
                break;
            case "reset":
                context.SetState(0);
                break;
            case "get":
                context.Return(context.GetState<int>());
                break;
        }
    }
}