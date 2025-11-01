# TimelineView Component - Usage Guide

## Overview

The `TimelineView` is a comprehensive, reusable Blazor component for CleanAspire that displays a unified feed of Activities, Interactions, and Notes in a timeline format.

**File Location:** `D:\@dev\ZenAspire\src\CleanAspire.ClientApp\Shared\Components\TimelineView.razor`

## Features

✅ **Unified Timeline Display** - Shows Activities, Interactions, and Notes in chronological order
✅ **Smart Filtering** - Filter by type, date range, with quick filters (Today, This Week, etc.)
✅ **Pagination** - Load more functionality for handling large datasets
✅ **Responsive Design** - Built with MudBlazor components, fully responsive
✅ **Icon Mapping** - Automatic icon assignment based on interaction/activity type
✅ **Relative Time Display** - Shows "2 hours ago", "Yesterday", etc.
✅ **Color Coding** - Visual indicators for status, priority, and type
✅ **Duration Formatting** - Smart display of activity/interaction durations
✅ **Loading States** - Proper loading, empty state, and error handling
✅ **Collapsible Filters** - Clean UI with expandable filter panel

## Component Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `OwnerType` | `OwnerType` | ✅ Yes | - | Type of entity (Client, Contact, etc.) |
| `OwnerId` | `Guid` | ✅ Yes | - | ID of the entity |
| `Title` | `string` | ❌ No | "Timeline" | Timeline section title |
| `ShowFilters` | `bool` | ❌ No | `true` | Show/hide filter panel |
| `PageSize` | `int` | ❌ No | `20` | Items per page |
| `MaxHeight` | `string` | ❌ No | "600px" | CSS max-height for timeline container |

## Basic Usage Examples

### Example 1: Client Timeline
```razor
@page "/clients/details/{ClientId}"
@using CleanAspire.ClientApp.Shared.Components
@using CleanAspire.ClientApp.DTOs

<!-- Display timeline for a specific client -->
<TimelineView OwnerType="OwnerType.Client"
              OwnerId="@clientId"
              Title="Client Activity Timeline"
              ShowFilters="true" />

@code {
    [Parameter]
    public string ClientId { get; set; } = string.Empty;

    private Guid clientId => Guid.Parse(ClientId);
}
```

### Example 2: Contact Timeline
```razor
@page "/contacts/details/{ContactId}"
@using CleanAspire.ClientApp.Shared.Components
@using CleanAspire.ClientApp.DTOs

<!-- Display timeline for a specific contact -->
<TimelineView OwnerType="OwnerType.Contact"
              OwnerId="@contactId"
              Title="Contact Interaction History" />

@code {
    [Parameter]
    public string ContactId { get; set; } = string.Empty;

    private Guid contactId => Guid.Parse(ContactId);
}
```

### Example 3: Compact Timeline (No Filters, Custom Height)
```razor
<!-- Compact timeline in a dashboard widget -->
<MudCard>
    <MudCardHeader>
        <MudText Typo="Typo.h6">Recent Activity</MudText>
    </MudCardHeader>
    <MudCardContent>
        <TimelineView OwnerType="OwnerType.Client"
                      OwnerId="@clientId"
                      Title="Recent Activity"
                      ShowFilters="false"
                      PageSize="10"
                      MaxHeight="400px" />
    </MudCardContent>
</MudCard>

@code {
    private Guid clientId = Guid.Parse("12345678-1234-1234-1234-123456789012");
}
```

### Example 4: Full-Featured Timeline in a Tab
```razor
<MudTabs Elevation="2" Rounded="true" ApplyEffectsToContainer="true">
    <MudTabPanel Text="Overview">
        <!-- Client/Contact Overview -->
    </MudTabPanel>

    <MudTabPanel Text="Timeline">
        <TimelineView OwnerType="OwnerType.Client"
                      OwnerId="@clientId"
                      Title="Complete Timeline"
                      ShowFilters="true"
                      PageSize="25"
                      MaxHeight="800px" />
    </MudTabPanel>

    <MudTabPanel Text="Documents">
        <!-- Documents -->
    </MudTabPanel>
</MudTabs>

@code {
    [Parameter]
    public Guid clientId { get; set; }
}
```

## API Integration

The component calls the following API endpoints:

- **Client Timeline:** `GET /api/crm/clients/{clientId}/timeline`
- **Contact Timeline:** `GET /api/crm/contacts/{contactId}/timeline`

### Query Parameters:
- `pageNumber` (int) - Page number (1-based)
- `pageSize` (int) - Items per page
- `typeFilter` (int?) - Filter by type (1=Activity, 2=Interaction, 3=Note)
- `startDate` (DateTime?) - Filter start date
- `endDate` (DateTime?) - Filter end date

### Response Format:
```json
{
  "items": [
    {
      "id": "guid",
      "type": 1,
      "date": "2024-10-31T10:30:00Z",
      "title": "Phone Call",
      "description": "Discussed Q4 requirements",
      "icon": "phone",
      "badge": "Completed",
      "colorHint": "success",
      "typeName": "Phone Call",
      "status": "Completed",
      "duration": 15,
      "user": "user@example.com",
      "isPinned": false,
      "isPrivate": false
    }
  ],
  "totalCount": 150,
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 8
}
```

## Icon Mapping

The component automatically maps item types to MudBlazor icons:

| Type | Icon |
|------|------|
| Email | `Icons.Material.Filled.Email` |
| Phone | `Icons.Material.Filled.Phone` |
| WhatsApp | `Icons.Material.Filled.WhatsApp` |
| Meeting | `Icons.Material.Filled.Event` |
| Task | `Icons.Material.Filled.Task` |
| Note | `Icons.Material.Filled.StickyNote2` |
| SMS | `Icons.Material.Filled.Sms` |
| Video Call | `Icons.Material.Filled.VideoCall` |
| Event | `Icons.Material.Filled.CalendarToday` |

## Color Coding

Timeline items are color-coded based on their `ColorHint` property:

| ColorHint | MudBlazor Color | Usage |
|-----------|----------------|-------|
| `"success"` | `Color.Success` | Completed, Inbound |
| `"info"` | `Color.Info` | Open, Outbound |
| `"warning"` | `Color.Warning` | In Progress, Pinned |
| `"error"` | `Color.Error` | Canceled, Failed |
| `"primary"` | `Color.Primary` | Default highlight |
| `"default"` | `Color.Default` | Neutral items |

## Filter Features

### Type Filters
- **All Types** - Show everything
- **Activities** - Tasks, meetings, calls, emails
- **Interactions** - Completed communications
- **Notes** - User notes and comments

### Date Filters
- **Start Date** - Filter items from this date forward
- **End Date** - Filter items up to this date

### Quick Filters (Chips)
- **Today** - Items from today only
- **This Week** - Current week's items
- **This Month** - Current month's items
- **All Time** - No date filtering

## Relative Time Display

The component shows relative times for recent items:
- Less than 1 minute: "Just now"
- Less than 1 hour: "X minutes ago"
- Less than 24 hours: "X hours ago"
- Less than 7 days: "X days ago"
- Less than 30 days: "X weeks ago"
- Less than 365 days: "X months ago"
- Older: Absolute date (e.g., "Oct 15, 2023")

## Customization Tips

### 1. Change Timeline Orientation
To change timeline orientation, modify the `MudTimeline` component properties:
```razor
<MudTimeline TimelineOrientation="TimelineOrientation.Horizontal"
             TimelinePosition="TimelinePosition.Start">
```

### 2. Customize Empty State
Find the empty state section and customize:
```razor
<MudStack AlignItems="AlignItems.Center" Class="py-8">
    <MudIcon Icon="@YourCustomIcon" Size="Size.Large" />
    <MudText Typo="Typo.h6">Your Custom Message</MudText>
</MudStack>
```

### 3. Add Custom Actions
Add action buttons to timeline items by modifying the `ItemContent` section:
```razor
<MudStack Row Spacing="1" Class="mt-2">
    <MudIconButton Icon="@Icons.Material.Filled.Edit" Size="Size.Small" />
    <MudIconButton Icon="@Icons.Material.Filled.Delete" Size="Size.Small" />
</MudStack>
```

### 4. Adjust Styling
Customize the appearance using MudBlazor's styling props:
```razor
<TimelineView OwnerType="OwnerType.Client"
              OwnerId="@clientId"
              MaxHeight="100vh"
              Class="custom-timeline" />
```

## Error Handling

The component includes comprehensive error handling:

1. **API Errors** - Displays error alert with message
2. **Loading States** - Shows spinner during data fetch
3. **Empty States** - Friendly message when no items exist
4. **Network Errors** - Catches and displays HTTP errors

Example error display:
```razor
<MudAlert Severity="Severity.Error" Class="mb-4">
    @L["Error loading timeline: {0}", _error]
</MudAlert>
```

## Localization

The component is fully localized using `@L["Key"]` syntax. All user-facing strings support localization:

- Filter labels
- Button text
- Error messages
- Relative time strings
- Type names

## Performance Considerations

1. **Pagination** - Default 20 items per page, prevents loading large datasets
2. **Lazy Loading** - "Load More" button for incremental loading
3. **Efficient Rendering** - Only renders visible items
4. **Smart Updates** - Reloads only when parameters change

## Testing

### Test the Component Manually:

1. **Navigate to a Client Details page**
2. **Add the TimelineView component**
3. **Test scenarios:**
   - Empty timeline (no data)
   - Single item
   - Multiple pages
   - Filter by type
   - Filter by date range
   - Quick filters
   - Load more functionality
   - Error handling (invalid ID)

### Example Test Page:

Create a test page at `Pages/Timeline/TestTimeline.razor`:

```razor
@page "/timeline/test"
@using CleanAspire.ClientApp.Shared.Components
@using CleanAspire.ClientApp.DTOs

<PageTitle>Timeline Component Test</PageTitle>

<MudContainer MaxWidth="MaxWidth.ExtraLarge" Class="mt-4">
    <MudText Typo="Typo.h3" Class="mb-4">Timeline Component Tests</MudText>

    <MudGrid>
        <MudItem xs="12">
            <MudPaper Class="pa-4 mb-4">
                <MudText Typo="Typo.h6" Class="mb-2">Test with Real Client ID</MudText>
                <MudTextField @bind-Value="testClientId"
                              Label="Enter Client GUID"
                              Variant="Variant.Outlined" />
                <MudButton OnClick="LoadTimeline" Variant="Variant.Filled" Color="Color.Primary" Class="mt-2">
                    Load Timeline
                </MudButton>
            </MudPaper>
        </MudItem>

        @if (showTimeline)
        {
            <MudItem xs="12">
                <TimelineView OwnerType="OwnerType.Client"
                              OwnerId="@clientGuid"
                              Title="Test Timeline"
                              ShowFilters="true"
                              PageSize="10" />
            </MudItem>
        }
    </MudGrid>
</MudContainer>

@code {
    private string testClientId = "";
    private Guid clientGuid;
    private bool showTimeline = false;

    private void LoadTimeline()
    {
        if (Guid.TryParse(testClientId, out var guid))
        {
            clientGuid = guid;
            showTimeline = true;
        }
        else
        {
            Snackbar.Add("Invalid GUID format", Severity.Error);
        }
    }
}
```

## Future Enhancements

Potential improvements for future iterations:

1. **Infinite Scroll** - Replace "Load More" with infinite scrolling
2. **Real-time Updates** - SignalR integration for live timeline updates
3. **Export Functionality** - Export timeline to PDF/CSV
4. **Item Click Actions** - Navigate to detail pages on item click
5. **Advanced Filters** - Filter by user, status, priority
6. **Search** - Full-text search within timeline items
7. **Grouping** - Group items by date (Today, Yesterday, Last Week)
8. **Attachments** - Display attachments in timeline items
9. **Comments** - Add comments directly to timeline items
10. **Pinning** - Allow users to pin/unpin items

## Troubleshooting

### Timeline Not Loading
- Verify API endpoints are accessible
- Check browser console for errors
- Ensure `OwnerId` is a valid GUID
- Verify authentication/authorization

### Filters Not Working
- Check that filter values are being passed to API
- Verify API supports the filter parameters
- Check date format compatibility

### Icons Not Displaying
- Verify MudBlazor is properly installed
- Check icon names match MudBlazor's icon set
- Ensure `@using MudBlazor` is in _Imports.razor

## Support

For issues or questions:
- Check API endpoint responses using Scalar
- Review browser console for errors
- Verify component parameters are correctly set
- Check that DTOs match API response format

---

**Component Version:** 1.0
**Last Updated:** October 31, 2024
**Compatible with:** CleanAspire .NET 10, MudBlazor 7.x
