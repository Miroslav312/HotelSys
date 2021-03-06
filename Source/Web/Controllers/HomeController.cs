﻿using Data;
using Data.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Web.Models.Home;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly MyHotelDb _context;

        public HomeController()
        {
            _context = new MyHotelDb();
        }

        //GET: /Index
        public IActionResult Index()
        {
            LoginViewModel model = new LoginViewModel();

            return View(model);
        }

        //POST: /Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (GetCookie("LoggedIn") == "true")
            {
                return Redirect("/Users");
            }
            else
            {
                User user = _context.Users.ToArray().Where(u => u.Username == model.Username).FirstOrDefault();
                if (user != null)
                {
                    if (user.PasswordHash == GetPasswordHash(model.PasswordHash))
                    {
                        SetCookie("LoggedIn", "true");
                        return Redirect("/Users");
                    }
                    else
                    {
                        ModelState.AddModelError("PasswordHash", "Password does not match!");
                        return View(model);
                    }
                }

                ModelState.AddModelError("Username", "User with that username does not exist!");
                return View(model);
            }
        }

        private string GetPasswordHash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private string GetCookie(string key)
        {
            try
            {
                return Request.Cookies[key];
            }
            catch (KeyNotFoundException)
            {
                return "false";
            }
        }

        private void SetCookie(string key, string value, int? expireTime = null)
        {
            CookieOptions option = new CookieOptions();

            if (expireTime.HasValue)
            {
                option.Expires = DateTime.Now.AddMinutes(expireTime.Value);

                Response.Cookies.Append(key, value, option);
            }
            else
            {
                Response.Cookies.Append(key, value);
            }
        }

        private void RemoveCookie(string key)
        {
            Response.Cookies.Delete(key);
        }
    }
}