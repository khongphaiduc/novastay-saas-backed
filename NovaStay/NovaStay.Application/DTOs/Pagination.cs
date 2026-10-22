namespace NovaStay.Application.DTOs;

public static class Pagination
{
    public const int MaxPageSize = 1000;

    public static int Offset(int pageIndex, int pageSize)
    {
        if (pageIndex < 1) throw new ArgumentException("pageIndex/page must be greater than 0.");
        if (pageSize is < 1 or > MaxPageSize) throw new ArgumentException($"pageSize must be between 1 and {MaxPageSize}.");
        var offset = (long)(pageIndex - 1) * pageSize;
        if (offset > int.MaxValue) throw new ArgumentException("Requested page is too large.");
        return (int)offset;
    }
}
