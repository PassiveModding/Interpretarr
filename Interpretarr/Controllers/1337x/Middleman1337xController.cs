using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using Interpretarr.Model;
using Interpretarr.Clients.FlareSolver.Model;
using Interpretarr.Clients.FlareSolver;
using System.IO;

namespace Interpretarr.Controllers
{

    [ApiController]
    [Route("/1337xMiddleman/")]
    public class Middleman1337xController : ControllerBase
    {
        private readonly FlareSolverClient flareSolverClient;
        private readonly IEnumerable<IMiddlemanHelper> middlemanHelpers;
        private const string websiteUrl = "https://1337x.to";

        public Middleman1337xController(FlareSolverClient flareSolverClient, IEnumerable<IMiddlemanHelper> middlemanHelpers)
        {
            this.flareSolverClient = flareSolverClient;
            this.middlemanHelpers = middlemanHelpers.Where(x => x.SupportedSite == SupportedSite.x1337xTo);
        }

        [HttpGet("{*path}")]
        public async Task<IActionResult> GetAsync(string path)
        {
            Console.WriteLine($"GET {path}");
            foreach (var middlemanHelper in middlemanHelpers)
            {
                if (middlemanHelper.HandleRequest(path, out var updatedPath))
                {
                    // log the helper that handled the request
                    Console.WriteLine($"GET {path} handled by {middlemanHelper.GetType().Name} => {updatedPath}");
                    var url = $"{websiteUrl}{(updatedPath.StartsWith('/') ? updatedPath : '/' + updatedPath)}";
                    var flareSolverrResponse = await flareSolverClient.GetAsync(url);
                    var response = await flareSolverClient.ParseResponseAsync(flareSolverrResponse, (updatedPath, middlemanHelper));
                    return response;
                }
            }

            return await HandleAsync(path);
        }

        private async Task<IActionResult> HandleAsync(string path)
        {
            var url = $"{websiteUrl}{(path.StartsWith('/') ? path : '/' + path)}";
            var fsResponse = await flareSolverClient.GetAsync(url);
            var response = await flareSolverClient.ParseResponseAsync(fsResponse, null);
            return response;
        }
    }
}
