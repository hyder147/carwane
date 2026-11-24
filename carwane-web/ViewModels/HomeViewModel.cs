using Carwane.Web.Models;

namespace Carwane.Web.ViewModels;

public class HomeViewModel
{
    public string From { get; set; } = "KHI";
    public string To { get; set; } = "JED";
    public DateOnly Depart { get; set; } = new(2026, 12, 18);
    public DateOnly Return { get; set; } = new(2027, 1, 4);
    public int Travellers { get; set; } = 1;
    public string Cabin { get; set; } = "Economy";
    public string TripType { get; set; } = "Round trip";
    public IReadOnlyList<FareCalendarDay> Calendar { get; set; } = Array.Empty<FareCalendarDay>();
    public IReadOnlyList<FlightOffer> Offers { get; set; } = Array.Empty<FlightOffer>();
    public FeeSetting Fees { get; set; } = new();
    public IReadOnlyList<ZiyaratPlace> Ziyarat { get; set; } = Array.Empty<ZiyaratPlace>();
    public string? Notice { get; set; }
    public string? EnquiryReference { get; set; }
    public string? AlertConfirmation { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public decimal SarToPkr { get; set; } = 73.76m;
}
