using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using FilesApi.Models;
using FilesApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FilesApi.Controllers
{
    [Route("api/{username}/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly FileService fileService;
        private readonly UserService userService;
        public FileController(FileService _fileService, UserService _userService){
            fileService = _fileService;
            userService = _userService;
        }

        [Authorize]
        [HttpPost]
        public ActionResult<string> Post(string username, IFormFile file)
        {
            var owner = HttpContext.User.Identity.Name;
            if(owner != username)
                return Forbid();

            fileService.Create(username, file);

            return Created(file.FileName,file.FileName);
        }

        [HttpGet("{filename}", Name = "GetFile")]
        public FileResult GetFile(string username, string filename)
        {
            var file = fileService.GetFile(username, filename);
            return File(file, "application/octet-stream", filename);
        }

        // [Authorize]
        // [HttpPost("{filename}/share")]
        // public ActionResult<string> shareFile(string username, string filename, [Required] string share)
        // {
        //     var owner = userService.GetByUsername(HttpContext.User.Identity.Name);
        //     if(owner.Username != username)
        //         return Forbid();
            

        // }

    }
}