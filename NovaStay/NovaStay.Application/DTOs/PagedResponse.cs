namespace NovaStay.Application.DTOs;

public class PagedResponse<T>
{
    public IReadOnlyList<T> Data { get; set; } = [];
    public int TotalRecords { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
