using Carwane.Web.Data;
using Carwane.Web.Models;
using Carwane.Web.Services;
using Carwane.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Carwane.Web.Controllers;

public class HomeController : Controller
{
    private readonly IFareProvider _fares;
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public HomeController(IFareProvider fares, AppDbContext db, IConfiguration config)
    {
        _fares = fares; _db = db; _config = config;
    }

    [HttpGet("/")]
    [HttpGet("/Home/Index")]
    public IActionResult Index(string from = "KHI", string to = "JED", string depart = "2026-12-18",
        string ret = "2027-01-04", int travellers = 1, string cabin = "Economy", string? notice = null)
    {
        if (travellers < 1) travellers = 1;
        if (travellers > 9) travellers = 9;
        var dep = DateOnly.TryParse(depart, out var d) ? d : new DateOnly(2026, 12, 18);
        var rt = DateOnly.TryParse(ret, out var r) ? r : new DateOnly(2027, 1, 4);
        if (dep.Year != 2026 || dep.Month != 12) dep = new DateOnly(2026, 12, 18);
        var fees = _db.FeeSettings.FirstOrDefault(f => f.IsCurrent) ?? new FeeSetting();
        // Mockup calendar totals already include the demo service fee (PKR 3,000).
        // Re-price the calendar with the CURRENT fee so an admin fee change moves
        // the calendar exactly like it moves the flight cards.
        var calendar = _fares.GetDecemberCalendar();
        if (fees.ServiceFeeMode == "flat_per_traveller")
        {
            var delta = fees.ServiceFeeAmountPkr - 3000;
            if (delta != 0)
            {
                calendar = calendar.Select(d => new FareCalendarDay
                {
                    Id = d.Id, Date = d.Date,
                    LowestRoundTripTotalPkr = d.LowestRoundTripTotalPkr + delta,
                    IsLowest = d.IsLowest, IsDemo = d.IsDemo
                }).ToList();
            }
        }
        var vm = new HomeViewModel
        {
            From = from, To = to, Depart = dep, Return = rt,
            Travellers = travellers, Cabin = cabin,
            Calendar = calendar,
            Offers = _fares.GetOffers(from, to, dep, rt, travellers, cabin),
            Fees = fees,
            Ziyarat = _db.ZiyaratPlaces.OrderBy(z => z.Id).ToList(),
            Notice = notice,
            SarToPkr = _config.GetValue<decimal>("Site:SarToPkrRate", 73.76m),
            GeneratedAt = DateTime.UtcNow
        };
        return View(vm);
    }

    // Flight details drawer content — fetched by JS so open/close/re-open is reliable.
    [HttpGet("/Home/FlightDetails/{id:int}")]
    public IActionResult FlightDetails(int id, int travellers = 1, string depart = "2026-12-18", string ret = "2027-01-04")
    {
        var _dep = DateOnly.TryParse(depart, out var _dd) ? _dd : new DateOnly(2026, 12, 18);
        var _rt = DateOnly.TryParse(ret, out var _rr) ? _rr : new DateOnly(2027, 1, 4);
        var offer = _fares.GetOffer(id, _dep, _rt);
        if (offer is null) return NotFound("Flight not found.");
        var dep = DateOnly.TryParse(depart, out var d) ? d : offer.DepartDate;
        var rt = DateOnly.TryParse(ret, out var r) ? r : offer.ReturnDate;
        offer.DepartDate = dep; offer.ReturnDate = rt;
        var fees = _db.FeeSettings.FirstOrDefault(f => f.IsCurrent) ?? new FeeSetting();
        var pricing = new PricingService();
        ViewBag.Breakdown = pricing.Calculate(offer, fees, travellers);
        ViewBag.Travellers = travellers;
        return PartialView("_FlightDetails", offer);
    }

    [HttpPost("/Home/FareAlert")]
    [ValidateAntiForgeryToken]
    public IActionResult FareAlert(int targetPrice, string contact, string channel = "WhatsApp")
    {
        if (targetPrice <= 0 || string.IsNullOrWhiteSpace(contact))
            return RedirectToAction("Index", new { notice = "Please enter a target price and a WhatsApp number or email for the fare alert." });
        _db.FareAlerts.Add(new FareAlert
        {
            Route = "KHI-JED", TargetPricePkr = targetPrice, Contact = contact.Trim(), Channel = channel
        });
        _db.SaveChanges();
        return RedirectToAction("Index", new { notice = $"Fare alert set — we will message you on {channel} when a KHI–JED round trip drops to {Money.Pkr(targetPrice)} or below. Fares are not guaranteed until ticketed." });
    }

    [HttpPost("/Home/Enquiry")]
    [ValidateAntiForgeryToken]
    public IActionResult Enquiry(string kind, string name, string phone, string? email, string? durationDays, string? message)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone))
            return RedirectToAction("Index", new { notice = "Please enter your name and phone number so we can call you back." });
        var reference = (kind == "Hajj" ? "HJ-" : "UM-") + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        _db.Enquiries.Add(new Enquiry
        {
            Reference = reference, Kind = kind, Name = name.Trim(), Phone = phone.Trim(),
            Email = email?.Trim(), DurationDays = durationDays, Message = message?.Trim()
        });
        _db.SaveChanges();
        return RedirectToAction("Index", new { notice = $"Thank you, {name.Trim()} — your enquiry is saved. Your reference is {reference}. Our team will call you back. (Demo: no message is sent yet.)" });
    }

    [HttpGet("/Home/Error")]
    public IActionResult Error() => View();
}
