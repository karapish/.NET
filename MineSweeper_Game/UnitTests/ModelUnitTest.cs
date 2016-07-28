using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minesweeper.Model;
using Minesweeper.UI;

namespace UnitTests
{
    [TestClass]
    public class ModelUnitTest
    {
        [TestMethod]
        public void CanCreateMap()
        {
            const int mapSize = 10;
            var map = new MineField(mapSize);
            Assert.AreEqual(map.NumberMines, mapSize / 2);
        }

        [TestMethod]
        public void CanCreateViewMapAndAutodiscover()
        {
            var viewmap = new ViewModel(3);
            viewmap.Autodiscover(0, 0);
        }
    }
}
