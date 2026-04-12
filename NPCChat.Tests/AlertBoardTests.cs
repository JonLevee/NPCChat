using System.Drawing;
using NPCChat.Core.AIClasses;

namespace NPCChat.Tests
{
    [TestClass]
    public class AlertBoardTests
    {
        private static AlertEvent Alert(int x, int y, AlertKind kind = AlertKind.NoiseHeard)
            => new AlertEvent { Position = new Point(x, y), Kind = kind, Tick = 1 };

        // ── Double buffer ────────────────────────────────────────────────────────

        [TestMethod]
        public void GetNearbyAlerts_BeforeAnyTick_ReturnsEmpty()
        {
            var board = new AlertBoard();
            board.Post(Alert(0, 0));
            // BeginTick not called yet — read buffer is empty
            var result = board.GetNearbyAlerts(new Point(0, 0), 10);
            Assert.AreEqual(0, result.Length);
        }

        [TestMethod]
        public void BeginTick_MakesPostedAlertsVisible()
        {
            var board = new AlertBoard();
            board.Post(Alert(0, 0));
            board.BeginTick();
            var result = board.GetNearbyAlerts(new Point(0, 0), 10);
            Assert.AreEqual(1, result.Length);
        }

        [TestMethod]
        public void BeginTick_ClearsPreviousTickAlerts()
        {
            var board = new AlertBoard();
            board.Post(Alert(0, 0));
            board.BeginTick();           // tick 1: alert visible
            board.BeginTick();           // tick 2: no new posts, read buffer cleared
            var result = board.GetNearbyAlerts(new Point(0, 0), 10);
            Assert.AreEqual(0, result.Length);
        }

        [TestMethod]
        public void Post_AfterBeginTick_NotVisibleUntilNextTick()
        {
            var board = new AlertBoard();
            board.BeginTick();
            board.Post(Alert(5, 5));     // posted to write buffer after swap
            var result = board.GetNearbyAlerts(new Point(5, 5), 10);
            Assert.AreEqual(0, result.Length); // not yet visible
        }

        // ── Proximity filtering ──────────────────────────────────────────────────

        [TestMethod]
        public void GetNearbyAlerts_NoAlerts_ReturnsEmpty()
        {
            var board = new AlertBoard();
            board.BeginTick();
            Assert.AreEqual(0, board.GetNearbyAlerts(new Point(0, 0), 10).Length);
        }

        [TestMethod]
        public void GetNearbyAlerts_AlertWithinRadius_Returned()
        {
            var board = new AlertBoard();
            board.Post(Alert(3, 0));
            board.BeginTick();
            var result = board.GetNearbyAlerts(new Point(0, 0), 5);
            Assert.AreEqual(1, result.Length);
        }

        [TestMethod]
        public void GetNearbyAlerts_AlertBeyondRadius_NotReturned()
        {
            var board = new AlertBoard();
            board.Post(Alert(10, 0));
            board.BeginTick();
            var result = board.GetNearbyAlerts(new Point(0, 0), 5);
            Assert.AreEqual(0, result.Length);
        }

        [TestMethod]
        public void GetNearbyAlerts_AlertAtExactRadius_Included()
        {
            var board = new AlertBoard();
            board.Post(Alert(5, 0));
            board.BeginTick();
            // Chebyshev distance = 5, radius = 5 → should be included
            var result = board.GetNearbyAlerts(new Point(0, 0), 5);
            Assert.AreEqual(1, result.Length);
        }

        [TestMethod]
        public void GetNearbyAlerts_UsesChebyshevDistance()
        {
            var board = new AlertBoard();
            // Chebyshev distance from (0,0) to (5,5) = max(5,5) = 5
            board.Post(Alert(5, 5));
            board.BeginTick();
            var result = board.GetNearbyAlerts(new Point(0, 0), 5);
            Assert.AreEqual(1, result.Length);

            // Manhattan would be 10, so this confirms Chebyshev is used
        }

        [TestMethod]
        public void GetNearbyAlerts_MultipleAlerts_FiltersCorrectly()
        {
            var board = new AlertBoard();
            board.Post(Alert(2, 0));   // within radius 5
            board.Post(Alert(10, 0));  // outside radius 5
            board.Post(Alert(0, 4));   // within radius 5
            board.BeginTick();
            var result = board.GetNearbyAlerts(new Point(0, 0), 5);
            Assert.AreEqual(2, result.Length);
        }

        [TestMethod]
        public void GetNearbyAlerts_AlertKind_PreservedInResult()
        {
            var board = new AlertBoard();
            board.Post(new AlertEvent
            {
                Position = new Point(0, 0),
                Kind     = AlertKind.PlayerDetected,
                Tick     = 42
            });
            board.BeginTick();
            var result = board.GetNearbyAlerts(new Point(0, 0), 1);
            Assert.AreEqual(AlertKind.PlayerDetected, result[0].Kind);
            Assert.AreEqual(42, result[0].Tick);
        }
    }
}
