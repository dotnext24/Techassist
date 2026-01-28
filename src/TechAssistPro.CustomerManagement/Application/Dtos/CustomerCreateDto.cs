namespace TechAssistPro.CustomerManagement.Application;
public record CustomerCreateDto(
    string Name,
    string Email,
    string? PhoneNumber,
    string? Address,
    string CreatedBy);
