using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ReedSolomonEncoding.UI;
internal interface IGraphArea {
    public void AddLabelAt(string label, Vector2 position, Vector2 renderSpaceOffset);
    public void AddPointAt(Vector2 position);
    public void AddFunction(Func<double, double> function);
    public void SetBounds(double xAxisMin, double xAxisMax, double yAxisMin, double yAxisMax);
    public void AddLine(Vector2 start, Vector2 end);
    public void SetCenter(Vector2 centerPoint);
    public bool HasAxes { get; set; }
    public bool HasGrid { get; set; }
    public bool HasAxesNumbers { get; set; }
    public double GridXSpacing { get; set; }
    public double GridYSpacing { get; set; }
}
