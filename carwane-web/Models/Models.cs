using System.ComponentModel.DataAnnotations;

namespace Carwane.Web.Models;

/// <summary>One demo flight option for a route/date. All values are DEMO placeholders.</summary>
public class FlightOffer
{
    public int Id { get; set; }
    public string Airline { get; set; } = "";
    public string? FlightNumber { get; set; } // Only SV 701 is from the mockup; others are unnumbered demo samples.
    public string FromCode { get; set; } = "KHI";
    public string ToCode { get; set; } = "JED";
    public DateOnly DepartDate { get; set; }
    public DateOnly ReturnDate { get; set; }
    public string DepartTime { get; set; } = "";
    public string ArriveTime { get; set; } = "";
    public string Duration { get; set; } = "";
    public string Stops { get; set; } = "";
    public string Via { get; set; } = "";
    public string Cabin { get; set; } = "Economy";
    public int BaseFarePkr { get; set; }      // per traveller, before taxes/fees
    public int TaxesPkr { get; set; }         // per traveller, from provider (demo value)
    public string Baggage { get; set; } = "1 x 23 kg checked";
    public string CabinBag { get; set; } = "7 kg cabin";
    public string RefundStatus { get; set; } = "Changeable with fee";
    public bool IsCheapest { get; set; }
    public bool IsFastest { get; set; }
    public bool IsDemo { get; set; } = true;
}

/// <summary>Lowest complete round-trip total for one outbound date, fixed return.</summary>
public class FareCalendarDay
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public int LowestRoundTripTotalPkr { get; set; } // per traveller, taxes + demo fee included
    public bool IsLowest { get; set; }
    public bool IsDemo { get; set; } = true;
}

public class Booking
{
    public int Id { get; set; }
    public string Reference { get; set; } = ""; // CTM-XXXXXXXX-XXXX
    public string FlightOfferIds { get; set; } = "";
    public string Airline { get; set; } = "";
    public DateOnly DepartDate { get; set; }
    public DateOnly ReturnDate { get; set; }
    public int Travellers { get; set; } = 1;
    public string Cabin { get; set; } = "Economy";
    public int BaseFarePkr { get; set; }
    public int TaxesPkr { get; set; }
    public int ServiceFeePkr { get; set; }
    public int SupplierFeePkr { get; set; }
    public int PaymentChargePkr { get; set; }
    public int DiscountPkr { get; set; }
    public int TotalPkr { get; set; }
    public string PassengerName { get; set; } = "";
    public string PassengerEmail { get; set; } = "";
    public string PassengerPhone { get; set; } = "";
    public string PaymentMethod { get; set; } = "Card";
    public string Status { get; set; } = "Agent-assisted — awaiting confirmation";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDemo { get; set; } = true;
}

public class Enquiry
{
    public int Id { get; set; }
    public string Reference { get; set; } = ""; // UM-XXXXXX (Umrah) or ENQ-XXXXXX
    public string Kind { get; set; } = "Umrah"; // Umrah | Hajj | General
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string? Email { get; set; }
    public string? DurationDays { get; set; } // 10 / 15 / 21 / 28
    public string? Message { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class FareAlert
{
    public int Id { get; set; }
    public string Route { get; set; } = "KHI-JED";
    public int TargetPricePkr { get; set; }
    public string Contact { get; set; } = ""; // phone or email
    public string Channel { get; set; } = "WhatsApp";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Versioned, audited fee settings. One row is current (IsCurrent).</summary>
public class FeeSetting
{
    public int Id { get; set; }
    public int Version { get; set; } = 1;
    public bool IsCurrent { get; set; } = true;
    public bool IsDemo { get; set; } = true;
    public string ServiceFeeMode { get; set; } = "flat_per_traveller"; // flat_per_traveller | flat_per_booking | percent_of_base
    public int ServiceFeeAmountPkr { get; set; } = 3000;
    public int ServiceFeePercent { get; set; } = 0;
    public bool SupplierFeesEnabled { get; set; } = false;
    public int SupplierFeeAmountPkr { get; set; } = 0;
    public bool CardChargeEnabled { get; set; } = false;
    public int CardChargeAmountPkr { get; set; } = 0;
    public int CardChargePercent { get; set; } = 0;
    public bool DiscountActive { get; set; } = false;
    public string? DiscountCode { get; set; }
    public string DiscountType { get; set; } = "flat"; // flat | percent
    public int DiscountAmountPkr { get; set; } = 0;
    public int DiscountPercent { get; set; } = 0;
    public string ChangedBy { get; set; } = "seed";
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
}

public class FeeSettingAudit
{
    public int Id { get; set; }
    public int Version { get; set; }
    public string ChangedBy { get; set; } = "";
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string Summary { get; set; } = "";
}

public class ZiyaratPlace
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string City { get; set; } = ""; // Makkah | Madinah
    public string? Image { get; set; }     // /images/ziyarat/xxx.jpg — null = pattern tile (no verified photo)
    public string? ImageCredit { get; set; }
    public string Description { get; set; } = "";
    public string? Hadith { get; set; }    // ONLY verified hadiths — never invent
    public string? HadithSource { get; set; }
    public string TimeNeeded { get; set; } = "";
    public string Distance { get; set; } = "";
    public bool DoFirst { get; set; }
}
