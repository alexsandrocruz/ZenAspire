# CleanAspire CRM Dialog Components

This directory contains comprehensive MudBlazor dialog components for CleanAspire CRM features.

## Available Dialogs

### 1. ActivityDialog.razor
**Purpose:** Create and edit CRM activities (tasks, calls, meetings, etc.)

**Features:**
- Support for 24+ activity types with icons (Task, PhoneCall, Meeting, Email, WhatsApp, Social Media, Events, etc.)
- Priority levels (Low, Normal, High, Urgent) with visual indicators
- Status management (Open, In Progress, Completed, Canceled, Deferred)
- Start and due date/time pickers with validation
- Duration and location fields
- Rich description editor (2000 chars)
- User assignment functionality
- Reminder date/time settings
- Form validation ensuring due date is after start date

**Usage Example:**
```razor
@inject IDialogService DialogService

private async Task OpenActivityDialog(Guid clientId)
{
    var parameters = new DialogParameters
    {
        { "Activity", null }, // null for create, ActivityDto for edit
        { "RegardingType", RegardingType.Client },
        { "RegardingId", clientId }
    };

    var dialog = await DialogService.ShowAsync<ActivityDialog>("New Activity", parameters);
    var result = await dialog.Result;

    if (!result.Cancelled && result.Data is ActivityRequest request)
    {
        // Call your API to create/update the activity
        await ActivityService.CreateAsync(request);
    }
}
```

### 2. NoteDialog.razor
**Purpose:** Create and edit notes attached to CRM entities

**Features:**
- Optional title field (200 chars)
- Rich note body editor (5000 chars)
- Pin note functionality for important notes
- Private note option (visible only to creator)
- Visual alerts for pinned and private states
- Form validation

**Usage Example:**
```razor
@inject IDialogService DialogService

private async Task OpenNoteDialog(Guid contactId)
{
    var parameters = new DialogParameters
    {
        { "Note", null }, // null for create, NoteDto for edit
        { "OwnerType", OwnerType.Contact },
        { "OwnerId", contactId }
    };

    var dialog = await DialogService.ShowAsync<NoteDialog>("New Note", parameters);
    var result = await dialog.Result;

    if (!result.Cancelled && result.Data is NoteRequest request)
    {
        // Call your API to create/update the note
        await NoteService.CreateAsync(request);
    }
}
```

### 3. AddressDialog.razor
**Purpose:** Create and edit physical addresses for clients and contacts

**Features:**
- Multi-line address fields (Line1, Line2)
- District/Neighborhood field
- City, State, ZIP validation
- Country dropdown with 30+ common countries
- Primary address designation
- Optional label field (Home, Office, Billing, etc.)
- Form validation for required fields

**Usage Example:**
```razor
@inject IDialogService DialogService

private async Task OpenAddressDialog(Guid clientId)
{
    var parameters = new DialogParameters
    {
        { "Address", null }, // null for create, AddressDto for edit
        { "OwnerType", OwnerType.Client },
        { "OwnerId", clientId }
    };

    var dialog = await DialogService.ShowAsync<AddressDialog>("New Address", parameters);
    var result = await dialog.Result;

    if (!result.Cancelled && result.Data is AddressRequest request)
    {
        // Call your API to create/update the address
        await AddressService.CreateAsync(request);
    }
}
```

### 4. ChannelDialog.razor
**Purpose:** Create and edit communication channels (email, phone, social media)

**Features:**
- Support for 12 channel types (Email, Phone, Mobile, WhatsApp, Website, Instagram, LinkedIn, Facebook, Twitter, Telegram, Skype, Other)
- Dynamic input field with type-specific placeholders and icons
- Advanced validation:
  - Email format validation
  - Phone number format validation (international)
  - URL validation for websites and LinkedIn
  - Social media handle validation
- Primary channel designation
- Marketing opt-in toggle for relevant channels
- Optional label field
- Visual alerts for opt-in status

**Usage Example:**
```razor
@inject IDialogService DialogService

private async Task OpenChannelDialog(Guid contactId)
{
    var parameters = new DialogParameters
    {
        { "Channel", null }, // null for create, ChannelDto for edit
        { "OwnerType", OwnerType.Contact },
        { "OwnerId", contactId }
    };

    var dialog = await DialogService.ShowAsync<ChannelDialog>("New Channel", parameters);
    var result = await dialog.Result;

    if (!result.Cancelled && result.Data is ChannelRequest request)
    {
        // Call your API to create/update the channel
        await ChannelService.CreateAsync(request);
    }
}
```

## Common Features Across All Dialogs

### MudDialog Integration
- All dialogs use MudDialogInstance for proper dialog lifecycle management
- Consistent Cancel/Save button layout
- Loading states with progress indicators
- Error message display

### Form Validation
- MudForm with real-time validation
- Required field validation
- MaxLength validation with character counters
- Custom validation rules per dialog type
- Validation error display

### Create/Edit Mode
- Single component handles both create and edit
- Edit mode: Pass existing DTO via parameter
- Create mode: Pass null DTO + OwnerType/OwnerId or RegardingType/RegardingId

### Return Values
- Returns `DialogResult.Ok(requestDto)` on successful save
- Returns `DialogResult.Cancel()` on cancel
- Caller receives request DTO and handles API calls

## Design Patterns

### Parameters
```csharp
[CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
[Parameter] public EntityDto? Entity { get; set; }
[Parameter] public OwnerType OwnerType { get; set; }
[Parameter] public Guid OwnerId { get; set; }
```

### Dialog Opening Pattern
```csharp
var parameters = new DialogParameters { /* ... */ };
var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true };
var dialog = await DialogService.ShowAsync<DialogComponent>("Title", parameters, options);
var result = await dialog.Result;

if (!result.Cancelled && result.Data is RequestDto request)
{
    // Handle the request
}
```

### Validation Pattern
```csharp
private async Task Submit()
{
    if (_form == null || !_form.IsValid)
    {
        Snackbar.Add("Please correct the validation errors", Severity.Error);
        return;
    }

    // Additional custom validation

    _isSubmitting = true;
    StateHasChanged();

    try
    {
        MudDialog.Close(DialogResult.Ok(_model));
    }
    catch (Exception ex)
    {
        Snackbar.Add($"An error occurred: {ex.Message}", Severity.Error);
        _isSubmitting = false;
    }
}
```

## Dependencies

All dialogs require:
- MudBlazor NuGet package
- ISnackbar injection for user notifications
- Corresponding DTOs from `CleanAspire.ClientApp.DTOs`

## Styling

All dialogs use:
- Variant.Outlined for consistent input styling
- Proper spacing with MudStack (Spacing="3")
- Responsive grid layout (MudGrid with xs/md breakpoints)
- Color-coded icons and alerts for better UX
- Character counters for text fields with limits

## Integration Notes

1. **API Integration:** Dialogs return request DTOs. The caller is responsible for API calls.
2. **Error Handling:** Dialogs handle validation errors. API errors should be handled by the caller.
3. **Localization Ready:** All static text can be wrapped with @L["..."] for localization.
4. **Accessibility:** All form fields have proper labels and ARIA attributes via MudBlazor.

## Future Enhancements

Potential improvements:
- Add autocomplete for user assignment in ActivityDialog
- Add address geocoding in AddressDialog
- Add channel verification workflow
- Add rich text editor for note body
- Add attachment support
- Add multi-language support
