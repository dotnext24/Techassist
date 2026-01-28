// src/TechAssistPro.CustomerManagement/Entities/Customer.cs
using TechAssistPro.SharedKernel.Domain;
using TechAssistPro.CustomerManagement.Events;

namespace TechAssistPro.CustomerManagement.Entities;
public class Customer : AggregateRoot
{
    public string Name { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string? PhoneNumber { get; private set; }
    public string? Address { get; private set; }
    public CustomerStatus Status { get; private set; }

    private Customer() { } // EF Core uses this

    public Customer(
        Guid id,
        string name,
        string email,
        string? phoneNumber,
        string? address)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Email = email ?? throw new ArgumentNullException(nameof(email));

        PhoneNumber = phoneNumber;
        Address = address;

        Status = CustomerStatus.Active;

        CreatedAtUtc = DateTime.UtcNow;
    }

    public static Customer Create(
        string name,
        string email,
        string? phoneNumber,
        string? address,
        string createdBy)
    {
        Guid id = Guid.NewGuid();
        var customer = new Customer(
            id,
            name,
            email,
            phoneNumber,
            address);
        customer.Touch(createdBy);
        customer.AddCreatedEvent();

        return customer;
    }

    public void Update(string name, string email, string? phoneNumber, string? address, string updatedBy)
    {
        Name = name;
        Email = email;
        PhoneNumber = phoneNumber;
        Address = address;
        Touch(updatedBy);
    }

    public void ChangeStatus(CustomerStatus status, string updatedBy)
    {
        Status = status;
        Touch(updatedBy);
    }

    public void SoftDelete(string deletedBy)
    {
        IsDeleted = true;
        Touch(deletedBy);
    }

    private void Touch(string updatedBy)
    {
        LastUpdatedAtUtc = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    private void AddCreatedEvent()
    {
        RaiseDomainEvent(new CustomerCreatedDomainEvent(this));
    }
}

public enum CustomerStatus
{
    Active,
    Inactive,
    Deleted
}