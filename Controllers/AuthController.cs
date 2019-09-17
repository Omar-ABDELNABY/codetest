using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using codetest.Business_Logic;
using codetest.Models;
using codetest.Models.Auth;
using codetest.Models.ViewModels;
using codetest.Repositories.Interfaces;
using codetest.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace codetest.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;
        private IHttpContextAccessor _accessor;

        public AuthController(IAuthRepository authRepository, IConfiguration configuration, IHttpContextAccessor accessor)
        {
            _authRepository = authRepository;
            _configuration = configuration;
            _accessor = accessor;
        }

        [Authorize]
        public IActionResult Index()
        {
            ICollection<Login> logins = AuthBL.GetAllLoginsSync(_authRepository);

            if (logins.Any())
                return View(new LoginsViewModel { Logins = logins.ToList() });

            List<Login> loginsFromJson = new GetLoginsFromJson("logins.json").Execute();

            AuthBL.AddManyLoginsSync(loginsFromJson, _authRepository);
            logins = loginsFromJson.ToList();

            return View(new LoginsViewModel { Logins = logins.ToList() });
        }

        [HttpPost]
        public IActionResult Login([FromForm] Login loginInput)
        {
            if (ModelState.IsValid)
            {
                Serilog.Log.Debug($"Login Attempt - Session: {HttpContext.Session.Id} - IP: {_accessor.HttpContext.Connection.RemoteIpAddress} - {loginInput}");
                LoginResponse loginResponse = AuthBL.AuthenticateUser(loginInput, _authRepository, _configuration);
                if (loginResponse.success)
                {
                    Serilog.Log.Debug($"Login Success - Session: {HttpContext.Session.Id} - IP: {_accessor.HttpContext.Connection.RemoteIpAddress} - {loginInput}");
                    HttpContext.Session.SetString("JWToken", loginResponse.token); //
                    return Redirect("~/");
                }
                else
                {
                    return RedirectToAction("Login","Auth","Invalid");
                }
            }
            else
            {
                return BadRequest("Invalid Login Model");
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpGet]
        public IActionResult logout()
        {
            HttpContext.Session.Clear();
            return Redirect("~/Auth/Login");
        }


    }
}