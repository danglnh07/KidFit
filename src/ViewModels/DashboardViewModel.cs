namespace KidFit.ViewModels
{
    public class DashboardViewModel
    {
        public string Username { get; set; } = "";
        public string Role { get; set; } = "";
        public string AvatarUrl { get; set; } = "";
        public int TotalUsers { get; set; }
        public int TotalCards { get; set; }
        public int TotalLessons { get; set; }
    }
}
