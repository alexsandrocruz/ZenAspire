# Phase 2: Scalar API Test Examples

This document provides test data examples for testing the Address and Channel endpoints via Scalar.

## Prerequisites

Before testing, you need existing Client or Contact IDs. You can:
1. Use the Client/Contact endpoints to create test entities
2. Query existing clients: `GET /api/crm/clients`
3. Query existing contacts: `GET /api/crm/contacts`

For these examples, we'll use:
- **Client ID**: `client-001` (OwnerType = 1)
- **Contact ID**: `contact-001` (OwnerType = 2)

## Address Endpoints

### 1. Create Address (POST /api/crm/addresses)

**Create primary business address for a Client:**
```json
{
  "ownerType": 1,
  "ownerId": "client-001",
  "line1": "Av. Paulista, 1578",
  "line2": "Conj. 702",
  "district": "Bela Vista",
  "city": "São Paulo",
  "state": "SP",
  "zip": "01310-200",
  "country": "BR",
  "geoLat": -23.5631,
  "geoLng": -46.6558,
  "isPrimary": true,
  "label": "Headquarters"
}
```

**Create secondary warehouse address:**
```json
{
  "ownerType": 1,
  "ownerId": "client-001",
  "line1": "Rua dos Três Irmãos, 421",
  "district": "Vila Progredior",
  "city": "São Paulo",
  "state": "SP",
  "zip": "05615-010",
  "country": "BR",
  "isPrimary": false,
  "label": "Warehouse"
}
```

**Create residential address for a Contact:**
```json
{
  "ownerType": 2,
  "ownerId": "contact-001",
  "line1": "Rua Oscar Freire, 2500",
  "line2": "Apto 501",
  "district": "Jardins",
  "city": "São Paulo",
  "state": "SP",
  "zip": "01426-001",
  "country": "BR",
  "geoLat": -23.5641,
  "geoLng": -46.6697,
  "isPrimary": true,
  "label": "Home"
}
```

**Create address without geocoding:**
```json
{
  "ownerType": 2,
  "ownerId": "contact-001",
  "line1": "Rua Augusta, 1508",
  "district": "Consolação",
  "city": "São Paulo",
  "state": "SP",
  "zip": "01304-001",
  "country": "BR",
  "isPrimary": false,
  "label": "Office"
}
```

### 2. Update Address (PUT /api/crm/addresses)

**Update address with geocoding:**
```json
{
  "id": "address-id-from-create-response",
  "ownerType": 1,
  "ownerId": "client-001",
  "line1": "Av. Paulista, 1578",
  "line2": "Conj. 705",
  "district": "Bela Vista",
  "city": "São Paulo",
  "state": "SP",
  "zip": "01310-200",
  "country": "BR",
  "geoLat": -23.5631,
  "geoLng": -46.6558,
  "isPrimary": true,
  "label": "Main Office"
}
```

### 3. Delete Address (DELETE /api/crm/addresses/{id})

**Request:**
```
DELETE /api/crm/addresses/{address-id}
```
No body required.

### 4. Set Primary Address (POST /api/crm/addresses/{id}/set-primary)

**Request:**
```
POST /api/crm/addresses/{address-id}/set-primary
```
No body required. This will unset any existing primary address for the same owner.

### 5. Get Address by ID (GET /api/crm/addresses/{id})

**Request:**
```
GET /api/crm/addresses/{address-id}
```

**Expected Response:**
```json
{
  "id": "address-id",
  "tenantId": "tenant-001",
  "ownerType": 1,
  "ownerId": "client-001",
  "line1": "Av. Paulista, 1578",
  "line2": "Conj. 702",
  "district": "Bela Vista",
  "city": "São Paulo",
  "state": "SP",
  "zip": "01310-200",
  "country": "BR",
  "geoLat": -23.5631,
  "geoLng": -46.6558,
  "isPrimary": true,
  "label": "Headquarters",
  "fullAddress": "Av. Paulista, 1578, Conj. 702, Bela Vista, São Paulo - SP, 01310-200, BR",
  "created": "2025-10-31T20:30:00Z",
  "lastModified": null
}
```

### 6. Get Addresses by Owner (GET /api/crm/addresses/owner/{ownerType}/{ownerId})

**Request:**
```
GET /api/crm/addresses/owner/1/client-001
```

**Expected Response:**
```json
[
  {
    "id": "address-1",
    "ownerType": 1,
    "ownerId": "client-001",
    "line1": "Av. Paulista, 1578",
    "isPrimary": true,
    "label": "Headquarters",
    "fullAddress": "Av. Paulista, 1578, Conj. 702, Bela Vista, São Paulo - SP, 01310-200, BR"
  },
  {
    "id": "address-2",
    "ownerType": 1,
    "ownerId": "client-001",
    "line1": "Rua dos Três Irmãos, 421",
    "isPrimary": false,
    "label": "Warehouse",
    "fullAddress": "Rua dos Três Irmãos, 421, Vila Progredior, São Paulo - SP, 05615-010, BR"
  }
]
```

### 7. Get Primary Address (GET /api/crm/addresses/owner/{ownerType}/{ownerId}/primary)

**Request:**
```
GET /api/crm/addresses/owner/1/client-001/primary
```

---

## Channel Identity Endpoints

### 1. Create Channel (POST /api/crm/channels)

**Create primary email for Client:**
```json
{
  "ownerType": 1,
  "ownerId": "client-001",
  "type": 1,
  "value": "contato@empresa.com.br",
  "isPrimary": true,
  "label": "Corporate"
}
```

**Create mobile with WhatsApp:**
```json
{
  "ownerType": 2,
  "ownerId": "contact-001",
  "type": 3,
  "value": "+5511987654321",
  "isPrimary": true,
  "label": "Personal"
}
```

**Create WhatsApp channel:**
```json
{
  "ownerType": 2,
  "ownerId": "contact-001",
  "type": 4,
  "value": "+5511987654321",
  "isPrimary": true,
  "label": "WhatsApp Business"
}
```

**Create business phone:**
```json
{
  "ownerType": 1,
  "ownerId": "client-001",
  "type": 2,
  "value": "+551133334444",
  "isPrimary": false,
  "label": "Main Line"
}
```

**Create website:**
```json
{
  "ownerType": 1,
  "ownerId": "client-001",
  "type": 5,
  "value": "https://www.empresa.com.br",
  "isPrimary": false,
  "label": "Website"
}
```

**Create social media channels:**

Instagram:
```json
{
  "ownerType": 1,
  "ownerId": "client-001",
  "type": 6,
  "value": "@empresa_oficial",
  "isPrimary": false,
  "label": "Instagram"
}
```

LinkedIn:
```json
{
  "ownerType": 1,
  "ownerId": "client-001",
  "type": 7,
  "value": "https://linkedin.com/company/empresa",
  "isPrimary": false,
  "label": "LinkedIn"
}
```

Facebook:
```json
{
  "ownerType": 1,
  "ownerId": "client-001",
  "type": 8,
  "value": "https://facebook.com/empresa",
  "isPrimary": false,
  "label": "Facebook Page"
}
```

Twitter/X:
```json
{
  "ownerType": 1,
  "ownerId": "client-001",
  "type": 9,
  "value": "@empresa_oficial",
  "isPrimary": false,
  "label": "Twitter"
}
```

### 2. Update Channel (PUT /api/crm/channels)

**Update email and verify:**
```json
{
  "id": "channel-id-from-create-response",
  "ownerType": 1,
  "ownerId": "client-001",
  "type": 1,
  "value": "contato.corporativo@empresa.com.br",
  "isPrimary": true,
  "label": "Corporate Email"
}
```

### 3. Delete Channel (DELETE /api/crm/channels/{id})

**Request:**
```
DELETE /api/crm/channels/{channel-id}
```
No body required.

### 4. Verify Channel (POST /api/crm/channels/{id}/verify)

**Request:**
```
POST /api/crm/channels/{channel-id}/verify
```
No body required. Sets `VerifiedAt` to current timestamp.

### 5. Update Opt-In (POST /api/crm/channels/{id}/opt-in)

**Opt-in for marketing (LGPD compliance):**
```json
{
  "id": "channel-id",
  "optedIn": true
}
```

**Opt-out from marketing:**
```json
{
  "id": "channel-id",
  "optedIn": false
}
```

### 6. Get Channel by ID (GET /api/crm/channels/{id})

**Request:**
```
GET /api/crm/channels/{channel-id}
```

**Expected Response:**
```json
{
  "id": "channel-id",
  "tenantId": "tenant-001",
  "ownerType": 1,
  "ownerId": "client-001",
  "type": 1,
  "value": "contato@empresa.com.br",
  "isPrimary": true,
  "verifiedAt": "2025-10-31T20:35:00Z",
  "isVerified": true,
  "label": "Corporate",
  "optedIn": true,
  "optedInAt": "2025-10-31T20:30:00Z",
  "displayValue": "contato@empresa.com.br",
  "created": "2025-10-31T20:30:00Z",
  "lastModified": null
}
```

### 7. Get Channels by Owner (GET /api/crm/channels/owner/{ownerType}/{ownerId})

**Request:**
```
GET /api/crm/channels/owner/1/client-001
```

**Expected Response:**
```json
[
  {
    "id": "channel-1",
    "type": 1,
    "value": "contato@empresa.com.br",
    "isPrimary": true,
    "isVerified": true,
    "label": "Corporate",
    "displayValue": "contato@empresa.com.br"
  },
  {
    "id": "channel-2",
    "type": 2,
    "value": "+551133334444",
    "isPrimary": false,
    "isVerified": false,
    "label": "Main Line",
    "displayValue": "+55 11 3333-4444"
  },
  {
    "id": "channel-3",
    "type": 5,
    "value": "https://www.empresa.com.br",
    "isPrimary": false,
    "isVerified": true,
    "label": "Website",
    "displayValue": "https://www.empresa.com.br"
  }
]
```

### 8. Get Channels by Type (GET /api/crm/channels/owner/{ownerType}/{ownerId}/type/{channelType})

**Get all email channels for a client:**
```
GET /api/crm/channels/owner/1/client-001/type/1
```

**Get all phone/mobile channels:**
```
GET /api/crm/channels/owner/2/contact-001/type/3
```

**ChannelType Enum Values:**
- 1 = Email
- 2 = Phone
- 3 = Mobile
- 4 = WhatsApp
- 5 = Website
- 6 = Instagram
- 7 = LinkedIn
- 8 = Facebook
- 9 = Twitter
- 10 = Telegram
- 11 = Skype
- 99 = Other

### 9. Get Primary Channels (GET /api/crm/channels/owner/{ownerType}/{ownerId}/primary)

**Request:**
```
GET /api/crm/channels/owner/1/client-001/primary
```

**Expected Response (returns one primary per channel type):**
```json
[
  {
    "id": "channel-1",
    "type": 1,
    "value": "contato@empresa.com.br",
    "isPrimary": true,
    "label": "Corporate Email"
  },
  {
    "id": "channel-2",
    "type": 3,
    "value": "+5511987654321",
    "isPrimary": true,
    "label": "Mobile"
  }
]
```

---

## Testing Scenarios

### Scenario 1: Complete Client Setup

1. **Create a Client** (use existing Client endpoint)
2. **Add primary business address**
3. **Add warehouse address**
4. **Add corporate email** (isPrimary: true)
5. **Add business phone**
6. **Add website**
7. **Add social media channels** (Instagram, LinkedIn)
8. **Verify email channel**
9. **Get all addresses for client**
10. **Get all channels for client**

### Scenario 2: Contact with Multi-Channel Communication

1. **Create a Contact** (use existing Contact endpoint)
2. **Add home address**
3. **Add work address and set as primary**
4. **Add personal email**
5. **Add mobile with WhatsApp**
6. **Add WhatsApp Business channel**
7. **Verify mobile**
8. **Opt-in to marketing communications**
9. **Get primary channels**

### Scenario 3: LGPD Compliance Flow

1. **Create email channel** (optedIn defaults to true)
2. **Create mobile channel**
3. **User requests opt-out**
4. **Update opt-in status to false** for email
5. **Verify opt-out timestamp is recorded**
6. **User opts back in**
7. **Update opt-in status to true**
8. **Verify new opt-in timestamp**

### Scenario 4: Geocoding and Location Search

1. **Create addresses with geocoding** (GeoLat, GeoLng)
2. **Query addresses by location** (future feature)
3. **Update address with new coordinates**

### Scenario 5: Primary Address/Channel Management

1. **Create address with isPrimary: false**
2. **Create second address with isPrimary: true**
3. **Verify first address is still non-primary**
4. **Set first address as primary** (POST /set-primary)
5. **Verify second address is now non-primary**
6. **Get primary address** and confirm it's the first one

---

## Common Validation Errors

### Address Validation

**Missing required fields:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Line1": ["'Line1' must not be empty."],
    "City": ["'City' must not be empty."],
    "Country": ["'Country' must be 2 characters in length."]
  }
}
```

**Invalid geocoding:**
```json
{
  "errors": {
    "GeoLat": ["'Geo Lat' must be between -90 and 90."],
    "GeoLng": ["'Geo Lng' must be between -180 and 180."]
  }
}
```

### Channel Validation

**Invalid email:**
```json
{
  "errors": {
    "Value": ["'Value' is not a valid email address."]
  }
}
```

**Invalid phone format:**
```json
{
  "errors": {
    "Value": ["'Value' is not a valid phone number format."]
  }
}
```

**Duplicate channel:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "A channel with this value already exists for this owner.",
  "status": 400
}
```

---

## OwnerType Reference

```csharp
public enum OwnerType
{
    Client = 1,
    Contact = 2
}
```

Use `1` for Client owners, `2` for Contact owners in all API requests.
