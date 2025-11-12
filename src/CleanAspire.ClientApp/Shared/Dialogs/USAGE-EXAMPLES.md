# Dialog Usage Examples - Quick Reference

## Required Imports

```razor
@using CleanAspire.ClientApp.DTOs
@using CleanAspire.ClientApp.Shared.Dialogs
@inject IDialogService DialogService
@inject ISnackbar Snackbar
```

## 1. Activity Dialog Examples

### Create New Activity for Client
```csharp
private async Task CreateClientActivity(Guid clientId)
{
    var parameters = new DialogParameters
    {
        { "Activity", null },
        { "RegardingType", RegardingType.Client },
        { "RegardingId", clientId }
    };

    var options = new DialogOptions
    {
        MaxWidth = MaxWidth.Large,
        FullWidth = true,
        CloseButton = true
    };

    var dialog = await DialogService.ShowAsync<ActivityDialog>(
        "New Activity",
        parameters,
        options
    );

    var result = await dialog.Result;

    if (!result.Cancelled && result.Data is ActivityRequest request)
    {
        try
        {
            var created = await Http.PostAsJsonAsync("/api/activities", request);
            if (created.IsSuccessStatusCode)
            {
                Snackbar.Add("Activity created successfully", Severity.Success);
                await RefreshActivities();
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
    }
}
```

### Edit Existing Activity
```csharp
private async Task EditActivity(ActivityDto activity)
{
    var parameters = new DialogParameters
    {
        { "Activity", activity },
        { "RegardingType", activity.RegardingType ?? RegardingType.Client },
        { "RegardingId", activity.RegardingId ?? Guid.Empty }
    };

    var dialog = await DialogService.ShowAsync<ActivityDialog>(
        "Edit Activity",
        parameters
    );

    var result = await dialog.Result;

    if (!result.Cancelled && result.Data is ActivityRequest request)
    {
        try
        {
            var response = await Http.PutAsJsonAsync($"/api/activities/{activity.Id}", request);
            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Activity updated successfully", Severity.Success);
                await RefreshActivities();
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
    }
}
```

### Create Task for Contact
```csharp
private async Task CreateContactTask(Guid contactId)
{
    var parameters = new DialogParameters
    {
        { "Activity", null },
        { "RegardingType", RegardingType.Contact },
        { "RegardingId", contactId }
    };

    var dialog = await DialogService.ShowAsync<ActivityDialog>("New Task", parameters);
    var result = await dialog.Result;

    if (!result.Cancelled && result.Data is ActivityRequest request)
    {
        // API call to create activity
        await ActivityService.CreateAsync(request);
    }
}
```

## 2. Note Dialog Examples

### Create Note for Client
```csharp
private async Task CreateClientNote(Guid clientId)
{
    var parameters = new DialogParameters
    {
        { "Note", null },
        { "OwnerType", OwnerType.Client },
        { "OwnerId", clientId }
    };

    var dialog = await DialogService.ShowAsync<NoteDialog>("New Note", parameters);
    var result = await dialog.Result;

    if (!result.Cancelled && result.Data is NoteRequest request)
    {
        try
        {
            var response = await Http.PostAsJsonAsync("/api/notes", request);
            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Note created successfully", Severity.Success);
                await RefreshNotes();
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
    }
}
```

### Edit Existing Note
```csharp
private async Task EditNote(NoteDto note)
{
    var parameters = new DialogParameters
    {
        { "Note", note },
        { "OwnerType", note.OwnerType },
        { "OwnerId", note.OwnerId }
    };

    var dialog = await DialogService.ShowAsync<NoteDialog>("Edit Note", parameters);
    var result = await dialog.Result;

    if (!result.Cancelled && result.Data is NoteRequest request)
    {
        var response = await Http.PutAsJsonAsync($"/api/notes/{note.Id}", request);
        if (response.IsSuccessStatusCode)
        {
            Snackbar.Add("Note updated successfully", Severity.Success);
            await RefreshNotes();
        }
    }
}
```

### Create Private Note
```csharp
// Notes can be marked as private and pinned in the dialog UI
private async Task CreatePrivateNote(Guid contactId)
{
    var parameters = new DialogParameters
    {
        { "Note", null },
        { "OwnerType", OwnerType.Contact },
        { "OwnerId", contactId }
    };

    var dialog = await DialogService.ShowAsync<NoteDialog>("New Private Note", parameters);
    var result = await dialog.Result;

    if (!result.Cancelled && result.Data is NoteRequest request)
    {
        // request.IsPrivate and request.IsPinned are set by user in dialog
        await NoteService.CreateAsync(request);
    }
}
```

## 3. Address Dialog Examples

### Create Address for Client
```csharp
private async Task CreateClientAddress(Guid clientId)
{
    var parameters = new DialogParameters
    {
        { "Address", null },
        { "OwnerType", OwnerType.Client },
        { "OwnerId", clientId }
    };

    var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true };
    var dialog = await DialogService.ShowAsync<AddressDialog>("New Address", parameters, options);
    var result = await dialog.Result;

    if (!result.Cancelled && result.Data is AddressRequest request)
    {
        try
        {
            var response = await Http.PostAsJsonAsync("/api/addresses", request);
            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Address created successfully", Severity.Success);
                await RefreshAddresses();
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
    }
}
```

### Edit Existing Address
```csharp
private async Task EditAddress(AddressDto address)
{
    var parameters = new DialogParameters
    {
        { "Address", address },
        { "OwnerType", address.OwnerType },
        { "OwnerId", address.OwnerId }
    };

    var dialog = await DialogService.ShowAsync<AddressDialog>("Edit Address", parameters);
    var result = await dialog.Result;

    if (!result.Cancelled && result.Data is AddressRequest request)
    {
        var response = await Http.PutAsJsonAsync($"/api/addresses/{address.Id}", request);
        if (response.IsSuccessStatusCode)
        {
            Snackbar.Add("Address updated successfully", Severity.Success);
            await RefreshAddresses();
        }
    }
}
```

### Create Billing Address for Contact
```csharp
private async Task CreateBillingAddress(Guid contactId)
{
    var parameters = new DialogParameters
    {
        { "Address", null },
        { "OwnerType", OwnerType.Contact },
        { "OwnerId", contactId }
    };

    var dialog = await DialogService.ShowAsync<AddressDialog>("New Billing Address", parameters);
    var result = await dialog.Result;

    if (!result.Cancelled && result.Data is AddressRequest request)
    {
        // User can set Label as "Billing" in the dialog
        await AddressService.CreateAsync(request);
    }
}
```

## 4. Channel Dialog Examples

### Create Email Channel for Contact
```csharp
private async Task CreateEmailChannel(Guid contactId)
{
    var parameters = new DialogParameters
    {
        { "Channel", null },
        { "OwnerType", OwnerType.Contact },
        { "OwnerId", contactId }
    };

    var dialog = await DialogService.ShowAsync<ChannelDialog>("New Email", parameters);
    var result = await dialog.Result;

    if (!result.Cancelled && result.Data is ChannelRequest request)
    {
        try
        {
            var response = await Http.PostAsJsonAsync("/api/channels", request);
            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Channel created successfully", Severity.Success);
                await RefreshChannels();
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
    }
}
```

### Edit Existing Channel
```csharp
private async Task EditChannel(ChannelDto channel)
{
    var parameters = new DialogParameters
    {
        { "Channel", channel },
        { "OwnerType", channel.OwnerType },
        { "OwnerId", channel.OwnerId }
    };

    var dialog = await DialogService.ShowAsync<ChannelDialog>("Edit Channel", parameters);
    var result = await dialog.Result;

    if (!result.Cancelled && result.Data is ChannelRequest request)
    {
        var response = await Http.PutAsJsonAsync($"/api/channels/{channel.Id}", request);
        if (response.IsSuccessStatusCode)
        {
            Snackbar.Add("Channel updated successfully", Severity.Success);
            await RefreshChannels();
        }
    }
}
```

### Create WhatsApp Channel with Validation
```csharp
private async Task CreateWhatsAppChannel(Guid clientId)
{
    var parameters = new DialogParameters
    {
        { "Channel", null },
        { "OwnerType", OwnerType.Client },
        { "OwnerId", clientId }
    };

    var dialog = await DialogService.ShowAsync<ChannelDialog>("New WhatsApp", parameters);
    var result = await dialog.Result;

    if (!result.Cancelled && result.Data is ChannelRequest request)
    {
        // Dialog validates phone format automatically
        // User sets Type = WhatsApp in the dialog
        await ChannelService.CreateAsync(request);
    }
}
```

## 5. Integrated Panel Examples

### Activity Panel with Dialog Integration
```razor
<MudCard>
    <MudCardHeader>
        <CardHeaderContent>
            <MudText Typo="Typo.h6">Activities</MudText>
        </CardHeaderContent>
        <CardHeaderActions>
            <MudButton StartIcon="@Icons.Material.Filled.Add"
                       Color="Color.Primary"
                       OnClick="@(() => CreateActivity())">
                Add Activity
            </MudButton>
        </CardHeaderActions>
    </MudCardHeader>
    <MudCardContent>
        @foreach (var activity in Activities)
        {
            <MudPaper Class="pa-3 mb-2">
                <MudStack Row="true" Justify="Justify.SpaceBetween">
                    <MudStack Spacing="1">
                        <MudText Typo="Typo.subtitle1">@activity.Subject</MudText>
                        <MudText Typo="Typo.body2" Color="Color.Secondary">
                            @activity.Start.ToString("g")
                        </MudText>
                    </MudStack>
                    <MudIconButton Icon="@Icons.Material.Filled.Edit"
                                   Size="Size.Small"
                                   OnClick="@(() => EditActivity(activity))" />
                </MudStack>
            </MudPaper>
        }
    </MudCardContent>
</MudCard>

@code {
    [Parameter] public Guid EntityId { get; set; }
    [Parameter] public RegardingType EntityType { get; set; }

    private List<ActivityDto> Activities { get; set; } = new();

    private async Task CreateActivity()
    {
        var parameters = new DialogParameters
        {
            { "Activity", null },
            { "RegardingType", EntityType },
            { "RegardingId", EntityId }
        };

        var dialog = await DialogService.ShowAsync<ActivityDialog>("New Activity", parameters);
        var result = await dialog.Result;

        if (!result.Cancelled && result.Data is ActivityRequest request)
        {
            await Http.PostAsJsonAsync("/api/activities", request);
            await LoadActivities();
        }
    }

    private async Task EditActivity(ActivityDto activity)
    {
        var parameters = new DialogParameters
        {
            { "Activity", activity },
            { "RegardingType", EntityType },
            { "RegardingId", EntityId }
        };

        var dialog = await DialogService.ShowAsync<ActivityDialog>("Edit Activity", parameters);
        var result = await dialog.Result;

        if (!result.Cancelled && result.Data is ActivityRequest request)
        {
            await Http.PutAsJsonAsync($"/api/activities/{activity.Id}", request);
            await LoadActivities();
        }
    }

    private async Task LoadActivities()
    {
        var response = await Http.GetAsync($"/api/activities?regardingType={EntityType}&regardingId={EntityId}");
        if (response.IsSuccessStatusCode)
        {
            Activities = await response.Content.ReadFromJsonAsync<List<ActivityDto>>() ?? new();
        }
    }
}
```

## 6. Quick Action Buttons

### Floating Action Button for Multiple Dialogs
```razor
<MudFab Color="Color.Primary"
        StartIcon="@Icons.Material.Filled.Add"
        OnClick="ToggleActionMenu"
        Style="position: fixed; bottom: 20px; right: 20px;" />

@if (_showActionMenu)
{
    <MudPaper Class="pa-2" Style="position: fixed; bottom: 80px; right: 20px;">
        <MudStack Spacing="1">
            <MudButton StartIcon="@Icons.Material.Filled.Task"
                       FullWidth="true"
                       OnClick="@(() => CreateActivity())">
                New Activity
            </MudButton>
            <MudButton StartIcon="@Icons.Material.Filled.Note"
                       FullWidth="true"
                       OnClick="@(() => CreateNote())">
                New Note
            </MudButton>
            <MudButton StartIcon="@Icons.Material.Filled.LocationOn"
                       FullWidth="true"
                       OnClick="@(() => CreateAddress())">
                New Address
            </MudButton>
            <MudButton StartIcon="@Icons.Material.Filled.Email"
                       FullWidth="true"
                       OnClick="@(() => CreateChannel())">
                New Channel
            </MudButton>
        </MudStack>
    </MudPaper>
}

@code {
    [Parameter] public Guid EntityId { get; set; }
    [Parameter] public OwnerType OwnerType { get; set; }

    private bool _showActionMenu = false;

    private void ToggleActionMenu() => _showActionMenu = !_showActionMenu;

    private async Task CreateActivity()
    {
        _showActionMenu = false;
        var parameters = new DialogParameters
        {
            { "Activity", null },
            { "RegardingType", (RegardingType)OwnerType }, // Assuming enums match
            { "RegardingId", EntityId }
        };
        var dialog = await DialogService.ShowAsync<ActivityDialog>("New Activity", parameters);
        var result = await dialog.Result;
        // Handle result...
    }

    private async Task CreateNote()
    {
        _showActionMenu = false;
        var parameters = new DialogParameters
        {
            { "Note", null },
            { "OwnerType", OwnerType },
            { "OwnerId", EntityId }
        };
        var dialog = await DialogService.ShowAsync<NoteDialog>("New Note", parameters);
        var result = await dialog.Result;
        // Handle result...
    }

    // Similar methods for CreateAddress and CreateChannel...
}
```

## Tips and Best Practices

1. **Always check for cancellation:**
   ```csharp
   if (!result.Cancelled && result.Data is RequestDto request)
   {
       // Process the request
   }
   ```

2. **Use appropriate dialog options:**
   ```csharp
   var options = new DialogOptions
   {
       MaxWidth = MaxWidth.Large,  // or Medium, Small
       FullWidth = true,
       CloseButton = true,
       DisableBackdropClick = false
   };
   ```

3. **Handle API errors gracefully:**
   ```csharp
   try
   {
       var response = await Http.PostAsJsonAsync("/api/endpoint", request);
       response.EnsureSuccessStatusCode();
       Snackbar.Add("Success!", Severity.Success);
   }
   catch (HttpRequestException ex)
   {
       Snackbar.Add($"API Error: {ex.Message}", Severity.Error);
   }
   ```

4. **Refresh data after changes:**
   ```csharp
   if (!result.Cancelled && result.Data is RequestDto request)
   {
       await CreateOrUpdateEntity(request);
       await RefreshEntityList(); // Always refresh
       StateHasChanged();         // Update UI
   }
   ```

5. **Use consistent dialog titles:**
   - Create: "New [Entity]"
   - Edit: "Edit [Entity]"
   - Quick actions: "Add [Entity]"
