using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace cli_life.Tests
{
    [TestClass]
    public class BoardTests
    {
        [TestMethod]
        public void TestRandomize()
        {
            var board = new Board(20, 20, 5, 0.5);
            int liveCount = board.CountLiveCells();
            Assert.IsTrue(liveCount >= 100 && liveCount <= 200);
        }

        [TestMethod]
        public void TestSymmetric()
        {
            var board = new Board(20, 20, 5, 0.5);
            int symmetricCount = board.Symmetric();
            Assert.IsTrue(symmetricCount >= 200 && symmetricCount <= 400);
        }
        
        [TestMethod]
        public void TestAdvance()
        {
        // Arrange
        var board = new Board(20, 20, 5, 0.5);
        var cell = board.Cells[0, 0];

        // Act
        bool initialLiveState = cell.IsAlive;
        int initialLiveNeighbors = cell.neighbors.Where(x => x.IsAlive).Count();
        board.Advance();
        bool newLiveState = cell.IsAlive;
        int newLiveNeighbors = cell.neighbors.Where(x => x.IsAlive).Count();

        // Assert
        if (initialLiveState)
        {
            Assert.IsTrue(newLiveState == (newLiveNeighbors == 2 || newLiveNeighbors == 3));
        }
        else
        {
            Assert.IsTrue(newLiveState == (newLiveNeighbors == 3));
        }
    }
        [TestMethod]
        public void TestAdvance()
        {
            var board = new Board(20, 20, 5, 0.5);
            var liveNeighbors = board.Cells[0, 0].neighbors.Where(x => x.IsAlive).Count();
            var initialLiveState = board.Cells[0, 0].IsAlive;
            board.Advance();
            var newLiveState = board.Cells[0, 0].IsAlive;
            var newLiveNeighbors = board.Cells[0, 0].neighbors.Where(x => x.IsAlive).Count();
            if (initialLiveState)
            {
                Assert.IsTrue(newLiveState == (newLiveNeighbors == 2 || newLiveNeighbors == 3));
            }
            else
            {
                Assert.IsTrue(newLiveState == (newLiveNeighbors == 3));
            }
        }

        [TestMethod]
        public void TestLoadBoardFromFile()
        {
            var board = new Board(20, 20, 5, 0.5);
            board.LoadBoardFromFile("gen.txt");
            Assert.IsTrue(board.Cells[0, 0].IsAlive);
            Assert.IsTrue(!board.Cells[1, 1].IsAlive);
        }
    }
}
