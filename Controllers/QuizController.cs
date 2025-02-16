using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizAPI.Data;
using QuizAPI.Models;
using QuizAPI.Filters;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizAPI.Controllers
{
    [ApiController]
    [ServiceFilter(typeof(AuthFilter))]
    [Route("api/v1/quiz")]
    public class QuizController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public QuizController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // POST: /api/quiz/create
       
        [HttpPost("create")]
        public async Task<IActionResult> CreateQuiz([FromBody] CreateQuizRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Title))
                    return BadRequest(new { success = false, message = "Title is empty" });

                if (request.Questions == null || !request.Questions.Any())
                    return BadRequest(new { success = false, message = "Please add at least one question" });

                var userId = HttpContext.Items["UserId"]?.ToString();
                if (userId == null)
                {
                    return Unauthorized(new { success = false, message = "User not found in context" });
                }

                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));
                if (user == null)
                {
                    return Unauthorized(new { success = false, message = "User not found in the database" });
                }

                // Create quiz first
                var newQuiz = new Quiz
                {
                    Title = request.Title,
                    AuthorId = user.Id,
                    Duration = request.Duration
                };

                // Add quiz to context
                await _dbContext.Quizzes.AddAsync(newQuiz);
                // Save to get the Quiz ID
                await _dbContext.SaveChangesAsync();

                // Now create and add questions
                var questions = request.Questions
                    .Where(IsValidQuestion)
                    .Select(q => new Question
                    {
                        Title = q.Title,
                        Options = q.Options,
                        Answers = q.Answers,
                        AuthorId = user.Id,
                        QuizId = newQuiz.Id  // Now we have a valid Quiz ID
                    })
                    .ToList();

                // Add questions to context
                await _dbContext.Questions.AddRangeAsync(questions);
                // Save questions
                await _dbContext.SaveChangesAsync();

                // Load the complete quiz with questions for the response
                var completeQuiz = await _dbContext.Quizzes
                    .Include(q => q.Questions)
                    .FirstOrDefaultAsync(q => q.Id == newQuiz.Id);

                return Ok(new { success = true, message = "Quiz created successfully", quiz = completeQuiz });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in CreateQuiz: {ex.Message}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }



        // GET: /api/quiz/show/{id}
        [HttpGet("show/{id}")]
        public async Task<IActionResult> ShowQuiz(Guid id)
        {
        //    var quizLockCheck = await _dbContext.Quizzes
        //.Where(q => q.Id == id)
        //.Select(q => new { q.IsLocked })
        //.FirstOrDefaultAsync();

        //    if (quizLockCheck == null)
        //        return NotFound(new { success = false, message = "Quiz not found" });

        //    if (quizLockCheck.IsLocked)
        //        return BadRequest(new { success = false, message = "This quiz is locked" });
            var quiz = await _dbContext.Quizzes
                .Where(q => q.Id == id)
                .Select(q => new
                {
                    q.Id,
                    q.Title,
                    q.Duration,
                    q.AuthorId,
                    Questions = q.Questions.Select(qu => new
                    {
                        qu.Id,
                        qu.Title,
                        Options = qu.Options,  // Ensure Options is correctly stored and retrieved
                        Answers = qu.Answers
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (quiz == null)
                return NotFound(new { success = false, message = "Quiz not found" });

            return Ok(new { success = true, quiz });
        }


        // POST: /api/quiz/lock
        [HttpPost("lock")]
        public async Task<IActionResult> LockQuiz([FromBody] LockQuizRequest request)
        {
            var quiz = await _dbContext.Quizzes.FindAsync(request.Id);

            if (quiz == null)
                return NotFound(new { success = false, message = "Quiz not found" });

            quiz.IsLocked = !quiz.IsLocked;
            await _dbContext.SaveChangesAsync();

            return Ok(new { success = true, message = $"Quiz is {(quiz.IsLocked ? "locked" : "unlocked")} successfully" });
        }

        // GET: /api/quiz/join/{id}
        [HttpGet("join/{id}")]
        public async Task<IActionResult> JoinQuiz(Guid id)
        {
            // First check if quiz exists and is locked
            var quizLockCheck = await _dbContext.Quizzes
                .Where(q => q.Id == id)
                .Select(q => new { q.IsLocked })
                .FirstOrDefaultAsync();

            if (quizLockCheck == null)
                return NotFound(new { success = false, message = "Quiz not found" });

            if (quizLockCheck.IsLocked)
                return BadRequest(new { success = false, message = "This quiz is locked" });

            // Get user ID from context
            var userId = HttpContext.Items["UserId"]?.ToString();
            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "User not found in context" });
            }

            // Check if user has already attempted this quiz
            var existingAttempt = await _dbContext.Attempts
                .AnyAsync(a => a.QuizId == id && a.PlayerId == Guid.Parse(userId));

            if (existingAttempt)
            {
                return BadRequest(new { success = false, message = "You have already attempted this quiz" });
            }

            // If no existing attempt, proceed with the original logic
            var quiz = await _dbContext.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null)
                return NotFound(new { success = false, message = "Quiz not found" });

            var quizResponse = new
            {
                quiz.Id,
                quiz.Title,
                quiz.Duration,
                quiz.AuthorId,
                quiz.Author.Name,
                quiz.IsLocked,
                Questions = quiz.Questions.Select(q => new
                {
                    q.Id,
                    q.Title,
                    q.Options,
                    NumberOfAnswers = q.Answers.Count
                })
            };

            return Ok(new { success = true, quiz = quizResponse });
        }


        // POST: /api/quiz/attempt
        [HttpPost("attempt")]
        public async Task<IActionResult> AttemptQuiz([FromBody] AttemptQuizRequest request)
        {
            var quiz = await _dbContext.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == request.QuizId);

            if (quiz == null)
                return NotFound(new { success = false, message = "Quiz not found" });

            if (quiz.IsLocked)
                return BadRequest(new { success = false, message = "Quiz is locked" });

            double totalScore = 0;

            var attemptedQuestions = request.Questions.Select(attempt =>
            {
                var question = quiz.Questions.FirstOrDefault(q => q.Id == attempt.QuestionId);

                if (question == null) return null;

                var correctCount = attempt.Answers.Intersect(question.Answers).Count();
                var incorrectCount = attempt.Answers.Except(question.Answers).Count();

                var score = (correctCount * 4) - (incorrectCount * 1); // Example scoring logic
                totalScore += score;
                string ans = "";
                if (correctCount > 0)
                {
                    if(correctCount==question.Answers.Count && incorrectCount == 0)
                    {
                        ans = "True";
                    }
                    else
                    {
                        ans = "Partial";
                    }
                }
                else
                {
                    ans = "False";
                }
                return new AttemptedQuestion
                {
                    QuestionId = question.Id,
                    Answers = attempt.Answers,
                    IsCorrect = ans,
                    Score = score
                };
            }).Where(aq => aq != null).ToList();


            var userId = HttpContext.Items["UserId"]?.ToString();
            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "User not found in context" });
            }

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));
            if (user == null)
            {
                return Unauthorized(new { success = false, message = "User not found in the database" });
            }
            var attemptRecord = new Attempt
            {
                PlayerId = user.Id,
                QuizId = quiz.Id,
                TotalScore = totalScore,
                Questions = attemptedQuestions
            };

            await _dbContext.Attempts.AddAsync(attemptRecord);
            await _dbContext.SaveChangesAsync();

            return Ok(new { success = true, resultId = attemptRecord.Id });
        }

        // POST: /api/quiz/feedback/{id}
        [HttpPost("feedback/{id}")]
        public async Task<IActionResult> FeedbackQuiz(Guid id, [FromBody] FeedbackRequest request)
        {
            var attempt = await _dbContext.Attempts.FindAsync(id);

            if (attempt == null)
                return NotFound(new { success = false, message = "Attempt not found" });

            var feedback = new Feedback
            {
                AttemptId = attempt.Id,
                Rating = request.Rating,
                Message = request.Message
            };

            await _dbContext.Feedbacks.AddAsync(feedback);
            await _dbContext.SaveChangesAsync();

            return Ok(new { success = true, message = "Feedback submitted successfully" });
        }

        // GET: /api/quiz/result/{id}
        [HttpGet("result/{id}")]
        public async Task<IActionResult> FetchResult(Guid id)
        {
            var attempt = await _dbContext.Attempts
                .Include(a => a.Questions)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attempt == null)
                return NotFound(new { success = false, message = "Result not found" });

            return Ok(new { success = true, result = attempt });
        }

        // GET: /api/quiz/review-result/{id}
        [HttpGet("review-result/{id}")]
        public async Task<IActionResult> FetchReviewResult(Guid id)
        {
            var attempt = await _dbContext.Attempts
                .Include(a => a.Questions)
                .ThenInclude(q => q.Question)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attempt == null)
                return NotFound(new { success = false, message = "Review result not found" });

            return Ok(new { success = true, result = attempt });
        }

        // GET: /api/quiz/summary/{id}
        [HttpGet("summary/{id}")]
        public async Task<IActionResult> FetchSummary(Guid id)
        {
            var attempts = await _dbContext.Attempts
                .Where(a => a.QuizId == id)
                .Include(a => a.Feedback)
                .Include(a => a.Player)
                .ToListAsync();

            var totalRatings = attempts.Sum(a => a.Feedback?.Rating ?? 0);
            var averageRating = attempts.Any() ? totalRatings / (double)attempts.Count : 0;

            // Create rating counts dictionary similar to Node.js version
            var ratingCounts = new Dictionary<int, int>
            {
                { 1, 0 },
                { 2, 0 },
                { 3, 0 },
                { 4, 0 },
                { 5, 0 }
            };

            // Count ratings
            foreach (var attempt in attempts)
            {
                if (attempt.Feedback?.Rating != null)
                {
                    ratingCounts[attempt.Feedback.Rating]++;
                }
            }

            var feedbackMessages = attempts
                .Where(a => a.Feedback != null)
                .Select(a => new
                {
                    a.Feedback.Message,
                    User = a.Player.Name
                });

            return Ok(new
            {
                success = true,
                averageRating = Math.Round(averageRating, 1),
                starCounts = ratingCounts,
                feedbackMessages
            });
        }


        // GET: /api/quiz/leaderboard/{id}
        [HttpGet("leaderboard/{id}")]
        public async Task<IActionResult> FetchLeaderboard(Guid id, int page = 1, int limit = 10)
        {
            // First get the quiz to check if it exists and get its locked status
            var quiz = await _dbContext.Quizzes
                .Where(q => q.Id == id)
                .Select(q => q.IsLocked)
                .FirstOrDefaultAsync();

            if (quiz == null)
            {
                return NotFound(new { success = false, message = "Quiz not found" });
            }

            var leaderboard = await _dbContext.Attempts
                .Where(a => a.QuizId == id)
                .OrderByDescending(a => a.TotalScore)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(a => new
                {
                    PlayerName = a.Player.Name,
                    a.TotalScore
                })
                .ToListAsync();

            var totalAttempts = await _dbContext.Attempts.CountAsync(a => a.QuizId == id);
            var totalPages = (int)Math.Ceiling((double)totalAttempts / limit);

            return Ok(new
            {
                success = true,
                leaderboard,
                totalPages,
                currentPage = page,
                isLocked = quiz // Include the isLocked status
            });
        }


        // GET: /api/quiz/my-quizzes
        [HttpGet("my-quizzes")]
        public async Task<IActionResult> FetchMyQuizzes([FromQuery] string? search)
        {
            var userId = HttpContext.Items["UserId"]?.ToString();
            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "User not found in context" });
            }

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));
            if (user == null)
            {
                return Unauthorized(new { success = false, message = "User not found in the database" });
            }

            var query = _dbContext.Quizzes
                .Include(q => q.Author)
                .Where(q => q.AuthorId == user.Id);

            // Apply search filter if search parameter is provided
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(q => q.Title.ToLower().Contains(search.ToLower()));
            }

            var quizzes = await query
                .Select(q => new
                {
                    id = q.Id,
                    title = q.Title,
                    authorName = q.Author.Name,
                    duration = q.Duration,
                    isLocked = q.IsLocked,
                    createdAt = q.CreatedAt,
                    updatedAt = q.UpdatedAt,
                    questionCount = q.Questions.Count
                })
                .ToListAsync();

            return Ok(new { success = true, quizzes });
        }



        // GET: /api/quiz/attempted-quizzes
        [HttpGet("attempted-quizzes")]
        public async Task<IActionResult> FetchAttemptedQuizzes([FromQuery] string? search = null)
        {
            var userId = HttpContext.Items["UserId"]?.ToString();
            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "User not found in context" });
            }

            var userGuid = Guid.Parse(userId);

            var query = _dbContext.Attempts
                .Where(a => a.PlayerId == userGuid)
                .Select(a => new
                {
                    a.Id,
                    a.TotalScore,
                    a.CreatedAt,
                    Quiz = new
                    {
                        a.Quiz.Id,
                        isLocked = a.Quiz.IsLocked,
                        a.Quiz.Title,
                        a.Quiz.Duration,
                        Author = new
                        {
                            a.Quiz.Author.Id,
                            a.Quiz.Author.Name
                        }
                    }
                });

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(a => a.Quiz.Title.ToLower().Contains(search));
            }

            var attempts = await query
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return Ok(new
            {
                success = true,
                attempts
            });
        }


        // GET: /api/quiz/nodes
        [HttpGet("nodes")]
        public async Task<IActionResult> GetQuizNodes(double? longitude, double? latitude)
        {
            Console.WriteLine("Fetching quiz nodes...");

            // 1. Handle Missing Parameters (Two Options):

            // Option A: Return all nodes if parameters are missing
            if (!longitude.HasValue || !latitude.HasValue)
            {
                var allNodes = await _dbContext.QuizNodes
                .Include(node => node.Quizzes)
                    .ThenInclude(quiz => quiz.Questions) // Ensure Author is loaded
                .ToListAsync();

                var allNodesResponse = allNodes.Select(node => new
                {
                    _id = node.Id,
                    longitude = node.Location.Coordinates[0],
                    latitude = node.Location.Coordinates[1],
                    quizzes = node.Quizzes.Select(quiz => new
                    {
                        _id = quiz.Id,
                        title = quiz.Title,
                        duration = quiz.Duration,
                        creator = quiz.AuthorId,
                        creatorName = quiz.Author != null ? quiz.Author.Name : "Unknown",  // Ensure null safety
                        isLocked = quiz.IsLocked,
                        totalQuestions = quiz.Questions.Count
                    }).ToList()
                });

                return Ok(new { success = true, message = "option 1", count = allNodes.Count, nodes = allNodesResponse });

            }

            // Option B: Return BadRequest if parameters are missing (If required)
            //if (!longitude.HasValue || !latitude.HasValue)
            //{
            //    return BadRequest(new { success = false, message = "Longitude and Latitude are required" });
            //}


            // 2. Query with Parameters (Only if parameters are provided)
            var nodes = await _dbContext.QuizNodes
                .Where(node =>
                    node.Location.Coordinates[0] == longitude.Value && // Now safe to use .Value
                    node.Location.Coordinates[1] == latitude.Value)
                .Include(node => node.Quizzes)
                .ThenInclude(quiz => quiz.Author)
                .ToListAsync();

            var response = nodes.Select(node => new // Transform nodes (same as before)
            {
                _id = node.Id,
                longitude = node.Location.Coordinates[0],
                latitude = node.Location.Coordinates[1],
                quizzes = node.Quizzes.Select(quiz => new
                {
                    _id = quiz.Id,
                    title = quiz.Title,
                    creator = quiz.AuthorId,
                    isLocked = quiz.IsLocked
                }).ToList()
            });

            return Ok(new { success = true, message = "option2", count = nodes.Count, nodes = response });
        }


        // POST: /api/quiz/nodes
        [HttpPost("nodes")]
        public async Task<IActionResult> CreateQuizNode([FromBody] CreateQuizNodeRequest request)
        {
            Console.WriteLine($"Received request: Longitude={request.Longitude}, Latitude={request.Latitude}, QuizIds={request.QuizId}");

            if (!request.Longitude.HasValue || !request.Latitude.HasValue)
                return BadRequest(new { success = false, message = "Longitude and latitude are required" });

            if (string.IsNullOrEmpty(request.QuizId) || !Guid.TryParse(request.QuizId, out Guid quizId))
            {
                return BadRequest(new { success = false, message = "Invalid QuizId format" });
            }

            Console.WriteLine($"Searching for Quiz with ID: {quizId}");

            var quiz = await _dbContext.Quizzes.FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
            {
                Console.WriteLine($"Quiz with ID {quizId} not found in database.");
                return NotFound(new { success = false, message = "Quiz not found" });
            }

            var node = new QuizNode
            {
                Location = new Location
                {
                    Coordinates = new List<double> { request.Longitude.Value, request.Latitude.Value }
                },
                Quizzes = new List<Quiz> { quiz }
            };

            await _dbContext.QuizNodes.AddAsync(node);
            await _dbContext.SaveChangesAsync();

            return Ok(new { success = true, node });
        }




        // PATCH: /api/quiz/nodes/{nodeId}/quiz/{quizId}
        [HttpPatch("nodes/{nodeId}/quiz/{quizId}")]
        public async Task<IActionResult> AddQuizToNode(Guid nodeId, Guid quizId)
        {
            var node = await _dbContext.QuizNodes.Include(n => n.Quizzes).FirstOrDefaultAsync(n => n.Id == nodeId);
            if (node == null)
                return NotFound(new { success = false, message = "Node not found" });

            var quiz = await _dbContext.Quizzes.FindAsync(quizId);
            if (quiz == null)
                return NotFound(new { success = false, message = "Quiz not found" });

            if (!node.Quizzes.Contains(quiz))
            {
                node.Quizzes.Add(quiz);
                await _dbContext.SaveChangesAsync();
            }

            return Ok(new { success = true, node });
        }

        // DELETE: /api/quiz/nodes/{nodeId}/quiz/{quizId}
        [HttpDelete("nodes/{nodeId}/quiz/{quizId}")]
        public async Task<IActionResult> RemoveQuizFromNode(Guid nodeId, Guid quizId)
        {
            var node = await _dbContext.QuizNodes.Include(n => n.Quizzes).FirstOrDefaultAsync(n => n.Id == nodeId);
            if (node == null)
                return NotFound(new { success = false, message = "Node not found" });

            var quiz = node.Quizzes.FirstOrDefault(q => q.Id == quizId);
            if (quiz != null)
            {
                node.Quizzes.Remove(quiz);
                await _dbContext.SaveChangesAsync();
            }

            return Ok(new { success = true, node });
        }

        // Helper method to validate questions
        private bool IsValidQuestion(QuestionRequest question)
        {
            return !string.IsNullOrWhiteSpace(question.Title) &&
                   question.Options != null && question.Options.Count >= 2 &&
                   question.Answers != null && question.Answers.Count >= 1 &&
                   question.Answers.All(a => a >= 0 && a < question.Options.Count);
        }
    }

    // Request models
    public class CreateQuizRequest
    {
        public string Title { get; set; }
        
        public List<QuestionRequest> Questions { get; set; }
        public int Duration { get; set; }
    }

    public class QuestionRequest
    {
        public string Title { get; set; }
        public List<string> Options { get; set; }
        public List<int> Answers { get; set; }
    }

    public class LockQuizRequest
    {
        public Guid Id { get; set; }
    }

    public class AttemptQuizRequest
    {
        public Guid QuizId { get; set; }
        public List<QuestionAttempt> Questions { get; set; }
    }

    public class QuestionAttempt
    {
        public Guid QuestionId { get; set; }
        public List<int> Answers { get; set; }
    }

    public class FeedbackRequest
    {
        public int Rating { get; set; }
        public string Message { get; set; }
    }

    public class CreateQuizNodeRequest
    {
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
        public string QuizId { get; set; }
    }
}
