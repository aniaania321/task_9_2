using Models.DTOs;

namespace API;

public interface IEmployeeService
{
    List<EmployeeListDto> GetAll();
    EmployeeDetailDto GetById(int id);
}