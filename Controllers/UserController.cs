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
            int? id = HttpContext.Session.GetInt32("LoggedUserId");
            
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            User User = _context.Users.SingleOrDefault(u => u.Id == (int)id);
            Portfolio Portfolio = _context.Portfolios
                .Include(p => p.Stocks)
                .SingleOrDefault(p => p.User == User);
            ViewBag.Portfolio = Portfolio;
            ViewBag.User = User;
            ViewBag.Total = 0;

            JObject apikey = JObject.Parse(System.IO.File.ReadAllText("apikey.json"));
            
            foreach (Stock Stock in Portfolio.Stocks)
            {
                Dictionary<string, object> Data = new Dictionary<string, object>();
                
                WebRequest.GetMarketData("TIME_SERIES_INTRADAY", Stock.Symbol, "1min", JsonResponse =>
                    {
                        Data = JsonResponse;
                    }
                ).Wait();

                JObject MetaData = (JObject)Data["Meta Data"];
                string LastRefreshed = (string)MetaData["3. Last Refreshed"];
                string Name = (string)MetaData["3. Last Refreshed"];

                JObject TimeSeries = (JObject)Data["Time Series (1min)"];
                JObject DataPoint = (JObject)TimeSeries[LastRefreshed];
                float Value = (float)DataPoint["4. close"];

                Stock.Value = Value;
                ViewBag.Total += Value; 
            }

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
            Portfolio Portfolio = _context.Portfolios.SingleOrDefault(p => p.User == User);
            
            if (ModelState.IsValid)
            {
                Stock NewStock = new Stock
                {
                    Symbol = s.Symbol,
                    Shares = s.Shares,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                };

                Portfolio.Stocks.Add(NewStock);
                _context.Add(NewStock);
                _context.SaveChanges();
                
                return RedirectToAction("Portfolio");
            }

            ViewBag.User = User;
            ViewBag.Portfolio = Portfolio;
            float Total = 0;

            JObject apikey = JObject.Parse(System.IO.File.ReadAllText("apikey.json"));

            foreach (Stock Stock in Portfolio.Stocks)
            {
                Dictionary<string, object> Data = new Dictionary<string, object>();

                WebRequest.GetMarketData("TIME_SERIES_INTRADAY", Stock.Symbol, "1min", JsonResponse =>
                    {
                        Data = JsonResponse;
                    }
                ).Wait();

                Dictionary<string, string> MetaData = (Dictionary<string, string>)Data["Meta Data"];
                string LastRefreshed = MetaData["3. Last Refreshed"];

                Dictionary<string, object> TimeSeries = (Dictionary<string, object>)Data["Time Series (1min)"];
                Dictionary<string, object> DataPoint = (Dictionary<string, object>)TimeSeries[LastRefreshed];
                float Value = (float)DataPoint["4. close"];

                Stock.Value = Value;
                Total += Value;
            }

            ViewBag.Total = Total;

            return View("Portfolio");
        }
    }
}
