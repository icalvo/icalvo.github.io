namespace CommandLine;

public record PartialDate(int Year, int? Month = null, int? Day = null) : IComparable<PartialDate>
{
    public static PartialDate From(DateTime date)
    {
        return new PartialDate(date.Year, date.Month, date.Day);
    }

    public static PartialDate Parse(string text)
    {
        var components = text.Split("-").Select(int.Parse).ToArray();

        return components switch
        {
            [var year] => new PartialDate(year),
            [var year, var month] => new PartialDate(year, month),
            [var year, var month, var day] => new PartialDate(year, month, day),
            _ => throw new ArgumentException("Invalid date format")
        };
    }

    public override string ToString()
    {
        return Day is not null ? $"{Year}-{Month:D2}-{Day:D2}" : Month is not null ? $"{Year}-{Month:D2}" : Year.ToString();
    }

    public int CompareTo(PartialDate? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        var yearComparison = Year.CompareTo(other.Year);
        if (yearComparison != 0) return yearComparison;
        var monthComparison = Nullable.Compare(Month, other.Month);
        if (monthComparison != 0) return monthComparison;
        return Nullable.Compare(Day, other.Day);
    }
}