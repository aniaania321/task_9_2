using Microsoft.EntityFrameworkCore;
using Models.DTOs;
using Models.Models;

namespace Application;

public class EmployeeService : IEmployeeService
{
    private readonly _2019sbdContext _context;
    public EmployeeService(_2019sbdContext context)
    {
        _context = context;
    }

    public List<EmployeeListDto> GetAll() =>
        _context.Employees
            .Include(e => e.Person)
            .Select(e => new EmployeeListDto
            {
                Id = e.Id,
                FullName = e.Person.FirstName + " " + e.Person.LastName
            }).ToList();

    public EmployeeDetailDto GetById(int id)
    {
        var e = _context.Employees
            .Include(e => e.Person)
            .Include(e => e.Position)
            .FirstOrDefault(e => e.Id == id);

        if (e == null) return null;

        return new EmployeeDetailDto
        {
            FirstName = e.Person.FirstName,
            MiddleName = e.Person.MiddleName,
            LastName = e.Person.LastName,
            Email = e.Person.Email,
            PhoneNumber = e.Person.PhoneNumber,
            Salary = e.Salary,
            HireDate = e.HireDate,
            Position = new PositionDto { Id = e.Position.Id, Name = e.Position.Name }
        };
    }
}