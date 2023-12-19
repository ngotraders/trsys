using System.ComponentModel.DataAnnotations;

namespace Trsys.Web.Requests;

public class UpdateUserRequest
{
    [Required]
    public string? Name { get; set; }
    [Required]
    public string? Username { get; set; }
    [Required]
    public string? EmailAddress { get; set; }
    public string? NewPassword { get; set; }
    [Required]
    public string? Role { get; set; }
}