using MassTransit;
using Microsoft.Extensions.Logging;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaStay.Infrastructure.Consumers
{
    public class NotificationEmailComsumer : IConsumer<NofiticationRegisterBusiness>
    {
        private readonly ILogger<NotificationEmailComsumer> _logger;
        private INotifications _notification;

        public NotificationEmailComsumer(INotifications notifications, ILogger<NotificationEmailComsumer> logger)
        {
            _logger = logger;
            _notification = notifications;
        }

        public async Task Consume(ConsumeContext<NofiticationRegisterBusiness> context)
        {
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

                    <!-- Header -->
                    <tr>
                        <td align='center' style='padding:45px 30px 40px 30px; border-bottom: 1px solid #f2f0eb;'>
                            <h1 style='color:#111111;margin:0;font-family:""Georgia"", serif;font-size:32px;font-weight:400;letter-spacing:3px;text-transform:uppercase;'>
                                NovaStay
                            </h1>
                            <p style='color:#8c8273;margin-top:10px;font-size:12px;text-transform:uppercase;letter-spacing:2px;'>
                                Smart Accommodation Management Platform
                            </p>
                        </td>
                    </tr>

                    <!-- Body -->
                    <tr>
                        <td style='padding:50px 50px 40px 50px;'>

                            <h2 style='color:#111111;margin-top:0;font-family:""Georgia"", serif;font-size:22px;font-weight:400;line-height:1.4;'>
                                Welcome to NovaStay
                            </h2>

                            <p style='color:#555555;line-height:1.8;font-size:14px;'>
                                Dear <strong>{context.Message.BusinessName}</strong>,
                            </p>

                            <p style='color:#555555;line-height:1.8;font-size:14px;margin-bottom:40px;'>
                                Thank you for choosing NovaStay. Your exclusive business account has been successfully created and is now ready for onboarding.
                            </p>

                            <!-- Business Info Table -->
                            <h3 style='color:#111111;margin-top:40px;margin-bottom:15px;font-size:13px;text-transform:uppercase;letter-spacing:1.5px;color:#c5a880;'>
                                Business Details
                            </h3>
                            <table width='100%' cellpadding='12' cellspacing='0' style='border-collapse:collapse;margin-bottom:40px;'>
                                <tr style='border-bottom:1px solid #f2f0eb;'>
                                    <td width='35%' style='padding-left:0;color:#888888;font-size:13px;'>Business Name</td>
                                    <td style='color:#111111;font-size:14px;font-weight:500;'>{context.Message.BusinessName}</td>
                                </tr>
                                <tr style='border-bottom:1px solid #f2f0eb;'>
                                    <td style='padding-left:0;color:#888888;font-size:13px;'>Email Address</td>
                                    <td style='color:#111111;font-size:14px;'>{context.Message.BusinessEmail}</td>
                                </tr>
                                <tr style='border-bottom:1px solid #f2f0eb;'>
                                    <td style='padding-left:0;color:#888888;font-size:13px;'>Phone Number</td>
                                    <td style='color:#111111;font-size:14px;'>{context.Message.BusinessPhone}</td>
                                </tr>
                                <tr style='border-bottom:1px solid #f2f0eb;'>
                                    <td style='padding-left:0;color:#888888;font-size:13px;'>Location</td>
                                    <td style='color:#111111;font-size:14px;'>{context.Message.BusinessCity}, {context.Message.BusinessCountry}</td>
                                </tr>
                                <tr style='border-bottom:1px solid #f2f0eb;'>
                                    <td style='padding-left:0;color:#888888;font-size:13px;'>Zip Code</td>
                                    <td style='color:#111111;font-size:14px;'>{context.Message.BusinessZipCode}</td>
                                </tr>
                            </table>

                            <!-- Account Info Table -->
                            <h3 style='color:#111111;margin-top:30px;margin-bottom:15px;font-size:13px;text-transform:uppercase;letter-spacing:1.5px;color:#c5a880;'>
                                Account Credentials
                            </h3>
                            <table width='100%' cellpadding='12' cellspacing='0' style='border-collapse:collapse;background-color:#faf9f6;border:1px solid #e8e6e1;'>
                                <tr style='border-bottom:1px solid #e8e6e1;'>
                                    <td width='35%' style='color:#888888;font-size:13px;'>Username</td>
                                    <td style='color:#111111;font-size:14px;font-family:monospace;'>{context.Message.BusinessEmail}</td>
                                </tr>
                                <tr>
                                    <td style='color:#888888;font-size:13px;'>Password</td>
                                    <td style='color:#111111;font-size:14px;font-family:monospace;font-weight:bold;'>{context.Message.Password}</td>
                                </tr>
                            </table>

                            <!-- Security Notice -->
                            <p style='margin-top:30px;color:#bd3a3a;font-size:12px;text-transform:uppercase;letter-spacing:1px;font-weight:600;'>
                                Security Notice:
                            </p>
                            <p style='color:#777777;line-height:1.6;font-size:13px;margin-bottom:45px;'>
                                For data protection, we strongly recommend updating your temporary password immediately upon your initial access.
                            </p>

                            <!-- CTA Button -->
                            <div style='text-align:center;margin-top:20px;margin-bottom:20px;'>
                                <a href='https://novastay.io.vn/login/owner'
                                   style='background:#111111;
                                          color:#ffffff;
                                          padding:16px 35px;
                                          text-decoration:none;
                                          font-size:13px;
                                          text-transform:uppercase;
                                          letter-spacing:2px;
                                          font-weight:600;
                                          display:inline-block;
                                          border:1px solid #111111;
                                          transition:all 0.3s ease;'>
                                    Access Your Dashboard
                                </a>
                            </div>

                            <p style='margin-top:50px;color:#777777;line-height:1.8;font-size:13px;'>
                                Should you require bespoke assistance, please do not hesitate to contact our dedicated support relations.
                            </p>

                            <p style='color:#555555;font-size:14px;margin-top:30px;font-family:""Georgia"", serif;font-style:italic;'>
                                Warm regards,<br/>
                                <span style='font-style:normal;font-weight:bold;color:#111111;display:inline-block;margin-top:5px;'>NovaStay Team</span>
                            </p>

                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td align='center'
                            style='background:#ffffff;
                                   padding:35px;
                                   color:#a0988e;
                                   font-size:11px;
                                   letter-spacing:1px;
                                   border-top: 1px solid #f2f0eb;'>
                            © {DateTime.UtcNow.Year} NOVASTAY. ALL RIGHTS RESERVED.
                        </td>
                    </tr>

                </table>
            </td>
        </tr>
    </table>
</body>
</html>";


            bool result = await _notification.SendRegisterAccount(new RequestSendMessage
            {
                Body = content,
                Subject = "NovaStay New Business Registration",
                To = context.Message.BusinessEmail
            });

            _logger.LogInformation("NotificationEmailComsumer: Email sent to {Email} with result: {Result}", context.Message.BusinessEmail, result);

        }
    }
}
