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
                var SessionId = HttpContext.Session.GetInt32("LoggedUserId");
                User User = _context.Users.SingleOrDefault(u => u.Id == SessionId);
                ViewBag.User = User;
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
                        HttpContext.Session.SetString("LoggedUserName", LoggedUser.FirstName);
                        return RedirectToAction("Portfolio");
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

        [HttpGet]
        [Route("Portfolio")]
        public IActionResult Portfolio()
        {
            // Retreive id from Session for User query
            int? id = HttpContext.Session.GetInt32("LoggedUserId");
            
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            // Retreive current User and Portfolio from the database
            User User = _context.Users.SingleOrDefault(u => u.Id == (int)id);
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
            }
            
            // Store values in ViewBag for Portfolio page rendering
            ViewBag.Portfolio = Portfolio;
            ViewBag.User = User;

            return View("Portfolio");
        }

        [HttpPost]
        [Route("PortfolioAdd")]
        public IActionResult PortfolioAdd(StockViewModel s)
        {
            int? id = HttpContext.Session.GetInt32("LoggedUserId");

            if (id == null)
            {
                return RedirectToAction("Index");
            }

            User User = _context.Users.SingleOrDefault(u => u.Id == (int)id);
            Portfolio Portfolio = _context.Portfolios
                .Include(p => p.Stocks)
                .SingleOrDefault(p => p.User == User);
            
            if (ModelState.IsValid)
            {
                Stock NewStock = new Stock
                {
                    Symbol = s.Symbol,
                    Shares = s.Shares,
                    PurchasePrice = s.PurchasePrice,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                };

                Portfolio.Stocks.Add(NewStock);
                _context.Add(NewStock);
                _context.SaveChanges();
                
                return RedirectToAction("Portfolio");
            }

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
            }

            // Store values in ViewBag for Portfolio page rendering
            ViewBag.Portfolio = Portfolio;
            ViewBag.User = User;
            
            return View("Portfolio");
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