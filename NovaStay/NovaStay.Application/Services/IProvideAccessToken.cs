using NovaStay.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaStay.Application.Services
{
    public interface IProvideAccessToken
    {
        Task<AccessTokenDto> GetAccessToken(string refreshtoken);
    }
}
