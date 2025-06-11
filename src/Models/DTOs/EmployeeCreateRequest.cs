namespace Models.DTOs;

public class EmployeeCreateRequest
{
    public PersonCreateRequest Person { get; set; }
    public decimal Salary { get; set; }
    public int PositionId { get; set; }
}