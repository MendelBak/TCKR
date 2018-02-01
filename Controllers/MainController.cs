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
    public class MainController : Controller
    {
        private tckrContext _context;
        
        public MainController(tckrContext context)
        {
            _context = context;
        }
        



        [HttpGet]
        [Route("Watchlist")]
        public IActionResult Watchlist()
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
            return View("Watchlist");
        }



        [HttpGet]
        [Route("Portfolio")]
        public IActionResult Portfolio()
        {
            // Retreive id from Session for User query
            int? id = HttpContext.Session.GetInt32("LoggedUserId");
            
            if (id == null)
            {
                return RedirectToAction("Index", "User");
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
                return RedirectToAction("Index", "User");
            }

            User User = _context.Users.SingleOrDefault(u => u.Id == (int)id);
            Portfolio Portfolio = _context.Portfolios
                .Include(p => p.Stocks)
                .SingleOrDefault(p => p.User == User);
            
            if (ModelState.IsValid)
            {
                try
                {
                    Dictionary<string, object> Data = new Dictionary<string, object>();
                    // Make a API call to ensure that the inputted ticker/symbol is a valid one before storing it in user's list of Stocks.
                    WebRequest.GetQuote(s.Symbol, JsonResponse =>
                    {
                        Data = JsonResponse;
                    }
                    ).Wait();


                    Stock NewStock = new Stock
                    {
                        Symbol = s.Symbol,
                        Shares = s.Shares,
                        Name = (string)Data["companyName"],
                        PurchasePrice = s.PurchasePrice,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                    };

                    Portfolio.Stocks.Add(NewStock);
                    _context.Add(NewStock);
                    _context.SaveChanges();
                    
                    return RedirectToAction("Portfolio");
                }
                // Catch will run if the ticker/symbol submitted was not found in the DB.
                catch
                {
                    // Return reason for error
                    TempData["NewStockError"] = "That stock does not exist in our database. Please try again.";
                    // Return vital data for view.
                    // ViewBag.Portfolio = Portfolio;
                    // ViewBag.User = User;
                    return RedirectToAction("Portfolio");
                }
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