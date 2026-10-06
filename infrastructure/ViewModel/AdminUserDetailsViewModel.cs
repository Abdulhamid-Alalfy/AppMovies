namespace infrastructure.ViewModel
{
    public class AdminUserDetailsViewModel : BaseUserDetailsViewModel
    {
        public List<string> Roles { get; set; } = new List<string>();
    }
}
