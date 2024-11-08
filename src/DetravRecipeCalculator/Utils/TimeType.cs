namespace DetravRecipeCalculator.Utils
{

    public enum TimeType
    {
        Auto = 0,
        Tick = 1,
        Second = 2,
        Minute = 3,
        Hour = 4,
        Day = 5,
        Week = 6,
        Month = 7,
        Year = 8
    }

    public static class TimeTypeExtentions
    {
        public static double GetTimeInSeconds(this TimeType timeType)
        {
            switch (timeType)
            {
                case TimeType.Tick: return 0.05;
                case TimeType.Second: return 1;
                case TimeType.Minute: return 60;
                case TimeType.Hour: return 60 * 60;
                case TimeType.Day: return 60 * 60 * 24;
                case TimeType.Week: return 60 * 60 * 24 * 7;
                case TimeType.Month: return 60 * 60 * 24 * 30;
                case TimeType.Year: return 60 * 60 * 24 * 365;
            }

            return 1;
        }

        public static string GetLocalizedShortValue(this TimeType timeType)
        {
            switch (timeType)
            {
                case TimeType.Tick: return Xloc.Get("__Enum_TimeType_Tick_v");
                case TimeType.Second: return Xloc.Get("__Enum_TimeType_Second_v");
                case TimeType.Minute: return Xloc.Get("__Enum_TimeType_Minute_v");
                case TimeType.Hour: return Xloc.Get("__Enum_TimeType_Hour_v");
                case TimeType.Day: return Xloc.Get("__Enum_TimeType_Day_v");
                case TimeType.Week: return Xloc.Get("__Enum_TimeType_Week_v");
                case TimeType.Month: return Xloc.Get("__Enum_TimeType_Month_v");
                case TimeType.Year: return Xloc.Get("__Enum_TimeType_Year_v");
            }

            return Xloc.Get("__Enum_TimeType_Auto_v");
        }
    }
}
