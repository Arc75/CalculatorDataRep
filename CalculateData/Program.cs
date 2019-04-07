using CalculateData;
using System;
using System.Linq;
using CalculateData.Assets;

namespace Calculate_Data
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.InitLogger();

            Logger.Log.Info("Начало работы.");

            Console.WriteLine(@"Введите данные через ';'");

            var input = Console.ReadLine();

            Logger.Log.Info($"Введены значения: {input}");

            var inputList = input.Trim().Split(';').ToList();

            Logger.Log.Info($"Распознано {inputList.Count} значений. Запуск калькулятора");

            try
            {
                var calculator = new Calculator(inputList);

                double result = 0;

                var consoleKey = Console.ReadKey(true);
                while (consoleKey.Key != ConsoleKey.Escape)
                {
                    result = calculator.Calculate();
                }

                if (consoleKey.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine($"Отменено пользователем");
                }
                else
                {
                    Console.WriteLine($"Результат: {result} ");
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error($"Произошла ошибка. {ex.Message}");

                Console.WriteLine($"В процессе вычислений произошла ошибка: {ex.Message}");
            }

            Console.ReadKey();
        }
    }
}