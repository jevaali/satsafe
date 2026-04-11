namespace WebApp.Areas.Admin.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalSatellites     { get; set; }
    public int ActiveSatellites    { get; set; }
    public int TotalOperators      { get; set; }
    public int TotalConjunctions   { get; set; }
    public int HighRiskEvents      { get; set; }
    public int PendingEvents       { get; set; }
    public int TotalAlerts         { get; set; }
    public int UnacknowledgedAlerts{ get; set; }
    public int TotalUsers          { get; set; }
    public int TotalTleRecords     { get; set; }

    public IEnumerable<RecentEventRow> RecentEvents { get; set; } = [];
    public IEnumerable<RecentAlertRow> RecentAlerts { get; set; } = [];

    public class RecentEventRow
    {
        public Guid   Id               { get; set; }
        public string PrimaryObject    { get; set; } = string.Empty;
        public string SecondaryObject  { get; set; } = string.Empty;
        public DateTime Tca            { get; set; }
        public double MissDistance     { get; set; }
        public string Status           { get; set; } = string.Empty;
        public bool   IsHighRisk       { get; set; }
    }

    public class RecentAlertRow
    {
        public Guid   Id             { get; set; }
        public string Severity       { get; set; } = string.Empty;
        public string Message        { get; set; } = string.Empty;
        public bool   IsAcknowledged { get; set; }
        public DateTime CreatedAt    { get; set; }
    }
}
