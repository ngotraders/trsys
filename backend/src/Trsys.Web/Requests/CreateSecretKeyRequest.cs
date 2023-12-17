using System.ComponentModel.DataAnnotations;
using Trsys.Models;

namespace Trsys.Web.Requests;

public class CreateSecretKeyRequest
{
    [Required]
    public SecretKeyType? KeyType { get; set; }
    public string? Key { get; set; }
    public string? Description { get; set; }
    public bool? IsApproved { get; set; }
}