using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectTimeTrackerAuth.Data;
using ProjectTimeTrackerAuth.Models;
using Microsoft.AspNetCore.Authorization;

namespace ProjectTimeTrackerAuth.Controllers
{
    public class TrackItController : Controller
    {
        private readonly TimerContext _context;

        public TrackItController(TimerContext context)
        {
            _context = context;    
        }

        // GET: TrackIt
        [Authorize]
        public async Task<IActionResult> Index()
        {
            //get last time log with no end time
            string query = "SELECT TOP 1 * FROM TimeLog WHERE Username = {0} and EndTime is null order by StartTime desc";
            var timeLog = await _context.TimeLogs
                .FromSql(query, User.Identity.Name)
                .AsNoTracking()
                .SingleOrDefaultAsync();
            ViewData["LastTimeLog"] = timeLog;

            //get list of activities
            ViewData["ActivityID"] = new SelectList(_context.Activities.Where(o => o.Username == User.Identity.Name), "ActivityID", "ActivityName");

            return View();
        }

        // GET: TrackIt/Create
        public IActionResult Create()
        {
            ViewData["ActivityID"] = new SelectList(_context.Activities, "ActivityID", "ActivityID");
            return View();
        }

        // POST: TrackIt/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TimeLogID,Username,ActivityID,UsersID,StartTime,EndTime")] TimeLog timeLog)
        {
            if (ModelState.IsValid)
            {
                _context.Add(timeLog);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["ActivityID"] = new SelectList(_context.Activities, "ActivityID", "ActivityID", timeLog.ActivityID);
            return View(timeLog);
        }

        // GET: TrackIt/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var timeLog = await _context.TimeLogs.SingleOrDefaultAsync(m => m.TimeLogID == id);
            if (timeLog == null)
            {
                return NotFound();
            }
            ViewData["ActivityID"] = new SelectList(_context.Activities, "ActivityID", "ActivityID", timeLog.ActivityID);
            return View(timeLog);
        }

        // POST: TrackIt/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TimeLogID,Username,ActivityID,UsersID,StartTime,EndTime")] TimeLog timeLog)
        {
            if (id != timeLog.TimeLogID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(timeLog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TimeLogExists(timeLog.TimeLogID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            ViewData["ActivityID"] = new SelectList(_context.Activities, "ActivityID", "ActivityID", timeLog.ActivityID);
            return View(timeLog);
        }

        private bool TimeLogExists(int id)
        {
            return _context.TimeLogs.Any(e => e.TimeLogID == id);
        }
    }
}
