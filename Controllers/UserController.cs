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
                    // If email is unique, perform registration.
                    if (EmailExists == null)
                    {
                        // Hash and store password in DB.
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

                        // Set user id in session for use in identification, future db calls, and for greeting the user.
                        HttpContext.Session.SetInt32("LoggedUserId", NewUser.Id);


                        // Redirect to Profile method.
                        return RedirectToAction("Profile");
                    }
                    // Redirect w/ error if email already exists in db.
                    else
                    {
                        ViewBag.email = "That email is already in use. Please try again using a different one.";
                        return View("landing");
                    }
                }
                // Catch should only run if there was an error with the password hashing or storing on the new user in the DB.
                catch
                {
                    return View("landing");
                }
            }
            // Else statement will run if the ModelState is invalid.
            else
            {
                return View("landing");
            }
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
                // If there are no errors upon form submit, check db for proper creds.
                // The reason for the multiple try/catch statements is to return the proper validation error message to the user. 
                // There are better ways to do it, but this is a simple, although crude, method that works for now.
                User LoggedUser;
                
                try
                {
                    LoggedUser = _context.Users.SingleOrDefault(u => u.Email == model.Log.Email);
                }
                // Catch will run if matching email is not found in DB.
                catch
                {
                    ViewBag.loginError = "Your email was incorrect.";
                    return View("landing");
                    
                }
                // If email is correct, verify that password is correct.
                try
                {
                    var Hasher = new PasswordHasher<User>();
                    // Check hashed password. 0 = false password match.
                    if(Hasher.VerifyHashedPassword(LoggedUser, LoggedUser.Password, model.Log.Password) != 0)
                    {
                        // Set user id in session for use in identification, future db calls, and for greeting the user.
                        HttpContext.Session.SetInt32("LoggedUserId", LoggedUser.Id);
                        return RedirectToAction("Profile");
                    }
                    // If password does not match
                    else
                    {
                        ViewBag.loginError = "Your password was incorrect.";
                        return View("landing");
                    }
                }
                // Catch should only run if there was some unusual error, like a DB connection error. Logout will clear session. That might have an effect.
                catch
                {
                    ViewBag.loginError = "Sorry, there was a problem logging you in. Please try again.";
                    return RedirectToAction("logout");
                }
            }
            // If ModelState is not valid redirect to login and display model validation errors.
            else
            {
                ViewBag.loginError = "Your email or password was incorrect.";
                return View("landing");
            }
        }


        // This is an older version. 
        // // This route handles login requests.
        // [HttpPost]
        // [Route("LoginSubmit")]
        // public IActionResult LoginSubmit(AllUserViewModels model)
        // {
        //         // If there are no errors upon form submit, check db for proper creds.
        //     if (ModelState.IsValid)
        //     {
        //         // There are better ways to do this validation scheme, especially where it will return more specific reasons for failing login(password vs email), but this is a simple method that works for now.
        //         try
        //         {
        //             User LoggedUser = _context.Users.SingleOrDefault(u => u.Email == model.Log.Email);

        //             var Hasher = new PasswordHasher<User>();
        //             // Check hashed password. 0 = false password match.
        //             if(Hasher.VerifyHashedPassword(LoggedUser, LoggedUser.Password, model.Log.Password) != 0)
        //             {
        //                 // Set user id in session for use in identification, future db calls, and for greeting the user.
        //                 HttpContext.Session.SetInt32("LoggedUserId", LoggedUser.Id);
        //                 return RedirectToAction("Profile");
        //             }
        //             // If password does not match
        //             else
        //             {
        //                 ViewBag.loginError = "Sorry, your password was incorrect.";
        //                 return View("landing");
        //             }

        //         }
        //         // Catch should only run if there was some unusual error, like a DB connection error. Logout will clear session. That might have an effect.
        //         catch
        //         {
        //             return RedirectToAction("logout");
        //         }
        //     }
        //     // If ModelState is not valid redirect to login and display model validation errors.
        //     else
        //     {
        //         ViewBag.loginError = "Sorry, your email or password was incorrect.";
        //         return View("landing");
        //     }
        // }

        [HttpGet]
        [Route("Profile")]
        public IActionResult Profile()
        {
            Console.WriteLine("GO TO PROFILE");
            // Check to ensure there is a properly logged in user by checking session.
            if (HttpContext.Session.GetInt32("LoggedUserId") >= 0)
            {
                try
                {
                    // Get UserId from session
                    var SessionId = HttpContext.Session.GetInt32("LoggedUserId");

                    // Get User object from DB
                    User User = _context.Users.SingleOrDefault(u => u.Id == SessionId);

                    // Put User in ViewBag to display in view.
                    ViewBag.User = User;
                    return View("Profile");
                }
                // Catch should only fire if there was an error getting/setting sesion id to ViewBag or if error getting User object from DB.
                catch
                {
                    return View("landing");
                }
            }
            // If no id is in session that means that the user is not properly logged on. Redirect to logout which will end up at landing page.
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
            User User = _context.Users.SingleOrDefault(u => u.Id == SessionId);
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
                User User = _context.Users.SingleOrDefault(u => u.Id == SessionId);
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
