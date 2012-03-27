using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Harden.ValidationAttributes;

namespace Project1.Core.Infrastructure
{
    [ValueType]
    public class Password
    {
        public static Password NewWithSalt()
        {
            return new Password();
        }
        public Password()
        {
            Salt = Guid.NewGuid().ToString();
        }

        [Length(150)]
        [NotNull]
        protected virtual string Hash { get; set; }
        [Length(150)]
        [NotNull]
        protected virtual string Salt { get; set; }

        public virtual bool CheckPassword(string password)
        {
            if (password == null) return false;

            return Hash == HashString(Salt, password);
        }

        public virtual void SetPassword(string password)
        {

            Hash = HashString(Salt, password);
        }

        public static implicit operator Password(string input)
        {
            if (string.IsNullOrEmpty(input)) throw new FormatException("Was null or empty");
            var rt = new Password();
            rt.SetPassword(input);
            return rt;
        }

        private static string HashString(string salt, string password)
        {
            var hmac = new HMACMD5(Encoding.Unicode.GetBytes(new string(salt.Reverse().ToArray())));
            var saltedHash = hmac.ComputeHash((Encoding.Unicode.GetBytes(password)));
            return Encoding.Unicode.GetString(saltedHash);
        }
    }
}