using Microsoft.AspNetCore.Mvc;
using VegaExpress.Worker.Core.Services;

namespace VegaExpress.Worker.Client.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StorageController : ControllerBase
    {
        private readonly LocalStorageService _storageService;

        public StorageController(LocalStorageService storageService)
        {
            _storageService = storageService;
        }

        // POST api/storage/init
        [HttpPost("init")]
        public async Task<IActionResult> Init([FromBody] string repositoryID)
        {
            try
            {
                await _storageService.InitRepository(repositoryID);
                return Ok("Repository initialized.");
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        // POST api/storage/clone
        [HttpPost("clone")]
        public async Task<IActionResult> Clone([FromBody] string repoUrl)
        {
            return Ok($"Repository cloned from {repoUrl}");
        }
        // POST api/storage/add
        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] string filePath)
        {
            return Ok($"File {filePath} added.");
        }
        // POST api/storage/commit
        [HttpPost("commit")]
        public  async Task<IActionResult> Commit([FromBody] string message)
        {
            return Ok($"Committed with message: {message}.");
        }
        // POST api/storage/push
        [HttpPost("push")]
        public async Task<IActionResult> Push()
        {
            return Ok("Changes pushed.");
        }

        // POST api/storage/pull
        [HttpPost("pull")]
        public async Task<IActionResult> Pull()
        {
            return Ok("Changes pulled.");
        }

        // POST api/storage/branch
        [HttpPost("branch")]
        public async Task<IActionResult> Branch([FromBody] string branchName)
        {
            return Ok($"Branch {branchName} created.");
        }

        // POST api/storage/checkout
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] string branchName)
        {
            return Ok($"Checked out to branch {branchName}");
        }

        // POST api/storage/merge
        [HttpPost("merge")]
        public async Task<IActionResult> Merge([FromBody] string branchName)
        {
            return Ok($"Merged with branch {branchName}");
        }

        // GET api/storage/status
        [HttpGet("status")]
        public async Task<IActionResult> Status()
        {
            return Ok("Respository status");
        }
    }
}
