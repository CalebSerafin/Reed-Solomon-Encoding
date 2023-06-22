using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using static System.Net.Mime.MediaTypeNames;

namespace ReedSolomonEncoding.UI;
internal class ConsoleGraphArea : IGraphArea {
    #region Constructors
    public ConsoleGraphArea() {

    }

    #endregion

    #region Properties
    public bool HasAxes { get; set; } = true;
    public bool HasAxesNumbers { get; set; } = true;
    public bool HasGrid { get; set; } = false;
    public double GridXSpacing { get; set; } = 1;
    public double GridYSpacing { get; set; } = 1;
    private int ConsoleWidth => Console.WindowWidth;
    private int ConsoleHeight => Console.WindowHeight;
    public ConsoleGraphAreaFills Fills { get; set; } = new();
    #endregion

    #region Public Methods
    public void Render() {
        if (HasGrid)
            using (new ConsoleColorScope(Fills.GridColor)) { RenderGrid(Fills.GridSymbol); }
        if (HasAxes)
            using (new ConsoleColorScope(Fills.GridColor)) { RenderAxes(Fills.AxesSymbol); }
        using (new ConsoleColorScope(Fills.LineColor)) { RenderLines(Fills.LineSymbol); }
        using (new ConsoleColorScope(Fills.FunctionColor)) { RenderFunctions(Fills.FunctionSymbol); }
        if (HasAxesNumbers)
            using (new ConsoleColorScope(Fills.AxesNumberColor)) { RenderAxesNumbers(); }
        using (new ConsoleColorScope(Fills.PointColor)) { RenderPoints(Fills.PointSymbol); }
        using (new ConsoleColorScope(Fills.LabelColor)) { RenderLabels(); }
        Console.SetCursorPosition(ConsoleWidth - 1, ConsoleHeight - 1);
        Console.WriteLine();
    }
    #endregion

    #region IGraphArea
    public void SetBounds(double xAxisMin, double xAxisMax, double yAxisMin, double yAxisMax) {
        this.xAxisMax = xAxisMax;
        this.xAxisMin = xAxisMin;
        this.yAxisMax = yAxisMax;
        this.yAxisMin = yAxisMin;
    }
    public void AddLabelAt(string label, Vector2 position, Vector2 renderSpaceOffset = default) => labels.Add(new (label, Position: position, RenderSpaceOffset: renderSpaceOffset));
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
    List<Label> labels = new();
    List<(Vector2, Vector2)> lines = new();
    #endregion

    #region Private Methods
    void ConsoleSetCursorPositionInverted(float left, float top) {
        ConsoleSetCursorPositionInverted((int)float.Round(left), (int)float.Round(top));
    }
    void ConsoleSetCursorPositionInverted(int left, int top) {
        Console.SetCursorPosition(left, ConsoleHeight - top - 1);
    }

    Vector2 ConvertToConsoleCoordinates(Vector2 point) {
        double x = point.X;
        double y = point.Y;
        double xRange = xAxisMax - xAxisMin;
        double yRange = yAxisMax - yAxisMin;
        double xScale = ((double)ConsoleWidth) / xRange;
        double yScale = ((double)ConsoleHeight) / yRange;
        double xConsole = (x - xAxisMin) * xScale;
        double yConsole = (y - yAxisMin) * yScale;
        return new Vector2((float)xConsole, (float)yConsole);
    }

    Vector2 ConvertToCartesianCoordinates(Vector2 point) {
        double x = point.X;
        double y = point.Y;
        double xRange = xAxisMax - xAxisMin;
        double yRange = yAxisMax - yAxisMin;
        double xScale = ((double)ConsoleWidth) / xRange;
        double yScale = ((double)ConsoleHeight) / yRange;
        double xCartesian = (x / xScale) + xAxisMin;
        double yCartesian = (y / yScale) + yAxisMin;
        return new Vector2((float)xCartesian, (float)yCartesian);
    }

    bool IsConsolePointInBounds(Vector2 consolePoint) {
        consolePoint = RoundVector(consolePoint);
        return consolePoint.X >= 0 && consolePoint.X < ConsoleWidth && consolePoint.Y >= 0 && consolePoint.Y < ConsoleHeight;
    }

    bool IsVectorNaN(Vector2 vector) {
        return float.IsNaN(vector.X) || float.IsNaN(vector.Y);
    }

    Vector2 FloorVector(Vector2 vector) => new((float)Math.Floor(vector.X), (float)Math.Floor(vector.Y));
    Vector2 CeilingVector(Vector2 vector) => new((float)Math.Ceiling(vector.X), (float)Math.Ceiling(vector.Y));
    Vector2 RoundVector(Vector2 vector) => new((float)Math.Round(vector.X), (float)Math.Round(vector.Y));

    Vector2 ClipConsolePointToBounds(Vector2 consolePoint) {
        if (consolePoint.X < 0)
            consolePoint.X = 0;
        if (consolePoint.X >= ConsoleWidth)
            consolePoint.X = ConsoleWidth - 1;
        if (consolePoint.Y < 0)
            consolePoint.Y = 0;
        if (consolePoint.Y >= ConsoleHeight)
            consolePoint.Y = ConsoleHeight - 1;
        return consolePoint;
    }

    (Vector2 point1, Vector2 point2, bool isZeroLength) ShrinkVerticleLine(Vector2 point1, Vector2 point2, float amountFromEnds) {
        Vector2 topPoint = point1.Y > point2.Y ? point1 : point2;
        Vector2 bottomPoint = point1.Y < point2.Y ? point1 : point2;
        topPoint.Y -= amountFromEnds;
        bottomPoint.Y += amountFromEnds;
        if (topPoint.Y < bottomPoint.Y) {
            return (point1, point1 with { X = bottomPoint.X }, true);
        }
        if (point1.Y > point2.Y)
            return (topPoint, bottomPoint, false);
        else 
            return (bottomPoint, topPoint, false);
    }

    (Vector2 lowestX, Vector2 highestX) SortVectorsByX(Vector2 point1, Vector2 point2) {
        return point1.X < point2.X ? (point1, point2) : (point2, point1);
    }

    (Vector2 lowestY, Vector2 highestY) SortVectorsByY(Vector2 point1, Vector2 point2) {
        return point1.Y < point2.Y ? (point1, point2) : (point2, point1);
    }

    private void RenderPoints(char symbol = '+') {
        foreach (Vector2 point in points) {
            Vector2 consolePoint = ConvertToConsoleCoordinates(point);
            if (!IsConsolePointInBounds(consolePoint))
                continue;
            ConsoleSetCursorPositionInverted(consolePoint.X, consolePoint.Y);
            Console.Write(symbol);
        }
    }

    private void RenderLabels() {
        foreach (Label label in labels)
            RenderLabel(label);
    }

    private void RenderLabel(Label label) {
        Vector2 consolePoint = ConvertToConsoleCoordinates(label.Position);
        consolePoint = RoundVector(consolePoint);
        consolePoint = Vector2.Add(consolePoint, label.RenderSpaceOffset);
        if (!IsConsolePointInBounds(consolePoint))
            return;
        ConsoleSetCursorPositionInverted(consolePoint.X, consolePoint.Y);
        int maxTextLength = int.Min(label.Text.Length, (ConsoleWidth - (int)consolePoint.X));
        Console.Write(label.Text[..maxTextLength]);
    }

    private void RenderFunctions(char symbol = '█') {
        foreach (Func<double, double> function in simpleFunctions)
            RenderFunction(function, symbol);
    }

    /// <summary>
    /// Ensures that each integer point is rendered if possible.<br/>
    /// Then fills in the gaps with possibly non-integer interpolated points.
    /// </summary>
    /// <param name="symbol"></param>
    private void RenderFunction(Func<double, double> function, char symbol = '█') {
        double xRange = xAxisMax - xAxisMin;
        double yRange = yAxisMax - yAxisMin;
        double xScale = ((double)ConsoleWidth) / xRange;
        double yScale = ((double)ConsoleHeight) / yRange;

        double invserseScale = 1 / xScale;
        double integerAxisXStep = xScale > 1 ? 1 : xScale;
        Vector2 lastConsolePoint = new(float.NaN, float.NaN);
        for (double integerAxisX = xAxisMin; integerAxisX <= xAxisMax; integerAxisX += integerAxisXStep) {
            for (double interpolatedAxisX = integerAxisX; interpolatedAxisX < (integerAxisX + integerAxisXStep); interpolatedAxisX += invserseScale) {
                double value = (float)function(interpolatedAxisX);
                Vector2 consolePoint = ConvertToConsoleCoordinates(new Vector2((float)interpolatedAxisX, (float)value));
                if (IsConsolePointInBounds(consolePoint)) {
                    ConsoleSetCursorPositionInverted(consolePoint.X, consolePoint.Y);
                    Console.Write(symbol);
                }
                if (!IsVectorNaN(lastConsolePoint)) {
                    Vector2 startPoint = RoundVector(ClipConsolePointToBounds(lastConsolePoint));
                    Vector2 endPoint = RoundVector(ClipConsolePointToBounds(consolePoint));
                    (Vector2 shrunkStartPoint, Vector2 shrunkEndPoint, bool isZeroLength) = ShrinkVerticleLine(startPoint, endPoint, 1);
                    if (!isZeroLength) {
                        if (startPoint.Y < endPoint.Y)
                            (shrunkStartPoint, shrunkEndPoint) = (shrunkEndPoint, shrunkStartPoint);
                        RenderVerticalLine(shrunkStartPoint, shrunkEndPoint, symbol);
                    }

                } else
                    _ = 1 + 1;
                lastConsolePoint = consolePoint;
            }
        }
    }

    private void RenderLines(char symbol = '▒') {
        foreach ((Vector2 start, Vector2 end) in lines)
            RenderLine(start, end, symbol);
    }

    private void RenderLine(Vector2 start, Vector2 end, char symbol = '▒') {
        Vector2 consoleStart = ConvertToConsoleCoordinates(start);
        Vector2 consoleEnd = ConvertToConsoleCoordinates(end);
        consoleStart = RoundVector(ClipConsolePointToBounds(consoleStart));
        consoleEnd = RoundVector(ClipConsolePointToBounds(consoleEnd));
        if (consoleStart.X == consoleEnd.X) 
            RenderVerticalLine(consoleStart, consoleEnd, symbol);
        else if (consoleStart.Y == consoleEnd.Y)
            RenderHorizontalLine(consoleStart, consoleEnd, symbol);
        else
            RenderDiagonalLine(consoleStart, consoleEnd, symbol);
    }

    private void RenderAxes(char symbol = '▒') {
        Vector2 xAxisStart = new Vector2((float)xAxisMin, 0);
        Vector2 xAxisEnd = new Vector2((float)xAxisMax, 0);
        Vector2 yAxisStart = new Vector2(0, (float)yAxisMin);
        Vector2 yAxisEnd = new Vector2(0, (float)yAxisMax);
        RenderLine(xAxisStart, xAxisEnd with { X = xAxisEnd.X }, symbol);
        RenderLine(yAxisStart, yAxisEnd with { Y = yAxisEnd.Y }, symbol);
    }

    private void RenderGrid(char symbol = '░') {
        if (!double.IsFinite(GridXSpacing) || !double.IsFinite(GridYSpacing))
            return;
        for (double x = xAxisMin; x <= xAxisMax; x += GridXSpacing) {
            Vector2 start = new ((float)x, (float)yAxisMin);
            Vector2 end = new ((float)x, (float)yAxisMax);
            RenderLine(start, end, symbol);
        }
        for (double y = yAxisMin; y <= yAxisMax; y += GridYSpacing) {
            Vector2 start = new ((float)xAxisMin, (float)y);
            Vector2 end = new ((float)xAxisMax, (float)y);
            RenderLine(start, end, symbol);
        }
    }

    private void RenderAxesNumbers() {
        for (double x = xAxisMin; x <= xAxisMax; x += GridXSpacing) {
            RenderLabel(new Label(x.ToString(), new Vector2((float)x, 0)));
        }
        for (double y = yAxisMin; y <= yAxisMax; y += GridYSpacing) {
            RenderLabel(new Label(y.ToString(), new Vector2(0, (float)y)));
        }
    }

    /// <summary>
    /// Rendered line is inclusive of start and end points.
    /// </summary>
    /// <param name="consoleStart"></param>
    /// <param name="consoleEnd"></param>
    /// <param name="symbol"></param>
    private void RenderHorizontalLine(Vector2 consoleStart, Vector2 consoleEnd, char symbol = '▒') {
        float y = consoleStart.Y;
        float xStart = consoleStart.X;
        float xEnd = consoleEnd.X;
        ConsoleSetCursorPositionInverted(xStart, y);
        for (float x = xStart; x <= xEnd; x += 1) {
            Console.Write(symbol);
        }
    }

    /// <summary>
    /// Rendered line is inclusive of start and end points.
    /// </summary>
    /// <param name="consoleStart"></param>
    /// <param name="consoleEnd"></param>
    /// <param name="symbol"></param>
    private void RenderVerticalLine(Vector2 consoleStart, Vector2 consoleEnd, char symbol = '▒') {
        float x = consoleStart.X;
        float yStart = float.Min(consoleStart.Y, consoleEnd.Y);
        float yEnd = float.Max(consoleStart.Y, consoleEnd.Y);
        for (float y = yStart; y <= yEnd; y += 1) {
            ConsoleSetCursorPositionInverted(x, y);
            Console.Write(symbol);
        }
    }

    private void RenderDiagonalLine(Vector2 consoleStart, Vector2 consoleEnd, char symbol = '▒') {
        double slope = (consoleEnd.Y - consoleStart.Y) / (consoleEnd.X - consoleStart.X);
        double yIntercept = consoleStart.Y - (slope * consoleStart.X);
        for (int x = 0; x < ConsoleWidth; x += 1) {
            double y = (slope * x) + yIntercept;
            Vector2 consolePoint = ConvertToConsoleCoordinates(new Vector2(x, (float)y));
            if (!IsConsolePointInBounds(consolePoint))
                continue;
            ConsoleSetCursorPositionInverted(consolePoint.X, consolePoint.Y);
            Console.Write(symbol);
        }
    }
    #endregion

    private record struct Label(string Text, Vector2 Position, Vector2 RenderSpaceOffset = default);
}

public class ConsoleGraphAreaFills {
    public char FunctionSymbol { get; set; } = '█';
    public ConsoleColor? FunctionColor { get; set; } = ConsoleColor.Green;

    public char LineSymbol { get; set; } = '█';
    public ConsoleColor? LineColor { get; set; } = ConsoleColor.Green;

    public char AxesSymbol { get; set; } = '▒';
    public ConsoleColor? AxesColour { get; set; } = ConsoleColor.DarkGray;

    public char GridSymbol { get; set; } = '░';
    public ConsoleColor? GridColor { get; set; } = ConsoleColor.DarkGray;

    public char PointSymbol { get; set; } = '@';
    public ConsoleColor? PointColor { get; set; } = ConsoleColor.Yellow;

    public ConsoleColor? LabelColor { get; set; } = ConsoleColor.Red;
    public ConsoleColor? AxesNumberColor { get; set; } = ConsoleColor.Cyan;
}

public sealed class ConsoleColorScope : IDisposable {
    private readonly ConsoleColor previousForegroundColor;
    private readonly ConsoleColor previousBackgroundColor;

    public ConsoleColorScope(ConsoleColor? foregroundColour, ConsoleColor? backgroundColour = null) {
        previousForegroundColor = Console.ForegroundColor;
        previousBackgroundColor = Console.BackgroundColor;
        Console.ForegroundColor = foregroundColour ?? previousForegroundColor;
        Console.BackgroundColor = backgroundColour ?? previousBackgroundColor;
    }

    public void Dispose() {
        Console.ForegroundColor = previousForegroundColor;
        Console.BackgroundColor = previousBackgroundColor;
    }
}