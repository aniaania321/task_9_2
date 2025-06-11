namespace Models.DTOs;

public class EmployeeDetailsDto
{
    public PersonDto Person { get; set; }
    public decimal Salary { get; set; }
    public string Position { get; set; }
    public DateTime HireDate { get; set; }
}