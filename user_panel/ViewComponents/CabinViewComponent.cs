using Microsoft.AspNetCore.Mvc;
using user_panel.Services.Entity.CabinServices;

namespace user_panel.ViewComponents {
    public class CabinViewComponent : ViewComponent {
        readonly ICabinService _cabinService;

        public CabinViewComponent(ICabinService cabinService) {
            _cabinService = cabinService;
        }

        public async Task<IViewComponentResult> InvokeAsync() {
            var cabins = await _cabinService.GetCabinsWithLocationAsync();
            return View(cabins);
        }
    }
}