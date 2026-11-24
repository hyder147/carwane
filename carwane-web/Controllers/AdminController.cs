using Carwane.Web.Data;
using Carwane.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Carwane.Web.Controllers;

public class AdminController : Controller
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private const string SessionKey = "AdminAuthed";

    public AdminController(AppDbContext db, IConfiguration config)
    {
        _db = db; _config = config;
    }

    private bool IsAuthed => HttpContext.Session.GetString(SessionKey) == "yes";

    [HttpGet("/Admin/Login")]
    public IActionResult Login() => View();

    [HttpPost("/Admin/Login")]
    [ValidateAntiForgeryToken]
    public IActionResult LoginPost(string passcode)
    {
        var expected = _config["Admin:Passcode"] ?? "admin123";
        if (passcode == expected)
        {
            HttpContext.Session.SetString(SessionKey, "yes");
            return RedirectToAction("Index");
        }
        ModelState.AddModelError("", "Wrong passcode.");
        return View("Login");
    }

    [HttpGet("/Admin/Logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove(SessionKey);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet("/Admin")]
    public IActionResult Index()
    {
        if (!IsAuthed) return RedirectToAction("Login");
        ViewBag.Fees = _db.FeeSettings.FirstOrDefault(f => f.IsCurrent) ?? new FeeSetting();
        ViewBag.Audit = _db.FeeSettingAudits.OrderByDescending(a => a.ChangedAt).Take(20).ToList();
        ViewBag.Bookings = _db.Bookings.OrderByDescending(b => b.CreatedAt).Take(20).ToList();
        ViewBag.Enquiries = _db.Enquiries.OrderByDescending(e => e.CreatedAt).Take(20).ToList();
        ViewBag.Alerts = _db.FareAlerts.OrderByDescending(a => a.CreatedAt).Take(20).ToList();
        return View();
    }

    [HttpPost("/Admin/Fees")]
    [ValidateAntiForgeryToken]
    public IActionResult SaveFees(string serviceFeeMode, int serviceFeeAmountPkr, int serviceFeePercent,
        bool supplierFeesEnabled, int supplierFeeAmountPkr,
        bool cardChargeEnabled, int cardChargeAmountPkr, int cardChargePercent,
        bool discountActive, string? discountCode, string discountType, int discountAmountPkr, int discountPercent,
        bool isDemo)
    {
        if (!IsAuthed) return RedirectToAction("Login");
        if (serviceFeeAmountPkr < 0 || serviceFeePercent is < 0 or > 100 || discountPercent is < 0 or > 100 || discountAmountPkr < 0)
        {
            TempData["AdminError"] = "Invalid values: amounts must be 0 or more, percents between 0 and 100.";
            return RedirectToAction("Index");
        }
        var current = _db.FeeSettings.FirstOrDefault(f => f.IsCurrent);
        if (current is not null) current.IsCurrent = false;
        var nextVersion = (current?.Version ?? 0) + 1;
        var next = new FeeSetting
        {
            Version = nextVersion, IsCurrent = true, IsDemo = isDemo,
            ServiceFeeMode = serviceFeeMode, ServiceFeeAmountPkr = serviceFeeAmountPkr, ServiceFeePercent = serviceFeePercent,
            SupplierFeesEnabled = supplierFeesEnabled, SupplierFeeAmountPkr = supplierFeeAmountPkr,
            CardChargeEnabled = cardChargeEnabled, CardChargeAmountPkr = cardChargeAmountPkr, CardChargePercent = cardChargePercent,
            DiscountActive = discountActive, DiscountCode = discountCode, DiscountType = discountType,
            DiscountAmountPkr = discountAmountPkr, DiscountPercent = discountPercent,
            ChangedBy = "admin", ChangedAt = DateTime.UtcNow, EffectiveFrom = DateTime.UtcNow
        };
        _db.FeeSettings.Add(next);
        _db.FeeSettingAudits.Add(new FeeSettingAudit
        {
            Version = nextVersion, ChangedBy = "admin", ChangedAt = DateTime.UtcNow,
            Summary = $"v{nextVersion}: service fee {serviceFeeMode} {(serviceFeeMode == "percent_of_base" ? serviceFeePercent + "%" : "PKR " + serviceFeeAmountPkr.ToString("N0"))}, supplier fees {(supplierFeesEnabled ? "on" : "off")}, card charge {(cardChargeEnabled ? "on" : "off")}, discount {(discountActive ? "on" : "off")}, demo={isDemo}."
        });
        _db.SaveChanges();
        TempData["AdminNotice"] = $"Fee settings saved as version {nextVersion}. All public fares now use these values.";
        return RedirectToAction("Index");
    }
}
