using CleanAspire.Application.Features.Segments.DTOs;
using CleanAspire.Domain.Enums;
using CleanAspire.Application.Pipeline;

namespace CleanAspire.Application.Features.Segments.Queries;

/// <summary>
/// Query to fetch segment members with pagination and filtering (returns DTO)
/// </summary>
public record GetSegmentMembersListQuery : IRequest<SegmentMembershipListDto>, IRequiresValidation
{
    public Guid Id { get; init; }
    public OwnerType? OwnerType { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Handler for processing GetSegmentMembersListQuery
/// </summary>
public class GetSegmentMembersListQueryHandler : IRequestHandler<GetSegmentMembersListQuery, SegmentMembershipListDto>
{
    private readonly IMediator _mediator;

    public GetSegmentMembersListQueryHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async ValueTask<SegmentMembershipListDto> Handle(GetSegmentMembersListQuery request, CancellationToken cancellationToken)
    {
        // Use the existing query to get the data
        var query = new GetSegmentMembersQuery(
            request.Id,
            request.OwnerType,
            Page: request.Page,
            PageSize: request.PageSize);

        var (members, totalCount) = await _mediator.Send(query, cancellationToken);

        return new SegmentMembershipListDto
        {
            Members = members,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}

/// <summary>
/// Validator for GetSegmentMembersListQuery
/// </summary>
public class GetSegmentMembersListQueryValidator : AbstractValidator<GetSegmentMembersListQuery>
{
    public GetSegmentMembersListQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Segment ID is required");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("Page size cannot exceed 100");

        When(x => x.OwnerType.HasValue, () =>
        {
            RuleFor(x => x.OwnerType)
                .IsInEnum()
                .WithMessage("OwnerType must be a valid enum value");
        });
    }
}