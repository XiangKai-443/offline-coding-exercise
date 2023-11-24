using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Diagnostics;
using System.Globalization;
using ToDoApp.Models;

namespace ToDoApp.Controllers
{
    public class HomeController : Controller
    {
        private ToDoContext context;
        public HomeController(ToDoContext ctx) => context = ctx;

        public IActionResult Index(string id, string sortOrder)
        {
            var filters = new Filters(id ?? "all-all-all");
            ViewBag.Filters = filters;
            ViewBag.Categories = context.Categories.ToList();
            ViewBag.Statuses = context.Statuses.ToList();
            ViewBag.DueFilters = Filters.DueFilterValues;

            IQueryable<ToDo> query = context.ToDos
                .Include(t => t.Category)
                .Include(t => t.Status);

            if (filters.HasCategory)
            {
                query = query.Where(t => t.CategoryId == filters.CategoryId);
            }
            if (filters.HasStatus)
            {
                query = query.Where(t => t.StatusId == filters.StatusId);
            }
            if (filters.HasDue)
            {
                var today =DateTime.Today;
                if (filters.IsPast)
                {
                    query = query.Where(t => t.DueDate < today);
                }
                else if (filters.IsFuture)
                {
                    query = query.Where(t => t.DueDate > today);
                }
                else if (filters.IsToday)
                {
                    query = query.Where(t => t.DueDate == today);
                }
            }
            //var tasks = query.OrderBy(t=> t.DueDate).ToList();
            var tasks = query.ToList();

            if (sortOrder == "Priority")
            {
                tasks = tasks.OrderByDescending(item => item.Priority).ToList();
            }
            else if(sortOrder == "Date")
            {
                tasks = tasks.OrderBy(item => item.DueDate).ToList();
            }

            return View(tasks);
        }

        [HttpGet]
        public IActionResult Add()
        {
            ViewBag.Categories = context.Categories.ToList();
            ViewBag.Statuses = context.Statuses.ToList();
            var task = new ToDo { StatusId = "open" };
            return View("AddEdit",task);
        }
        [HttpPost]
        public IActionResult AddEdit(ToDo task)
        {
            if (ModelState.IsValid)
            {
                if (task.Id == 0)
                {
                    // Logic for adding a new task
                    context.ToDos.Add(task);
                }
                else
                {
                    // Logic for updating an existing task
                    var existingTask = context.ToDos.Find(task.Id);

                    if (existingTask == null)
                    {
                        return NotFound();
                    }

                    // Update existing task properties
                    existingTask.Description = task.Description;
                    existingTask.CategoryId = task.CategoryId;
                    existingTask.DueDate = task.DueDate;
                    existingTask.StatusId = task.StatusId;
                    existingTask.Priority = task.Priority;
                }

                context.SaveChanges();

                return RedirectToAction("Index");
            }
            /*if (ModelState.IsValid)
            {
                context.ToDos.Add(task);
                context.SaveChanges();
                return RedirectToAction("Index");
            }*/
            else{
                ViewBag.Categories = context.Categories.ToList();
                ViewBag.Statuses =context.Statuses.ToList();
                return View("AddEdit",task);
            }
        }

        [HttpPost]
        public IActionResult Filter(string[] filter)
        {
            string id = string.Join("-", filter);
            return RedirectToAction("Index", new { ID = id });
        }

        [HttpPost]
        public IActionResult MarkComplete([FromRoute]string id, ToDo selected)
        {
            selected = context.ToDos.Find(selected.Id)!;

            if(selected != null)
            {
                selected.StatusId = "closed";
                context.SaveChanges();
            }
            return RedirectToAction("Index", new { ID = id });
        
        }
        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            var task = context.ToDos.Find(id);

            if (task == null)
            {
                return NotFound();
            }

            context.ToDos.Remove(task);
            context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DeleteComplete(string id)
        {
            var toDelete = context.ToDos.Where(t => t.StatusId == "closed").ToList();
            foreach (var task in toDelete)
            {
                context.ToDos.Remove(task);
            }
            context.SaveChanges();

            return RedirectToAction("Index", new { ID = id });
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var task = context.ToDos.Find(id);

            if (task == null)
            {
                return NotFound(); 
            }

            /*var taskViewModel = new ToDo
            {
                Id = task.Id,
                Description = task.Description,
                CategoryId = task.CategoryId,
                DueDate = task.DueDate,
                StatusId = task.StatusId,
                Priority = task.Priority
            };*/
            ViewBag.Categories = context.Categories.ToList();
            ViewBag.Statuses = context.Statuses.ToList();

            // Pass the ViewModel to the view for editing
            return View("AddEdit",task);
        }

    }
}