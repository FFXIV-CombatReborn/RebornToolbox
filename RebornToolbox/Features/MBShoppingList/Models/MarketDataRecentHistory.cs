﻿using System.Text.Json.Serialization;

namespace RebornToolbox.Features.MBShoppingList.Models;

/// Credit: https://github.com/Caraxi/MarketBoardPlugin/blob/develop/MarketBoardPlugin/Models/Universalis/MarketDataRecentHistory.cs
/// <summary>
/// A model representing a market data recent history from Universalis.
/// </summary>
public class MarketDataRecentHistory
{
    /// <summary>
    /// Gets or sets the name of the buyer.
    /// </summary>
    [JsonPropertyName("buyerName")]
    public string BuyerName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the items are HQ.
    /// </summary>
    [JsonPropertyName("hq")]
    public bool Hq { get; set; }

    /// <summary>
    /// Gets or sets the price per unit.
    /// </summary>
    [JsonPropertyName("pricePerUnit")]
    public long PricePerUnit { get; set; }

    /// <summary>
    /// Gets or sets the quantity.
    /// </summary>
    [JsonPropertyName("quantity")]
    public long Quantity { get; set; }

    /// <summary>
    /// Gets or sets the timestamp.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the total.
    /// </summary>
    [JsonPropertyName("total")]
    public long Total { get; set; }

    /// <summary>
    /// Gets or sets the name of the world.
    /// </summary>
    [JsonPropertyName("worldName")]
    public string WorldName { get; set; }
}