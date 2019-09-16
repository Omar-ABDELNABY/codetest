using codetest.Models;
using codetest.Repositories.Interfaces;
using codetest.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace codetest.Business_Logic
{
    public static class UserBL
    {
        public static CustomResponse AddUser(User user, IUserRepository _userRepository)
        {
            if (!new UserSpecification().IsSatisfiedBy(user))
            {
                return new CustomResponse { success = false, responseText = "Couldn't Add User - User not valid." };
            }
            if (_userRepository.AnySync(x =>
                x.Email == user.Email ||
                x.UserName == user.UserName))
            {
                return new CustomResponse { success = false, responseText = "Couldn't Add User - email or username already taken." };
            }
            _userRepository.AddSync(user);
            return new CustomResponse { success = true, responseText = "User successfully added!" };

        }
        public static CustomResponse EditUser(string id, User user, IUserRepository _userRepository)
        {
            if (!new UserSpecification().IsSatisfiedBy(user))
            {
                return new CustomResponse { success = false, responseText = "Couldn't Edit User - User not valid." };
            }
            if (_userRepository.AnySync(x =>
                (x.Email == user.Email ||
                x.UserName == user.UserName) 
                && x.Id != id))
            {
                return new CustomResponse { success = false, responseText = "Couldn't Edit User - email or username already taken." };
            }
            _userRepository.ReplaceOneSync(id, user);
            return new CustomResponse { success = true, responseText = "User successfully Edited!" };

        }
    }
}
