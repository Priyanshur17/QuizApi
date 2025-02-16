using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizAPI.Models
{
    public class Attempt
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("Quiz")]
        public Guid QuizId { get; set; }
        public Quiz Quiz { get; set; }

        [ForeignKey("User")]
        public Guid PlayerId { get; set; }
        public User Player { get; set; }

        public List<AttemptedQuestion> Questions { get; set; } = new List<AttemptedQuestion>();

        public double TotalScore { get; set; } = 0;

        [ForeignKey("Feedback")]
        public Guid? FeedbackId { get; set; }
        public Feedback Feedback { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class AttemptedQuestion
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("Question")]
        public Guid QuestionId { get; set; }
        public Question Question { get; set; }

        public List<int> Answers { get; set; } = new List<int>();

        public string IsCorrect { get; set; } = "false";

        public double Score { get; set; } = 0;
    }
}
