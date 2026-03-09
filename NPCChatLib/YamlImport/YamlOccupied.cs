using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPCChatLib.YamlImport;

namespace NPCChatLib.YamlImport
{
    public class YamlOccupied : Dictionary<Point, IYamlObject>
    {
        public void Add(IYamlObject o)
        {
            var added = new Stack<Point>();
            foreach (var point in GetAllObjectPoints(o.Location, o.Size))
            {
                if (TryGetValue(point, out var existingObject))
                {
                    while (added.Any())
                        Remove(added.Pop());
                    throw new Exception(
                        $"Object [{o.Description}] overlaps with object {existingObject.Description}");
                }
                Add(point, o);
                added.Push(point);
            }
        }

        public IEnumerable<Point> GetAllObjectPoints(Point point, Size size)
        {
            for (var y = point.Y; y < point.Y + size.Width; ++y)
            {
                for (var x = point.X; x < point.X + size.Height; ++x)
                {
                    yield return new Point(x, y);
                }
            }
        }
    }
}
