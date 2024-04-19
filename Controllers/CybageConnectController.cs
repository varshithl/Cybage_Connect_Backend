using Cybage_Connect.Data;
using Cybage_Connect.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.VisualBasic;
using System.Collections.Generic;

namespace Cybage_Connect.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CybageConnectController : ControllerBase
    {

        private readonly JWTServices _jwtService;
        private readonly CybageConnectDbContext cybageConnectDb;

        public CybageConnectController(JWTServices jwtService, CybageConnectDbContext dbContext)
        {
            _jwtService = jwtService;
            cybageConnectDb = dbContext;
        }


        [HttpGet("SearchForUser")]
        public IEnumerable<UserRegistration> SearchForUser(string username)
        {
            IEnumerable<UserRegistration> users = cybageConnectDb.UserRegistrations.Where(e=>e.UserName!=username).ToList();
            return users;
        }


        [HttpGet("GetDetails")]
        [Authorize]
        public IEnumerable<UserRegistration> Users(string UserName)
        {
            IEnumerable<UserRegistration> users = cybageConnectDb.UserRegistrations.Where(e => e.UserName == UserName).ToList();
            return users;
        }


        [HttpPut("EditDetails")]
        [Authorize]
        public string UpdateDetails([FromBody] UpdateUserDetailsModel updateUserDetailsModel)
        {
            try
            {
                bool EmailExists = cybageConnectDb.UserRegistrations.Any(u => u.Email == updateUserDetailsModel.Email && u.UserName!=updateUserDetailsModel.UserName);
                bool PhoneNumberExists = cybageConnectDb.UserRegistrations.Any(u => u.MobileNumber == updateUserDetailsModel.MobileNumber && u.UserName != updateUserDetailsModel.UserName);
                if (EmailExists)
                {
                    return "Email already Taken";
                }
                else if (PhoneNumberExists)
                {
                    return "Phone Number already Taken";
                }
                var user = cybageConnectDb.UserRegistrations.SingleOrDefault(e => e.UserName == updateUserDetailsModel.UserName);
                if (user == null)
                {
                    return "No such user exits";
                }
                user.FullName = updateUserDetailsModel.FullName;
                user.UserPassword = updateUserDetailsModel.UserPassword;
                user.Email = updateUserDetailsModel.Email;
                user.MobileNumber = updateUserDetailsModel.MobileNumber;
                user.Designation=updateUserDetailsModel.Designation;

                cybageConnectDb.UserRegistrations.Update(user);
                cybageConnectDb.SaveChanges();

                return "success";
            }
            catch (Exception ex)
            {
                return "failed";
            }
        }


        
        [HttpPost("Register")]
        public string Register([FromBody] RegistrationModel Registrations)
        {
            bool usernameExists = cybageConnectDb.UserRegistrations.Any(u => u.UserName == Registrations.UserName);
            bool EmailExists = cybageConnectDb.UserRegistrations.Any(u => u.Email == Registrations.Email);
            bool PhoneNumberExists = cybageConnectDb.UserRegistrations.Any(u => u.MobileNumber == Registrations.MobileNumber);
            if (usernameExists)
            {
                return "UserName already Taken";
            }
            else if (EmailExists)
            {
                return "Email already Taken";
            }
            else if (PhoneNumberExists)
            {
                return "Phone Number already Taken";
            }

            else if (Registrations.UserPassword.Length != 0 &&
                Registrations.UserName.Length != 0 &&
                Registrations.Email.Length != 0 &&
                Registrations.MobileNumber.ToString().Length == 10 &&
                Registrations.FullName.Length != 0 && Registrations.Designation.Length!=0)
            {
                UserRegistration userRegistration = new UserRegistration()
                {
                    FullName = Registrations.FullName,
                    UserName = Registrations.UserName,
                    UserPassword = Registrations.UserPassword,
                    Email = Registrations.Email,
                    MobileNumber = Registrations.MobileNumber,
                    Designation=Registrations.Designation
                };

                cybageConnectDb.UserRegistrations.Add(userRegistration);
                cybageConnectDb.SaveChanges();

                return "success";
            }
            else
            {
                return "Invalid registration data";
            }
        }
  


        [HttpPost("Login", Name = "Loginuser")]

        public IActionResult Login([FromBody] LoginModel model)
        {
            var user = cybageConnectDb.UserRegistrations.FirstOrDefault(u => u.UserName == model.UserName && u.UserPassword == model.UserPassword);
            if (user == null)
                return Unauthorized();

            var token = _jwtService.GenerateToken(user);
            return Ok(new { Token = token });
        }



        [HttpGet("MyFriends")]
        [Authorize]
        public IEnumerable<FriendsListModel> MyFriends(string username)
        {
            var userId = cybageConnectDb.UserRegistrations.FirstOrDefault(e => e.UserName == username);
            var friendRequests = cybageConnectDb.RequestStorages
                .Where(e => (e.RequestReceiverId == userId.UserId || e.RequestSenderId == userId.UserId)&& e.ConnectionStatus == "Accepted")
                .ToList();
            List<FriendsListModel> friends = new List<FriendsListModel>();
            foreach (var friendRequest in friendRequests)
            {
                int friendId = Convert.ToInt32(friendRequest.RequestSenderId == userId.UserId
                                ? friendRequest.RequestReceiverId
                                : friendRequest.RequestSenderId);

                var friend = cybageConnectDb.UserRegistrations.FirstOrDefault(e => e.UserId == friendId);
                if (friend != null)
                {
                    var friendsData = new FriendsListModel()
                    {
                        UserName = friend.UserName,
                        UserId = friend.UserId,
                        Email = friend.Email,
                        FullName = friend.FullName,
                        Designation=friend.Designation,
                    };
                    friends.Add(friendsData);
                }
            }
            return friends;
        }


        //networking
        [HttpDelete("DeleteConnection")]
        [Authorize]
        public bool DeleteConnection(string username, int FriendId)
        {
            var from = cybageConnectDb.UserRegistrations.FirstOrDefault(rec => rec.UserName == username);
            int count = cybageConnectDb.Database.ExecuteSqlRaw("EXEC deleteConnection @fromUserId, @toUserId",
                new SqlParameter("@fromUserId", from.UserId),
                new SqlParameter("@toUserId", FriendId));
            return true;
        }


        //notification
        [HttpPost("SendRequest")]
        
        public bool sendRequest(string fromUser, string toUser)
        {
            var from = cybageConnectDb.UserRegistrations.FirstOrDefault(rec => rec.UserName == fromUser);
            var to = cybageConnectDb.UserRegistrations.FirstOrDefault(rec => rec.UserName == toUser);
            var count = cybageConnectDb.Database.ExecuteSqlRaw("EXEC sendRequest @fromUserId, @toUserId",
             new SqlParameter("@fromUserId", from.UserId),
             new SqlParameter("@toUserId", to.UserId));
            return true;
        }
        //pending cancle
        [HttpDelete("DeleteRequest")]
        public bool DeleteRequest(string username, int FriendId)
        {
            var from = cybageConnectDb.UserRegistrations.FirstOrDefault(rec => rec.UserName == username);
            var DelRequest = cybageConnectDb.RequestStorages.FirstOrDefault(e => e.RequestSenderId == from.UserId && e.RequestReceiverId == FriendId);
            var DelNotification = cybageConnectDb.FriendRequests.FirstOrDefault(e => e.SenderId == from.UserId && e.ReceiverId == FriendId);
            if (DelRequest != null && DelNotification!=null)
            {
                cybageConnectDb.RequestStorages.Remove(DelRequest);
                cybageConnectDb.FriendRequests.Remove(DelNotification);
                cybageConnectDb.SaveChanges();
                return true;
            }
            return false;
        }


        [HttpGet("Notifications")]
       
        public IEnumerable<FriendRequest> Notification(string username)
        {
            try
            {
                var user = cybageConnectDb.UserRegistrations.FirstOrDefault(rec => rec.UserName == username);

                if (user != null)
                {
                    var NotificationOfUser = cybageConnectDb.FriendRequests.Where(rec => rec.ReceiverId == user.UserId).ToList();
                    return NotificationOfUser;
                }
                else
                {
                    throw new Exception("User not found");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching notifications", ex);
            }
        }




        [HttpPost("AcceptRequest")]
  
        public void AcceptRequest(string toUser, int fromUserId)
        {
            try
            {
                var to = cybageConnectDb.UserRegistrations.FirstOrDefault(rec => rec.UserName == toUser);
                cybageConnectDb.Database.ExecuteSqlRaw("EXEC OnAccept @fromUserId, @toUserId",
                    new SqlParameter("@fromUserId", to.UserId),
                    new SqlParameter("@toUserId", fromUserId));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }


        [HttpPost("DeclineRequest")]
      
        public bool DeclineRequest(int toUser, string fromUser)
        {
            bool status = false;
            var from = cybageConnectDb.UserRegistrations.FirstOrDefault(rec => rec.UserName == fromUser);
            int count = cybageConnectDb.Database.ExecuteSqlRaw("EXEC OnDelete @fromUserId, @toUserId",
                new SqlParameter("@fromUserId", from.UserId),
                new SqlParameter("@toUserId", toUser));
            if (count > 0)
            {
                status = true;
            }
            return status;
        }






        [HttpGet("Article")]
        [Authorize]
        public IEnumerable<ArticlesOfUser> Article(string UserName)
        {
            var articlesOfUser = from articles in cybageConnectDb.ArticlesOfUsers where articles.UserName == UserName select articles;
            return articlesOfUser.ToList();
        }
        [HttpGet("Blog")]
        [Authorize]
        public IEnumerable<BlogsOfUser> Blog(string UserName)
        {
            var articlesOfUser = from blogs in cybageConnectDb.BlogsOfUsers where blogs.UserName == UserName select blogs;
            return articlesOfUser.ToList();
        }
        [HttpGet("ProjectInsight")]
        [Authorize]
        public IEnumerable<ProjectInsightsOfUser> ProjectInsight(string UserName)
        {
            var articlesOfUser = from projectInsights in cybageConnectDb.ProjectInsightsOfUsers where projectInsights.UserName == UserName select projectInsights;
            return articlesOfUser.ToList();
        }



        [HttpDelete("ArticleDelete")]
        [Authorize]
        public bool ArticleDelete(int id)
        {
            var articleToRemove = cybageConnectDb.ArticlesOfUsers.FirstOrDefault(article => article.ArticleId == id);

            if (articleToRemove != null)
            {
                var likes = cybageConnectDb.Likes.Where(e => e.ArticleId == articleToRemove.ArticleId);
                foreach (var lik in likes)
                {
                    cybageConnectDb.Likes.Remove(lik);
                }
                var comments = cybageConnectDb.Comments.Where(e => e.ArticleId == articleToRemove.ArticleId);
                foreach (var com in comments)
                {
                    cybageConnectDb.Comments.Remove(com);
                }
                cybageConnectDb.ArticlesOfUsers.Remove(articleToRemove);
                cybageConnectDb.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }
        [HttpDelete("BlogDelete")]
        [Authorize]
        public bool BlogDelete(int id)
        {
            var blogToRemove = cybageConnectDb.BlogsOfUsers.FirstOrDefault(blog => blog.BlogId == id);

            if (blogToRemove != null)
            {
                var likes = cybageConnectDb.Likes.Where(e => e.BlogId == blogToRemove.BlogId);
                foreach (var lik in likes)
                {
                    cybageConnectDb.Likes.Remove(lik);
                }
                var comments = cybageConnectDb.Comments.Where(e => e.BlogId == blogToRemove.BlogId);
                foreach (var com in comments)
                {
                    cybageConnectDb.Comments.Remove(com);
                }
                cybageConnectDb.BlogsOfUsers.Remove(blogToRemove);
                cybageConnectDb.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }
        [HttpDelete("ProjectInsightsDelete")]
        [Authorize]
        public bool ProjectInsightsDelete(int id)
        {
            var projectInsightToRemove = cybageConnectDb.ProjectInsightsOfUsers.FirstOrDefault(project => project.ProjectInsightId == id);

            if (projectInsightToRemove != null)
            {
                var likes = cybageConnectDb.Likes.Where(e => e.ProjectInsightId == projectInsightToRemove.ProjectInsightId);
                foreach (var lik in likes)
                {
                    cybageConnectDb.Likes.Remove(lik);
                }
                var comments = cybageConnectDb.Comments.Where(e => e.ProjectInsightId == projectInsightToRemove.ProjectInsightId);
                foreach (var com in comments)
                {
                    cybageConnectDb.Comments.Remove(com);
                }
                cybageConnectDb.ProjectInsightsOfUsers.Remove(projectInsightToRemove);
                cybageConnectDb.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }




        [HttpPost("AddArticle")]
        [Authorize]
        public void AddArticle(AddArticleModel newArticle)
        {
            var id = cybageConnectDb.UserRegistrations.FirstOrDefault(e => e.UserName == newArticle.UserName);
            int userId = id.UserId;
            ArticlesOfUser articlesOfUser = new ArticlesOfUser()
            {
                ArticleTitle = newArticle.ArticleTitle,
                Content=newArticle.Content,
                UserId = userId,
                UserName = newArticle.UserName,
                PublishedDate = DateTime.Now
            };
            cybageConnectDb.ArticlesOfUsers.Add(articlesOfUser);
            cybageConnectDb.SaveChanges();
        }
        [HttpPost("AddBlog")]
        [Authorize]
        public void AddBlog(AddBolgModel newBlog)
        {
            var id = cybageConnectDb.UserRegistrations.FirstOrDefault(e => e.UserName == newBlog.UserName);
            int userId = id.UserId;
            BlogsOfUser blogsOfUser = new BlogsOfUser()
            {
                BlogTitle = newBlog.BlogTitle,
                Content=newBlog.Content,
                UserId = userId,
                UserName = newBlog.UserName,
                PublishedDateOfBlog = DateTime.Now
            };
            cybageConnectDb.BlogsOfUsers.Add(blogsOfUser);
            cybageConnectDb.SaveChanges();

        }
        [HttpPost("AddProject")]
        [Authorize]
        public void AddProject(AddProjectInsightModel newProjectInsight)
        {
            var id = cybageConnectDb.UserRegistrations.FirstOrDefault(e => e.UserName == newProjectInsight.UserName);
            int userId = id.UserId;
            ProjectInsightsOfUser projectInsightOfUser = new ProjectInsightsOfUser()
            {
                ProjectTitle = newProjectInsight.ProjectTitle,
                UserId = userId,
                UserName = newProjectInsight.UserName,
                ProjectDescription=newProjectInsight.ProjectDescription,
                Duration=newProjectInsight.Duration,
                Tools=newProjectInsight.Tools,
                PublishedDateOfProjectInsight = DateTime.Now,
            };
            cybageConnectDb.ProjectInsightsOfUsers.Add(projectInsightOfUser);
            cybageConnectDb.SaveChanges();

        }


        [HttpGet("GetArticleById")]
        [Authorize]
        public ArticlesOfUser GetArticleById(int articleId)
        {
            ArticlesOfUser users = cybageConnectDb.ArticlesOfUsers.SingleOrDefault(e => e.ArticleId == articleId);
            return users;
        }

        [HttpPut("EditArticle")]
        [Authorize]
        public bool UpdateArticle([FromBody] EditArticleModel editArticleModel)
        {
            try
            {
                var user = cybageConnectDb.ArticlesOfUsers.SingleOrDefault(e => e.ArticleId == editArticleModel.ArticleId);
                if (user == null)
                {
                    return false;
                }
                user.ArticleTitle = editArticleModel.ArticleTitle;
                user.Content=editArticleModel.Content;
                user.PublishedDate = DateTime.Now;

                cybageConnectDb.ArticlesOfUsers.Update(user);
                cybageConnectDb.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        [HttpGet("GetProjectInsightsById")]
        [Authorize]
        public ProjectInsightsOfUser GetProjectInsightsById(int projectInsightId)
        {
            ProjectInsightsOfUser users = cybageConnectDb.ProjectInsightsOfUsers.SingleOrDefault(e => e.ProjectInsightId == projectInsightId);
            return users;
        }

        [HttpPut("EditProjectInsight")]
        [Authorize]
        public bool UpdateProjectInsight([FromBody] EditProjectModel editProjectModel)
        {
            try
            {
                var user = cybageConnectDb.ProjectInsightsOfUsers.SingleOrDefault(e => e.ProjectInsightId == editProjectModel.ProjectInsightId);
                if (user == null)
                {
                    return false;
                }
                user.ProjectTitle = editProjectModel.ProjectTitle;
                user.ProjectDescription = editProjectModel.ProjectDescription;
                user.Duration = editProjectModel.Duration;
                user.Tools = editProjectModel.Tools;
                user.PublishedDateOfProjectInsight = DateTime.Now;

                cybageConnectDb.ProjectInsightsOfUsers.Update(user);
                cybageConnectDb.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        [HttpGet("GetBlogById")]
        [Authorize]
        public BlogsOfUser GetBlogById(int blogId)
        {
            BlogsOfUser users = cybageConnectDb.BlogsOfUsers.SingleOrDefault(e => e.BlogId == blogId);
            return users;
        }

        [HttpPut("EditBlog")]
        [Authorize]
        public bool UpdateBlog([FromBody] EditBlogModel editBlogModel)
        {
            try
            {
                var user = cybageConnectDb.BlogsOfUsers.SingleOrDefault(e => e.BlogId == editBlogModel.BlogId);
                if (user == null)
                {
                    return false;
                }
                user.BlogTitle = editBlogModel.BlogTitle;
                user.Content = editBlogModel.Content;
                user.PublishedDateOfBlog = DateTime.Now;

                cybageConnectDb.BlogsOfUsers.Update(user);
                cybageConnectDb.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }




        [HttpGet("FriendsArticles")]
        [Authorize]
        public IEnumerable<ArticlesOfUser> FriendsArticles(string username)
        {
            var userId = cybageConnectDb.UserRegistrations.FirstOrDefault(e => e.UserName == username);
            var friendRequests = cybageConnectDb.RequestStorages
                .Where(e => (e.RequestReceiverId == userId.UserId || e.RequestSenderId == userId.UserId) && e.ConnectionStatus == "Accepted")
                .ToList();

            List<ArticlesOfUser> articles = new List<ArticlesOfUser>();
            foreach (var friendRequest in friendRequests)
            {
                int friendId = Convert.ToInt32(friendRequest.RequestSenderId == userId.UserId ? friendRequest.RequestReceiverId : friendRequest.RequestSenderId);

                var friendArticles = cybageConnectDb.ArticlesOfUsers
                    .Where(e => e.UserId == friendId)
                    .ToList();
                articles.AddRange(friendArticles);
            }

            return articles;
        }
        [HttpGet("FriendsBlogs")]
        [Authorize]
        public IEnumerable<BlogsOfUser> FriendsBlogs(string username)
        {
            var userId = cybageConnectDb.UserRegistrations.FirstOrDefault(e => e.UserName == username);
            var friendRequests = cybageConnectDb.RequestStorages
                .Where(e => (e.RequestReceiverId == userId.UserId || e.RequestSenderId == userId.UserId) && e.ConnectionStatus == "Accepted")
                .ToList();

            List<BlogsOfUser> blogs = new List<BlogsOfUser>();
            foreach (var friendRequest in friendRequests)
            {
                int friendId = Convert.ToInt32(friendRequest.RequestSenderId == userId.UserId ? friendRequest.RequestReceiverId : friendRequest.RequestSenderId);

                var friendBlogs = cybageConnectDb.BlogsOfUsers
                    .Where(e => e.UserId == friendId)
                    .ToList();
                blogs.AddRange(friendBlogs);
            }

            return blogs;
        }
        [HttpGet("FriendsProjectInsights")]
        [Authorize]
        public IEnumerable<ProjectInsightsOfUser> FriendsProjectInsights(string username)
        {
            var userId = cybageConnectDb.UserRegistrations.FirstOrDefault(e => e.UserName == username);
            var friendRequests = cybageConnectDb.RequestStorages
                .Where(e =>( e.RequestReceiverId == userId.UserId || e.RequestSenderId == userId.UserId) && e.ConnectionStatus == "Accepted")
                .ToList();

            List<ProjectInsightsOfUser> projects = new List<ProjectInsightsOfUser>();
            foreach (var friendRequest in friendRequests)
            {
                int friendId = Convert.ToInt32(friendRequest.RequestSenderId == userId.UserId ? friendRequest.RequestReceiverId : friendRequest.RequestSenderId);

                var friendProjects = cybageConnectDb.ProjectInsightsOfUsers
                    .Where(e => e.UserId == friendId)
                    .ToList();
                projects.AddRange(friendProjects);
            }

            return projects;
        }
        [HttpGet("EnableConnection")]
        [Authorize]
        public int AboutConnections(string username, string friendName)
        {
            var userId = cybageConnectDb.UserRegistrations.FirstOrDefault(e => e.UserName == username);
            var friendId = cybageConnectDb.UserRegistrations.FirstOrDefault(e => e.UserName == friendName);
            var acceptedRequest = cybageConnectDb.RequestStorages
                .FirstOrDefault(e => (e.RequestReceiverId == userId.UserId && e.RequestSenderId == friendId.UserId) || (e.RequestSenderId == userId.UserId && e.RequestReceiverId == friendId.UserId) && e.ConnectionStatus == "Accepted");
            var pendingRequest = cybageConnectDb.RequestStorages
                .FirstOrDefault(e => (e.RequestReceiverId == userId.UserId && e.RequestSenderId == friendId.UserId) || (e.RequestSenderId == userId.UserId && e.RequestReceiverId == friendId.UserId) && e.ConnectionStatus == "Pending");
            if (acceptedRequest != null)
            {
                return 1;
            }
            if (pendingRequest != null)
            {
                return 2;
            }
            else
            {
                return 0;
            }
        }


        [HttpGet("LikeList")]

        public async Task<List<LikesList>> LikeList(int ArticleId, int BlogId, int ProjectInsightsId)
        {
            List<LikesList> likedUsers = new List<LikesList>();

            if (ArticleId != 0)
            {
                var likes = await cybageConnectDb.Likes.Where(e => e.ArticleId == ArticleId).ToListAsync();
                foreach (var like in likes)
                {
                    var user = await cybageConnectDb.UserRegistrations.FirstOrDefaultAsync(e => e.UserId == like.LikedById);
                    if (user != null)
                    {
                        likedUsers.Add(new LikesList { userId = user.UserId, userName = user.UserName });
                    }
                }
            }
            else if (BlogId != 0)
            {
                var likes = await cybageConnectDb.Likes.Where(e => e.BlogId == BlogId).ToListAsync();
                foreach (var like in likes)
                {
                    var user = await cybageConnectDb.UserRegistrations.FirstOrDefaultAsync(e => e.UserId == like.LikedById);
                    if (user != null)
                    {
                        likedUsers.Add(new LikesList { userId = user.UserId, userName = user.UserName });
                    }
                }
            }
            else if (ProjectInsightsId != 0)
            {
                var likes = await cybageConnectDb.Likes.Where(e => e.ProjectInsightId == ProjectInsightsId).ToListAsync();
                foreach (var like in likes)
                {
                    var user = await cybageConnectDb.UserRegistrations.FirstOrDefaultAsync(e => e.UserId == like.LikedById);
                    if (user != null)
                    {
                        likedUsers.Add(new LikesList { userId = user.UserId, userName = user.UserName });
                    }
                }
            }

            return likedUsers;
        }


        [HttpGet("knowLikes")]

        public int knowLikes(int ArticleId, int BlogId, int ProjectInsightsId,string UserName)
        {
            var user = cybageConnectDb.UserRegistrations.FirstOrDefault(e => e.UserName == UserName);
            var status = cybageConnectDb.Likes.FirstOrDefault(e => e.ArticleId == ArticleId && e.BlogId == BlogId && e.ProjectInsightId == ProjectInsightsId && e.LikedById == user.UserId);
            if (status != null)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }


        [HttpPost("LikeThePost")]
    
        public void LikeThePost(int ArticleId, int BlogId, int ProjectInsightsId, string UserName)
        {
            try
            {
                var to = cybageConnectDb.UserRegistrations.FirstOrDefault(rec => rec.UserName == UserName);
                cybageConnectDb.Database.ExecuteSqlRaw("EXEC LikeProcedure @articleId, @blogId,@projectId,@userId",
                    new SqlParameter("@articleId", ArticleId),
                    new SqlParameter("@blogId", BlogId),
                    new SqlParameter("@projectId", ProjectInsightsId),
                    new SqlParameter("@userId", to.UserId));
            }
            catch (Exception ex)
            {
                
            }
        }
        
        [HttpPost("UnlikePost")]
        
        public void UnlikePost(int ArticleId, int BlogId, int ProjectInsightsId, string UserName)
        {
            try
            {
                var to = cybageConnectDb.UserRegistrations.FirstOrDefault(rec => rec.UserName == UserName);
                cybageConnectDb.Database.ExecuteSqlRaw("EXEC UnlikeLikeProcedure @articleId, @blogId,@projectId,@userId",
                    new SqlParameter("@articleId", ArticleId),
                    new SqlParameter("@blogId", BlogId),
                    new SqlParameter("@projectId", ProjectInsightsId),
                    new SqlParameter("@userId", to.UserId));
            }
            catch (Exception ex)
            {

            }
        }


        [HttpGet("CommentList")]
        
        public async Task<List<CommentsList>> CommentList(int ArticleId, int BlogId, int ProjectInsightsId)
        {
            List<CommentsList> commentedUsers = new List<CommentsList>();

            if (ArticleId != 0)
            {
                var likes = await cybageConnectDb.Comments.Where(e => e.ArticleId == ArticleId).ToListAsync();
                foreach (var like in likes)
                {
                    var user = await cybageConnectDb.UserRegistrations.FirstOrDefaultAsync(e => e.UserId == like.CommentedById);
                    if (user != null)
                    {
                        commentedUsers.Add(new CommentsList {id=like.Id, userName = user.UserName,comment=like.Comment1 });
                    }
                }
            }
            else if (BlogId != 0)
            {
                var likes = await cybageConnectDb.Comments.Where(e => e.BlogId == BlogId).ToListAsync();
                foreach (var like in likes)
                {
                    var user = await cybageConnectDb.UserRegistrations.FirstOrDefaultAsync(e => e.UserId == like.CommentedById);
                    if (user != null)
                    {
                        commentedUsers.Add(new CommentsList { id = like.Id, userName = user.UserName, comment = like.Comment1 });
                    }
                }
            }
            else if (ProjectInsightsId != 0)
            {
                var likes = await cybageConnectDb.Comments.Where(e => e.ProjectInsightId == ProjectInsightsId).ToListAsync();
                foreach (var like in likes)
                {
                    var user = await cybageConnectDb.UserRegistrations.FirstOrDefaultAsync(e => e.UserId == like.CommentedById);
                    if (user != null)
                    {
                        commentedUsers.Add(new CommentsList { id = like.Id, userName = user.UserName, comment = like.Comment1 });
                    }
                }
            }
            return commentedUsers;
        }

        [HttpPost("CommentThePost")]
      
        public void CommentThePost(int ArticleId, int BlogId, int ProjectInsightsId, string UserName,string comment)
        {
            try
            {
                var to = cybageConnectDb.UserRegistrations.FirstOrDefault(rec => rec.UserName == UserName);
                cybageConnectDb.Database.ExecuteSqlRaw("EXEC CommentProcedure @articleId, @blogId,@projectId,@userId,@comment",
                    new SqlParameter("@articleId", ArticleId),
                    new SqlParameter("@blogId", BlogId),
                    new SqlParameter("@projectId", ProjectInsightsId),
                    new SqlParameter("@userId", to.UserId),
                    new SqlParameter("@comment", comment));
            }
            catch (Exception ex)
            {

            }
        }

        [HttpDelete("DeleteCommentThePost")]
      
        public string DeleteCommentThePost(int id, string username)
        {
            var user = cybageConnectDb.UserRegistrations.FirstOrDefault(e => e.UserName == username);
            if (user == null)
            {
                return "User not found";
            }

            var comment = cybageConnectDb.Comments.FirstOrDefault(e => e.Id == id && e.CommentedById == user.UserId);
            if (comment == null)
            {
                return "only your comment can be delete";
            }


            if (comment.ArticleId != 0)
            {
                var article=cybageConnectDb.ArticlesOfUsers.FirstOrDefault(e=>e.ArticleId==comment.ArticleId);
                article.Comments -= 1; 
                cybageConnectDb.ArticlesOfUsers.Update(article);
            }else if (comment.BlogId != 0)
            {
                var blog = cybageConnectDb.BlogsOfUsers.FirstOrDefault(e => e.BlogId == comment.BlogId);
                blog.Comments -= 1;
                cybageConnectDb.BlogsOfUsers.Update(blog);
            }
            else
            {
                var project = cybageConnectDb.ProjectInsightsOfUsers.FirstOrDefault(e => e.ProjectInsightId == comment.ProjectInsightId);
                project.Comments -= 1;
                cybageConnectDb.ProjectInsightsOfUsers.Update(project);
            }

            cybageConnectDb.Comments.Remove(comment);
            cybageConnectDb.SaveChanges();
            return "Comment deleted successfully";
        }

    }
}
