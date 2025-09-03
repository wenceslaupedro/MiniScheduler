using System.ComponentModel.DataAnnotations;

namespace MiniScheduler.Models;

public class Appointment
{
	public int Id { get; set; }

	[Required]
	[MaxLength(100)]
	[Display(Name = "Appointment")]
	public string Name { get; set; } = string.Empty;

	[EmailAddress]
	[MaxLength(200)]
	public string Email { get; set; } = string.Empty;

	[Required]
	[Display(Name = "Date and Time")]
	public DateTime DateTime { get; set; }

	[MaxLength(200)]
	[Display(Name = "Location")]
	public string? Location { get; set; }
}


