using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaStay.Application.DTOs
{
    public class NofiticationRegisterBusiness
    {
        public string BusinessName { get; set; } = string.Empty;
        public string BusinessEmail { get; set; } = string.Empty;
        public string BusinessPhone { get; set; } = string.Empty;
        public string BusinessCity { get; set; } = string.Empty;
        public string BusinessZipCode { get; set; } = string.Empty;
        public string BusinessCountry { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

    }
}
