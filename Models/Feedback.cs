using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizAPI.Models
{
	public class Feedback
	{
		[Key]
		public Guid Id { get; set; }

		[ForeignKey("Attempt")]
		public Guid AttemptId { get; set; }
		public Attempt Attempt { get; set; }

		[Required]
		public int Rating { get; set; }

		public string Message { get; set; } = string.Empty;
	}
}
