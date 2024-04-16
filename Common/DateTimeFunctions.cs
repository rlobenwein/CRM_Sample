using System;
using System.Collections.Generic;

namespace CRM_Sample.Common
{
    public class DateTimeFunctions
    {
        public DateTime GetNow()
        {
            return TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"));
        }
        public static int CountWorkingDays(DateTime startDate, DateTime endDate)
        {
            List<DateTime> holidays = new();

            for (int year = startDate.Year; year <= endDate.Year; year++)
            {
                holidays.AddRange(HolidaysList(year));
            }

            int workingDays = 0;

            while (startDate <= endDate)
            {
                if (startDate.DayOfWeek != DayOfWeek.Saturday && startDate.DayOfWeek != DayOfWeek.Sunday & !holidays.Contains(startDate))
                    workingDays++;

                startDate = startDate.AddDays(1);
            }

            return workingDays;
        }
        public static List<DateTime> HolidaysList(int year)
        {
            List<DateTime> holidays = new()
            {
                new DateTime(year, 1, 1),
                new DateTime(year, 4, 21),
                new DateTime(year, 5, 1),
                new DateTime(year, 9, 7),
                new DateTime(year, 10, 12),
                new DateTime(year, 11, 2),
                new DateTime(year, 11, 15),
                new DateTime(year, 12, 25),
                CalculateEaster(year).AddDays(-2), // Páscoa
                CalculateEaster(year).AddDays(-47), // Terça-feira de carnaval
                CalculateEaster(year).AddDays(-48), // Segunda-feira de carnaval
                CalculateEaster(year).AddDays(60), // Corpus Christi
            };

            return holidays;
        }
        private static DateTime CalculateEaster(int year)
        {
            int goldenNumber = (year % 19) + 1;
            int century = year / 100 + 1;
            int skippedLeapYears = 3 * century / 4 - 12;
            int correction = (8 * century + 5) / 25 - 5;
            int sunday = 5 * year / 4 - skippedLeapYears - 10;
            int epact = (11 * goldenNumber + 20 + correction - skippedLeapYears) % 30;
            if ((epact == 25 && goldenNumber > 11) || epact == 24)
                epact++;

            int day = 44 - epact;
            if (day < 21)
                day += 30;

            day += 7 - (sunday + day) % 7;
            if (day > 31)
            {
                int month = 4;
                day -= 31;
                return new DateTime(year, month, day);
            }
            else
            {
                int month = 3;
                return new DateTime(year, month, day);
            }
        }
    }
}

