using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace CancellableWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly List<string> _products = new List<string>() { "apple" };

        /// <summary>
        /// This action waits before returning the list of products
        /// </summary>
        /// <param name="waitTime">Wait time in miliseconds</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The list of products</returns>
        [HttpGet]
        public async Task<IEnumerable<string>> Get(int waitTime, CancellationToken cancellationToken)
        {
            Thread.Sleep(waitTime);

            if (cancellationToken.IsCancellationRequested)
                throw new TaskCanceledException();

            return await Task.FromResult(_products);
        }
    }
}
