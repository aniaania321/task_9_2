using Models.DTOs;

namespace API;

public interface IDeviceService
{
    List<DeviceDto> GetAll();
    DeviceDetailsDto GetById(int id);
    DeviceDetailsDto Create(DeviceCreateRequest request);
    DeviceDetailsDto Update(int id, DeviceCreateRequest request);
    bool Delete(int id);

}