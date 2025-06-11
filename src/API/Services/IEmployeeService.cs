using Models.DTOs;

namespace API;

public interface IEmployeeService
{
    List<EmployeeListDto> GetAll();
    EmployeeDetailsDto GetById(int id);
}