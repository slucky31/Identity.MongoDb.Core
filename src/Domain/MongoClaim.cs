using System.Security.Claims;
using MongoDB.Bson;

namespace Identity.MongoDb.Core.Domain;

public sealed class MongoClaim
{

    /// <summary>
    ///     Gets or sets the claim type for this claim.
    /// </summary>
    public string ClaimType { get; private set; }

    /// <summary>
    ///     Gets or sets the claim value for this claim.
    /// </summary>
    public string ClaimValue { get; private set; }

    /// <summary>
    ///     Constructs a new claim with the type and value.
    /// </summary>
    /// <returns>The <see cref="T:System.Security.Claims.Claim" /> that was produced.</returns>
    public Claim ToClaim()
    {
        return new Claim(this.ClaimType, this.ClaimValue);
    }

    public MongoClaim()
    {
    }

    public MongoClaim(Claim other):this()
    {        
        this.ClaimType = other?.Type;
        this.ClaimValue = other?.Value;
    }
}
