using System.Diagnostics.Metrics;

namespace MyApp.Namespace
{
    public static class TaskMetrics
    {
        public static readonly Meter Meter = new Meter("MyApp.Tasks", "1.0.0");
        public static readonly Counter<int> TasksCompleted = Meter.CreateCounter<int>(
            name: "tasks_completed",  // Hyphens aren't ideal, use underscores
            unit: "tasks",
            description: "The total number of tasks completed");
        public static readonly Meter MeterUsers = new Meter("MyApp.Users", "1.0.0");
        public static readonly Counter<int> UsersOnline = MeterUsers.CreateCounter<int>(
            name: "users_online",  // Hyphens aren't ideal, use underscores
            unit: "users",
            description: "The total number of users logged in at the time");
    }
}