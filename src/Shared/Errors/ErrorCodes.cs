using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Errors
{
    public static class ErrorCodes
    {
        public const string InvalidDocument = "INVALID_DOCUMENT";
        public const string UserUnauthorized = "USER_UNAUTHORIZED";
        public const string InvalidAccount = "INVALID_ACCOUNT";
        public const string InactiveAccount = "INACTIVE_ACCOUNT";
        public const string InvalidValue = "INVALID_VALUE";
        public const string InvalidType = "INVALID_TYPE";
        public const string InvalidToken = "INVALID_TOKEN";
    }
}
