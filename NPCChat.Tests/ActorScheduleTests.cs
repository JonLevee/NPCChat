using NPCChat.Core.BehaviorClasses;

namespace NPCChat.Tests
{
    [TestClass]
    public class ActorScheduleTests
    {
        [TestMethod]
        public void GetModeForTime_NoEntries_ReturnsEmpty()
        {
            var schedule = new ActorSchedule();
            Assert.AreEqual(string.Empty, schedule.GetModeForTime(12));
        }

        [TestMethod]
        public void GetModeForTime_HourInsideRange_ReturnsMode()
        {
            var schedule = new ActorSchedule()
                .Add(new GameTimeRange(9, 17), "work");
            Assert.AreEqual("work", schedule.GetModeForTime(12));
        }

        [TestMethod]
        public void GetModeForTime_HourAtStartOfRange_ReturnsMode()
        {
            var schedule = new ActorSchedule()
                .Add(new GameTimeRange(9, 17), "work");
            Assert.AreEqual("work", schedule.GetModeForTime(9));
        }

        [TestMethod]
        public void GetModeForTime_HourAtEndOfRange_ReturnsMode()
        {
            var schedule = new ActorSchedule()
                .Add(new GameTimeRange(9, 17), "work");
            Assert.AreEqual("work", schedule.GetModeForTime(17));
        }

        [TestMethod]
        public void GetModeForTime_HourOutsideRange_ReturnsEmpty()
        {
            var schedule = new ActorSchedule()
                .Add(new GameTimeRange(9, 17), "work");
            Assert.AreEqual(string.Empty, schedule.GetModeForTime(8));
            Assert.AreEqual(string.Empty, schedule.GetModeForTime(18));
        }

        [TestMethod]
        public void GetModeForTime_MultipleEntries_FirstMatchWins()
        {
            var schedule = new ActorSchedule()
                .Add(new GameTimeRange(0, 23), "catch-all")
                .Add(new GameTimeRange(9, 17), "work");
            // "catch-all" is added first and covers everything — it wins
            Assert.AreEqual("catch-all", schedule.GetModeForTime(12));
        }

        [TestMethod]
        public void GetModeForTime_MultipleEntries_SecondMatchUsedWhenFirstMisses()
        {
            var schedule = new ActorSchedule()
                .Add(new GameTimeRange(9, 17), "work")
                .Add(new GameTimeRange(18, 22), "relax");
            Assert.AreEqual("relax", schedule.GetModeForTime(20));
        }

        [TestMethod]
        public void GetModeForTime_EdgeHour0_MatchesOvernightRange()
        {
            // Overnight range: 22 to 6
            var schedule = new ActorSchedule()
                .Add(new GameTimeRange(22, 6), "sleep");
            Assert.AreEqual("sleep", schedule.GetModeForTime(0));
        }

        [TestMethod]
        public void GetModeForTime_EdgeHour23_MatchesOvernightRange()
        {
            var schedule = new ActorSchedule()
                .Add(new GameTimeRange(22, 6), "sleep");
            Assert.AreEqual("sleep", schedule.GetModeForTime(23));
        }

        [TestMethod]
        public void GetModeForTime_OvernightRange_HourInMiddleOfNight_ReturnsMode()
        {
            var schedule = new ActorSchedule()
                .Add(new GameTimeRange(22, 6), "sleep");
            Assert.AreEqual("sleep", schedule.GetModeForTime(3));
        }

        [TestMethod]
        public void GetModeForTime_OvernightRange_HourOutsideRange_ReturnsEmpty()
        {
            var schedule = new ActorSchedule()
                .Add(new GameTimeRange(22, 6), "sleep");
            Assert.AreEqual(string.Empty, schedule.GetModeForTime(12));
        }
    }
}
