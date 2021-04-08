using System;
using System.Collections.Generic;
using System.Text;

namespace OutOfSchool.EmailService
{
    public interface IEmailSender
    {
        void SendEmail(Message message);
    }
}
