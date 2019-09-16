using codetest.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using codetest.Repositories.Interfaces;
using codetest.Specifications;
using codetest.Business_Logic;

namespace codetest.Controllers.Api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        public JsonResult Get()
        {
            var users = _userRepository.GetAllSync();
            return new JsonResult(users);
        }

        [HttpPost]
        public ActionResult Add([FromBody]User user)
        {
            CustomResponse response = UserBL.AddUser(user, _userRepository);
            if (!response.success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }


        [HttpPatch]
        public ActionResult Change(string id, [FromBody]User user)
        {
            CustomResponse response = UserBL.EditUser(id, user, _userRepository);
            if (!response.success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpDelete]
        public JsonResult Delete(string id)
        {
            _userRepository.DeleteSync(x => x.Id == id);
            return new JsonResult(new { success = true, responseText = "User successfully deleted!" });
        }
    }
}
