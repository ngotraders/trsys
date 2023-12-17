using System.ComponentModel.DataAnnotations;

namespace Trsys.Web.Requests;

public class CreateUserRequest
{
    [Required]
    public string? Name { get; set; }
    [Required]
    public string? Username { get; set; }
    [Required]
    public string? EmailAddress { get; set; }
    [Required]
    public string? Password { get; set; }
    [Required]
    public string? Role { get; set; }
}