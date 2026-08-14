namespace EndlessDungeon.Input;

public class InputManager
{
    public ConsoleKey ReadKey()
    {
        return Console.ReadKey(intercept: true).Key;
    }
}