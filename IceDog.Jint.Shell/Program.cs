using Jint;

namespace IceDog.Jint.Shell
{
    public class Program
    {
        static void Main(string[] args)
        {
            var engine = new Engine();

            Console.WriteLine("Welcome to use Jint Shell,Jint Verssion:3.1.1,this is implement by video:\r\n" +
                "https://docs.microsoft.com/shows/code-conversations/sebastien-ros-on-jint-javascript-interpreter-net\r\n" +
                "input exit() to exit");
            var code = "let hw=()=>\"hello world\";hw();";
            Console.WriteLine($"[Jint Shell]>{code}");
            var result = engine.Evaluate(code);
            Console.WriteLine(result.ToString());
            while (true)
            {
                try
                {
                    Console.Write("[Jint Shell]>");
                    var input = Console.ReadLine();
                    if (input == "exit()")
                    {
                        Console.WriteLine("Bye Bye !");
                        Environment.Exit(0);
                    }

                    result = engine.Evaluate(input);
                    var output = string.Empty;
                    if (result.IsUndefined())
                    {
                        output = input;
                    }
                    else
                    {
                        output = result.ToString();
                    }

                    Console.WriteLine(output);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}