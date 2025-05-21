using Models.DTOs;

namespace Application;

public interface IEmployeeService
{
    List<EmployeeListDto> GetAll();
    EmployeeDetailDto GetById(int id);
}