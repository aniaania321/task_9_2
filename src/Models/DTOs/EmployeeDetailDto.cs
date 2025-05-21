namespace Models.DTOs;

public class EmployeeDetailDto
{
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public decimal Salary { get; set; }
    public DateTime HireDate { get; set; }
    public PositionDto Position { get; set; }

}