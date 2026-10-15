namespace Vehicles.Application;

public record CategoryDto(int Id, string Name, string Icon, decimal MinWeight, decimal? MaxWeight);

public record CategoryRequest(string Name, string Icon, decimal MinWeight);

public record ManufacturerDto(int Id, string Name);

public record VehicleRequest(string OwnerName, int ManufacturerId, int Year, decimal Weight);

public record VehicleListItem(
    int Id,
    string OwnerName,
    string Manufacturer,
    int Year,
    decimal Weight,
    string CategoryName,
    string CategoryIcon);
