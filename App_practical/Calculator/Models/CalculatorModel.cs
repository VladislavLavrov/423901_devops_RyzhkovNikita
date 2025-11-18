using CalculatorApp.Models;

namespace CalculatorApp.Models
{
    public class CalculatorModel
    {
        public double FirstNumber { get; set; }
        public double SecondNumber { get; set; }
        public string Operation { get; set; } = "+";
        public double Result { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public List<CalculationHistory>? History { get; set; }

        public void Calculate()
        {
            try
            {
                ErrorMessage = string.Empty;

                switch (Operation)
                {
                    case "+":
                        Result = FirstNumber + SecondNumber;
                        break;
                    case "-":
                        Result = FirstNumber - SecondNumber;
                        break;
                    case "*":
                        Result = FirstNumber * SecondNumber;
                        break;
                    case "/":
                        if (SecondNumber == 0)
                        {
                            ErrorMessage = "Деление на ноль невозможно";
                            Result = 0;
                        }
                        else
                        {
                            Result = FirstNumber / SecondNumber;
                        }
                        break;
                    case "^":
                        Result = Math.Pow(FirstNumber, SecondNumber);
                        break;
                    case "sqrt":
                        if (FirstNumber < 0)
                        {
                            ErrorMessage = "Корень из отрицательного числа невозможен";
                            Result = 0;
                        }
                        else
                        {
                            Result = Math.Sqrt(FirstNumber);
                        }
                        break;
                    default:
                        ErrorMessage = "Неизвестная операция";
                        break;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
            }
        }
    }
}