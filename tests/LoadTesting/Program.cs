using NBomber.Contracts;
using NBomber.CSharp;
using System.Threading.Tasks;

namespace LoadTesting
{

    class Program
    {
        static void Main(string[] args)
        {
            var step = Step.Create("step", async context =>
            {
                await Task.Delay(1);
                return Response.Ok();
            });

            var scenario = ScenarioBuilder.CreateScenario("hello_world", step);

            NBomberRunner
                .RegisterScenarios(scenario)
                .Run();
        }
    }
}
