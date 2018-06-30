using System;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EW.ObjectModel;
using EW.Utility;

//[assembly: CompilationRelaxations(CompilationRelaxations.NoStringInterning)]
namespace EW
{
    static public class Program
    {
        [STAThread]
        static public void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.Unicode;
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("ru-RU");
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("ru-RU");
            Console.Title = "Engineers Wars Bot";
            Console.ForegroundColor = ConsoleColor.Green;
            AppDomain.CurrentDomain.UnhandledException += Error;
            MySave.CreateDirectories();
            MySave.Load();
            if (!MySave.Players.Exists(x => x.Vk == 0)) MySave.Players = MySave.Players.Add(new MyPlayer("Console", string.Empty, 0, 0, PlayerStatus.Guest, true, false, false));

            Console.WriteLine($"{DateTime.Now}: Запуск прослушивателя…");
            MyVkApi vk = new MyVkApi();
            vk.SetLastApi();
            new Task(ConsoleCommands).Start();
            Console.WriteLine($"{DateTime.Now}: Готово!");
            try
            {
                Console.Beep();
            }
            catch (Exception)
            {
                Console.WriteLine($"{DateTime.Now}: Силы хаоса заблокировали возможность попищать! Бип!");
            }

            Console.Write($"{DateTime.Now}: Автосохранение: ");
            if (MySave.BotSettings.AutoSaveInterval == 0)
            {
                Console.WriteLine("ВЫКЛ");
            }
            else
            {
                Console.Write(MySave.BotSettings.AutoSaveInterval);
                Console.WriteLine(" минут");
            }

            vk.StartListen();
        }

        static private void ConsoleCommands()
        {
            MyCommand ccc = new MyCommand(0);
            while (true)
            {
                string text = ccc.ExecuteCommand(Console.ReadLine(), out string title);
                /*text = text.Replace("♔", "* ");
                text = text.Replace("🗹", "+");
                text = text.Replace("🗷", "—");
                text = text.Replace("　", "  ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                if (!string.IsNullOrWhiteSpace(title)) Console.WriteLine(title);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(text.TrimEnd());
                Console.ForegroundColor = ConsoleColor.Green;*/
                MyVkApi.LastApi.SendMessage(0, text, default, title);
            }

            // ReSharper disable once FunctionNeverReturns
        }

        static private void Error(object sender, UnhandledExceptionEventArgs a)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Exception e = (Exception) a.ExceptionObject;
            Console.WriteLine($"{DateTime.Now}: Критическая ошибка: {e}");
            Console.WriteLine("Нажмите любую клавишу для выхода…");
            Console.ReadKey();
        }
    }
}
