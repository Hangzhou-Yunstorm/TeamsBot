using Microsoft.AspNetCore.Mvc;
using Stanley.KB.Bot.SDP;
using System.Threading.Tasks;

namespace Stanley.KB.Bot.Controllers
{
    [Route("[controller]")]
    public class SolutionsController : Controller
    {
        private readonly SolutionHelper _solution;
        public SolutionsController(SolutionHelper solution)
        {
            _solution = solution;
        }

        [ResponseCache(Duration = 30 * 60)]
        [HttpGet, Route("{id}")]
        public async Task<IActionResult> Index(string id)
        {
            var response = await _solution.GetSolutionAsync(id);
            if (response?.response_status.status == "success")
            {
                return View(response.solution);
            }

            return Content($"NotFound solution {id}!");
        }
    }
}
