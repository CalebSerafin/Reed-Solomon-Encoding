using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ReedSolomonEncoding.MathExpression;

class MathExpression {
    public static Expression<Func<double, double>> CreateLagrangeEquationExpression(ReadOnlySpan<double> dataPoints) {
        ParameterExpression xAxisParemter = Expression.Parameter(typeof(double), "xAxis");

        Expression[] lagrangePolynomials = new Expression[dataPoints.Length];
        for (int i = 0; i < dataPoints.Length; i++) {
            double currentPoint = dataPoints[i];
            if (double.IsFinite(currentPoint)) {
                Expression<Func<double, double>> lagrangePolynomial = CreateLagrangePolynomialExpression(dataPoints, i);
                lagrangePolynomials[i] = Expression.Multiply(Expression.Constant(currentPoint), Expression.Invoke(lagrangePolynomial, xAxisParemter));
            } else {
                lagrangePolynomials[i] = Expression.Constant(0d);
            }
        }
        return Expression.Lambda<Func<double, double>>(SumChildrenExpression(lagrangePolynomials), xAxisParemter);
    }

    public static Expression<Func<double,double>> CreateLagrangePolynomialExpression(ReadOnlySpan<double> dataPoints, int indexOfTarget) {
        ParameterExpression xAxisParemter = Expression.Parameter(typeof(double), "xAxis");

        // construct roots of polynomial from the dataPoints
        Expression[] individualPolynomialRoots = new Expression[dataPoints.Length];
        for (int i = 0; i < dataPoints.Length; i++) {
            double currentPoint = dataPoints[i];
            if (!double.IsFinite(currentPoint) || i == indexOfTarget) {
                individualPolynomialRoots[i] = Expression.Constant(1d, typeof(double));
                continue;
            }
            individualPolynomialRoots[i] = Expression.Subtract(xAxisParemter, Expression.Constant(currentPoint, typeof(double)));
        }

        Expression<Func<double, double>> incompleteLagrangePolynomial = Expression.Lambda<Func<double,double>>(
            MultiplyChildrenExpression(individualPolynomialRoots),
            new ParameterExpression[] { xAxisParemter }
        );
        // Plug in x = indexOfTarget +1 to find coefficient
        double inverseCoefficient = incompleteLagrangePolynomial.Compile()(indexOfTarget + 1);
        double coefficient = 1 / inverseCoefficient;

        // Insert coefficient into polynomial
        individualPolynomialRoots[indexOfTarget] = Expression.Constant(coefficient);

        // Multiply all roots together
        Expression<Func<double, double>> completeLagrangePolynomial = Expression.Lambda<Func<double, double>>(
            MultiplyChildrenExpression(individualPolynomialRoots),
            new ParameterExpression[] { xAxisParemter }
        );
        return completeLagrangePolynomial;
    }

    internal static Expression MultiplyChildrenExpression(ReadOnlySpan<Expression> children) {
        if (children.Length == 0)
            throw new ArgumentException("Must have at least one child", nameof(children));
        if (children.Length == 1)
            return children[0];
        return ApplyCommutativeBinaryExpression(Expression.Multiply, children);
    }

    internal static Expression SumChildrenExpression(ReadOnlySpan<Expression> children) {
        if (children.Length == 0)
            return Expression.Constant(0d);
        if (children.Length == 1)
            return children[0];
        return ApplyCommutativeBinaryExpression(Expression.Add, children);
    }

    /// <summary>
    /// Applies the binary expression to the operands in a commutative manner that is optimized to produce the lowest depth.
    /// Does not handle cases where operands is empty or has only one element.
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="operands"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    internal static Expression ApplyCommutativeBinaryExpression(Func<Expression, Expression, BinaryExpression> expression, ReadOnlySpan<Expression> operands) {
        if (operands.Length < 2)
            throw new ArgumentException("Must have at least two operands", nameof(operands));

        Expression[] buffer = operands.ToArray();
        int length = buffer.Length;
        int skip = 1;
        while (skip < length) {
            for (int i = 0; i < length; i += 2 * skip) {
                if (i + skip < length)
                    buffer[i] = expression(buffer[i], buffer[i + skip]);
            }
            skip *= 2;
        }
        return buffer[0];
    }
}