using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using ReedSolomonEncoding.UI;
namespace ReedSolomonEncoding;

internal class Program {
    static void Main(string[] args) {
        double[] dataPoints = new double[] { 2, 3, 1, 5, 1, 6, 4, 7, 3, 1, 4 };
        Func<double, double> lagrangeEquation = MathExpression.MathExpression.CreateLagrangeEquationExpression(dataPoints).Compile();

        //Console.WriteLine("Press Key to accept console size.");
        //Console.ReadKey();

        ConsoleGraphArea graphArea = new();
        graphArea.AddLabelAt($"=========================", new Vector2(1, 12), new Vector2(2, 1));
        graphArea.AddLabelAt($"Plot of Lagrange Equation", new Vector2(1, 12), new Vector2(2, 0));
        graphArea.AddLabelAt($"=========================", new Vector2(1, 12), new Vector2(2, -1));
        for (int i = 0; i < dataPoints.Length; i++) {
            double currentValue = dataPoints[i];
            Vector2 dataPoint = new Vector2(i, (float)currentValue);
            graphArea.AddLabelAt($"({i},{currentValue})", dataPoint, new Vector2(0, 1));
            graphArea.AddPointAt(dataPoint);
        }
        //graphArea.AddFunction(x => x * x);
        //graphArea.AddFunction(x =>  1 / x);
        //graphArea.AddFunction(x => (2 * x) - 7);
        //graphArea.AddFunction(x => double.Log2(x));
        //graphArea.SetCenter(new Vector2(0, 0));

        graphArea.AddFunction(lagrangeEquation);
        graphArea.SetBounds(-1, 12, -1, 13);
        //graphArea.SetBounds(-0.5, 12, -1, 40);
        //graphArea.SetBounds(-100, 100, -100, 100);
        graphArea.HasGrid = true;
        graphArea.Fills.AxesSymbol = '+';
        graphArea.Fills.GridSymbol = '.';
        graphArea.Render();
        Console.WriteLine("Press Enter to exit");
        Console.ReadLine();
    }


}
