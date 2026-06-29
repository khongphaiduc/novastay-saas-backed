using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaStay.Domain.Enums
{
    public enum ApprovalStatus
    {
        Pending = 0,        // Chờ duyệt
        Approved = 1,       // Đã duyệt
        Rejected = 2,       // Từ chối
        Cancelled = 3       // Đã hủy
    }
}
