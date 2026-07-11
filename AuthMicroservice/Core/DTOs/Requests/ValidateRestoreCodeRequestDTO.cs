using System.ComponentModel.DataAnnotations;

namespace AuthMicroservice.Core.DTOs.Requests;

public class ValidateRestoreCodeRequestDTO
{
    [Required(ErrorMessage = "VAL009")]
    public string UserEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "VAL007")]
    [MaxLength(3, ErrorMessage = "VAL008")]
    public string UserIsoCountry { get; set; } = string.Empty;

    [Required(ErrorMessage = "VAL010")]
    public string RestoreCode { get; set; } = string.Empty;
}
