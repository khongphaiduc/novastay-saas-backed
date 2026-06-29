using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaStay.Domain.Enums
{
    public enum IncomeCategory
    {
        RoomRent = 0,              // Tiền phòng
        ServiceFee = 1,            // Tiền dịch vụ
        UtilityFee = 2,            // Tiền điện, nước, internet...
        BookingDeposit = 3,        // Tiền cọc giữ phòng
        SecurityDeposit = 4,       // Tiền đặt cọc thuê phòng
        LatePaymentFee = 5,        // Phí trả chậm (trễ hạn)
        DamageCompensation = 6,    // Bồi thường hư hỏng
        ParkingFee = 7,            // Phí gửi xe
        LaundryFee = 8,            // Phí giặt là
        CleaningFee = 9,           // Phí vệ sinh
        Other = 10                 // Khoản thu khác
    }
}
