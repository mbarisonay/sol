using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using user_panel.Settings;

public class HomeController : Controller

{

    private readonly ILogger<HomeController> _logger;

    private readonly GoogleMapsSettings _mapsSettings;

    public HomeController(ILogger<HomeController> logger, IOptions<GoogleMapsSettings> mapsOptions)

    {

        _logger = logger;

        _mapsSettings = mapsOptions.Value;

    }

    public IActionResult Index()

    {

        ViewData["GoogleMapsApiKey"] = _mapsSettings.ApiKey;

        return View();

    }

}
