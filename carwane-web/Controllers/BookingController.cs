using Carwane.Web.Data;
using Carwane.Web.Models;
using Carwane.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Carwane.Web.Controllers;

public class BookingController : Controller
{
    private readonly IFareProvider _fares;
    private readonly AppDbContext _db;

    public BookingController(IFareProvider fares, AppDbContext db)
    {
        _fares = fares; _db = db;
    }

    [HttpGet("/Booking/Create/{id:int}")]
    public IActionResult Create(int id, int travellers = 1, string depart = "2026-12-18", string ret = "2027-01-04", string cabin = "Economy")
    {
        var _dep = DateOnly.TryParse(depart, out var _dd) ? _dd : new DateOnly(2026, 12, 18);
        var _rt = DateOnly.TryParse(ret, out var _rr) ? _rr : new DateOnly(2027, 1, 4);
        var offer = _fares.GetOffer(id, _dep, _rt);
        if (offer is null) return NotFound();
        var dep = DateOnly.TryParse(depart, out var d) ? d : offer.DepartDate;
        var rt = DateOnly.TryParse(ret, out var r) ? r : offer.ReturnDate;
        offer.DepartDate = dep; offer.ReturnDate = rt; offer.Cabin = cabin;
        var fees = _db.FeeSettings.FirstOrDefault(f => f.IsCurrent) ?? new FeeSetting();
        ViewBag.Breakdown = new PricingService().Calculate(offer, fees, travellers);
        ViewBag.Travellers = travellers;
        ViewBag.Fees = fees;
        return View(offer);
    }

    [HttpPost("/Booking/Create/{id:int}")]
    [ValidateAntiForgeryToken]
    public IActionResult CreatePost(int id, int travellers, string name, string email, string phone, string paymentMethod = "Card", string depart = "2026-12-18", string ret = "2027-01-04")
    {
        var _dep = DateOnly.TryParse(depart, out var _dd) ? _dd : new DateOnly(2026, 12, 18);
        var _rt = DateOnly.TryParse(ret, out var _rr) ? _rr : new DateOnly(2027, 1, 4);
        var offer = _fares.GetOffer(id, _dep, _rt);
        if (offer is null) return NotFound();
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone))
        {
            ModelState.AddModelError("", "Please enter the lead passenger's name and phone number.");
            var fees0 = _db.FeeSettings.FirstOrDefault(f => f.IsCurrent) ?? new FeeSetting();
            ViewBag.Breakdown = new PricingService().Calculate(offer, fees0, travellers);
            ViewBag.Travellers = travellers; ViewBag.Fees = fees0;
            return View("Create", offer);
        }
        var fees = _db.FeeSettings.FirstOrDefault(f => f.IsCurrent) ?? new FeeSetting();
        var b = new PricingService().Calculate(offer, fees, travellers, paymentMethod);
        var reference = "CTM-" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()
                              + "-" + Guid.NewGuid().ToString("N")[..4].ToUpperInvariant();
        var booking = new Booking
        {
            Reference = reference, FlightOfferIds = offer.Id.ToString(), Airline = offer.Airline,
            DepartDate = offer.DepartDate, ReturnDate = offer.ReturnDate,
            Travellers = travellers, Cabin = offer.Cabin,
            BaseFarePkr = b.BaseFarePkr * travellers, TaxesPkr = b.TaxesPkr * travellers,
            ServiceFeePkr = fees.ServiceFeeMode == "flat_per_booking" ? b.ServiceFeePkr : b.ServiceFeePkr * travellers,
            SupplierFeePkr = b.SupplierFeePkr * travellers, PaymentChargePkr = b.PaymentChargePkr * travellers,
            DiscountPkr = b.DiscountPkr * travellers, TotalPkr = b.TotalPayable,
            PassengerName = name.Trim(), PassengerEmail = email?.Trim() ?? "", PassengerPhone = phone.Trim(),
            PaymentMethod = paymentMethod, IsDemo = true
        };
        _db.Bookings.Add(booking);
        _db.SaveChanges();
        return RedirectToAction("Confirmation", new { reference });
    }

    [HttpGet("/Booking/Confirmation")]
    public IActionResult Confirmation(string reference)
    {
        var booking = _db.Bookings.FirstOrDefault(b => b.Reference == reference);
        if (booking is null) return NotFound("Booking not found.");
        return View(booking);
    }
}
