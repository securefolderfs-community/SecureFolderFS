namespace SecureFolderFS.Cli.Helpers
{
    public static class PasswordHelper
    {
        private const char PASSWORD_CHAR = '*';
        
        public static string AskForPassword(bool visualFeedback = true)
        {
            Console.Write("Enter password (CTRL + C to cancel, CTRL + U to clear input): ");
            
            var password = string.Empty;
            ConsoleKeyInfo keyInfo;
            do
            {
                keyInfo = Console.ReadKey(intercept: true);
                if (keyInfo.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    if (visualFeedback)
                        Console.Write("\b \b");
                    
                    password = password[0..^1];
                }
                else if (keyInfo.Key == ConsoleKey.U && keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control) && password.Length > 0)
                {
                    if (visualFeedback)
                        for (var i = 0; i < password.Length; i++)
                            Console.Write("\b \b");

                    password = string.Empty;
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    if (visualFeedback)
                        Console.Write(PASSWORD_CHAR);
                    
                    password += keyInfo.KeyChar;
                }
            } while (keyInfo.Key != ConsoleKey.Enter);
            
            Console.WriteLine();
            return password;
        }
    }
}