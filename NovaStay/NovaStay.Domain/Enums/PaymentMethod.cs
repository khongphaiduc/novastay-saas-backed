using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaStay.Domain.Enums
{
    public enum PaymentMethod
    {
        Cash = 0,                 // Tiền mặt
        BankTransfer = 1,         // Chuyển khoản ngân hàng
        QRCode = 2,               // Thanh toán bằng mã QR
        CreditCard = 3,           // Thẻ tín dụng
        DebitCard = 4,            // Thẻ ghi nợ
        EWallet = 5,              // Ví điện tử (MoMo, ZaloPay, ShopeePay...)
        VirtualAccount = 6,       // Tài khoản định danh/VA
        AutoDebit = 7,            // Trích nợ tự động
        Other = 8                 // Khác
    }
}
