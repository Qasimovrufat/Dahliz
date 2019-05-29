using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using AutoTecheille.Data;
using AutoTecheille.Models;
using AutoTecheille.Models.VIewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AutoTecheille.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly AutoEntity _db;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AccountController(AutoEntity db, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _db = db;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            
            return View();
        }


        
        public async Task<IActionResult> Singout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home", new { area = "Admin" });

        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var isAdmin = await _userManager.IsInRoleAsync(user, "admin");
                    if (isAdmin)
                    {
                        //var result  =await _signInManager.CheckPasswordSignInAsync(user, model.Password, true);
                        var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, true);
                        if (result.Succeeded)
                        {
                            return RedirectToAction("Index", "Home", new { area = "Admin" });
                        }
                        else
                        {
                            return View();
                        }
                    }
                    else
                    {
                        return View();
                    }
                }
                else
                {
                    return View();
                }
            }
            else
            {
                return View();
            }
        }
    }
    }