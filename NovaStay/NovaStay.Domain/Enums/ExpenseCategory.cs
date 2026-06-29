using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaStay.Domain.Enums
{
    public enum ExpenseCategory
    {
        Electricity = 0,          // Tiền điện
        Water = 1,                // Tiền nước
        Internet = 2,             // Internet
        Maintenance = 3,          // Bảo trì
        Repair = 4,               // Sửa chữa
        AssetDepreciation = 5,    // Khấu hao tài sản
        Cleaning = 6,             // Vệ sinh
        Security = 7,             // An ninh
        Salary = 8,               // Lương nhân viên
        OfficeSupplies = 9,       // Văn phòng phẩm
        Marketing = 10,           // Marketing
        Utilities = 11,           // Tiện ích khác
        Tax = 12,                 // Thuế, phí
        Insurance = 13,           // Bảo hiểm
        Service = 14,             // Chi phí dịch vụ
        Equipment = 15,           // Mua sắm trang thiết bị
        Furniture = 16,           // Nội thất
        Transportation = 17,      // Đi lại, vận chuyển
        Miscellaneous = 18        // Chi phí khác
    }
}
