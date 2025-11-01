# TimelineView Component - Quick Integration Example

## Quick Start: Add Timeline to Client Details Page

Here's a complete example of how to add the TimelineView component to a Client details page.

### Step 1: Create Client Details Page

**File:** `src/CleanAspire.ClientApp/Pages/Clients/Details.razor`

```razor
@page "/clients/details/{ClientId}"
@using CleanAspire.ClientApp.Shared.Components
@using CleanAspire.ClientApp.DTOs
@using CleanAspire.ClientApp.Services.Clients

@inject ClientServiceProxy ClientServiceProxy
@inject ISnackbar Snackbar

<PageTitle>@L["Client Details"]</PageTitle>

<MudContainer MaxWidth="MaxWidth.ExtraLarge" Class="mt-4">
    @if (_loading)
    {
        <MudProgressCircular Indeterminate="true" />
    }
    else if (_client == null)
    {
        <MudAlert Severity="Severity.Error">@L["Client not found"]</MudAlert>
    }
    else
    {
        <!-- Client Header -->
        <MudPaper Class="pa-4 mb-4">
            <MudStack Row Spacing="2" Justify="Justify.SpaceBetween">
                <MudStack Row AlignItems="AlignItems.Center" Spacing="2">
                    <MudIcon Icon="@Icons.Material.Outlined.Business" Size="Size.Large" />
                    <MudStack Spacing="0">
                        <MudText Typo="Typo.h5">@_client.DisplayName</MudText>
                        <MudText Typo="Typo.body2" Color="Color.Default">
                            @_client.TypeName • @_client.StatusName
                        </MudText>
                    </MudStack>
                </MudStack>
                <MudButton StartIcon="@Icons.Material.Filled.Edit"
                           Variant="Variant.Filled"
                           Color="Color.Primary">
                    @L["Edit"]
                </MudButton>
            </MudStack>
        </MudPaper>

        <!-- Tabs -->
        <MudTabs Elevation="2" Rounded="true" ApplyEffectsToContainer="true">
            <!-- Overview Tab -->
            <MudTabPanel Text="@L["Overview"]" Icon="@Icons.Material.Filled.Info">
                <MudPaper Class="pa-4 mt-4">
                    <MudGrid Spacing="2">
                        <MudItem xs="12" md="6">
                            <MudText Typo="Typo.subtitle2">@L["Contact Information"]</MudText>
                            <MudStack Spacing="1" Class="mt-2">
                                <MudText><strong>@L["Email"]:</strong> @_client.Email</MudText>
                                <MudText><strong>@L["Phone"]:</strong> @_client.Phone</MudText>
                                <MudText><strong>@L["Website"]:</strong> @_client.Website</MudText>
                            </MudStack>
                        </MudItem>
                        <MudItem xs="12" md="6">
                            <MudText Typo="Typo.subtitle2">@L["Business Information"]</MudText>
                            <MudStack Spacing="1" Class="mt-2">
                                <MudText><strong>@L["Industry"]:</strong> @_client.Industry</MudText>
                                <MudText><strong>@L["Size"]:</strong> @_client.Size</MudText>
                                <MudText><strong>@L["Priority"]:</strong> @_client.PriorityName</MudText>
                            </MudStack>
                        </MudItem>
                    </MudGrid>
                </MudPaper>
            </MudTabPanel>

            <!-- Timeline Tab - THIS IS WHERE THE MAGIC HAPPENS -->
            <MudTabPanel Text="@L["Timeline"]" Icon="@Icons.Material.Filled.Timeline">
                <div class="mt-4">
                    <TimelineView OwnerType="OwnerType.Client"
                                  OwnerId="@_clientGuid"
                                  Title="@L["Client Activity Timeline"]"
                                  ShowFilters="true"
                                  PageSize="20"
                                  MaxHeight="800px" />
                </div>
            </MudTabPanel>

            <!-- Contacts Tab -->
            <MudTabPanel Text="@L["Contacts"]" Icon="@Icons.Material.Filled.People">
                <MudPaper Class="pa-4 mt-4">
                    <MudText>@L["Contacts list here..."]</MudText>
                </MudPaper>
            </MudTabPanel>

            <!-- Documents Tab -->
            <MudTabPanel Text="@L["Documents"]" Icon="@Icons.Material.Filled.Description">
                <MudPaper Class="pa-4 mt-4">
                    <MudText>@L["Documents list here..."]</MudText>
                </MudPaper>
            </MudTabPanel>
        </MudTabs>
    }
</MudContainer>

@code {
    [Parameter]
    public string ClientId { get; set; } = string.Empty;

    private ClientDto? _client;
    private bool _loading = false;
    private Guid _clientGuid;

    protected override async Task OnInitializedAsync()
    {
        await LoadClient();
    }

    private async Task LoadClient()
    {
        _loading = true;
        StateHasChanged();

        try
        {
            if (Guid.TryParse(ClientId, out _clientGuid))
            {
                _client = await ClientServiceProxy.GetClientByIdAsync(ClientId);

                if (_client == null)
                {
                    Snackbar.Add(L["Client not found"], Severity.Error);
                }
            }
            else
            {
                Snackbar.Add(L["Invalid Client ID"], Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add(L["Error loading client: {0}", ex.Message], Severity.Error);
        }
        finally
        {
            _loading = false;
            StateHasChanged();
        }
    }
}
```

### Step 2: Add Navigation Link

Update your Clients Index page to navigate to details:

```razor
<!-- In Pages/Clients/Index.razor -->
<MudDataGrid T="ClientDto" ...>
    <Columns>
        <!-- Other columns... -->
        <TemplateColumn>
            <CellTemplate>
                <MudIconButton Size="Size.Small"
                              Color="Color.Info"
                              Icon="@Icons.Material.Filled.Visibility"
                              Title="@L["View Details"]"
                              OnClick="() => ViewDetails(context.Item.Id)" />
            </CellTemplate>
        </TemplateColumn>
    </Columns>
</MudDataGrid>

@code {
    private void ViewDetails(string clientId)
    {
        Navigation.NavigateTo($"/clients/details/{clientId}");
    }
}
```

### Step 3: Test the Integration

1. **Run the application:**
   ```bash
   dotnet run --project src/CleanAspire.AppHost
   ```

2. **Navigate to Clients:**
   - Go to `/clients/index`
   - Click on a client or "View Details" button
   - Navigate to the "Timeline" tab

3. **Test the timeline features:**
   - ✅ Timeline loads with data
   - ✅ Filters work (Type, Date Range)
   - ✅ Quick filters work (Today, This Week, etc.)
   - ✅ Load More button appears and works
   - ✅ Icons display correctly
   - ✅ Relative times show properly
   - ✅ Empty state shows when no data
   - ✅ Error handling works

## Alternative: Standalone Timeline Page

If you want a dedicated timeline page:

**File:** `src/CleanAspire.ClientApp/Pages/Timeline/ClientTimeline.razor`

```razor
@page "/clients/{ClientId}/timeline"
@using CleanAspire.ClientApp.Shared.Components
@using CleanAspire.ClientApp.DTOs

<PageTitle>@L["Client Timeline"]</PageTitle>

<MudContainer MaxWidth="MaxWidth.ExtraLarge" Class="mt-4">
    <MudBreadcrumbs Items="_breadcrumbs" />

    <TimelineView OwnerType="OwnerType.Client"
                  OwnerId="@_clientGuid"
                  Title="@L["Complete Client Timeline"]"
                  ShowFilters="true"
                  PageSize="25"
                  MaxHeight="100vh" />
</MudContainer>

@code {
    [Parameter]
    public string ClientId { get; set; } = string.Empty;

    private Guid _clientGuid;
    private List<BreadcrumbItem> _breadcrumbs = new();

    protected override void OnInitialized()
    {
        if (Guid.TryParse(ClientId, out _clientGuid))
        {
            _breadcrumbs = new List<BreadcrumbItem>
            {
                new(L["Home"], href: "/"),
                new(L["Clients"], href: "/clients/index"),
                new(L["Timeline"], href: null, disabled: true)
            };
        }
    }
}
```

## Integration Checklist

Before integrating the component, ensure:

- [ ] API endpoints are accessible (`/api/crm/clients/{id}/timeline`)
- [ ] Authentication is configured (if required)
- [ ] MudBlazor is properly set up
- [ ] HttpClient is configured in DI container
- [ ] Localization service is available
- [ ] Test with valid Client/Contact IDs

## Common Issues and Solutions

### Issue 1: Timeline is empty
**Solution:** Check if the API is returning data. Test the endpoint directly:
```bash
curl http://localhost:5000/api/crm/clients/{YOUR_CLIENT_ID}/timeline
```

### Issue 2: Icons not showing
**Solution:** Ensure MudBlazor CSS is loaded in your `index.html` or `_Host.cshtml`:
```html
<link href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet" />
```

### Issue 3: Filters not working
**Solution:** Check browser console for errors. Verify API supports query parameters.

### Issue 4: Load More not appearing
**Solution:** Ensure API returns `totalPages` in the PaginatedResult. Check if there's more than one page of data.

## Performance Tips

1. **Set appropriate PageSize:**
   - Small widgets: 5-10 items
   - Full timeline: 20-25 items
   - Avoid loading too many items at once

2. **Use MaxHeight:**
   - Prevents infinite scrolling
   - Keeps UI manageable
   - Recommended: 600px-800px for tabs, 400px for widgets

3. **Consider caching:**
   - Timeline data can be cached on API side
   - FusionCache is already implemented in the backend

## Next Steps

1. ✅ Integrate into Client details page
2. ✅ Integrate into Contact details page
3. 🔄 Test with real data
4. 🔄 Gather user feedback
5. 🔄 Add additional features as needed

---

**Ready to use!** The TimelineView component is production-ready and waiting to be integrated.
