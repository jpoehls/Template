using System;
using System.Net.Mail;
using Harden.ValidationAttributes;

namespace Project1.Core.Infrastructure
{
    [ValueType]
    public class Email
    {
        [Length(150)]
        protected virtual string Value { get; set; }

        public bool Equals(Email other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Value, Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Email)) return false;
            return Equals((Email)obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static implicit operator Email(string emailString)
        {
            if (string.IsNullOrWhiteSpace(emailString)) return null;
            if (!IsValid(emailString))
            {
                throw new FormatException("'" + emailString + "' is not a valid email address");
            }
            return new Email { Value = emailString };
        }

        public override string ToString()
        {
            return this.Value;
        }


        public static bool IsValid(string s)
        {
            try
            {
                var email = new MailAddress(s);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}