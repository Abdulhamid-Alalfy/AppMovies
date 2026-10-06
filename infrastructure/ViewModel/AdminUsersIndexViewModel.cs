namespace infrastructure.ViewModel
{
    public class AdminUsersIndexViewModel
    {
        public List<UserRoleViewModel> Users { get; set; } = new();
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int AdminUsers { get; set; }
        public int BlockedUsers { get; set; }
    }
}
