namespace Nikklim.App.Navigation.Extensions;

internal static class MainThreadExtensions
{
    internal static bool IsBeingRunFromTest = false;

    internal static bool IsMainThread
    {
        get
        {
            if (IsBeingRunFromTest)
            {
                return true;
            }
            
            return MainThread.IsMainThread;
        }
    }

    internal static Task InvokeOnMainThreadAsync(Action action)
    {
        if (IsBeingRunFromTest)
        {
            action();
            return Task.CompletedTask;
        }

        return MainThread.InvokeOnMainThreadAsync(action);
    }

    internal static Task<T> InvokeOnMainThreadAsync<T>(Func<T> func)
    {
        if (IsBeingRunFromTest)
        {
            return Task.FromResult(func());
        }

        return MainThread.InvokeOnMainThreadAsync(func);
    }

    internal static Task InvokeOnMainThreadAsync(Func<Task> funcTask)
    {
        if (IsBeingRunFromTest)
        {
            return funcTask();
        }

        return MainThread.InvokeOnMainThreadAsync(funcTask);
    }
    
    internal static Task<T> InvokeOnMainThreadAsync<T>(Func<Task<T>> funcTask)
    {
        if (IsBeingRunFromTest)
        {
            return funcTask();
        }

        return MainThread.InvokeOnMainThreadAsync(funcTask);
    }
}