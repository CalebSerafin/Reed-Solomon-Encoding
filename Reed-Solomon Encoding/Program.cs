using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using ReedSolomonEncoding.UI;
namespace ReedSolomonEncoding;

internal class Program {
    static void Main(string[] args) {
        double[] dataPoints = new double[] { 2, 3, 1, 5, 1, 6, 4, 7, 3, 1, 4 };
        Func<double, double> lagrangeEquation = MathExpression.MathExpression.CreateLagrangeEquationExpression(dataPoints).Compile();

        ConsoleGraphArea graphArea = new();
        graphArea.AddLabelAt("Hello World", new Vector2(0, 0));
        graphArea.AddPointAt(new Vector2(0, -1));
        //graphArea.AddFunction(x => x * x);
        //graphArea.AddFunction(x =>  1 / x);
        //graphArea.AddFunction(x => (2 * x) - 7);
        //graphArea.AddFunction(x => double.Log2(x));
        //graphArea.SetCenter(new Vector2(0, 0));

        graphArea.AddFunction(lagrangeEquation);
        graphArea.SetBounds(-0.5, 12, -1, 13);
        //graphArea.SetBounds(-0.5, 12, -1, 40);
        //graphArea.SetBounds(-100, 100, -100, 100);
        graphArea.Render();
    }


}
