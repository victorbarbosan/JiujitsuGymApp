namespace JiujitsuGymApp.Dtos
{
    /// <summary>
    /// What the demo seed currently accounts for in the database. Every count
    /// is derived from the same ownership rule the purge uses, so a zeroed
    /// status is a promise that a purge would delete nothing.
    /// </summary>
    public class DemoDataStatusDto
    {
        public bool IsSeeded { get; set; }
        public int Teachers { get; set; }
        public int Members { get; set; }
        public int Schedules { get; set; }
        public int Classes { get; set; }
        public int Attendances { get; set; }
        public int Products { get; set; }

        /// <summary>Shared sign-in password for every seeded account.</summary>
        public string DemoPassword { get; set; } = string.Empty;

        /// <summary>Email domain that marks an account as demo-owned.</summary>
        public string DemoEmailDomain { get; set; } = string.Empty;
    }
}
