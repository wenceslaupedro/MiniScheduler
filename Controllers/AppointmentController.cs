using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using MiniScheduler.Data;
using MiniScheduler.Models;

namespace MiniScheduler.Controllers;

[Authorize]
public class AppointmentController : Controller
{
	private readonly AppDbContext _dbContext;

	public AppointmentController(AppDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public IActionResult Index()
	{
		// Calendar view; events are loaded via /Appointment/Events
		return View();
	}

	public IActionResult Create(DateTime? date)
	{
		DateTime target = date ?? DateTime.UtcNow.AddHours(1);
		if (target.Kind == DateTimeKind.Unspecified)
		{
			// Assume local input from browser, convert to UTC
			target = DateTime.SpecifyKind(target, DateTimeKind.Local).ToUniversalTime();
		}
		target = new DateTime(target.Year, target.Month, target.Day, target.Hour, target.Minute, 0, DateTimeKind.Utc);
		return View(new Appointment { DateTime = target });
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create(Appointment model)
	{
		// Normalize to minute precision in UTC
		model.DateTime = DateTime.SpecifyKind(model.DateTime, DateTimeKind.Utc);
		model.DateTime = new DateTime(model.DateTime.Year, model.DateTime.Month, model.DateTime.Day, model.DateTime.Hour, model.DateTime.Minute, 0, DateTimeKind.Utc);

		if (model.DateTime <= DateTime.UtcNow)
		{
			ModelState.AddModelError(nameof(model.DateTime), "Date/time must be in the future.");
		}

		var doubleBooked = await _dbContext.Appointments
			.AnyAsync(a => a.DateTime == model.DateTime);
		if (doubleBooked)
		{
			ModelState.AddModelError(nameof(model.DateTime), "This slot is already booked.");
		}

		if (!ModelState.IsValid)
		{
			return View(model);
		}

		_dbContext.Appointments.Add(model);
		await _dbContext.SaveChangesAsync();
		return RedirectToAction(nameof(Index));
	}

	[HttpGet]
	public async Task<IActionResult> Events(DateTime? start, DateTime? end)
	{
		// Return events for FullCalendar
		var query = _dbContext.Appointments.AsQueryable();
		if (start.HasValue)
		{
			var sUtc = start.Value.Kind == DateTimeKind.Utc ? start.Value : start.Value.ToUniversalTime();
			query = query.Where(a => a.DateTime >= sUtc);
		}
		if (end.HasValue)
		{
			var eUtc = end.Value.Kind == DateTimeKind.Unspecified ? end.Value : end.Value.ToUniversalTime();
			query = query.Where(a => a.DateTime <= eUtc);
		}
		var items = await query.Select(a => new
		{
			id = a.Id,
			title = a.Name,
			start = a.DateTime,
			allDay = false
		}).ToListAsync();
		return Json(items);
	}

	public async Task<IActionResult> Delete(int id)
	{
		var appt = await _dbContext.Appointments.FindAsync(id);
		if (appt == null)
		{
			return NotFound();
		}
		return View(appt);
	}

	[HttpPost, ActionName("Delete")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> DeleteConfirmed(int id)
	{
		var appt = await _dbContext.Appointments.FindAsync(id);
		if (appt != null)
		{
			_dbContext.Appointments.Remove(appt);
			await _dbContext.SaveChangesAsync();
		}
		return RedirectToAction(nameof(Index));
	}
}


