using System.Drawing;
using NPCChat.Core.Validation;
using NPCChat.Core.LocalEventArgs;

namespace NPCChat.Core.YamlImport
{
    public class YamlOccupied
    {
        private Dictionary<Point, IYamlObject> occupied = [];
        public event EventHandler<ChangeEventArgs<IYamlObject>> ObjectAdded;
        public event EventHandler<ChangeEventArgs<IYamlObject>> ObjectRemoved;
        public event EventHandler<ChangeEventArgs<IYamlObject>> ObjectUpdated;

        public bool IsLocationOccupied(Point location, Size size, out IYamlObject overlapObject)
        {
            var overlappedPoint = GetAllObjectPoints(location, size).FirstOrDefault(occupied.ContainsKey);
            overlapObject = overlappedPoint != Point.Empty ? occupied[overlappedPoint] : null;
            return overlapObject != null;
        }

        public bool ContainsKey(Point point) => occupied.ContainsKey(point);

        public void Add(IYamlObject o)
        {
            if (!InternalTryAdd(o, out string errorMessage))
                throw new InvalidOperationException($"Failed to add object [{o.Description}]: {errorMessage}");
            ObjectAdded?.Invoke(this, new ChangeEventArgs<IYamlObject>(null, o));
        }

        public void Remove(IYamlObject o)
        {
            if (!InternalTryRemove(o, out string errorMessage))
            {
                throw new InvalidOperationException($"Failed to remove object [{o.Description}]: {errorMessage}");
            }
            ObjectRemoved?.Invoke(this, new ChangeEventArgs<IYamlObject>(o, null));
        }

        public void Update<T>(T oldObject, Action<T> updateAction) where T : YamlObjectBase
        {
            if (!InternalTryUpdate(oldObject, updateAction, out T newObject, out string errorMessage))
            {
                throw new InvalidOperationException($"Failed to update object [{oldObject.Description}]: {errorMessage}");
            }
            ObjectUpdated?.Invoke(this, new ChangeEventArgs<IYamlObject>(oldObject, newObject));
        }

        public IEnumerable<Point> GetAllObjectPoints(Point point, Size size)
        {
            Require.AreNotEqual(Size.Empty, size, $"Invalid Size: {size}");
            Require.AreNotEqual(Point.Empty, point, $"Invalid Point: {point}");
            for (var y = point.Y; y < point.Y + size.Width; ++y)
            {
                for (var x = point.X; x < point.X + size.Height; ++x)
                {
                    yield return new Point(x, y);
                }
            }
        }

        private bool InternalTryAdd(IYamlObject o, out string errorMessage)
        {
            var points = GetAllObjectPoints(o.Location, o.Size).ToList();
            var overlapObject = points
                .Select(p => occupied.TryGetValue(p, out var existingObject) ? existingObject : null)
                .FirstOrDefault(existingObject => existingObject != null);
            if (overlapObject != null)
            {
                errorMessage = $"Object [{o.Description}] overlaps with object {overlapObject.Description}";
                return false;
            }
            points.ForEach(p => occupied.Add(p, o));
            errorMessage = null;
            return true;
        }

        private bool InternalTryRemove(IYamlObject o, out string errorMessage)
        {
            var points = GetAllObjectPoints(o.Location, o.Size).ToList();
            foreach (var point in points)
            {
                if (!occupied.TryGetValue(point, out IYamlObject existingObject))
                {
                    errorMessage = $"Object [{o.Description}] does not have point {point} in occupied";
                    return false;
                }
                existingObject ??= YamlObject.Empty;
                if (existingObject != o)
                {
                    errorMessage = $"Object [{o.Description}] does not match object {existingObject.Description} at point {point} in occupied";
                    return false;
                }
            }
            errorMessage = null;
            points.ForEach(p => occupied.Remove(p));
            return true;
        }

        private bool InternalTryUpdate<T>(T o, Action<T> updateAction, out T updated, out string errorMessage) where T : YamlObjectBase
        {
            errorMessage = null;
            updated = o.Clone() as T;
            if (InternalTryRemove(o, out errorMessage))
            {
                updateAction(updated);
                if (InternalTryAdd(updated, out errorMessage))
                {
                    return true;
                }
                if (!InternalTryAdd(o, out string addError))
                {
                    errorMessage += $" Failed to revert original object: {addError}";
                }
            }
            return false;
        }
    }
}
