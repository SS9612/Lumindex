using System.ComponentModel.DataAnnotations;

namespace Lumindex.Api.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; set; } = "lumindex";

    [Required]
    public string Audience { get; set; } = "lumindex";

    [Required]
    [MinLength(32, ErrorMessage = "Jwt:SigningKey must be at least 32 characters for HMAC-SHA256.")]
    public string SigningKey { get; set; } = default!;

    [Range(1, 1440)]
    public int AccessTokenLifetimeMinutes { get; set; } = 60;
}
