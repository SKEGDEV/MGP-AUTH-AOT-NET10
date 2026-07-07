using System.ComponentModel.DataAnnotations;

namespace AuthMicroservice.Core.DTOs.Requests;

public class SignupRequestDTO
{
    [Required(ErrorMessage = "VAL001")]
    public string UserFirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "VAL002")]
    public string UserLastName { get; set; } = string.Empty;

    public string? UserName { get; set; }

    public string? UserEmail { get; set; }

    [Required(ErrorMessage = "VAL005")]
    [MinLength(8, ErrorMessage = "VAL006")]
    public string UserPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "VAL007")]
    [MaxLength(3, ErrorMessage = "VAL008")]
    public string UserIsoCountry { get; set; } = string.Empty;
}
