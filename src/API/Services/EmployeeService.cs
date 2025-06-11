using API.Data;
using Microsoft.EntityFrameworkCore;
using Models.DTOs;

namespace API;

public class EmployeeService : IEmployeeService
{
    private readonly _2019sbdContext _context;
    public EmployeeService(_2019sbdContext context) => _context = context;

    public List<EmployeeListDto> GetAll() =>
        _context.Employees.Include(e => e.Person)
            .Select(e => new EmployeeListDto
            {
                Id = e.Id,
                FullName = e.Person.FirstName + " " + e.Person.LastName
            }).ToList();

    public EmployeeDetailsDto GetById(int id)
    {
        var e = _context.Employees.Include(e => e.Person).Include(e => e.Position).FirstOrDefault(e => e.Id == id);
        if (e == null) return null;

        return new EmployeeDetailsDto
        {
            Person = new PersonDto
            {
                PassportNumber = e.Person.PassportNumber,
                FirstName = e.Person.FirstName,
                MiddleName = e.Person.MiddleName,
                LastName = e.Person.LastName,
                PhoneNumber = e.Person.PhoneNumber,
                Email = e.Person.Email
            },
            Salary = e.Salary,
            Position = e.Position.Name,
            HireDate = e.HireDate
        };
    }
}