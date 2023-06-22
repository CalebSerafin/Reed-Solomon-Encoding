using System.Linq.Expressions;

using Xunit.Abstractions;

namespace ReedSolomonEncoding.Tests.MathExpressionTests;

public class MathExpressionTests {
    public MathExpressionTests(ITestOutputHelper output) {
        this.output = output;
    }
    [Theory]
    [InlineData(01)]
    [InlineData(02)]
    [InlineData(03)]
    [InlineData(04)]
    [InlineData(05)]
    [InlineData(06)]
    [InlineData(07)]
    [InlineData(08)]
    [InlineData(09)]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(12)]
    [InlineData(13)]
    [InlineData(14)]
    [InlineData(15)]
    [InlineData(16)]
    public void MultiplyChildrenExpressionWorks(int childrenAmount) {
        // Arrange
        Expression[] children = new Expression[childrenAmount];
        double expected = 1;
        Random random = new(0);
        for (int i = 0; i < childrenAmount; i++) {
            double data = random.Next(10);
            expected *= data;
            children[i] = Expression.Constant((double)data, typeof(double));
        }

        Expression result = MathExpression.MathExpression.MultiplyChildrenExpression(children);
        Expression<Func<double>> lambdaExpression = Expression.Lambda<Func<double>>(result);
        Func<double> func = lambdaExpression.Compile();

        // Act
        double actual = func();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void CreateLagrangePolynomialExpressionWorks(int dataPointAmount) {
        // Arrange
        double[] dataPoints = new double[dataPointAmount];
        Random random = new(0);
        for (int i = 0; i < dataPointAmount; i++) {
            dataPoints[i] = random.Next(10);
        }
        for (int indexOfTarget = 0; indexOfTarget < dataPoints.Length; indexOfTarget++) {
            output.WriteLine($"Constructing polynomial for data-point at {indexOfTarget} with value {dataPoints[indexOfTarget]}");
            // Act
            Func<double, double> equation = MathExpression.MathExpression.CreateLagrangePolynomialExpression(dataPoints, indexOfTarget).Compile();

            // Assert
            for (int xAxis = 0; xAxis < dataPoints.Length; xAxis++) {
                double expected = xAxis == indexOfTarget ? 1 : 0;
                output.WriteLine($"Validating constructed polynomial with xAxis {xAxis} expecting with value {expected}");
                double actual = equation((double)xAxis);
                Assert.Equal(expected, actual);
            }
        }
    }

    [Theory]
    [InlineData(01)]
    [InlineData(02)]
    [InlineData(03)]
    [InlineData(04)]
    [InlineData(05)]
    [InlineData(06)]
    [InlineData(07)]
    [InlineData(08)]
    [InlineData(09)]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(12)]
    [InlineData(13)]
    [InlineData(14)]
    [InlineData(15)]
    [InlineData(16)]
    public void CreateLagrangeEquationExpressionWorks(int dataPointAmount) {
        // Arrange
        double[] dataPoints = new double[dataPointAmount];
        Random random = new(0);
        for (int i = 0; i < dataPointAmount; i++) {
            dataPoints[i] = random.Next(10);
        }
        // Act
        Func<double, double> equation = MathExpression.MathExpression.CreateLagrangeEquationExpression(dataPoints).Compile();
        // Assert
        for (int xAxis = 0; xAxis < dataPoints.Length; xAxis++) {
            double expected = dataPoints[xAxis];
            output.WriteLine($"Validating constructed polynomial with xAxis {xAxis} expecting with value {expected}");
            double actual = equation((double)xAxis);
            actual = double.Round(actual, 10);
            Assert.Equal(expected, actual);
        }

    }

    readonly ITestOutputHelper output;
}