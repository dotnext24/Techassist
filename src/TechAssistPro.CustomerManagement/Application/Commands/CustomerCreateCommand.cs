using MediatR;

namespace TechAssistPro.CustomerManagement.Application;

public record CustomerCreateCommand(
    string Name,
    string Email,
    string? PhoneNumber,
    string? Address,
    string CreatedBy
) : IRequest<CustomerCreateResponse>;

