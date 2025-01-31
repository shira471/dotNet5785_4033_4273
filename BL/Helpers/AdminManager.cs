using BlImplementation;
using BO;
using DalApi;
namespace Helpers;
using System.Runtime.CompilerServices;
using System.Threading;
using BL.Helpers;

/// <summary>
/// Internal BL manager for all Application's Clock logic policies
/// </summary>
internal static class AdminManager //stage 4
{
    #region Stage 4
    private static readonly DalApi.Idal s_dal = DalApi.Factory.Get; //stage 4
    #endregion Stage 4
    private static CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    #region Stage 5
    internal static event Action? ConfigUpdatedObservers; //prepared for stage 5 - for config update observers
    internal static event Action? ClockUpdatedObservers; //prepared for stage 5 - for clock update observers
    private static void NotifyObservers(Action? observers)
    {
        if (observers == null) return;

        foreach (var observer in observers.GetInvocationList())
        {
            try
            {
                ((Action)observer)?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Observer threw an exception: {ex.Message}");
            }
        }
    }
    #endregion Stage 5

    #region Stage 4
    /// <summary>
    /// Property for providing/setting current configuration variable value for any BL class that may need it
    /// </summary>
    internal static TimeSpan MaxRange
    {
        get => s_dal.config.RiskTimeRange;
        set
        {
            s_dal.config.RiskTimeRange = value;
            ConfigUpdatedObservers?.Invoke(); // stage 5
        }
    }

    /// <summary>
    /// Property for providing current application's clock value for any BL class that may need it
    /// </summary>
    internal static DateTime Now { get => s_dal.config.clock; } //stage 4

    internal static void ResetDB() //stage 4
    {
        lock (BlMutex) //stage 7
        {
            s_dal.ResetDB();
            UpdateClock(Now);
            MaxRange = MaxRange;
            //AdminManager.UpdateClock(AdminManager.Now); //stage 5 - needed since we want the label on Pl to be updated
            //AdminManager.MaxRange = AdminManager.MaxRange; // stage 5 - needed to update PL 
        }
    }

    internal static void InitializeDB() //stage 4
    {
        lock (BlMutex) //stage 7
        {
            DalTest.Initialization.Do();
            //AdminManager.UpdateClock(AdminManager.Now);  //stage 5 - needed since we want the label on Pl to be updated
            //AdminManager.MaxRange = AdminManager.MaxRange; // stage 5 - needed for update the PL 
            UpdateClock(Now);
            MaxRange = MaxRange;
        }
    }

    private static Task? _periodicTask = null;

    internal static void UpdateClock(DateTime newClock)
    {
        var oldClock = s_dal.config.clock;
        s_dal.config.clock = newClock;

        if (_periodicTask is null || _periodicTask.IsCompleted)
        {
            _periodicTask = Task.Run(() =>
            {
                try
                {
                    VolunteerManager.PeriodicVolunteersUpdates(oldClock, newClock);
                    CallManager.PeriodicCallUpdates(oldClock, newClock);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in periodic updates: {ex.Message}");
                }
            });
        }
        //ClockUpdatedObservers?.Invoke();
        NotifyObservers(ClockUpdatedObservers);
    }

    #endregion Stage 4

    #region Stage 7 base

    /// <summary>    
    /// Mutex to use from BL methods to get mutual exclusion while the simulator is running
    /// </summary>
    internal static readonly object BlMutex = new(); // BlMutex = s_dal; // This field is actually the same as s_dal - it is defined for readability of locks
    /// <summary>
    /// The thread of the simulator
    /// </summary>
    private static volatile Thread? s_thread;
    /// <summary>
    /// The Interval for clock updating
    /// in minutes by second (default value is 1, will be set on Start())    
    /// </summary>
    private static int s_interval = 1;
    /// <summary>
    /// The flag that signs whether simulator is running
    /// 
    private static volatile bool s_stop = false;


    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7                                                 
    public static void ThrowOnSimulatorIsRunning()
    {
        if (s_thread != null)
            throw new BO.BlTemporaryNotAvailableException("Cannot perform the operation since Simulator is running");
    }
    
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7                                                 
    internal static void Start(int interval)
    {
        if (s_thread is null)
        {
            s_interval = interval;
            s_stop = false;
            s_thread = new(clockRunner) { Name = "ClockRunner" };
            s_thread.Start();
        }
    }

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7                                                 
    internal static void Stop()
    {
        if (s_thread is not null)
        {
            try
            {
                s_stop = true;
                _cancellationTokenSource.Cancel();
                s_thread.Interrupt();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping thread: {ex.Message}");
            }
            finally
            {
                s_thread = null;
            }
        }
    }

    private static Task? _simulateTask = null;

    private static void clockRunner()
    {
        while (!s_stop)
        {

            DateTime oldClock = Now;
            DateTime newClock = Now.AddMinutes(s_interval);
            UpdateClock(newClock);
            NotifyObservers(ClockUpdatedObservers);

            //TO_DO:
            //Add calls here to any logic simulation that was required in stage 7
            //for example: course registration simulation
            if (_simulateTask is null || _simulateTask.IsCompleted)//stage 7
                _simulateTask = Task.Run(() => CallManager.PeriodicCallUpdates(oldClock,newClock));

            //etc...

            try
            {
                Thread.Sleep(1000); // 1 second
            }
            catch (ThreadInterruptedException) {
                Console.WriteLine("Thread was interrupted.");
                break; // יציאה מהלולאה בצורה מסודרת
            }
        }
    }
    private static async Task clockRunnerAsync()
    {
        var cancellationToken = _cancellationTokenSource.Token;

        while (!cancellationToken.IsCancellationRequested)
        {
            DateTime oldClock = Now; // שומר את הזמן לפני העדכון
            DateTime newClock = Now.AddMinutes(s_interval); // מחשב את הזמן החדש

            UpdateClock(newClock); // מעדכן את השעון

            // קריאה לפונקציה עם הפרמטרים המתאימים
            await PerformPeriodicTasksAsync(oldClock, newClock);
            // השהיה של שנייה
            try
            {
                await Task.Delay(1000, cancellationToken);
            }
            catch (TaskCanceledException) 
            {
                break;
            }
        }
    }

    private static async Task PerformPeriodicTasksAsync(DateTime oldClock, DateTime newClock)
    {
        if (_simulateTask is null || _simulateTask.IsCompleted)
        {
            _simulateTask = Task.Run(async () =>
            {
                try
                {
                    CallManager.PeriodicCallUpdates(oldClock,newClock);// הפעלה אסינכרונית
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in periodic task: {ex.Message}");
                }
            });
        }

        if (_periodicTask is not null) // לוודא ש-_periodicTask לא null
        {
            await _periodicTask;
        }
    }
    
    #endregion Stage 7 base
}