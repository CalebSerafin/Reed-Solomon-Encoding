using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ReedSolomonEncoding.UI;
internal class ConsoleGraphArea : IGraphArea {
    #region Constructors
    public ConsoleGraphArea() {

    }

    #endregion

    #region Properties
    public bool DrawAxes { get; set; } = true;
    public bool DrawGrid { get; set; } = false;
    public float GridXSpacing { get; set; }
    public float GridYSpacing { get; set; }
    private int ConsoleWidth => Console.WindowWidth;
    private int ConsoleHeight => Console.WindowHeight;
    #endregion

    #region Public Methods
    public void Render() {
        if (DrawGrid)
            RenderGrid();
        if (DrawAxes)
            RenderAxes();
        RenderLines();
        RenderFunctions();
        RenderPoints();
        RenderLabels();
        Console.SetCursorPosition(0, ConsoleHeight - 1);
    }
    #endregion

    #region IGraphArea
    public void SetBounds(double xAxisMin, double xAxisMax, double yAxisMin, double yAxisMax) {
        this.xAxisMax = xAxisMax;
        this.xAxisMin = xAxisMin;
        this.yAxisMax = yAxisMax;
        this.yAxisMin = yAxisMin;
    }
    public void AddLabelAt(string label, Vector2 position) => labels.Add((label, position));
    public void AddPointAt(Vector2 position) => points.Add(position);
    public void AddFunction(Func<double, double> function) => simpleFunctions.Add(function);
    public void SetCenter(Vector2 centerPoint) {
        double xRange = xAxisMax - xAxisMin;
        double yRange = yAxisMax - yAxisMin;
        xAxisMin = (centerPoint.X - xRange) / 2;
        xAxisMax = (centerPoint.X + xRange) / 2;
        yAxisMin = (centerPoint.Y - yRange) / 2;
        yAxisMax = (centerPoint.Y + yRange) / 2;
    }

    public void AddLine(Vector2 start, Vector2 end) => lines.Add((start, end));
    #endregion

    #region Fields
    double xAxisMin = -1;
    double xAxisMax = 10;
    double yAxisMin = -1;
    double yAxisMax = 10;
    List<Func<double, double>> simpleFunctions = new();
    List<Vector2> points = new();
    List<(string, Vector2)> labels = new();
    List<(Vector2, Vector2)> lines = new();
    #endregion

    #region Private Methods
    Vector2 ConvertToConsoleCoordinates(Vector2 point) {
        double x = point.X;
        double y = point.Y;
        double xRange = xAxisMax - xAxisMin;
        double yRange = yAxisMax - yAxisMin;
        double xScale = ConsoleWidth / xRange;
        double yScale = ConsoleHeight / yRange;
        double xConsole = (x - xAxisMin) * xScale;
        double yConsole = (y - yAxisMin) * yScale;
        return new Vector2((float)xConsole, (float)yConsole);
    }

    Vector2 ConvertToCartesianCoordinates(Vector2 point) {
        double x = point.X;
        double y = point.Y;
        double xRange = xAxisMax - xAxisMin;
        double yRange = yAxisMax - yAxisMin;
        double xScale = ConsoleWidth / xRange;
        double yScale = ConsoleHeight / yRange;
        double xCartesian = (x / xScale) + xAxisMin;
        double yCartesian = (y / yScale) + yAxisMin;
        return new Vector2((float)xCartesian, (float)yCartesian);
    }

    bool IsConsolePointInBounds(Vector2 consolePoint) {
        return consolePoint.X >= 0 && consolePoint.X < ConsoleWidth && consolePoint.Y >= 0 && consolePoint.Y < ConsoleHeight;
    }

    private void RenderPoints() {
        foreach (Vector2 point in points) {
            Vector2 consolePoint = ConvertToConsoleCoordinates(point);
            if (!IsConsolePointInBounds(consolePoint))
                continue;
            Console.SetCursorPosition((int)consolePoint.X, ConsoleHeight - (int)consolePoint.Y - 1);
            Console.Write('+');
        }
    }

    private void RenderLabels() {
        foreach ((string label, Vector2 position) in labels) {
            Vector2 consolePoint = ConvertToConsoleCoordinates(position);
            if (!IsConsolePointInBounds(consolePoint))
                continue;
            Console.SetCursorPosition((int)consolePoint.X, ConsoleHeight - (int)consolePoint.Y - 1);
            Console.Write(label);
        }
    }

    private void RenderFunctions() {
        foreach (Func<double, double> function in simpleFunctions) {
            Vector2 lastConsolePoint = new(float.NaN, float.NaN);
            for (int x = 0; x < ConsoleWidth; x += 1) {
                Vector2 cartesianPoint = ConvertToCartesianCoordinates(new Vector2((float)x, 0));
                cartesianPoint.Y = (float)function(cartesianPoint.X);
                Vector2 consolePoint = ConvertToConsoleCoordinates(cartesianPoint);
                if (!IsConsolePointInBounds(consolePoint)) {
                    lastConsolePoint = new(float.NaN, float.NaN);
                    continue;
                }
                if (float.IsFinite(lastConsolePoint.X) && float.IsFinite(lastConsolePoint.Y) && float.Abs((int)lastConsolePoint.Y - (int)consolePoint.Y) >= 2) {
                    RenderVerticalLine(lastConsolePoint, consolePoint);
                }
                Console.SetCursorPosition((int)consolePoint.X, ConsoleHeight - (int)consolePoint.Y - 1);
                Console.Write('█');
                lastConsolePoint = consolePoint;
            }
        }
    }

    private void RenderLines() {
        foreach ((Vector2 start, Vector2 end) in lines)
            RenderLine(start, end);
    }

    private void RenderLine(Vector2 start, Vector2 end) {
        Vector2 consoleStart = ConvertToConsoleCoordinates(start);
        Vector2 consoleEnd = ConvertToConsoleCoordinates(end);
        if (!IsConsolePointInBounds(consoleStart) && !IsConsolePointInBounds(consoleEnd))
            return;
        if (consoleStart.X == consoleEnd.X) 
            RenderVerticalLine(consoleStart, consoleEnd);
        else if (consoleStart.Y == consoleEnd.Y)
            RenderHorizontalLine(consoleStart, consoleEnd);
        else
            RenderDiagonalLine(consoleStart, consoleEnd);
    }

    private void RenderAxes() {
        Vector2 xAxisStart = new Vector2((float)xAxisMin, 0);
        Vector2 xAxisEnd = new Vector2((float)xAxisMax, 0);
        Vector2 yAxisStart = new Vector2(0, (float)yAxisMin);
        Vector2 yAxisEnd = new Vector2(0, (float)yAxisMax);
        RenderLine(xAxisStart, xAxisEnd);
        RenderLine(yAxisStart, yAxisEnd);
    }

    private void RenderGrid() {
        for (double x = xAxisMin; x < xAxisMax; x += GridXSpacing) {
            Vector2 start = new Vector2((float)x, (float)yAxisMin);
            Vector2 end = new Vector2((float)x, (float)yAxisMax);
            RenderLine(start, end);
        }
        for (double y = yAxisMin; y < yAxisMax; y += GridYSpacing) {
            Vector2 start = new Vector2((float)xAxisMin, (float)y);
            Vector2 end = new Vector2((float)xAxisMax, (float)y);
            RenderLine(start, end);
        }
    }

    private void RenderHorizontalLine(Vector2 consoleStart, Vector2 consoleEnd) {
        int y = ConsoleHeight - (int)consoleStart.Y - 1;
        int xStart = (int)consoleStart.X;
        int xEnd = (int)consoleEnd.X;
        Console.SetCursorPosition(xStart, y);
        for (int x = xStart; x < xEnd; x += 1) {
            Console.Write('▒');
        }
    }

    private void RenderVerticalLine(Vector2 consoleStart, Vector2 consoleEnd) {
        int x = (int)consoleStart.X;
        int yStart = int.Min((int)consoleStart.Y, (int)consoleEnd.Y);
        int yEnd = int.Max((int)consoleStart.Y, (int)consoleEnd.Y);
        for (int y = yStart; y < yEnd; y += 1) {
            Console.SetCursorPosition(x, ConsoleHeight - y - 1);
            Console.Write('▒');
        }
    }

    private void RenderDiagonalLine(Vector2 consoleStart, Vector2 consoleEnd) {
        double slope = (consoleEnd.Y - consoleStart.Y) / (consoleEnd.X - consoleStart.X);
        double yIntercept = consoleStart.Y - (slope * consoleStart.X);
        for (int x = 0; x < ConsoleWidth; x += 1) {
            double y = (slope * x) + yIntercept;
            Vector2 consolePoint = ConvertToConsoleCoordinates(new Vector2((float)x, (float)y));
            if (!IsConsolePointInBounds(consolePoint))
                continue;
            Console.SetCursorPosition((int)consolePoint.X, ConsoleHeight - (int)consolePoint.Y - 1);
            Console.Write('▒');
        }
    }
    #endregion
}
