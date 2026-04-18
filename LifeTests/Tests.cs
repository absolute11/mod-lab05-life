using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using cli_life;
using Xunit;

namespace LifeTests
{
    public class CellTests
    {
        private static Cell MakeCell(bool isAlive, int aliveNeighbors)
        {
            var cell = new Cell { IsAlive = isAlive };
            for (int i = 0; i < 8; i++)
                cell.neighbors.Add(new Cell { IsAlive = i < aliveNeighbors });
            return cell;
        }

        private static void Step(Cell cell)
        {
            cell.DetermineNextLiveState();
            cell.Advance();
        }

        [Fact]
        public void AliveCellWith2NeighborsSurvives()
        {
            var c = MakeCell(true, 2);
            Step(c);
            Assert.True(c.IsAlive);
        }

        [Fact]
        public void AliveCellWith3NeighborsSurvives()
        {
            var c = MakeCell(true, 3);
            Step(c);
            Assert.True(c.IsAlive);
        }

        [Fact]
        public void AliveCellWith1NeighborDies()
        {
            var c = MakeCell(true, 1);
            Step(c);
            Assert.False(c.IsAlive);
        }

        [Fact]
        public void AliveCellWith4NeighborsDies()
        {
            var c = MakeCell(true, 4);
            Step(c);
            Assert.False(c.IsAlive);
        }

        [Fact]
        public void AliveCellWith0NeighborsDies()
        {
            var c = MakeCell(true, 0);
            Step(c);
            Assert.False(c.IsAlive);
        }

        [Fact]
        public void DeadCellWith3NeighborsBecomeAlive()
        {
            var c = MakeCell(false, 3);
            Step(c);
            Assert.True(c.IsAlive);
        }

        [Fact]
        public void DeadCellWith2NeighborsStaysDead()
        {
            var c = MakeCell(false, 2);
            Step(c);
            Assert.False(c.IsAlive);
        }

        [Fact]
        public void DeadCellWith4NeighborsStaysDead()
        {
            var c = MakeCell(false, 4);
            Step(c);
            Assert.False(c.IsAlive);
        }
    }

    public class BoardTests
    {
        [Fact]
        public void CountAliveReturnsCorrectCount()
        {
            var board = new Board(10, 10, 1, 0.0);
            board.Cells[0, 0].IsAlive = true;
            board.Cells[5, 5].IsAlive = true;
            Assert.Equal(2, board.CountAlive());
        }

        [Fact]
        public void RandomizeWithZeroDensityAllDead()
        {
            var board = new Board(10, 10, 1, 0.0);
            Assert.Equal(0, board.CountAlive());
        }

        [Fact]
        public void RandomizeWithFullDensityAllAlive()
        {
            var board = new Board(10, 10, 1, 1.0);
            Assert.Equal(100, board.CountAlive());
        }

        [Fact]
        public void BoardDimensionsAreCorrect()
        {
            var board = new Board(50, 20, 1, 0.0);
            Assert.Equal(50, board.Columns);
            Assert.Equal(20, board.Rows);
        }

        [Fact]
        public void SaveAndLoadRoundtrip()
        {
            var board = new Board(10, 10, 1, 0.0);
            board.Cells[2, 3].IsAlive = true;
            board.Cells[7, 8].IsAlive = true;

            string path = Path.GetTempFileName();
            try
            {
                board.SaveToFile(path);
                var loaded = Board.LoadFromFile(path);
                Assert.Equal(board.CountAlive(), loaded.CountAlive());
                Assert.True(loaded.Cells[2, 3].IsAlive);
                Assert.True(loaded.Cells[7, 8].IsAlive);
                Assert.False(loaded.Cells[0, 0].IsAlive);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void GetComponentsCountsIsolatedCells()
        {
            var board = new Board(20, 20, 1, 0.0);
            board.Cells[2, 2].IsAlive = true;
            board.Cells[10, 10].IsAlive = true;
            var components = board.GetComponents();
            Assert.Equal(2, components.Count);
        }

        [Fact]
        public void GetComponentsConnectedCellsAreOneComponent()
        {
            var board = new Board(20, 20, 1, 0.0);
            board.Cells[5, 5].IsAlive = true;
            board.Cells[6, 5].IsAlive = true;
            board.Cells[7, 5].IsAlive = true;
            var components = board.GetComponents();
            Assert.Single(components);
            Assert.Equal(3, components[0].Count);
        }

        [Fact]
        public void BlinkerOscillates()
        {
            var board = new Board(10, 10, 1, 0.0);
            board.Cells[3, 5].IsAlive = true;
            board.Cells[4, 5].IsAlive = true;
            board.Cells[5, 5].IsAlive = true;

            board.Advance();

            Assert.True(board.Cells[4, 4].IsAlive);
            Assert.True(board.Cells[4, 5].IsAlive);
            Assert.True(board.Cells[4, 6].IsAlive);
            Assert.False(board.Cells[3, 5].IsAlive);
            Assert.False(board.Cells[5, 5].IsAlive);
        }

        [Fact]
        public void BlockIsStillLife()
        {
            var board = new Board(10, 10, 1, 0.0);
            board.Cells[4, 4].IsAlive = true;
            board.Cells[5, 4].IsAlive = true;
            board.Cells[4, 5].IsAlive = true;
            board.Cells[5, 5].IsAlive = true;

            board.Advance();

            Assert.True(board.Cells[4, 4].IsAlive);
            Assert.True(board.Cells[5, 4].IsAlive);
            Assert.True(board.Cells[4, 5].IsAlive);
            Assert.True(board.Cells[5, 5].IsAlive);
            Assert.Equal(4, board.CountAlive());
        }

        [Fact]
        public void TorusTopologyWrapsAround()
        {
            var board = new Board(10, 10, 1, 0.0);
            board.Cells[9, 9].IsAlive = true;
            board.Cells[9, 0].IsAlive = true;
            board.Cells[0, 9].IsAlive = true;

            board.Advance();

            Assert.True(board.Cells[0, 0].IsAlive);
        }

        [Fact]
        public void EmptyBoardRemainsEmpty()
        {
            var board = new Board(10, 10, 1, 0.0);
            board.Advance();
            Assert.Equal(0, board.CountAlive());
        }
    }

    public class FigureClassifierTests
    {
        [Fact]
        public void ClassifiesBlock()
        {
            var comp = new List<(int, int)> { (0, 0), (1, 0), (0, 1), (1, 1) };
            Assert.Equal("Block", FigureClassifier.Classify(comp));
        }

        [Fact]
        public void ClassifiesBlinker()
        {
            var comp = new List<(int, int)> { (0, 0), (1, 0), (2, 0) };
            Assert.Equal("Blinker", FigureClassifier.Classify(comp));
        }

        [Fact]
        public void ClassifiesBlinkerVertical()
        {
            var comp = new List<(int, int)> { (0, 0), (0, 1), (0, 2) };
            Assert.Equal("Blinker", FigureClassifier.Classify(comp));
        }

        [Fact]
        public void ClassifiesBeehive()
        {
            var comp = new List<(int, int)> { (1, 0), (2, 0), (0, 1), (3, 1), (1, 2), (2, 2) };
            Assert.Equal("Beehive", FigureClassifier.Classify(comp));
        }

        [Fact]
        public void ClassifiesUnknown()
        {
            var comp = new List<(int, int)> { (0, 0), (2, 0), (4, 0) };
            Assert.Equal("Unknown", FigureClassifier.Classify(comp));
        }
    }

    public class StabilityAnalyzerTests
    {
        [Fact]
        public void EmptyBoardIsImmediatelyStable()
        {
            var board = new Board(10, 10, 1, 0.0);
            int gen = StabilityAnalyzer.GenerationsToStability(board, 100, 5);
            Assert.True(gen > 0);
        }

        [Fact]
        public void BlockIsImmediatelyStable()
        {
            var board = new Board(10, 10, 1, 0.0);
            board.Cells[4, 4].IsAlive = true;
            board.Cells[5, 4].IsAlive = true;
            board.Cells[4, 5].IsAlive = true;
            board.Cells[5, 5].IsAlive = true;
            int gen = StabilityAnalyzer.GenerationsToStability(board, 100, 5);
            Assert.True(gen > 0);
        }

        [Fact]
        public void RunExperimentReturnsDensityKeys()
        {
            double[] densities = { 0.1, 0.5, 0.9 };
            var result = StabilityAnalyzer.RunExperiment(20, 20, 1, densities, runs: 3, maxGen: 100, stableWindow: 5);
            Assert.Equal(3, result.Count);
            foreach (var d in densities)
                Assert.True(result.ContainsKey(d));
        }
    }

    public class GameSettingsTests
    {
        [Fact]
        public void DefaultSettingsHaveExpectedValues()
        {
            var s = new GameSettings();
            Assert.Equal(50, s.Width);
            Assert.Equal(20, s.Height);
            Assert.Equal(1, s.CellSize);
            Assert.Equal(0.5, s.Density);
        }

        [Fact]
        public void LoadReturnsDefaultsWhenFileNotFound()
        {
            var s = GameSettings.Load("nonexistent_xyz_12345.json");
            Assert.Equal(50, s.Width);
            Assert.Equal(20, s.Height);
        }

        [Fact]
        public void SaveAndLoadRoundtrip()
        {
            var s = new GameSettings { Width = 100, Height = 50, CellSize = 2, Density = 0.3 };
            string path = Path.GetTempFileName();
            try
            {
                s.Save(path);
                var loaded = GameSettings.Load(path);
                Assert.Equal(100, loaded.Width);
                Assert.Equal(50, loaded.Height);
                Assert.Equal(2, loaded.CellSize);
                Assert.Equal(0.3, loaded.Density, precision: 5);
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}
