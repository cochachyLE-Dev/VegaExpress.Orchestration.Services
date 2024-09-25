using Microsoft.AspNetCore.Mvc;

using VegaExpress.Worker.Client.Models;
using VegaExpress.Worker.Core.Common;

namespace VegaExpress.Worker.Client.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController :ControllerBase
    {
        private readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

        public FileController()
        {
            if (Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        // POST: api/file/upload
        [HttpPost("upload")]
        public IActionResult UploadFile([FromBody] FileUploadModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.FileContent))
                    return BadRequest("No file content provided.");

                // Decodificar el contenido base64
                byte[] fileBytes = Convert.FromBase64String(model.FileContent);

                var filePath = Path.Combine(_uploadPath, model.FileHash!);

                SaveBinaryFile(filePath, fileBytes);

                return Ok(new Response { Message = "File uploaded successfully" });
            }
            catch (FormatException)
            {
                return BadRequest("Invalid Base64 content");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
            void SaveBinaryFile(string path, byte[] data)
            {
                using (BinaryWriter writer = new BinaryWriter(System.IO.File.Open(path, FileMode.OpenOrCreate)))
                {
                    writer.Write(data);
                }
            }
        }

        // POST: api/file/status
        [HttpPost("status")]
        public IActionResult StatusFiles()
        {
            try
            {
                var rootPath = Path.Combine(_uploadPath, DateTime.Now.ToString("dd-MM-yyyy"));
                return Ok(GetFilesInDirectory(_uploadPath));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
           IEnumerable<string> GetFilesInDirectory(string path)
            {
                if(!Directory.Exists(path))
                    throw new DirectoryNotFoundException($"Directory not found: {path}");

                var files = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    yield return Path.GetFileName(file);
                }
            }
        }

        // GET: api/file/download/{fileName}
        [HttpGet("download/{fileName}")]
        public IActionResult DownloadFile(string fileName)
        {
            var filePath = Path.Combine(_uploadPath, fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound("File not found.");

            try
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader reader = new BinaryReader(fileStream))
                    {
                        byte[] fileBytes = reader.ReadBytes((int)fileStream.Length);
                        string fileContent = Convert.ToBase64String(fileBytes);

                        return Ok(new FileDownloadModel { FileHash = fileName, FileContent = fileContent });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
