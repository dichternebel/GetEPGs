using Microsoft.Win32.TaskScheduler;
using System.Diagnostics;

namespace GetEPGs
{
    internal class TaskManager
    {
        static string exePath = Process.GetCurrentProcess().MainModule.FileName;
        static string taskName = "Get EPGs";

        static internal void InstallTask()
        {
            string taskDescription = "Task to run GetEPGs application.";

            using (TaskService ts = new TaskService())
            {
                try
                {
                    TaskDefinition td = ts.NewTask();
                    td.RegistrationInfo.Description = taskDescription;

                    // Create a daily trigger
                    td.Triggers.Add(new DailyTrigger { StartBoundary = DateTime.Today.AddHours(6) }); // Runs daily at 6 AM

                    // Create an action to run your application
                    td.Actions.Add(new ExecAction(exePath, null, null));

                    // Register the task in the root folder
                    ts.RootFolder.RegisterTaskDefinition(taskName, td);

                    $"Task '{taskName}' scheduled successfully.".Info(true);
                }
                catch (UnauthorizedAccessException)
                {
                    "Access denied. Make sure you have sufficient privileges to create scheduled tasks.".Warn(true);
                }
                catch (Exception ex)
                {
                    ex.Message.Error(true);
                }

            }
        }

        static internal void RemoveTask()
        {
            using (TaskService ts = new TaskService())
            {
                try
                {
                    ts.RootFolder.DeleteTask(taskName);
                    $"Task '{taskName}' has been successfully deleted.".Info(true);
                }
                catch (UnauthorizedAccessException)
                {
                    "Access denied. Make sure you have sufficient privileges to create scheduled tasks.".Warn(true);
                }
                catch (Exception ex)
                {
                    ex.Message.Error(true);
                }
            }
        }
    }
}
