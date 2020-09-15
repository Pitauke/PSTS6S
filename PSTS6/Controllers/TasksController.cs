﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PSTS6.Data;
using PSTS6.Models;

namespace PSTS6.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        private readonly PSTS6Context _context;
        
        public TasksController(PSTS6Context context)
        {
            _context = context;
          
        }

        // GET: Tasks
        public async Task<IActionResult> Index()
        {
            return View(await _context.Task.ToListAsync());
        }

        // GET: Tasks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Task
                .FirstOrDefaultAsync(m => m.ID == id);
            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        

        
        public async Task<IActionResult> Create(string btnAddTask)
        {
            var dbUsers = await _context.Users.ToListAsync();

            IEnumerable<SelectListItem> users = dbUsers.Select(x => new SelectListItem
            {
                Text = x.UserName,
                Value = x.UserName
                
            });

            var projects = await _context.Project.ToListAsync();

            IEnumerable<SelectListItem> projectsToSelect = projects.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.ID.ToString(),
                        
            });

            var viewModel = new TaskCreateViewModel();

            viewModel.availableOwners = users;
            viewModel.StartDate = DateTime.Today;
            viewModel.EstimatedEndDate = DateTime.Today;
             
            viewModel.availableProjects = projectsToSelect.Where(x=>x.Value==btnAddTask);
            
            

            return View(viewModel);
        }

        // POST: Tasks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StartDate,EstimatedEndDate,ID,Name,Description,ProjectID")] PSTS6.Models.Task task)
        {
            if (ModelState.IsValid)
            {

                string selectedProject = Request.Form["Project"].ToString();

                task.ProjectID = Convert.ToInt32(selectedProject);

                _context.Add(task);
                await _context.SaveChangesAsync();
                
                return RedirectToAction("Edit","Projects", new { id= task.ProjectID});
            }
            return View(task);
        }

        // GET: Tasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Task.Where(t => t.ID == id).Include(t => t.Activities).FirstOrDefaultAsync();

            var viewModel = new TaskEditViewModel();

            viewModel.ID = task.ID;
            viewModel.Name = task.Name;
            viewModel.Description = task.Description;
            viewModel.ActualEndDate = task.ActualEndDate;
            viewModel.EstimatedEndDate = task.EstimatedEndDate;
            viewModel.StartDate = task.StartDate;
           viewModel.Activities = task.Activities.ToList();
            viewModel.ProjectID = task.ProjectID;


            if (task == null)
            {
                return NotFound();
            }
            return View(viewModel);
        }

        // POST: Tasks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PrcCompleted,Budget,StartDate,EstimatedEndDate,ActualEndDate,Spent,ID,Name,Description,ProjectID")] PSTS6.Models.Task task)
        {
            if (id != task.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(task);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskExists(task.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Edit", "Projects", new { id = task.ProjectID });
            }
            return View(task);
        }

        // GET: Tasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Task
                .FirstOrDefaultAsync(m => m.ID == id);
            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        // POST: Tasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var task = await _context.Task.FindAsync(id);

            var projectid = task.ProjectID;

            _context.Task.Remove(task);
            await _context.SaveChangesAsync();
            return RedirectToAction("Edit", "Projects", new { id = projectid });
        }

        private bool TaskExists(int id)
        {
            return _context.Task.Any(e => e.ID == id);
        }
    }
}
