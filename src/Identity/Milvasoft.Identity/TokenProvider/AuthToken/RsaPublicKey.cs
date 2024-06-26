﻿using System.Text.Json.Serialization;

namespace Milvasoft.Identity.TokenProvider.AuthToken;

/// <summary>
/// Represents the RSA public key.
/// </summary>
public class RsaPublicKey
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [JsonPropertyName("kid")]
    public string Kid { get; set; }

    [JsonPropertyName("kty")]
    public string Kty { get; set; }

    [JsonPropertyName("alg")]
    public string Alg { get; set; }

    [JsonPropertyName("use")]
    public string Use { get; set; }

    [JsonPropertyName("n")]
    public string N { get; set; }

    [JsonPropertyName("e")]
    public string E { get; set; }

    [JsonPropertyName("x5c")]
    public List<string> X5c { get; set; }

    [JsonPropertyName("x5t")]
    public string X5t { get; set; }

    [JsonPropertyName("x5t#S256")]
    public string X5tS256 { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
