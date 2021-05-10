using System.IO;
using FilesApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FilesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly FileService fileService;
        public FileController(FileService _fileService){
            fileService = _fileService;
        }

        [HttpPost]
        public ActionResult<string> Post(IFormFile file)
        {
            fileService.Create(file.FileName,file);
            return Created(file.FileName,file.FileName);
        }

        [HttpGet("{filename}", Name = "GetFile")]
        public FileResult GetFile(string filename)
        {
            var file = fileService.GetFile(filename);
            return File(file, "application/octet-stream", filename);
        }
    }
}