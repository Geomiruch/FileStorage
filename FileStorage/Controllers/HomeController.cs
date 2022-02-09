using FileStorage.Data;
using FileStorage.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FileStorage.Controllers
{
    public class HomeController : Controller
    {
        ApplicationDbContext context;
        private UserManager<IdentityUser> _userManager;
        IWebHostEnvironment _appEnvironment;
        string filepath = "C:/Users/Welcome/source/repos/FileStorage/FileStorage/wwwroot/Files";

        public HomeController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment appEnvironment)
        {
            this.context = context;
            _userManager = userManager;
            _appEnvironment = appEnvironment;
        }
        
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var files = await context.Files.ToListAsync();
            return View(files);
        }
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(IFormFile uploadedFile)
        {
            if (uploadedFile != null)
            {
                // путь к папке Files
                string path = "/Files/" + uploadedFile.FileName;
                // сохраняем файл в папку Files в каталоге wwwroot
                using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }
                var user = await _userManager.GetUserAsync(User);
                FileModel file = new FileModel { Name = uploadedFile.FileName, Path = path, Owner = user.Email };
                context.Files.Add(file);
                await context.SaveChangesAsync();
            }
            return RedirectToAction("List");
        }
       
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id != null)
            {
                var user = await _userManager.GetUserAsync(User);
                FileModel file = await context.Files.FirstOrDefaultAsync(p => p.Id == id);
                if(file.Owner==user.Email || User.IsInRole("admin"))
                {
                    if (file != null)
                    {
                        FileInfo fileInf = new FileInfo(file.Path);
                        if (fileInf.Exists)
                        {
                            fileInf.CopyTo(filepath+"/temp***", true);
                            fileInf.Delete();
                        }
                        return View(file);
                    }
                }
                return RedirectToAction("OwnerError");
            }
            return NotFound();
        }
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> Edit(FileModel file)
        {
            context.Files.Update(file);
            FileInfo fileInf = new FileInfo(filepath+"/temp***");
            if (fileInf.Exists)
            {
                fileInf.CopyTo(filepath+"/"+file.Name, true);
                fileInf.Delete();
            }
            await context.SaveChangesAsync();
            return RedirectToAction("List");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id != null)
            {
                FileModel file = await context.Files.FirstOrDefaultAsync(p => p.Id == id);
                if (file != null)
                    return View(file);
            }
            return NotFound();
        }
        [Authorize]
        [HttpGet]
        [ActionName("Delete")]
        public async Task<IActionResult> ConfirmDelete(int? id)
        {
            if (id != null)
            {
                var user = await _userManager.GetUserAsync(User);
                FileModel file = await context.Files.FirstOrDefaultAsync(p => p.Id == id);
                if (file.Owner == user.Email || User.IsInRole("admin"))
                {
                    if (file != null)
                        return View(file);
                }
                return RedirectToAction("OwnerError");
            }
            return NotFound();
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id != null)
            {
                FileModel file = await context.Files.FirstOrDefaultAsync(p => p.Id == id);
                if (file != null)
                {
                    FileInfo fileInf = new FileInfo(file.Path);
                    if (fileInf.Exists)
                    {
                        fileInf.Delete();
                    }
                    context.Files.Remove(file);
                    await context.SaveChangesAsync();
                    return RedirectToAction("List");
                }
            }
            return NotFound();
        }

        public IActionResult OwnerError()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
