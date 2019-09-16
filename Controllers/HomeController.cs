using codetest.Models;
using codetest.Models.ViewModels;
using codetest.Utilities;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq;
using codetest.Repositories.Interfaces;
using System.Collections.Generic;
using codetest.Business_Logic;

namespace codetest.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserRepository _userRepository;

        public HomeController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        
        public IActionResult Index()
        {
            var users = _userRepository.GetAllSync();

            if (users.Any())
                return View( new UsersViewModel {Users = users.ToList()});

            var usersFromJson = new GetUsersFromJson("users.json").Execute();
            _userRepository.AddManySync(usersFromJson);
            users = usersFromJson.ToList();

            return View(new UsersViewModel { Users = users.ToList() });
        }

        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpGet]
        public ActionResult AddUser()
        {
            User user = new codetest.Models.User();
            return PartialView("CreateUser", user);
        }
        [HttpPost]
        public ActionResult AddUser([FromForm]User user)
        {
            CustomResponse response = UserBL.AddUser(user, _userRepository);
            if (!response.success)
            {
                return PartialView("Error", response);
            }
            ICollection<User> users = _userRepository.GetAllSync();
            return PartialView("GetAllUsers", new UsersViewModel { Users = users.ToList() });
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult DeleteUser(string id)
        {
            _userRepository.DeleteSync(x => x.Id == id);
            ICollection<User> users = _userRepository.GetAllSync();
            return PartialView("GetAllUsers", new UsersViewModel { Users = users.ToList() });
        }

        [HttpGet]
        public ActionResult EditUser(string id)
        {
            User user = _userRepository.GetSingleByExpression(x => x.Id == id).Result;
            return PartialView("EditUser", user);
        }

        [HttpPost]
        public ActionResult EditUser(string id, [FromForm]User user)
        {
            CustomResponse response = UserBL.EditUser(id, user, _userRepository);
            if (!response.success)
            {
                return PartialView("Error", response);
            }
            ICollection<User> users = _userRepository.GetAllSync();
            return PartialView("GetAllUsers", new UsersViewModel { Users = users.ToList() });
        }

        [HttpGet]
        public ActionResult DetailsOFUser (string id)
        {
            User user = _userRepository.GetSingleByExpression(x => x.Id == id).Result;
            return PartialView("DetailsOFUser", user);
        }

    }
}