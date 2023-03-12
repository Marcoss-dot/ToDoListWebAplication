using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoListWebAplication.Models;

namespace ToDoListWebAplication.Controllers
{
    public class ToDoListController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private static DateTime? _currentDateFilter = null;
        private readonly TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

        public ToDoListController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ActionResult> Index(DateTime? date)
        {
            _currentDateFilter = date ?? _currentDateFilter;
            List<ToDoItem> toDoList;
            if (_currentDateFilter != null)
            {
                TempData["Info"] = $"Current filter: {_currentDateFilter.Value.Date:d}";
                IQueryable<ToDoItem> items = _dbContext.ToDoItems.Where(i => i.DateTime.Date == _currentDateFilter.Value.Date).OrderBy(i => i.DateTime);

                toDoList = await items.ToListAsync();
            }
            else
            {
                TempData["Info"] = "None filtering. View all of items.";
                IQueryable<ToDoItem> items = _dbContext.ToDoItems.OrderBy(i => i.DateTime);

                toDoList = await items.ToListAsync();
            }

            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo);

            if (toDoList.Any(
                i =>
                {
                    var diff = (i.DateTime - now).TotalMinutes;
                    return diff <= 60 && diff > 0;
                }))
                TempData["UpcomingTask"] = "UPCOMING TASK! Time to completion less than 1h.";

            return View(toDoList);

        }

        public IActionResult GetAllItems()
        {
            _currentDateFilter = null;
            return RedirectToAction("Index");
        }

        public IActionResult ToDayList()
        {
            _currentDateFilter = null;
            return RedirectToAction("Index", DateTime.Now);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ToDoItem item)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Add(item);
                await _dbContext.SaveChangesAsync();

                TempData["Success"] = "The item has been added!";
                return RedirectToAction("Index");
            }

            return View(item);
        }

        public async Task<ActionResult> Edit(int id)
        {
            var item = await _dbContext.ToDoItems.FindAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ToDoItem item)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Update(item);
                await _dbContext.SaveChangesAsync();

                TempData["Success"] = "The item has been updated!";
                return RedirectToAction("Index");
            }

            return View(item);
        }

        public async Task<ActionResult> SetAsDone(int id)
        {
            var item = await _dbContext.ToDoItems.FindAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            item.IsDone = !item.IsDone;
            _dbContext.Update(item);
            await _dbContext.SaveChangesAsync();

            TempData["Success"] = "The item has been done!";
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Delete(int id)
        {
            var item = await _dbContext.ToDoItems.FindAsync(id);

            if (item == null)
            {
                TempData["Error"] = "The item does not exist!";
            }
            else
            {
                _dbContext.Remove(item);
                await _dbContext.SaveChangesAsync();

                TempData["Success"] = "The item has been deleted!";
            }

            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}