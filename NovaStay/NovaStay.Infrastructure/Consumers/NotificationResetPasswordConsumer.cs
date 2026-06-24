using MassTransit;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaStay.Infrastructure.Consumers
{
    public class NotificationResetPasswordConsumer : IConsumer<NofiticationResetPassword>
    {
        private readonly IPublishEndpoint _rabbitMQ;
        private readonly INotifications _notification;

        public NotificationResetPasswordConsumer(IPublishEndpoint publishEndpoint, INotifications notifications)
        {
            _rabbitMQ = publishEndpoint;
            _notification = notifications;
        }

        public async Task Consume(ConsumeContext<NofiticationResetPassword> context)
        {
            var newPassword = context.Message.NewPassword;
            var email = context.Message.Email;

            string subject = "NovaStay | Your Password Has Been Reset";

            string content = $@"
<!DOCTYPE html>
<html>
<body style=""margin:0; padding:0; background-color:#f7f3ee; font-family:Arial, Helvetica, sans-serif;"">
    <div style=""max-width:620px; margin:0 auto; background-color:#ffffff; border-radius:16px; overflow:hidden; border:1px solid #eadfce;"">
        
        <div style=""background-color:#1f1a17; padding:32px 24px; text-align:center;"">
            <h1 style=""margin:0; color:#d6b56d; font-size:30px; letter-spacing:2px;"">
                NovaStay
            </h1>
            <p style=""margin:8px 0 0; color:#f4ead8; font-size:14px;"">
                Luxury Stay Management
            </p>
        </div>

        <div style=""padding:36px 32px; color:#2b2b2b;"">
            <h2 style=""margin-top:0; font-size:22px; color:#1f1a17;"">
                Your password has been reset
            </h2>

            <p style=""font-size:15px; line-height:1.7;"">
                Dear valued resident,
            </p>

            <p style=""font-size:15px; line-height:1.7;"">
                Your NovaStay account password has been reset successfully.
                For your security, we have generated a temporary password for you.
            </p>

            <div style=""margin:28px 0; padding:22px; background-color:#faf6ef; border:1px solid #e6d3ad; border-radius:12px; text-align:center;"">
                <p style=""margin:0 0 10px; font-size:13px; color:#7a6a52;"">
                    Temporary Password
                </p>
                <p style=""margin:0; font-size:26px; font-weight:bold; color:#1f1a17; letter-spacing:1px;"">
                    {newPassword}
                </p>
            </div>

            <p style=""font-size:15px; line-height:1.7;"">
                Please sign in using this temporary password and change it immediately
                in your account settings to keep your account secure.
            </p>

            <p style=""font-size:15px; line-height:1.7;"">
                Thank you for choosing NovaStay — where every stay is managed with
                elegance, comfort, and care.
            </p>

            <p style=""margin-top:32px; font-size:15px; line-height:1.7;"">
                Warm regards,<br/>
                <strong>NovaStay Support Team</strong>
            </p>
        </div>

        <div style=""background-color:#f1eadf; padding:18px; text-align:center; color:#8a7a65; font-size:12px;"">
            This is an automated security email from NovaStay. Please do not reply to this message.
        </div>
    </div>
</body>
</html>";

            await _notification.SendEmail(new RequestSendMessage
            {
                To = email,
                Subject = subject,
                Body = content
            });
        }
    }
}
