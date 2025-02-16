using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizAPI.Models
{
    public class Quiz
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; }

        [ForeignKey("User")]
        public Guid AuthorId { get; set; }
        public User Author { get; set; }

        public List<Question> Questions { get; set; } = new List<Question>();

        public int Duration { get; set; } = 180; // Duration in minutes

        public bool IsLocked { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
