namespace SWGen.FileSystems;

public static class TaskEx
{
    public static Task CompletedTask(Action action)
    {
        action();
        return Task.CompletedTask;
    }
}