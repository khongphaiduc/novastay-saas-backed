using MassTransit;
using Microsoft.Extensions.Logging;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using System;
using System.Threading.Tasks;

namespace NovaStay.Infrastructure.Consumers
{
    public class NotificationContractRenewConsumer : IConsumer<NotificationContractRenewEvent>
    {
        private readonly ILogger<NotificationContractRenewConsumer> _logger;
        private readonly INotifications _notification;

        public NotificationContractRenewConsumer(INotifications notifications, ILogger<NotificationContractRenewConsumer> logger)
        {
            _logger = logger;
            _notification = notifications;
        }

        public async Task Consume(ConsumeContext<NotificationContractRenewEvent> context)
        {
            var msg = context.Message;
            string content = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='margin:0;padding:0;background-color:#faf9f6;font-family:""Helvetica Neue"", Helvetica, Arial, sans-serif;'>
    <table width='100%' cellpadding='0' cellspacing='0' style='background-color:#faf9f6;padding:50px 0;'>
        <tr>
            <td align='center'>
                <table width='650' cellpadding='0' cellspacing='0'
                       style='background:#ffffff;border: 1px solid #e8e6e1;overflow:hidden;'>
                    <tr>
                        <td align='center' style='padding:45px 30px 40px 30px; border-bottom: 1px solid #f2f0eb;'>
                            <h1 style='color:#111111;margin:0;font-family:""Georgia"", serif;font-size:32px;font-weight:400;letter-spacing:3px;text-transform:uppercase;'>
                                NovaStay
                            </h1>
                            <p style='color:#8c8273;margin-top:10px;font-size:12px;text-transform:uppercase;letter-spacing:2px;'>
                                Thông báo hợp đồng thuê phòng
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding:50px 50px 40px 50px;'>
                            <h2 style='color:#111111;margin-top:0;font-family:""Georgia"", serif;font-size:22px;font-weight:400;line-height:1.4;'>
                                Xin chào {msg.ResidentName},
                            </h2>
                            <p style='color:#555555;line-height:1.8;font-size:14px;margin-bottom:40px;'>
                                Chúng tôi xin thông báo rằng hợp đồng thuê phòng của bạn tại <strong>Phòng {msg.RoomNumber} - {msg.PropertyName}</strong> sắp hết hạn.
                            </p>
                            <table width='100%' cellpadding='12' cellspacing='0' style='border-collapse:collapse;margin-bottom:40px;'>
                                <tr style='border-bottom:1px solid #f2f0eb;'>
                                    <td width='35%' style='padding-left:0;color:#888888;font-size:13px;'>Tòa nhà / Cơ sở</td>
                                    <td style='color:#111111;font-size:14px;font-weight:500;'>{msg.PropertyName}</td>
                                </tr>
                                <tr style='border-bottom:1px solid #f2f0eb;'>
                                    <td style='padding-left:0;color:#888888;font-size:13px;'>Phòng</td>
                                    <td style='color:#111111;font-size:14px;'>{msg.RoomNumber}</td>
                                </tr>
                                <tr style='border-bottom:1px solid #f2f0eb;'>
                                    <td style='padding-left:0;color:#888888;font-size:13px;'>Ngày hết hạn</td>
                                    <td style='color:#bd3a3a;font-size:14px;font-weight:bold;'>{msg.EndDate:dd/MM/yyyy}</td>
                                </tr>
                            </table>
                            <p style='color:#777777;line-height:1.6;font-size:13px;margin-bottom:45px;'>
                                Vui lòng liên hệ với Ban quản lý hoặc Chủ cơ sở để tiến hành các thủ tục gia hạn hợp đồng nếu bạn có nguyện vọng tiếp tục lưu trú.
                            </p>
                            <p style='color:#555555;font-size:14px;margin-top:30px;font-family:""Georgia"", serif;font-style:italic;'>
                                Trân trọng,<br/>
                                <span style='font-style:normal;font-weight:bold;color:#111111;display:inline-block;margin-top:5px;'>NovaStay Team</span>
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td align='center'
                            style='background:#ffffff;padding:35px;color:#a0988e;font-size:11px;letter-spacing:1px;border-top: 1px solid #f2f0eb;'>
                            © {DateTime.UtcNow.Year} NOVASTAY. ALL RIGHTS RESERVED.
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";

            bool result = await _notification.SendEmail(new RequestSendMessage
            {
                Body = content,
                Subject = $"[NovaStay] Hợp đồng thuê phòng sắp hết hạn (Phòng {msg.RoomNumber})",
                To = msg.ResidentEmail
            });

            if (result)
            {
                _logger.LogInformation("NotificationContractRenewConsumer: Email sent to {Email} successfully.", msg.ResidentEmail);
            }
            else
            {
                _logger.LogWarning("NotificationContractRenewConsumer: Failed to send email to {Email}.", msg.ResidentEmail);
            }
        }
    }
}
