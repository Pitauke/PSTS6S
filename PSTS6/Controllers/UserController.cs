﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PSTS6.Data;
using PSTS6.Models;
using PSTS6.Models.IdentityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PSTS6.Controllers
{
    public class UserController : Controller
    {
        private readonly PSTS6Context _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(PSTS6Context context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {

           

            return View(await _context.Users.ToListAsync());
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.Where(x => x.Id == id).FirstOrDefaultAsync();

          
           

            var userRole = await _context.UserRoles.Where(x => x.UserId == id).FirstOrDefaultAsync();

            var roles = await _context.Roles.ToListAsync();

            var role = roles.Where(x => x.Id == userRole.RoleId).FirstOrDefault();

            var rolesSelectList = roles.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Name,


            });

            foreach (var item in rolesSelectList)
            {
                if (item.Text.Equals(role))
                {
                    item.Selected = true;
                }
            }

            var viewModel = new UserEditViewModel 
            { 
                UserName=user.UserName,
                Email=user.Email,

               
                AvailableRoles=rolesSelectList
            };

            

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("UserName,Email,Id,PasswordHash,EmailConfirmed,LockoutEnabled")] IdentityUser user)
        {

            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {

                   
                    


                    user = _context.Users.Where(x => x.Id == user.Id).FirstOrDefault();

                    string selectedRole = Request.Form["Role"].ToString();

                    user.UserName = Request.Form["UserName"].ToString();

                    user.Email = Request.Form["Email"].ToString();
                    
                   // var userRoles = await _userManager.GetRolesAsync(user);

                    List<IdentityUserRole<string>> userRoles = _context.UserRoles.Where(x => x.UserId == user.Id).ToList();

                    List<IdentityUserRole<string>> allRoles = _context.UserRoles.ToList();

                    foreach (var item in userRoles)
                    {
                        _context.Remove(item);
                    }
                     
                   // await _userManager.RemoveFromRolesAsync(user, _userManager.GetRolesAsync(user).Result);

                    if (await _userManager.IsInRoleAsync(user, selectedRole))
                    {

                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, selectedRole);
                    }
                   

                    _context.Update(user);

                   await _context.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View();
        }
        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

    }

   

}

