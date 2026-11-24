# Carwane Tajdare Madina — Flight Booking Website (.NET 8)

A complete, deployable ASP.NET Core 8 (MVC + Razor) rebuild of the Carwane Tajdare Madina
flight-booking site, with the same design as the staging demo: dark-green header with the
gold calligraphy logo, white pages with dark readable text, gold accents, full PKR amounts
with thousands separators (75,800 — never 80.9k), and visible lightweight animation.

> **STAGING DEMO — all fares and fees shown are demo placeholders, not real prices.**
> Live fares will update automatically once connected (Duffel). Duffel is **not** integrated
> in this build, and demo prices do not change daily.

## What is inside

- **Home / flight search** — KHI → Jeddah default (Dec 2026, round trip, 1 Adult Economy,
  return Sun 04 Jan 2027), 5 Pakistani + 4 Saudi airports, recent searches (on-device only).
- **Fare calendar** — December 2026, 31 days. Each cell is the lowest complete **round-trip
  total** per outbound date with the fixed return. Light warm-ivory, white day cards, bold
  near-black dates, dark-green full prices (15px minimum). Phone defaults to a full-width
  day list; the month grid is one tap away ("Show month grid"). Lowest badge, solid-green
  selected day with white text + gold ring.
- **Flight results** — white cards, solid-gold DEMO FARE badge, bold dark prices, baggage /
  refund chips, details drawer (opens, closes and re-opens reliably; Escape closes;
  scroll-lock restored), compare 2–3 flights (Cheapest / Fastest badges), flexible-date
  savings panel, fare-drop alerts, result counts, sort by cheapest / fastest.
- **Booking** — full itemised breakdown **before** booking: base + taxes/carrier + agency
  service fee + supplier fees + payment charge − discount = total, per traveller and party
  total. Booking reference format `CTM-XXXXXXXX-XXXX`. Agent-assisted: no payment is taken
  online; the team calls the customer to complete payment by **card only**.
- **Umrah** — visa cards 10 / 15 / 21 / 28 days, SAR 465–500 (Visa + Hotel BRN + Catering
  BRN, rates subject to change), approx. PKR 34,300–36,900 **ESTIMATED** at 1 SAR = PKR 73.76
  (24 Nov 2026 — the rate changes daily). No per-duration prices are invented. Hajj card +
  enquiry form that saves with a reference (`UM-XXXXXX` / `HJ-XXXXXX`).
- **Live from the Haramain** — official streams only, tap-to-play (no autoplay, no sound):
  Makkah via Quran TV (Saudi Broadcasting Authority, YouTube channel
  `UCos52azQNBgW63_9uDJoPDA`), Madinah via Sunnah TV (`UCROKYPep-UuODNwyipe6JMw`),
  with a "Watch on YouTube" link and a photo fallback.
- **Ziyarat gallery** — 19 cards (11 Makkah, 8 Madinah), All / Makkah / Madinah filter, city
  tags, gold "Do first" badges, time-needed / distance lines from research only. Only the
  verified hadiths are quoted (Ibn Majah 1406, Bukhari 1190 + 1196, Ibn Majah 1412 + Bukhari
  1193, Bukhari 4422, Quran 2:158, Quran 96:1–5) — nothing invented. Photos are Wikimedia
  Commons / Unsplash as credited on each card; places without a verified photo use a plain
  pattern tile. **No hotel pictures** — hotels stay out until the client gives real ones.
- **Refund & change policy** — white background, dark text, pale-gold card, labelled
  "standard policy — to be confirmed by the agency".
- **Admin (`/Admin`)** — fee settings (service fee mode/amount, supplier fees, card charge,
  discount), versioned with an audit log, bookings / enquiries / fare alerts lists. Demo
  passcode `admin123` — change it before production (see below). Admin is **not** linked
  from the public header.
- **Money** — one formatter (`Services/Money.Pkr`) for the whole site: full amounts with
  thousands separators, whole rupees.
- **Fares** — one swappable source: `IFareProvider` / `DemoFareService`. When Duffel is
  connected, implement `IFareProvider` against Duffel and register it instead; the UI,
  calendar and breakdown do not change.

## Demo data and pricing

- Calendar totals are the mockup's December 2026 demo values (lowest day 15 Dec: PKR 78,500;
  selected day 18 Dec, Saudia SV 701 sample: PKR 88,700). Only SV 701 carries a flight
  number from the mockup; other options are unnumbered demo samples.
- Breakdown arithmetic (server-side only, `PricingService`): e.g. 18 Dec —
  base 73,348 + taxes 12,352 + demo service fee 3,000 = **88,700** per traveller;
  15 Dec — base 63,148 + 12,352 + 3,000 = **78,500**.
- Demo fee settings (staging only): service fee PKR 3,000 per traveller, everything else
  0 / off, marked DEMO. Changing the fee in Admin re-prices the calendar and all flight
  cards immediately (verified: 3,000 → 3,500 moved 18 Dec from 88,700 to 89,200).

## Run locally

Requirements: **.NET 8 SDK** (`dotnet --version` → 8.x).

```bash
cd carwane-web
dotnet run
# open http://localhost:5000
```

The first run creates `carwane.db` (SQLite) and seeds the demo fee settings + Ziyarat places.
Admin: open `http://localhost:5000/Admin`, passcode `admin123`.

## Switch SQLite → PostgreSQL

In `appsettings.json`:

```json
"Database": { "Provider": "Postgres" },
"ConnectionStrings": {
  "CarwaneDb": "Host=<host>;Port=5432;Database=carwane;Username=carwane;Password=<password>"
}
```

Or set the environment variable `ConnectionStrings__CarwaneDb` (and
`Database__Provider=Postgres`). Free PostgreSQL to start: Neon (neon.tech) or Supabase.
The same EF Core models are used for both providers; the database is created on startup
(`EnsureCreated`). For production, switch to EF Core migrations (`dotnet ef migrations add …`).

## Deploy

The frontend and the API are the **same app** (one ASP.NET Core site) — there is no
separate frontend to deploy.

**Option A — Render (free to start, recommended for this project)**
1. Push this folder to a GitHub repo (e.g. `hyder147/carwane-tajdare-madina`).
2. Render → New → Web Service → connect the repo. Build command:
   `dotnet publish -c Release -o out`, start command: `dotnet out/carwane-web.dll`.
   Set environment `ASPNETCORE_ENVIRONMENT=Production`.
3. Add a free PostgreSQL database on Neon or Supabase; set `Database__Provider=Postgres`
   and `ConnectionStrings__CarwaneDb` in Render → Environment.
4. Set `Admin__Passcode` to a strong value in Render → Environment (never commit a real
   passcode). Add real authentication before go-live — the passcode is a demo gate only.
5. Buy a domain (e.g. `carwanetajdaremadina.com`, roughly PKR 3,000–6,000/year from
   GoDaddy / Namecheap / PKNIC) → Render → Settings → Custom Domain → add the domain →
   copy the DNS records Render shows into your domain's DNS. Render issues the free SSL
   certificate automatically (HTTPS). Allow up to 24 hours for DNS to spread.

**Option B — Azure App Service (.NET-native)**
1. Create an Azure App Service (Linux, .NET 8) + Azure Database for PostgreSQL (or Neon).
2. Deploy from GitHub Actions (Azure App Service deploy task) or `az webapp up`.
3. Set the same environment variables in Configuration → Application settings
   (`Database__Provider`, `ConnectionStrings__CarwaneDb`, `Admin__Passcode`).
4. Add the custom domain in App Service → Custom domains and bind the free App Service
   Managed Certificate for HTTPS.

**Option C — VPS / Hostinger VPS (full control)**
1. Ubuntu 24.04 server, install the ASP.NET Core 8 runtime
   (`sudo apt install aspnetcore-runtime-8.0`), PostgreSQL, and nginx as a reverse proxy.
2. `dotnet publish -c Release -o /var/www/carwane`, run it as a systemd service on
   `http://127.0.0.1:5000`, proxy `https://yourdomain` → localhost:5000 in nginx.
3. Free SSL with Certbot (`sudo certbot --nginx -d yourdomain`).

In all cases: keep `Site:IsDemo` **true** until the launch checklist below is done.

## What is demo vs what the client still owes (do NOT invent)

| Item | Status |
|---|---|
| All fares, flight times, savings | DEMO placeholders (mockup values) |
| Service fee PKR 3,000 / traveller | DEMO placeholder — client to confirm the real fee |
| Umrah prices per duration (10/15/21/28 days) | NOT given — only the 465–500 SAR range; client to confirm each |
| Phone / WhatsApp / email | NOT confirmed — mockup shows 021 3480 9444 (unverified) and a listing shows +92 21 34127965 (unverified). Neither is published on the site; the site says "to be confirmed". |
| Office address | Not confirmed — not published |
| What G.L. No. 2717 means | Not confirmed — shown only in the logo artwork; not presented as a licence |
| IATA / TAAP membership | Not confirmed — no logos published |
| Refund / change policy | Standard big-company policy shown, labelled "to be confirmed by the agency" |
| Logo master | Transparent PNGs/SVGs in `wwwroot/images/logo/` are traced from the raster logo — ask the client for the original AI/EPS/SVG master before production |
| Real hotel names / photos, Umrah group photos | Not supplied — hotels are deliberately not shown |
| Duffel access | Ali's side, later — no integration in this build |
| Payment | Credit / debit card only (confirmed). No JazzCash / Easypaisa / bank transfer shown as Carwane's. |

## Launch checklist (do not go live until all are done)

- [ ] Client confirms real service fee / markup and it is entered in Admin (untick "DEMO")
- [ ] Client confirms current phone / WhatsApp / email and they are added to the site
- [ ] Client confirms per-duration Umrah prices
- [ ] Client confirms refund / change policy (or approves the standard policy as shown)
- [ ] Original logo master received; G.L. No. 2717 meaning confirmed
- [ ] Admin passcode changed; real authentication added
- [ ] Duffel sandbox tested (Phase 2) — or the site stays clearly labelled demo
- [ ] No mockup fare remains in the code as if it were real

## Project layout

```
carwane-web/
  Program.cs                    # Startup, DB provider switch, session, seeding
  appsettings.json              # SQLite default; Postgres connection string; admin passcode
  Models/Models.cs              # FlightOffer, FareCalendarDay, Booking, Enquiry,
                                #   FareAlert, FeeSetting (+ audit), ZiyaratPlace
  Data/AppDbContext.cs          # EF Core context + seeder (demo fees, 19 Ziyarat places)
  Services/Services.cs          # Money formatter, IFareProvider/DemoFareService,
                                #   PricingService (server-side breakdown), ZiyaratService
  Controllers/                  # Home (search/calendar/details/alerts/enquiries),
                                #   Booking (create/confirmation), Admin (login/fees/audit)
  Views/                        # Razor views incl. flight-details drawer partial
  wwwroot/css/site.css          # White bg, dark text, green header/buttons, gold accents
  wwwroot/js/site.js            # Drawer, calendar toggle, sort, compare, Ziyarat filter,
                                #   live-stream tap-to-play, recent searches
  wwwroot/images/               # Gold/black/white logos, hero (Prophet's Mosque),
                                #   Ziyarat photos (Wikimedia Commons / Unsplash)
```

## Notes for the developer (Ali)

- Your stack fits this build: ASP.NET Core 8, EF Core, Razor, plain CSS/JS — no React.
- To add Duffel later: implement `IFareProvider` in a new `DuffelFareService`, keep the
  `DemoFareService` for staging, and swap the registration in `Program.cs`. Keep the
  calendar meaning (round-trip total, fixed return) and the breakdown order exactly as is.
- Never store card details. Provider API keys live server-side only (environment
  variables), never in browser code or the repo.
