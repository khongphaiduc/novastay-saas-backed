using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaStay.Application.DTOs
{
    public class NotificationContractRenewEvent
    {
        public Guid ContractId { get; set; }
        public string ResidentEmail { get; set; } = string.Empty;
        public string ResidentName { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public DateTime EndDate { get; set; }
    }
}
