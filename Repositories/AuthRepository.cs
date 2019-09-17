using codetest.Models;
using codetest.Repositories.Interfaces;
using MongoDB.Driver;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace codetest.Repositories
{
    public class AuthRepository: BaseRepository<Login>, IAuthRepository
    {
        public AuthRepository(IMongoDatabase mongoDatabase) : base(mongoDatabase)
        { }

    }
}
