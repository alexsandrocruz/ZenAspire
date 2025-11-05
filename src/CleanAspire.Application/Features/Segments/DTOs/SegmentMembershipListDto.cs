namespace CleanAspire.Application.Features.Segments.DTOs;

/// <summary>
/// Response DTO for paginated segment members list
/// </summary>
public class SegmentMembershipListDto
{
    /// <summary>
    /// List of segment members
    /// </summary>
    public List<SegmentMembershipDto> Members { get; set; } = new();

    /// <summary>
    /// Total number of members matching the filter
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    /// Whether there's a next page
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Whether there's a previous page
    /// </summary>
    public bool HasPreviousPage => Page > 1;
}