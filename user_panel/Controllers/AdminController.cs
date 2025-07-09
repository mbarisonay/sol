using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using user_panel.Data;
using user_panel.Services.Entity.ApplicationUserServices;
using user_panel.Services.Entity.CabinServices;
using user_panel.ViewModels;

namespace user_panel.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IApplicationUserService _userService;
        private readonly ICabinService _cabinService;

        public AdminController(IApplicationUserService userService, ICabinService cabinService)
        {
            _userService = userService;
            _cabinService = cabinService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AddCabin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddCabin(CabinInfoViewModel model)
        {
            if (ModelState.IsValid)
            {
                var newCabin = new Cabin
                {
                    Location = model.Location,
                    Description = model.Description,
                    PricePerHour = model.PricePerHour
                };
                await _cabinService.CreateAsync(newCabin);

                TempData["StatusMessage"] = "Cabin added successfully!";
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateCabin()
        {
            var cabins = await _cabinService.GetAllAsync();
            return View(cabins);
        }

        [HttpGet]
        public async Task<IActionResult> EditCabin(int id)
        {
            ViewData["Title"] = "Edit Cabin";
            var cabin = await _cabinService.GetByIdAsync(id);

            if (cabin == null)
            {
                TempData["StatusMessage"] = "Error: Cabin not found!";
                return RedirectToAction("UpdateCabin");
            }

            return View(cabin);
        }

        [HttpPost]
        public async Task<IActionResult> EditCabin(Cabin model)
        {
            ViewData["Title"] = "Edit Cabin";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _cabinService.UpdateAsync(model);

            TempData["StatusMessage"] = $"Cabin '{model.Location}' updated successfully!";
            return RedirectToAction("UpdateCabin");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCabin(int id)
        {
            var cabinToDelete = await _cabinService.GetByIdAsync(id);
            if (cabinToDelete == null)
            {
                TempData["StatusMessage"] = "Error: Cabin not found for deletion.";
                return RedirectToAction("UpdateCabin");
            }

            await _cabinService.DeleteAsync(id);

            TempData["StatusMessage"] = $"Cabin '{cabinToDelete.Location}' successfully deleted.";
            return RedirectToAction("UpdateCabin");
        }
    }
}
