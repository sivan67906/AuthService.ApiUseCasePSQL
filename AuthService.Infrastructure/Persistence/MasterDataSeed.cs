using System.Linq;
using AuthService.Domain.Entities.Masters;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Persistence;

/// <summary>
/// Seeds master data for Company Module: Countries, States, Cities, Currencies, TimeZones
/// </summary>
public static class MasterDataSeed
{
    public static async Task SeedMasterDataAsync(AppDbContext context, ILogger logger)
    {
        logger.LogInformation("Starting master data seeding...");

        // Only seed if data doesn't exist
        if (await context.Countries.AnyAsync())
        {
            logger.LogInformation("Master data already exists, skipping...");
            return;
        }

        await SeedCurrencies(context, logger);
        await SeedTimeZones(context, logger);
        await SeedCountriesStatesAndCities(context, logger);

        logger.LogInformation("Master data seeding completed successfully");
    }

    private static async Task SeedCurrencies(AppDbContext context, ILogger logger)
    {
        logger.LogInformation("Seeding currencies...");

        var currencies = new List<Currency>
        {
            new() { Id = Guid.NewGuid(), Code = "INR", Name = "Indian Rupee", Symbol = "₹", DecimalPlaces = 2, DisplayOrder = 1, IsActive = true },
            new() { Id = Guid.NewGuid(), Code = "USD", Name = "US Dollar", Symbol = "$", DecimalPlaces = 2, DisplayOrder = 2, IsActive = true },
            new() { Id = Guid.NewGuid(), Code = "EUR", Name = "Euro", Symbol = "€", DecimalPlaces = 2, DisplayOrder = 3, IsActive = true },
            new() { Id = Guid.NewGuid(), Code = "GBP", Name = "British Pound", Symbol = "£", DecimalPlaces = 2, DisplayOrder = 4, IsActive = true },
            new() { Id = Guid.NewGuid(), Code = "JPY", Name = "Japanese Yen", Symbol = "¥", DecimalPlaces = 0, DisplayOrder = 5, IsActive = true },
            new() { Id = Guid.NewGuid(), Code = "AUD", Name = "Australian Dollar", Symbol = "A$", DecimalPlaces = 2, DisplayOrder = 6, IsActive = true },
            new() { Id = Guid.NewGuid(), Code = "CAD", Name = "Canadian Dollar", Symbol = "C$", DecimalPlaces = 2, DisplayOrder = 7, IsActive = true },
            new() { Id = Guid.NewGuid(), Code = "CHF", Name = "Swiss Franc", Symbol = "CHF", DecimalPlaces = 2, DisplayOrder = 8, IsActive = true },
            new() { Id = Guid.NewGuid(), Code = "CNY", Name = "Chinese Yuan", Symbol = "¥", DecimalPlaces = 2, DisplayOrder = 9, IsActive = true },
            new() { Id = Guid.NewGuid(), Code = "SGD", Name = "Singapore Dollar", Symbol = "S$", DecimalPlaces = 2, DisplayOrder = 10, IsActive = true },
            new() { Id = Guid.NewGuid(), Code = "AED", Name = "UAE Dirham", Symbol = "د.إ", DecimalPlaces = 2, DisplayOrder = 11, IsActive = true },
            new() { Id = Guid.NewGuid(), Code = "SAR", Name = "Saudi Riyal", Symbol = "﷼", DecimalPlaces = 2, DisplayOrder = 12, IsActive = true },
        };

        context.Currencies.AddRange(currencies);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} currencies", currencies.Count);
    }

    private static async Task SeedTimeZones(AppDbContext context, ILogger logger)
    {
        logger.LogInformation("Seeding time zones...");

        var timeZones = new List<TimeZoneMaster>
        {
            new() { Id = Guid.NewGuid(), Name = "India Standard Time", Identifier = "Asia/Kolkata", Offset = "UTC+05:30", DisplayName = "(UTC+05:30) Chennai, Kolkata, Mumbai, New Delhi", DisplayOrder = 1, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Pacific Standard Time", Identifier = "America/Los_Angeles", Offset = "UTC-08:00", DisplayName = "(UTC-08:00) Pacific Time (US & Canada)", DisplayOrder = 2, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Mountain Standard Time", Identifier = "America/Denver", Offset = "UTC-07:00", DisplayName = "(UTC-07:00) Mountain Time (US & Canada)", DisplayOrder = 3, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Central Standard Time", Identifier = "America/Chicago", Offset = "UTC-06:00", DisplayName = "(UTC-06:00) Central Time (US & Canada)", DisplayOrder = 4, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Eastern Standard Time", Identifier = "America/New_York", Offset = "UTC-05:00", DisplayName = "(UTC-05:00) Eastern Time (US & Canada)", DisplayOrder = 5, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "GMT Standard Time", Identifier = "Europe/London", Offset = "UTC+00:00", DisplayName = "(UTC+00:00) London, Dublin, Edinburgh", DisplayOrder = 6, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Central European Time", Identifier = "Europe/Paris", Offset = "UTC+01:00", DisplayName = "(UTC+01:00) Brussels, Copenhagen, Madrid, Paris", DisplayOrder = 7, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Eastern European Time", Identifier = "Europe/Helsinki", Offset = "UTC+02:00", DisplayName = "(UTC+02:00) Helsinki, Kyiv, Riga, Sofia", DisplayOrder = 8, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Arabian Standard Time", Identifier = "Asia/Dubai", Offset = "UTC+04:00", DisplayName = "(UTC+04:00) Abu Dhabi, Muscat, Dubai", DisplayOrder = 9, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "China Standard Time", Identifier = "Asia/Shanghai", Offset = "UTC+08:00", DisplayName = "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi", DisplayOrder = 10, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Japan Standard Time", Identifier = "Asia/Tokyo", Offset = "UTC+09:00", DisplayName = "(UTC+09:00) Osaka, Sapporo, Tokyo", DisplayOrder = 11, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "AUS Eastern Standard Time", Identifier = "Australia/Sydney", Offset = "UTC+10:00", DisplayName = "(UTC+10:00) Canberra, Melbourne, Sydney", DisplayOrder = 12, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Singapore Standard Time", Identifier = "Asia/Singapore", Offset = "UTC+08:00", DisplayName = "(UTC+08:00) Kuala Lumpur, Singapore", DisplayOrder = 13, IsActive = true },
        };

        context.TimeZones.AddRange(timeZones);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} time zones", timeZones.Count);
    }

    private static async Task SeedCountriesStatesAndCities(AppDbContext context, ILogger logger)
    {
        logger.LogInformation("Seeding countries, states, and cities...");

        // Get all timezones for mapping
        var allTimeZones = await context.TimeZones.ToListAsync();
        var countryTimeZoneMappings = new List<CountryTimeZone>();

        // India
        var india = new Country
        {
            Id = Guid.NewGuid(),
            Name = "India",
            Code = "IN",
            Code3 = "IND",
            NumericCode = "356",
            PhoneCode = "+91",
            CurrencyCode = "INR",
            DisplayOrder = 1,
            IsActive = true
        };

        var indiaStates = new[]
        {
            ("Andhra Pradesh", "AP"),
            ("Karnataka", "KA"),
            ("Kerala", "KL"),
            ("Maharashtra", "MH"),
            ("Tamil Nadu", "TN"),
            ("Telangana", "TG"),
            ("Delhi", "DL"),
            ("Gujarat", "GJ"),
            ("Rajasthan", "RJ"),
            ("Uttar Pradesh", "UP"),
            ("West Bengal", "WB"),
            ("Madhya Pradesh", "MP"),
            ("Punjab", "PB"),
            ("Haryana", "HR"),
            ("Bihar", "BR")
        };

        var indiaCities = new Dictionary<string, string[]>
        {
            ["Karnataka"] = new[] { "Bangalore", "Mysore", "Mangalore", "Hubli", "Belgaum" },
            ["Maharashtra"] = new[] { "Mumbai", "Pune", "Nagpur", "Thane", "Nashik" },
            ["Delhi"] = new[] { "New Delhi", "Central Delhi", "South Delhi", "North Delhi", "East Delhi" },
            ["Tamil Nadu"] = new[] { "Chennai", "Coimbatore", "Madurai", "Tiruchirappalli", "Salem" },
            ["Telangana"] = new[] { "Hyderabad", "Warangal", "Nizamabad", "Karimnagar", "Khammam" },
            ["Gujarat"] = new[] { "Ahmedabad", "Surat", "Vadodara", "Rajkot", "Gandhinagar" },
            ["Andhra Pradesh"] = new[] { "Visakhapatnam", "Vijayawada", "Guntur", "Tirupati", "Nellore" },
            ["Kerala"] = new[] { "Thiruvananthapuram", "Kochi", "Kozhikode", "Thrissur", "Kollam" },
            ["West Bengal"] = new[] { "Kolkata", "Howrah", "Durgapur", "Asansol", "Siliguri" },
            ["Uttar Pradesh"] = new[] { "Lucknow", "Kanpur", "Ghaziabad", "Agra", "Varanasi" },
        };

        context.Countries.Add(india);
        await context.SaveChangesAsync();

        var stateOrder = 1;
        foreach (var (stateName, stateCode) in indiaStates)
        {
            var state = new State
            {
                Id = Guid.NewGuid(),
                Name = stateName,
                Code = stateCode,
                CountryId = india.Id,
                DisplayOrder = stateOrder++,
                IsActive = true
            };
            context.States.Add(state);
            await context.SaveChangesAsync();

            if (indiaCities.TryGetValue(stateName, out var cities))
            {
                var cityOrder = 1;
                foreach (var cityName in cities)
                {
                    context.Cities.Add(new City
                    {
                        Id = Guid.NewGuid(),
                        Name = cityName,
                        StateId = state.Id,
                        DisplayOrder = cityOrder++,
                        IsActive = true
                    });
                }
                await context.SaveChangesAsync();
            }
        }

        // United States
        var usa = new Country
        {
            Id = Guid.NewGuid(),
            Name = "United States",
            Code = "US",
            Code3 = "USA",
            NumericCode = "840",
            PhoneCode = "+1",
            CurrencyCode = "USD",
            DisplayOrder = 2,
            IsActive = true
        };

        var usaStates = new[]
        {
            ("California", "CA"),
            ("Texas", "TX"),
            ("Florida", "FL"),
            ("New York", "NY"),
            ("Pennsylvania", "PA"),
            ("Illinois", "IL"),
            ("Ohio", "OH"),
            ("Georgia", "GA"),
            ("North Carolina", "NC"),
            ("Michigan", "MI"),
            ("Washington", "WA"),
            ("Arizona", "AZ"),
            ("Massachusetts", "MA"),
            ("Virginia", "VA"),
            ("Colorado", "CO")
        };

        var usaCities = new Dictionary<string, string[]>
        {
            ["California"] = new[] { "Los Angeles", "San Francisco", "San Diego", "San Jose", "Sacramento" },
            ["Texas"] = new[] { "Houston", "San Antonio", "Dallas", "Austin", "Fort Worth" },
            ["Florida"] = new[] { "Jacksonville", "Miami", "Tampa", "Orlando", "St. Petersburg" },
            ["New York"] = new[] { "New York City", "Buffalo", "Rochester", "Yonkers", "Syracuse" },
            ["Illinois"] = new[] { "Chicago", "Aurora", "Rockford", "Joliet", "Naperville" },
            ["Washington"] = new[] { "Seattle", "Spokane", "Tacoma", "Vancouver", "Bellevue" },
            ["Massachusetts"] = new[] { "Boston", "Worcester", "Springfield", "Cambridge", "Lowell" },
        };

        context.Countries.Add(usa);
        await context.SaveChangesAsync();

        stateOrder = 1;
        foreach (var (stateName, stateCode) in usaStates)
        {
            var state = new State
            {
                Id = Guid.NewGuid(),
                Name = stateName,
                Code = stateCode,
                CountryId = usa.Id,
                DisplayOrder = stateOrder++,
                IsActive = true
            };
            context.States.Add(state);
            await context.SaveChangesAsync();

            if (usaCities.TryGetValue(stateName, out var cities))
            {
                var cityOrder = 1;
                foreach (var cityName in cities)
                {
                    context.Cities.Add(new City
                    {
                        Id = Guid.NewGuid(),
                        Name = cityName,
                        StateId = state.Id,
                        DisplayOrder = cityOrder++,
                        IsActive = true
                    });
                }
                await context.SaveChangesAsync();
            }
        }

        // United Kingdom
        var uk = new Country
        {
            Id = Guid.NewGuid(),
            Name = "United Kingdom",
            Code = "GB",
            Code3 = "GBR",
            NumericCode = "826",
            PhoneCode = "+44",
            CurrencyCode = "GBP",
            DisplayOrder = 3,
            IsActive = true
        };

        var ukRegions = new[]
        {
            ("England", "ENG"),
            ("Scotland", "SCT"),
            ("Wales", "WLS"),
            ("Northern Ireland", "NIR")
        };

        var ukCities = new Dictionary<string, string[]>
        {
            ["England"] = new[] { "London", "Birmingham", "Manchester", "Leeds", "Liverpool" },
            ["Scotland"] = new[] { "Edinburgh", "Glasgow", "Aberdeen", "Dundee", "Inverness" },
            ["Wales"] = new[] { "Cardiff", "Swansea", "Newport", "Wrexham", "Barry" },
            ["Northern Ireland"] = new[] { "Belfast", "Derry", "Lisburn", "Newry", "Bangor" },
        };

        context.Countries.Add(uk);
        await context.SaveChangesAsync();

        stateOrder = 1;
        foreach (var (regionName, regionCode) in ukRegions)
        {
            var state = new State
            {
                Id = Guid.NewGuid(),
                Name = regionName,
                Code = regionCode,
                CountryId = uk.Id,
                DisplayOrder = stateOrder++,
                IsActive = true
            };
            context.States.Add(state);
            await context.SaveChangesAsync();

            if (ukCities.TryGetValue(regionName, out var cities))
            {
                var cityOrder = 1;
                foreach (var cityName in cities)
                {
                    context.Cities.Add(new City
                    {
                        Id = Guid.NewGuid(),
                        Name = cityName,
                        StateId = state.Id,
                        DisplayOrder = cityOrder++,
                        IsActive = true
                    });
                }
                await context.SaveChangesAsync();
            }
        }

        // Add more countries as needed (UAE, Singapore, Australia, etc.)
        var uae = new Country { Id = Guid.NewGuid(), Name = "United Arab Emirates", Code = "AE", Code3 = "ARE", PhoneCode = "+971", CurrencyCode = "AED", DisplayOrder = 4, IsActive = true };
        var singapore = new Country { Id = Guid.NewGuid(), Name = "Singapore", Code = "SG", Code3 = "SGP", PhoneCode = "+65", CurrencyCode = "SGD", DisplayOrder = 5, IsActive = true };
        var australia = new Country { Id = Guid.NewGuid(), Name = "Australia", Code = "AU", Code3 = "AUS", PhoneCode = "+61", CurrencyCode = "AUD", DisplayOrder = 6, IsActive = true };
        var canada = new Country { Id = Guid.NewGuid(), Name = "Canada", Code = "CA", Code3 = "CAN", PhoneCode = "+1", CurrencyCode = "CAD", DisplayOrder = 7, IsActive = true };
        var germany = new Country { Id = Guid.NewGuid(), Name = "Germany", Code = "DE", Code3 = "DEU", PhoneCode = "+49", CurrencyCode = "EUR", DisplayOrder = 8, IsActive = true };
        var france = new Country { Id = Guid.NewGuid(), Name = "France", Code = "FR", Code3 = "FRA", PhoneCode = "+33", CurrencyCode = "EUR", DisplayOrder = 9, IsActive = true };
        var japan = new Country { Id = Guid.NewGuid(), Name = "Japan", Code = "JP", Code3 = "JPN", PhoneCode = "+81", CurrencyCode = "JPY", DisplayOrder = 10, IsActive = true };
        var china = new Country { Id = Guid.NewGuid(), Name = "China", Code = "CN", Code3 = "CHN", PhoneCode = "+86", CurrencyCode = "CNY", DisplayOrder = 11, IsActive = true };

        context.Countries.AddRange(uae, singapore, australia, canada, germany, france, japan, china);
        await context.SaveChangesAsync();

        // Add basic states/regions for these countries
        await AddBasicStatesForCountry(context, uae, new[] { ("Abu Dhabi", "AD"), ("Dubai", "DU"), ("Sharjah", "SH") });
        await AddBasicStatesForCountry(context, singapore, new[] { ("Central Region", "CR"), ("East Region", "ER"), ("North Region", "NR"), ("West Region", "WR") });
        await AddBasicStatesForCountry(context, australia, new[] { ("New South Wales", "NSW"), ("Victoria", "VIC"), ("Queensland", "QLD"), ("Western Australia", "WA") });

        // Seed CountryTimeZone mappings
        logger.LogInformation("Seeding country-timezone mappings...");

        // Map timezones to countries
        var indiaTimeZone = allTimeZones.FirstOrDefault(t => t.Identifier == "Asia/Kolkata");
        var pstTimeZone = allTimeZones.FirstOrDefault(t => t.Identifier == "America/Los_Angeles");
        var mstTimeZone = allTimeZones.FirstOrDefault(t => t.Identifier == "America/Denver");
        var cstTimeZone = allTimeZones.FirstOrDefault(t => t.Identifier == "America/Chicago");
        var estTimeZone = allTimeZones.FirstOrDefault(t => t.Identifier == "America/New_York");
        var gmtTimeZone = allTimeZones.FirstOrDefault(t => t.Identifier == "Europe/London");
        var cetTimeZone = allTimeZones.FirstOrDefault(t => t.Identifier == "Europe/Paris");
        var eetTimeZone = allTimeZones.FirstOrDefault(t => t.Identifier == "Europe/Helsinki");
        var arabianTimeZone = allTimeZones.FirstOrDefault(t => t.Identifier == "Asia/Dubai");
        var chinaTimeZone = allTimeZones.FirstOrDefault(t => t.Identifier == "Asia/Shanghai");
        var japanTimeZone = allTimeZones.FirstOrDefault(t => t.Identifier == "Asia/Tokyo");
        var ausTimeZone = allTimeZones.FirstOrDefault(t => t.Identifier == "Australia/Sydney");
        var singaporeTimeZone = allTimeZones.FirstOrDefault(t => t.Identifier == "Asia/Singapore");

        // India -> IST (default)
        if (indiaTimeZone != null)
        {
            countryTimeZoneMappings.Add(new CountryTimeZone
            {
                Id = Guid.NewGuid(),
                CountryId = india.Id,
                TimeZoneId = indiaTimeZone.Id,
                IsDefault = true
            });
        }

        // USA -> PST, MST, CST, EST (EST as default)
        if (estTimeZone != null)
        {
            countryTimeZoneMappings.Add(new CountryTimeZone
            {
                Id = Guid.NewGuid(),
                CountryId = usa.Id,
                TimeZoneId = estTimeZone.Id,
                IsDefault = true
            });
        }
        if (pstTimeZone != null)
        {
            countryTimeZoneMappings.Add(new CountryTimeZone
            {
                Id = Guid.NewGuid(),
                CountryId = usa.Id,
                TimeZoneId = pstTimeZone.Id,
                IsDefault = false
            });
        }
        if (mstTimeZone != null)
        {
            countryTimeZoneMappings.Add(new CountryTimeZone
            {
                Id = Guid.NewGuid(),
                CountryId = usa.Id,
                TimeZoneId = mstTimeZone.Id,
                IsDefault = false
            });
        }
        if (cstTimeZone != null)
        {
            countryTimeZoneMappings.Add(new CountryTimeZone
            {
                Id = Guid.NewGuid(),
                CountryId = usa.Id,
                TimeZoneId = cstTimeZone.Id,
                IsDefault = false
            });
        }

        // UK -> GMT (default)
        if (gmtTimeZone != null)
        {
            countryTimeZoneMappings.Add(new CountryTimeZone
            {
                Id = Guid.NewGuid(),
                CountryId = uk.Id,
                TimeZoneId = gmtTimeZone.Id,
                IsDefault = true
            });
        }

        // UAE -> Arabian Time (default)
        if (arabianTimeZone != null)
        {
            countryTimeZoneMappings.Add(new CountryTimeZone
            {
                Id = Guid.NewGuid(),
                CountryId = uae.Id,
                TimeZoneId = arabianTimeZone.Id,
                IsDefault = true
            });
        }

        // Singapore -> Singapore Time (default)
        if (singaporeTimeZone != null)
        {
            countryTimeZoneMappings.Add(new CountryTimeZone
            {
                Id = Guid.NewGuid(),
                CountryId = singapore.Id,
                TimeZoneId = singaporeTimeZone.Id,
                IsDefault = true
            });
        }

        // Australia -> AUS Eastern (default)
        if (ausTimeZone != null)
        {
            countryTimeZoneMappings.Add(new CountryTimeZone
            {
                Id = Guid.NewGuid(),
                CountryId = australia.Id,
                TimeZoneId = ausTimeZone.Id,
                IsDefault = true
            });
        }

        // Canada -> Multiple timezones (EST as default for now)
        if (estTimeZone != null)
        {
            countryTimeZoneMappings.Add(new CountryTimeZone
            {
                Id = Guid.NewGuid(),
                CountryId = canada.Id,
                TimeZoneId = estTimeZone.Id,
                IsDefault = true
            });
        }

        // Germany -> CET (default)
        if (cetTimeZone != null)
        {
            countryTimeZoneMappings.Add(new CountryTimeZone
            {
                Id = Guid.NewGuid(),
                CountryId = germany.Id,
                TimeZoneId = cetTimeZone.Id,
                IsDefault = true
            });
        }

        // France -> CET (default)
        if (cetTimeZone != null)
        {
            countryTimeZoneMappings.Add(new CountryTimeZone
            {
                Id = Guid.NewGuid(),
                CountryId = france.Id,
                TimeZoneId = cetTimeZone.Id,
                IsDefault = true
            });
        }

        // Japan -> JST (default)
        if (japanTimeZone != null)
        {
            countryTimeZoneMappings.Add(new CountryTimeZone
            {
                Id = Guid.NewGuid(),
                CountryId = japan.Id,
                TimeZoneId = japanTimeZone.Id,
                IsDefault = true
            });
        }

        // China -> CST (default)
        if (chinaTimeZone != null)
        {
            countryTimeZoneMappings.Add(new CountryTimeZone
            {
                Id = Guid.NewGuid(),
                CountryId = china.Id,
                TimeZoneId = chinaTimeZone.Id,
                IsDefault = true
            });
        }

        // Save all country-timezone mappings
        context.CountryTimeZones.AddRange(countryTimeZoneMappings);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} country-timezone mappings", countryTimeZoneMappings.Count);

        logger.LogInformation("Completed seeding countries, states, and cities");
    }

    private static async Task AddBasicStatesForCountry(AppDbContext context, Country country, (string Name, string Code)[] states)
    {
        var order = 1;
        foreach (var (name, code) in states)
        {
            var state = new State
            {
                Id = Guid.NewGuid(),
                Name = name,
                Code = code,
                CountryId = country.Id,
                DisplayOrder = order++,
                IsActive = true
            };
            context.States.Add(state);
        }
        await context.SaveChangesAsync();
    }
}
