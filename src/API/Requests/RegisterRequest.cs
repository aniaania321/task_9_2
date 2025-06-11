namespace API;

using System.ComponentModel.DataAnnotations;

public class RegisterRequest
{
    [Required]
    [MaxLength(100)]
    [RegularExpression(@"^[^\d].*", ErrorMessage = "Username can't start with a digit!")]
    public string Username { get; set; }

    [Required]
    [MinLength(12)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).*$", ErrorMessage = "Password has to contain uppercase and lowercase letters, numbers, and special characters")]
    public string Password { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public int RoleId { get; set; }
}
