using NovaStay.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaStay.Application.Services
{
    public interface INotifications
    {
        public string TypeService { get; }
        Task<bool> SendRegisterAccount(RequestSendMessage request);
    }
}
