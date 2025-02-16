using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;
using QuizAPI.Filters;
namespace QuizAPI.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public bool IsVerified { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
        public ICollection<Attempt> Attempts { get; set; } = new List<Attempt>();

        public bool MatchPassword(string enteredPassword)
        {
            using var sha256 = SHA256.Create();
            var hashedPassword = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(enteredPassword)));
            return hashedPassword == Password;
        }

        public void HashPassword()
        {
            using var sha256 = SHA256.Create();
            Password = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(Password)));
        }
    }
}
