using TurnTracker.Data;

namespace TurnTracker.Domain.Models
{
    public class UserInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Role Role { get; set; }
    }
}