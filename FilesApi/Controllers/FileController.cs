using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using FilesApi.Models;
using FilesApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FilesApi.Controllers
{
    [Route("api/{username}/files")]
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
        [RequestSizeLimit(100_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 209715200)]
        public IActionResult Post(string username)
        {
            var owner = HttpContext.User.Identity.Name;
            var files = Request.Form.Files;
            var file = files.FirstOrDefault();
            if(owner != username)
                return Forbid();

            var fileMetadata = fileService.Create(username, file);

            return Created(file.FileName,fileMetadata);
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetOwnFiles(string username)
        {
            var currentUser = HttpContext.User.Identity.Name;
            if(currentUser != username)
                return Forbid();
            return Ok(fileService.GetOwnFiles(username));
        }

        [Authorize]
        [HttpGet("shared")]
        public IActionResult GetSharedFiles(string username)
        {
            var currentUser = HttpContext.User.Identity.Name;
            if(currentUser != username)
                return Forbid();
            return Ok(fileService.GetSharedFiles(username));
        }

        [Authorize]
        [HttpGet("{filename}", Name = "GetFile")]
        public IActionResult GetFile(string username, string filename)
        {
            var fileMeta =fileService.GetFileMetadata(username, filename);

            if(fileMeta==null)
                return NotFound();
            
            var currentUser = HttpContext.User.Identity.Name;
            if(fileMeta.owner != currentUser && !fileMeta.sharedWith.Contains(currentUser))
                return Forbid();

            var file = fileService.GetFile(username, filename);

            return File(file, "application/octet-stream", filename);
        }

        [Authorize]
        [HttpDelete("{filename}", Name = "DeleteFile")]
        public IActionResult DeleteFile(string username, string filename)
        {
            fileService.DeleteFile(username, filename);
            return NoContent();
        }
        [Authorize]
        [HttpGet("{filename}/share")]
        public ActionResult<string> shareFile(string username, string filename, [Required] string share)
        {
            var owner = userService.GetByUsername(HttpContext.User.Identity.Name);
            if(owner.Username != username)
                return Forbid();
            
            var fileMeta = fileService.UpdateShares(username, filename, share);

            if(fileMeta == null)
                return NotFound("File not found");

            return Ok(fileMeta);

        }

    }
}