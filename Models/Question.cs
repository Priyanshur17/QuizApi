using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizAPI.Models
{
    public class Question
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("Quiz")]
        public Guid QuizId { get; set; }
        public Quiz Quiz { get; set; }

        [ForeignKey("User")]
        public Guid AuthorId { get; set; }
        public User Author { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public List<string> Options { get; set; } = new List<string>();

        [Required]
        public List<int> Answers { get; set; } = new List<int>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
