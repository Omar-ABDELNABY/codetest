using codetest.Models;
using codetest.MongoDB.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace codetest.Repositories.Interfaces
{
    public interface IAuthRepository : IRepository<Login>
    {
    }
}
