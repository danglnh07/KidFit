using KidFit.Models;

namespace KidFit.Shared.TaskRequests
{
    public class SendWelcomeEmailRequest(ApplicationUser account, string token)
    {
        public string Id { get; set; } = account.Id;
        public string Username { get; set; } = account.UserName!;
        public string Email { get; set; } = account.Email!;
        public string Fullname { get; set; } = account.FullName;
        public string Token { get; set; } = token;
    }
}
