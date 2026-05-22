using System;

namespace yhctapp.Model.DTO
{
    public class DashboardStatVM
    {
        public string Id { get; set; } = string.Empty;
        public string Room { get; set; } = string.Empty;
        public int ExpiredCount { get; set; }
        public int SoonToExpireCount { get; set; }
        public int TotalCount { get; set; }
    }
}
