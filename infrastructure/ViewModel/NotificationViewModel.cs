using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure.ViewModel
{
    public class NotificationViewModel
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedDate { get; set; }
        public string FormattedDate => CreatedDate.ToString("yyyy-MM-dd HH:mm");
        public int Type { get; set; }
        public string RelatedMovieId { get; set; }
    }

    public class NotificationCountViewModel 
    {

        public int UnreadCount { get; set; }
        public List<NotificationViewModel> RecentNotifications { get; set; }
    }
}
