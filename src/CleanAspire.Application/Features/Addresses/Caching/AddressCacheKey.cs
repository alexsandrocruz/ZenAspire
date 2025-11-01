// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Domain.Enums;

namespace CleanAspire.Application.Features.Addresses.Caching;

/// <summary>
/// Cache key definitions for Address entities
/// </summary>
public static class AddressCacheKey
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromHours(1);
    private static readonly TimeSpan DefaultDuration = TimeSpan.FromHours(2);

    public static TimeSpan? Duration => DefaultDuration;

    public static string GetAllCacheKey => "all-addresses";

    public static string GetByIdCacheKey(string id) => $"address-by-id-{id}";

    public static string GetByOwnerCacheKey(OwnerType ownerType, string ownerId) =>
        $"addresses-owner-{ownerType}-{ownerId}";

    public static string GetPrimaryByOwnerCacheKey(OwnerType ownerType, string ownerId) =>
        $"addresses-primary-{ownerType}-{ownerId}";

    private static CancellationTokenSource _tokenSource = new();
    public static CancellationToken SharedExpiryToken => _tokenSource.Token;

    public static void Refresh() => SharedExpiryTokenSource();

    private static void SharedExpiryTokenSource()
    {
        _tokenSource.Cancel();
        _tokenSource = new CancellationTokenSource(RefreshInterval);
    }
}
