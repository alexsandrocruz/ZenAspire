using System;
using System.Collections.Generic;
using System.Text;
using CleanAspire.Domain.Common;

namespace CleanAspire.Domain.Entities;

public class Customer : BaseAuditableEntity, IAuditTrial
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}
