using System;
using System.Collections.Generic;

namespace NovaStay.Infrastructure.Models;

public partial class ExpenseCategory
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public string CategoryCode { get; set; } = null!;

    public string CategoryName { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

    public virtual Organization Organization { get; set; } = null!;
}
