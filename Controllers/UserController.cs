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
            return View("register");
        }

        // Newuser route is the registration route for a new user.
        [HttpPost]
        [Route("NewUser")]
        public IActionResult NewUser(RegisterViewModel model)
        {
            // Check if models received any validation errors.
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if email already exists in DB.
                    var EmailExists = _context.Users.Where(e => e.Email == model.Email).SingleOrDefault();
                    if (EmailExists == null)
                    {
                        // Hash password
                        PasswordHasher<RegisterViewModel> Hasher = new PasswordHasher<RegisterViewModel>();
                        string HashedPassword = Hasher.HashPassword(model, model.Password);
                        User NewUser = new User
                        {
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            Email = model.Email,
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

                        // Set user id and first name in session for use in identification, future db calls, and for greeting the user.
                        HttpContext.Session.SetInt32("LoggedUserId", NewUser.Id);
                        HttpContext.Session.SetString("LoggedUserName", NewUser.FirstName);

                        // Redirect to Account method in Account controller.
                        return RedirectToAction("Portfolio");
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
        public IActionResult LoginSubmit(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // If there are no errors upon form submit check db for proper creds.
                    User LoggedUser = _context.Users.SingleOrDefault(u => u.Email == model.Email);
                    var Hasher = new PasswordHasher<User>();
                    // Check hashed password.
                    if (Hasher.VerifyHashedPassword(LoggedUser, LoggedUser.Password, model.Password) != 0)
                    {
                        // Set user id and first name in session for use in identification, future db calls, and for greeting the user.
                        HttpContext.Session.SetInt32("LoggedUserId", LoggedUser.Id);
                        HttpContext.Session.SetString("LoggedUserName", LoggedUser.FirstName);
                        return RedirectToAction("Portfolio");
                    }
                    else
                    {
                        ViewBag.loginError = "Sorry, your password was incorrect.";
                        return View("login");
                    }
                }
                // If no proper creds redirect to login page and return error.
                catch
                {
                    ViewBag.loginError = "Sorry, your email or password were incorrect.";
                    return View("login");
                }
            }
            // If form submit was illegal redirect to login and display model validation errors.
            else
            {
                return View("login");
            }
        }

        [HttpGet]
        [Route("Account")]
        public IActionResult Account()
        {
            // Check to ensure there is a properly logged in user by checking session.
            if (HttpContext.Session.GetInt32("LoggedUserId") >= 0)
            {
                try
                {

                    // Save first name in session to display greeting on navbar.
                    ViewBag.FirstName = HttpContext.Session.GetString("LoggedUserName");
                    // Save id in session and then send to View using Viewbag
                    ViewBag.UserId = HttpContext.Session.GetInt32("LoggedUserId");
                    return View("Portfolio");
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
            return RedirectToAction("LoginPage");
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
    }
}
