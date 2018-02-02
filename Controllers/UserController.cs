using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using tckr.Models;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;

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
            if (HttpContext.Session.GetInt32("LoggedUserId") >= 0)
            {
                // Nav Bar will be checking to see if ViewBag.Id is valid
                var SessionId = HttpContext.Session.GetInt32("LoggedUserId");
                ViewBag.Id = SessionId;
            }
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
                        Portfolio Portfolio = new Portfolio
                        {
                            User = NewUser,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                        };
                        Watchlist Watchlist = new Watchlist
                        {
                            User = NewUser,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                        };
                        _context.Add(NewUser);
                        _context.Add(Portfolio);
                        _context.Add(Watchlist);
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

        // This route handles login requests.
        [HttpPost]
        [Route("LoginSubmit")]
        public IActionResult LoginSubmit(AllUserViewModels model)
        {
            Console.WriteLine("GOT HERE");
            if (ModelState.IsValid)
            {
                // If there are no errors upon form submit, check db for proper creds.
                // The reason for the multiple try/catch statements is to return the proper validation error message to the user. 
                // There are better ways to do it (AJAX in the modal), but this is a simple, although crude, method that works for now.
                User LoggedUser;
                Console.WriteLine("EMAIL");
                Console.WriteLine(model.Log.Email);
                
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
                        Console.WriteLine("GOT TO END");
                        // Set user id in session for use in identification, future db calls, and for greeting the user.
                        HttpContext.Session.SetInt32("LoggedUserId", LoggedUser.Id);
                        HttpContext.Session.SetString("LoggedUserName", LoggedUser.FirstName);
                        return RedirectToAction("Portfolio", "Main");
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
                    return RedirectToAction("Logout");
                }
            }
            // If ModelState is not valid redirect to login and display model validation errors.
            else
            {
                ViewBag.loginError = "Your email or password was incorrect.";
                return View("landing");
            }
        }


        [HttpGet]
        [Route("Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            // LoginPage Method is in User Controller
            return RedirectToAction("Index");
        }



        [HttpGet]
        [Route("Profile")]
        public IActionResult Profile()
        {
            var SessionId = HttpContext.Session.GetInt32("LoggedUserId");
            ViewBag.Id = SessionId;
            Console.WriteLine("GO TO PROFILE");
            // Check to ensure there is a properly logged in user by checking session.
            if (HttpContext.Session.GetInt32("LoggedUserId") >= 0)
            {
                User User = _context.Users.SingleOrDefault(u => u.Id == SessionId);
                // Put User in ViewBag to display in view.
                ViewBag.User = User;

                // Get Stock data for Portfolio and Watch List
                Portfolio Portfolio = _context.Portfolios
                .Include(p => p.Stocks)
                .SingleOrDefault(p => p.User == User);
        
                // For each Stock in Portfolio, call API based on values in database
                // Also, populate Stocks list for later use in ViewBag
                ViewBag.Total = 0;
                foreach (Stock Stock in Portfolio.Stocks)
                {
                    // Create a Dictionary object to store JSON values from API call
                    Dictionary<string, object> Data = new Dictionary<string, object>();
                    
                    // Make API call
                    WebRequest.GetQuote(Stock.Symbol, JsonResponse =>
                        {
                            Data = JsonResponse;
                        }
                    ).Wait();

                    // Define values for each stock to be stored in ViewBag
                    double CurrentPrice = Convert.ToDouble(Data["latestPrice"]);
                    
                    Stock.Name = (string)Data["companyName"];
                    Stock.PurchaseValue = Stock.PurchasePrice * Stock.Shares;
                    Stock.CurrentPrice = CurrentPrice;
                    Stock.CurrentValue = CurrentPrice * Stock.Shares;
                    Stock.GainLossPrice = CurrentPrice - Stock.PurchasePrice;
                    Stock.GainLossValue = (CurrentPrice - Stock.PurchasePrice) * Stock.Shares;
                    Stock.GainLossPercent = 100 * (CurrentPrice - Stock.PurchasePrice) / (Stock.PurchasePrice);
                    Stock.Week52Low = Convert.ToDouble(Data["week52Low"]);
                    Stock.Week52High = Convert.ToDouble(Data["week52High"]);
                    Stock.UpdatedAt = DateTime.Now;

                    _context.SaveChanges();

                    ViewBag.Total += Stock.CurrentValue;

                    // This is for the Watchlist
                    
                }
                // Store values in ViewBag for Portfolio page rendering
                ViewBag.Portfolio = Portfolio;

                // Watchlist START
                Watchlist Watchlist = _context.Watchlists
                .Include(w => w.Stocks)
                .SingleOrDefault(p => p.User == User);


                ViewBag.Total = 0;
                foreach (Stock Stock in Watchlist.Stocks)
                {
                    // Create a Dictionary object to store JSON values from API call
                    Dictionary<string, object> Data = new Dictionary<string, object>();

                    // Make API call
                    WebRequest.GetQuote(Stock.Symbol, JsonResponse =>
                        {
                            Data = JsonResponse;
                        }
                    ).Wait();

                    // Define values for each stock to be stored in ViewBag
                    double CurrentPrice = Convert.ToDouble(Data["latestPrice"]);

                    Stock.Name = (string)Data["companyName"];
                    Stock.PurchaseValue = Stock.PurchasePrice * Stock.Shares;
                    Stock.CurrentPrice = CurrentPrice;
                    Stock.CurrentValue = CurrentPrice * Stock.Shares;
                    Stock.GainLossPrice = CurrentPrice - Stock.PurchasePrice;
                    Stock.GainLossValue = (CurrentPrice - Stock.PurchasePrice) * Stock.Shares;
                    Stock.GainLossPercent = 100 * (CurrentPrice - Stock.PurchasePrice) / (Stock.PurchasePrice);
                    Stock.Week52Low = Convert.ToDouble(Data["week52Low"]);
                    Stock.Week52High = Convert.ToDouble(Data["week52High"]);
                    Stock.UpdatedAt = DateTime.Now;

                    _context.SaveChanges();

                    ViewBag.Total += Stock.CurrentValue;
                }

                // Store values in ViewBag for Portfolio page rendering
                ViewBag.Watchlist = Watchlist;
                return View("Profile");
            }
            else
            {
                return View("landing");
            }
            // If no id is in session that means that the user is not properly logged on. Redirect to logout (to clear session, just in case) which will end up at landing page.
        }


        [HttpPost]
        [Route("UpdateBio")]
        public IActionResult UpdateBio(Dictionary<string,string> Data)
        {
            if(Data != null){
                var SessionId = HttpContext.Session.GetInt32("LoggedUserId");
                User User = _context.Users.SingleOrDefault(u => u.Id == SessionId);
                User.Bio = Data["Bio"];
                _context.Update(User);
                _context.SaveChanges();
            }
            return RedirectToAction("Profile");
        }
        [HttpPost]
        [Route("UpdateEmail")]
        public IActionResult UpdateEmail(Dictionary<string,string> Data)
        {
            if(Data["NewEmailA"] == Data["NewEmailB"]){
                var SessionId = HttpContext.Session.GetInt32("LoggedUserId");
                User User = _context.Users.SingleOrDefault(u => u.Id == SessionId);
                User.Email = Data["NewEmailA"];
                _context.Update(User);
                _context.SaveChanges();
            }
            else{
                @ViewBag.EmailError = "Emails need to match.";
            }
            return RedirectToAction("Profile");
        }

        
        [HttpPost]
        [Route("UpdatePassword")]
        public IActionResult UpdatePassword(Dictionary<string,string> Data)
        {
            Console.WriteLine("Data");
            Console.WriteLine(Data);
            Console.WriteLine(Data["Password"]);
            Console.WriteLine(Data["PasswordA"]);
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
                            Console.WriteLine("ABOUT TO UPDATE");
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