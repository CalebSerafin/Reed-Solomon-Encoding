using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ReedSolomonEncoding.UI;
internal interface IGraphArea {
    public void AddLabelAt(string label, Vector2 position);
    public void AddPointAt(Vector2 position);
    public void AddFunction(Func<double, double> function);
    public void SetBounds(double xAxisMin, double xAxisMax, double yAxisMin, double yAxisMax);
    public void AddLine(Vector2 start, Vector2 end);
    public void SetCenter(Vector2 centerPoint);
    public bool DrawAxes { get; set; }
    public bool DrawGrid { get; set; }
    public float GridXSpacing { get; set; }
    public float GridYSpacing { get; set; }
}
