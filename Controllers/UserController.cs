using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using tckr.Models;
using Microsoft.AspNetCore.Identity;

namespace tckr.Controllers
{
    public class UserController : Controller
    {

        private tckrContext _context;

        public UserController(tckrContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            return View("landing");
        }

        // Newuser route is the registration route for a new user.
        [HttpPost]
        [Route("NewUser")]
        public IActionResult NewUser(AllUserViewModels model)
        {
            // Check if models received any validation errors.
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if email already exists in DB.
                    var EmailExists = _context.Users.Where(e => e.Email == model.Reg.Email).SingleOrDefault();
                    if (EmailExists == null)
                    {
                        // Hash password
                        PasswordHasher<RegisterViewModel> Hasher = new PasswordHasher<RegisterViewModel>();
                        string HashedPassword = Hasher.HashPassword(model.Reg, model.Reg.Password);
                        User NewUser = new User
                        {
                            FirstName = model.Reg.FirstName,
                            LastName = model.Reg.LastName,
                            Email = model.Reg.Email,
                            Password = HashedPassword,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                        };
                        _context.Add(NewUser);
                        _context.SaveChanges();

                        //Now that the user has been added to the DB and a UserId has been created, query for that user and save the UserId.
                        User User = _context.Users.SingleOrDefault(u => u.Email == model.Email);
                        // Set user id in session for use in identification, future db calls, and for greeting the user.
                        HttpContext.Session.SetInt32("LoggedUserId", User.UserId);

                        // Redirect to Profile method.
                        return RedirectToAction("Profile");
                    }
                    // Redirect w/ error if email already exists in db.
                    else
                    {
                        ViewBag.email = "That email is already in use. Please try again using another.";
                    }
                }
                // Catch should only run if there was an error with the db connection/query
                catch
                {
                    return View("register");
                }
            // If block will run if the ModelState is invalid.
            }
            return View("register");
        }


        [HttpGet]
        [Route("LoginPage")]
        public IActionResult LoginPage(LoginViewModel model)
        {
            return View("login");
        }





        // This route handles login requests.
        [HttpPost]
        [Route("LoginSubmit")]
        public IActionResult LoginSubmit(AllUserViewModels model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // If there are no errors upon form submit, check db for proper creds.
                    User LoggedUser = _context.Users.SingleOrDefault(u => u.Email == model.Log.Email);
                    var Hasher = new PasswordHasher<User>();
                    // Check    hashed password.
                    if (Hasher.VerifyHashedPassword(LoggedUser, LoggedUser.Password, model.Log.Password) != 0)
                    {
                        // Set user id in session for use in identification, future db calls, and for greeting the user.
                        HttpContext.Session.SetInt32("LoggedUserId", LoggedUser.UserId);
                        return RedirectToAction("Profile");
                    }
                    else
                    {
                        ViewBag.loginError = "Sorry, your password was incorrect.";
                        return View("landing");
                    }
                }
                // If no proper creds redirect to login page and return error.
                catch
                {
                    ViewBag.loginError = "Sorry, your email or password were incorrect.";
                    return View("landing");
                }
            }
            // If form submit was illegal redirect to login and display model validation errors.
            else
            {
                return View("landing");
            }
        }

        [HttpGet]
        [Route("Profile")]
        public IActionResult Profile()
        {
            Console.WriteLine("GOT TO PROFILE");
            // Check to ensure there is a properly logged in user by checking session.
            if (HttpContext.Session.GetInt32("LoggedUserId") >= 0)
            {
                try
                {
                    // Get UserId from session
                    var SessionId = HttpContext.Session.GetInt32("LoggedUserId");
                    // Getting User from DB
                    User User = _context.Users.SingleOrDefault(u => u.UserId == SessionId);
                    // Put User in ViewBag for View
                    ViewBag.User = User;
                    return View("Profile");
                }
                // Catch should only fire if there was an error getting/setting sesion id and username to ViewBag but if session id exists (which means a user is logged in). Send to login page.
                catch
                {
                    return View("Login");
                }
            }
            // If no id is in session that means that the user is not properly logged on. Redirect to logout which will end up at LoginPage.
            return RedirectToAction("Logout");
        }

        [HttpGet]
        [Route("Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            // LoginPage Method is in User Controller
            return RedirectToAction("Index");
        }
        [HttpPost]
        [Route("UpdateBio")]
        public IActionResult UpdateBio(User model)
        {
            if(model.Bio != null){
            var SessionId = HttpContext.Session.GetInt32("LoggedUserId");
            User User = _context.Users.SingleOrDefault(u => u.UserId == SessionId);
            User.Bio = model.Bio;
            _context.SaveChanges();
            }
            return RedirectToAction("Profile");
        }
        [HttpPost]
        [Route("UpdatePassword")]
        public IActionResult UpdatePassword(Dictionary<string,string> Data)
        {
            if(Data["Password"] != null && Data["PasswordA"] != null && Data["PasswordB"] != null)
            {
                var SessionId = HttpContext.Session.GetInt32("LoggedUserId");
                User User = _context.Users.SingleOrDefault(u => u.UserId == SessionId);
                Console.WriteLine("OLD");
                Console.WriteLine(User.Password);
                var Hasher = new PasswordHasher<User>();
                if (Hasher.VerifyHashedPassword(User, User.Password, Data["Password"]) != 0)
                    {
                        if(Data["PasswordA"] != Data["PasswordB"]){
                            // Don't match error
                        }
                        else
                        {
                            Console.WriteLine("BOUT TO UPDATE");
                            PasswordHasher<Dictionary<string,string>> NewHasher = new PasswordHasher<Dictionary<string,string>>();
                            string HashedPassword = NewHasher.HashPassword(Data, Data["Password"]);
                            User.Password = HashedPassword;
                            _context.Update(User);
                            _context.SaveChanges();
                            Console.WriteLine("NEW");
                            Console.WriteLine(HashedPassword);
                            return RedirectToAction("Profile");
                        }
                        // Set user id in session for use in identification, future db calls, and for greeting the user.
                        return RedirectToAction("Profile");
                    }

            }
            return RedirectToAction("Profile");
        }
    }
}
