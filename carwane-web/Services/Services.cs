using Carwane.Web.Models;

namespace Carwane.Web.Services;

/// <summary>Single money formatter for the whole site: full PKR amounts with
/// thousands separators, e.g. 75,800 — NEVER abbreviated (no 80.9k).</summary>
public static class Money
{
    public static string Pkr(int amount) => "PKR " + amount.ToString("N0");
    public static string PkrPlain(int amount) => amount.ToString("N0");
}

/// <summary>Swappable fare source. Today this returns DEMO fares. When Duffel is
/// connected, implement IFareProvider against Duffel and register it instead —
/// the UI, calendar and breakdown do not change.</summary>
public interface IFareProvider
{
    string ProviderName { get; }
    bool IsLive { get; }
    IReadOnlyList<FareCalendarDay> GetDecemberCalendar();
    IReadOnlyList<FlightOffer> GetOffers(string from, string to, DateOnly depart, DateOnly ret, int travellers, string cabin);
    FlightOffer? GetOffer(int id, DateOnly depart, DateOnly ret);
}

public class DemoFareService : IFareProvider
{
    public string ProviderName => "Demo fares (Duffel not connected yet)";
    public bool IsLive => false;

    // December 2026 round-trip totals per outbound date, fixed return 04 Jan 2027.
    // Taken from the client mockup — illustrative DEMO values, not real fares.
    private static readonly int[] DecTotals =
    {
        86400, 82900, 89200, 91500, 94100, 87800, 84600, 86900, 81700, 79900,
        88200, 93400, 89600, 83800, 78500, 82200, 85900, 88700, 96300, 102900,
        91100, 87400, 85200, 94500, 98300, 104600, 97800, 92700, 80400, 84100, 89900
    };

    private static readonly DateOnly ReturnDate = new(2027, 1, 4);

    public IReadOnlyList<FareCalendarDay> GetDecemberCalendar()
    {
        var days = new List<FareCalendarDay>();
        var min = DecTotals.Min();
        for (var d = 1; d <= 31; d++)
        {
            days.Add(new FareCalendarDay
            {
                Id = d,
                Date = new DateOnly(2026, 12, d),
                LowestRoundTripTotalPkr = DecTotals[d - 1],
                IsLowest = DecTotals[d - 1] == min
            });
        }
        return days;
    }

    public IReadOnlyList<FlightOffer> GetOffers(string from, string to, DateOnly depart, DateOnly ret, int travellers, string cabin)
    {
        var day = GetDecemberCalendar().FirstOrDefault(x => x.Date == depart);
        var lowest = day?.LowestRoundTripTotalPkr ?? 88700;
        // Breakdown of the lowest total: base + taxes (12,352) + demo service fee (3,000).
        // Verified example: 58,820 + 12,352 + 3,000 = 74,172 (cheapest day, 15 Dec).
        const int taxes = 12352;
        const int fee = 3000;
        var cheapestBase = lowest - taxes - fee;
        var list = new List<FlightOffer>
        {
            new()
            {
                Id = 1, Airline = "Saudia", FlightNumber = "SV 701",
                FromCode = from, ToCode = to, DepartDate = depart, ReturnDate = ret,
                DepartTime = "04:25", ArriveTime = "14:00", Duration = "9h 35m",
                Stops = "1 stop", Via = "via Riyadh", Cabin = cabin,
                BaseFarePkr = cheapestBase, TaxesPkr = taxes,
                Baggage = "1 x 23 kg checked", RefundStatus = "Changeable with fee",
                IsCheapest = true
            },
            new()
            {
                Id = 2, Airline = "PIA", FlightNumber = null,
                FromCode = from, ToCode = to, DepartDate = depart, ReturnDate = ret,
                DepartTime = "07:10", ArriveTime = "13:45", Duration = "6h 35m",
                Stops = "Non-stop", Via = "Direct", Cabin = cabin,
                BaseFarePkr = cheapestBase + 4200, TaxesPkr = taxes,
                Baggage = "2 x 23 kg checked", RefundStatus = "Changeable with fee",
                IsFastest = true
            },
            new()
            {
                Id = 3, Airline = "Gulf Air", FlightNumber = null,
                FromCode = from, ToCode = to, DepartDate = depart, ReturnDate = ret,
                DepartTime = "11:40", ArriveTime = "19:55", Duration = "8h 15m",
                Stops = "1 stop", Via = "via Bahrain", Cabin = cabin,
                BaseFarePkr = cheapestBase + 6800, TaxesPkr = taxes,
                Baggage = "1 x 23 kg checked", RefundStatus = "Non-refundable after ticketing"
            },
            new()
            {
                Id = 4, Airline = "Emirates", FlightNumber = null,
                FromCode = from, ToCode = to, DepartDate = depart, ReturnDate = ret,
                DepartTime = "18:20", ArriveTime = "02:10 +1", Duration = "7h 50m",
                Stops = "1 stop", Via = "via Dubai", Cabin = cabin,
                BaseFarePkr = cheapestBase + 9400, TaxesPkr = taxes,
                Baggage = "2 x 23 kg checked", RefundStatus = "Changeable with fee"
            }
        };
        // Only the mockup sample carries a flight number; the rest are unnumbered
        // demo samples, and every card is labelled DEMO FARE.
        return list;
    }

    public FlightOffer? GetOffer(int id, DateOnly depart, DateOnly ret)
    {
        return GetOffers("KHI", "JED", depart, ret, 1, "Economy").FirstOrDefault(o => o.Id == id);
    }

    public static IReadOnlyList<(string Code, string City, string Airport)> PakistaniAirports { get; } = new[]
    {
        ("KHI", "Karachi", "Jinnah International Airport"),
        ("LHE", "Lahore", "Allama Iqbal International Airport"),
        ("ISB", "Islamabad", "Islamabad International Airport"),
        ("PEW", "Peshawar", "Bacha Khan International Airport"),
        ("MUX", "Multan", "Multan International Airport"),
    };

    public static IReadOnlyList<(string Code, string City, string Airport)> SaudiAirports { get; } = new[]
    {
        ("JED", "Jeddah", "King Abdulaziz International Airport"),
        ("MED", "Madinah", "Prince Mohammad Bin Abdulaziz Airport"),
        ("RUH", "Riyadh", "King Khalid International Airport"),
        ("DMM", "Dammam", "King Fahd International Airport"),
    };
}

public record PriceBreakdown(
    int BaseFarePkr, int TaxesPkr, int ServiceFeePkr, int SupplierFeePkr,
    int PaymentChargePkr, int DiscountPkr, int TotalPerTraveller, int Travellers, int TotalPayable);

/// <summary>Server-side pricing only. The front end never calculates a total.</summary>
public class PricingService
{
    public PriceBreakdown Calculate(FlightOffer offer, FeeSetting fees, int travellers, string paymentMethod = "Card")
    {
        var serviceFee = fees.ServiceFeeMode switch
        {
            "flat_per_booking" => fees.ServiceFeeAmountPkr,
            "percent_of_base" => (int)Math.Round(offer.BaseFarePkr * fees.ServiceFeePercent / 100.0),
            _ => fees.ServiceFeeAmountPkr // flat_per_traveller
        };
        var supplier = fees.SupplierFeesEnabled ? fees.SupplierFeeAmountPkr : 0;
        var payment = paymentMethod == "Card" && fees.CardChargeEnabled
            ? fees.CardChargeAmountPkr + (int)Math.Round(offer.BaseFarePkr * fees.CardChargePercent / 100.0)
            : 0;
        var discount = 0;
        if (fees.DiscountActive)
        {
            discount = fees.DiscountType == "percent"
                ? (int)Math.Round(offer.BaseFarePkr * fees.DiscountPercent / 100.0)
                : fees.DiscountAmountPkr;
        }
        var perTraveller = offer.BaseFarePkr + offer.TaxesPkr + serviceFee + supplier + payment - discount;
        if (perTraveller < 0) perTraveller = 0;
        // flat_per_booking fee is charged once, not per traveller
        var total = fees.ServiceFeeMode == "flat_per_booking"
            ? (offer.BaseFarePkr + offer.TaxesPkr + supplier + payment - discount) * travellers + serviceFee
            : perTraveller * travellers;
        return new PriceBreakdown(offer.BaseFarePkr, offer.TaxesPkr, serviceFee, supplier, payment,
            discount, perTraveller, travellers, total);
    }
}

public class ZiyaratService
{
    public IReadOnlyList<ZiyaratPlace> All() => new List<ZiyaratPlace>
    {
        new() { Id = 1, Name = "Masjid al-Haram & the Kaaba", City = "Makkah", Image = "/images/ziyarat/haram.jpg", ImageCredit = "Photo: Wikimedia Commons — The Kaaba during Hajj",
            Description = "The Sacred Mosque and the Kaaba — the heart of Umrah and Hajj, and the direction of prayer for Muslims everywhere.",
            Hadith = "“One prayer in my mosque is better than one thousand prayers elsewhere, except the Sacred Mosque, and one prayer in the Sacred Mosque is better than one hundred thousand prayers elsewhere.”",
            HadithSource = "Sunan Ibn Majah 1406, Sahih", TimeNeeded = "As long as you wish", Distance = "In the centre of Makkah", DoFirst = true },
        new() { Id = 2, Name = "Safa & Marwa", City = "Makkah", Image = null,
            Description = "The two hills of Sa'i, walked between during Umrah in remembrance of Hajar's search for water.",
            Hadith = "Indeed, Safa and Marwa are among the symbols of Allah.",
            HadithSource = "Quran 2:158", TimeNeeded = "Part of Umrah", Distance = "Inside Masjid al-Haram", DoFirst = true },
        new() { Id = 3, Name = "Zamzam Well", City = "Makkah", Image = "/images/ziyarat/zamzam.jpg", ImageCredit = "Photo: Wikimedia Commons — Zamzam dispensers, Masjid al-Haram",
            Description = "The blessed well inside the Haram, from which pilgrims drink and take water home.",
            TimeNeeded = "15 minutes", Distance = "Inside Masjid al-Haram", DoFirst = true },
        new() { Id = 4, Name = "Cave Hira — Jabal al-Noor", City = "Makkah", Image = "/images/ziyarat/hira.jpg", ImageCredit = "Photo: Wikimedia Commons — Hira Cave",
            Description = "The cave on the Mountain of Light where the first revelation came to the Prophet (peace be upon him). A steep climb — go early, take water, and wear good shoes.",
            Hadith = "Read, in the name of your Lord who created.",
            HadithSource = "Quran 96:1–5 — the first revelation, received in Cave Hira", TimeNeeded = "2–3 hours", Distance = "About 4 km north-east of the Haram", DoFirst = false },
        new() { Id = 5, Name = "Jabal Thawr", City = "Makkah", Image = "/images/ziyarat/thawr.jpg", ImageCredit = "Photo: Wikimedia Commons — Jabal Thawr",
            Description = "The mountain whose cave sheltered the Prophet (peace be upon him) and Abu Bakr (may Allah be pleased with him) during the Hijrah.",
            TimeNeeded = "Ask your Ziyarat guide", Distance = "South of Makkah", DoFirst = false },
        new() { Id = 6, Name = "Mount Arafat — Jabal al-Rahmah", City = "Makkah", Image = "/images/ziyarat/arafat.jpg", ImageCredit = "Photo: Wikimedia Commons — Jabal al-Rahmah, Mount Arafat",
            Description = "The plain and Mount of Mercy where pilgrims stand on the Day of Arafat — the greatest day of Hajj.",
            TimeNeeded = "45 minutes", Distance = "About 20 km south-east of Makkah", DoFirst = false },
        new() { Id = 7, Name = "Mina, Jamarat & Masjid al-Khayf", City = "Makkah", Image = "/images/ziyarat/mina.jpg", ImageCredit = "Photo: Wikimedia Commons — Mina tents",
            Description = "The tent city of Hajj, the Jamarat pillars, and the historic mosque where many Prophets are said to have prayed.",
            TimeNeeded = "30 minutes", Distance = "Between Makkah and Muzdalifah", DoFirst = false },
        new() { Id = 8, Name = "Muzdalifah", City = "Makkah", Image = null,
            Description = "The open plain where pilgrims spend the night after Arafat, rest under the sky, and collect pebbles for the Jamarat.",
            TimeNeeded = "30 minutes", Distance = "Between Arafat and Mina", DoFirst = false },
        new() { Id = 9, Name = "Jannat al-Mualla", City = "Makkah", Image = "/images/ziyarat/mualla.jpg", ImageCredit = "Photo: Wikimedia Commons — Jannat al-Mualla",
            Description = "Makkah's historic cemetery, resting place of Khadijah (may Allah be pleased with her) and other early Muslims.",
            TimeNeeded = "30 minutes", Distance = "About 1 km north of the Haram", DoFirst = false },
        new() { Id = 10, Name = "Masjid al-Jinn", City = "Makkah", Image = "/images/ziyarat/jinn.jpg", ImageCredit = "Photo: Wikimedia Commons — Mosque of the Jinn",
            Description = "The small mosque near Jannat al-Mualla linked with the night the jinn listened to the Quran.",
            TimeNeeded = "15 minutes", Distance = "Near Jannat al-Mualla", DoFirst = false },
        new() { Id = 11, Name = "Masjid Aisha — Taneem", City = "Makkah", Image = "/images/ziyarat/taneem.jpg", ImageCredit = "Photo: Wikimedia Commons — Masjid Aisha, Taneem",
            Description = "The miqat mosque about 7 km north of Makkah, where pilgrims in Makkah enter ihram for a second Umrah.",
            TimeNeeded = "1 hour", Distance = "About 7 km north of Makkah", DoFirst = false },
        new() { Id = 12, Name = "Masjid an-Nabawi & the Rawdah", City = "Madinah", Image = "/images/hero-madinah.jpg", ImageCredit = "Photo: Unsplash — the Prophet's Mosque, Madinah",
            Description = "The Prophet's Mosque, home to the Rawdah — one of the gardens of Paradise — and the resting place of the Prophet (peace be upon him). Book a Rawdah visit on the Nusuk app.",
            Hadith = "“One prayer in my mosque is better than one thousand prayers in any other mosque, except al-Masjid al-Haram.”",
            HadithSource = "Sahih al-Bukhari 1190 — and: “Between my house and my pulpit is one of the gardens of Paradise.” (Sahih al-Bukhari 1196)",
            TimeNeeded = "1 hour for the Rawdah (Nusuk permit needed)", Distance = "Centre of Madinah", DoFirst = true },
        new() { Id = 13, Name = "Masjid Quba", City = "Madinah", Image = "/images/ziyarat/quba.jpg", ImageCredit = "Photo: Wikimedia Commons — Masjid Quba",
            Description = "The first mosque built in Islam. The Prophet (peace be upon him) went there every Saturday and prayed two rak'ahs.",
            Hadith = "“Whoever purifies himself in his house, then comes to the Quba Mosque and offers one prayer therein, will have a reward like that for Umrah.”",
            HadithSource = "Sunan Ibn Majah 1412, Hasan — the Prophet went there every Saturday and prayed two rak'ahs (Sahih al-Bukhari 1193)",
            TimeNeeded = "1 hour", Distance = "About 3.5 km south of the Prophet's Mosque", DoFirst = true },
        new() { Id = 14, Name = "Mount Uhud & Archers' Hill", City = "Madinah", Image = "/images/ziyarat/uhud.jpg", ImageCredit = "Photo: Wikimedia Commons — Mount Uhud",
            Description = "The mountain of the Battle of Uhud, and the hill of the archers. The martyrs of Uhud, including Hamzah (may Allah be pleased with him), are buried at its foot.",
            Hadith = "“This is Uhud, a mountain that loves us and is loved by us.”",
            HadithSource = "Sahih al-Bukhari 4422", TimeNeeded = "1–2 hours", Distance = "About 5 km north of Madinah", DoFirst = true },
        new() { Id = 15, Name = "Jannat al-Baqi", City = "Madinah", Image = null,
            Description = "Madinah's cemetery, resting place of many Companions and family of the Prophet (peace be upon him). Visit as the Prophet did — with salaam and supplication for the dead.",
            TimeNeeded = "30 minutes (usually opens after Fajr and Asr)", Distance = "Next to Masjid an-Nabawi", DoFirst = false },
        new() { Id = 16, Name = "Masjid Qiblatain", City = "Madinah", Image = "/images/ziyarat/qiblatain.jpg", ImageCredit = "Photo: Wikimedia Commons — Masjid Qiblatain",
            Description = "The Mosque of the Two Qiblas — where the direction of prayer changed from Jerusalem to Makkah during prayer.",
            TimeNeeded = "30 minutes", Distance = "About 4 km north-west of the Prophet's Mosque", DoFirst = false },
        new() { Id = 17, Name = "Seven Mosques — Masjid al-Fath", City = "Madinah", Image = "/images/ziyarat/seven.jpg", ImageCredit = "Photo: Wikimedia Commons — Mosque of Salman al-Farisi, Seven Mosques",
            Description = "The small historic mosques at the site of the Battle of the Trench (Khandaq), including Masjid al-Fath.",
            TimeNeeded = "Ask your Ziyarat guide", Distance = "About 3 km west of the Prophet's Mosque", DoFirst = false },
        new() { Id = 18, Name = "Masjid al-Ghamama", City = "Madinah", Image = null,
            Description = "The mosque near the Prophet's Mosque where the Prophet (peace be upon him) prayed the Eid and rain prayers.",
            TimeNeeded = "15 minutes", Distance = "A short walk from Masjid an-Nabawi", DoFirst = false },
        new() { Id = 19, Name = "Dar Al Madinah Museum & Hejaz Railway", City = "Madinah", Image = "/images/ziyarat/hejaz.jpg", ImageCredit = "Photo: Wikimedia Commons — Hejaz Railway station, Madinah",
            Description = "The museum and the old Hejaz Railway station — the story of Madinah, the Prophet's city, and the railway that once carried pilgrims from Damascus.",
            TimeNeeded = "1–2 hours", Distance = "Near the Prophet's Mosque", DoFirst = false },
    };
}
