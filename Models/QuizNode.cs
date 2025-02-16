using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizAPI.Models
{
    public class QuizNode
    {
        [Key]
        public Guid Id { get; set; }

        public Location Location { get; set; }

        public List<Quiz> Quizzes { get; set; } = new List<Quiz>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Location
    {
        [Required]
        public string Type { get; set; } = "Point";

        [Required]
        public List<double> Coordinates { get; set; } = new List<double>(); // [longitude, latitude]
    }
}
