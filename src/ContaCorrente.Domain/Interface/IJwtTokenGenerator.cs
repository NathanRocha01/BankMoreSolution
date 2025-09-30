using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContaCorrente.Infrastructure.Security
{
    public interface IJwtTokenGenerator
    {
        string GerarToken(string contaId);
    }
}
