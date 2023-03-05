using Knapcode.FactorioTools.OilField.Data;
using Knapcode.FactorioTools.OilField.Grid;

namespace Knapcode.FactorioTools.OilField.Steps;

public class PlanUndergroundPipesTest
{
    public class Execute : Facts
    {
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void Horizontal_TooShort(int length)
        {
            var pipes = new HashSet<Location>();
            AddPipes(pipes, minX: 1, maxX: length, minY: 1, maxY: 1);
            var context = GetContext(pipes);
            var originalPipes = new HashSet<Location>(pipes);

            Run(context, pipes);

            Assert.All(originalPipes, p => Assert.IsType<Pipe>(context.Grid[p]));
            Assert.True(originalPipes.SetEquals(pipes));
        }

        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(9)]
        [InlineData(10)]
        [InlineData(11)]
        public void Horizontal_OneRun(int length)
        {
            var pipes = new HashSet<Location>();
            AddPipes(pipes, minX: 1, maxX: length, minY: 1, maxY: 1);
            var context = GetContext(pipes);

            Run(context, pipes);

            Assert.Equal(2, context.Grid.EntityToLocation.Count);
            Assert.All(context.Grid.EntityToLocation.Keys, e => Assert.IsType<UndergroundPipe>(e));

            var left = Assert.IsType<UndergroundPipe>(context.Grid[new Location(1, 1)]);
            Assert.Equal(Direction.Left, left.Direction);
            var right = Assert.IsType<UndergroundPipe>(context.Grid[new Location(length, 1)]);
            Assert.Equal(Direction.Right, right.Direction);
        }


        [Theory]
        [InlineData(12)]
        [InlineData(13)]
        public void Horizontal_OneRun_WithExtra(int length)
        {
            var pipes = new HashSet<Location>();
            AddPipes(pipes, minX: 1, maxX: length, minY: 1, maxY: 1);
            var context = GetContext(pipes);

            Run(context, pipes);

            Assert.Equal(2 + (length - 11), context.Grid.EntityToLocation.Count);
            Assert.All(context.Grid.EntityToLocation.Keys, e => Assert.IsAssignableFrom<Pipe>(e));
            Assert.Equal(2, context.Grid.EntityToLocation.Keys.Count(e => e is UndergroundPipe));
            Assert.Equal(2 + (length - 11), context.Grid.EntityToLocation.Keys.Count(e => e is Pipe));

            var left = Assert.IsType<UndergroundPipe>(context.Grid[new Location(1, 1)]);
            Assert.Equal(Direction.Left, left.Direction);
            var right = Assert.IsType<UndergroundPipe>(context.Grid[new Location(11, 1)]);
            Assert.Equal(Direction.Right, right.Direction);

            Assert.All(Enumerable.Range(12, length - 11), x => Assert.IsType<Pipe>(context.Grid[new Location(x, 1)]));
        }

        [Theory]
        [InlineData(14)]
        [InlineData(15)]
        [InlineData(16)]
        [InlineData(17)]
        [InlineData(18)]
        [InlineData(19)]
        [InlineData(20)]
        [InlineData(21)]
        [InlineData(22)]
        public void Horizontal_TwoRuns(int length)
        {
            var pipes = new HashSet<Location>();
            AddPipes(pipes, minX: 1, maxX: length, minY: 1, maxY: 1);
            var context = GetContext(pipes);

            Run(context, pipes);

            Assert.Equal(4, context.Grid.EntityToLocation.Count);
            Assert.All(context.Grid.EntityToLocation.Keys, e => Assert.IsAssignableFrom<Pipe>(e));
            Assert.Equal(4, context.Grid.EntityToLocation.Keys.Count(e => e is UndergroundPipe));

            var leftA = Assert.IsType<UndergroundPipe>(context.Grid[new Location(1, 1)]);
            Assert.Equal(Direction.Left, leftA.Direction);
            var rightA = Assert.IsType<UndergroundPipe>(context.Grid[new Location(11, 1)]);
            Assert.Equal(Direction.Right, rightA.Direction);

            var leftB = Assert.IsType<UndergroundPipe>(context.Grid[new Location(12, 1)]);
            Assert.Equal(Direction.Left, leftB.Direction);
            var rightB = Assert.IsType<UndergroundPipe>(context.Grid[new Location(length, 1)]);
            Assert.Equal(Direction.Right, rightB.Direction);
        }


        [Theory]
        [InlineData(23)]
        [InlineData(24)]
        public void Horizontal_TwoRuns_WithExtra(int length)
        {
            var pipes = new HashSet<Location>();
            AddPipes(pipes, minX: 1, maxX: length, minY: 1, maxY: 1);
            var context = GetContext(pipes);

            Run(context, pipes);

            Assert.Equal(4 + (length - 22), context.Grid.EntityToLocation.Count);
            Assert.All(context.Grid.EntityToLocation.Keys, e => Assert.IsAssignableFrom<Pipe>(e));
            Assert.Equal(4, context.Grid.EntityToLocation.Keys.Count(e => e is UndergroundPipe));
            Assert.Equal(4 + (length - 22), context.Grid.EntityToLocation.Keys.Count(e => e is Pipe));

            var leftA = Assert.IsType<UndergroundPipe>(context.Grid[new Location(1, 1)]);
            Assert.Equal(Direction.Left, leftA.Direction);
            var rightA = Assert.IsType<UndergroundPipe>(context.Grid[new Location(11, 1)]);
            Assert.Equal(Direction.Right, rightA.Direction);

            var leftB = Assert.IsType<UndergroundPipe>(context.Grid[new Location(12, 1)]);
            Assert.Equal(Direction.Left, leftB.Direction);
            var rightB = Assert.IsType<UndergroundPipe>(context.Grid[new Location(22, 1)]);
            Assert.Equal(Direction.Right, rightB.Direction);

            Assert.All(Enumerable.Range(23, length - 22), x => Assert.IsType<Pipe>(context.Grid[new Location(x, 1)]));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void Vertical_TooShort(int length)
        {
            var pipes = new HashSet<Location>();
            AddPipes(pipes, minX: 1, maxX: 1, minY: 1, maxY: length);
            var context = GetContext(pipes);
            var originalPipes = new HashSet<Location>(pipes);

            Run(context, pipes);

            Assert.All(originalPipes, p => Assert.IsType<Pipe>(context.Grid[p]));
            Assert.True(originalPipes.SetEquals(pipes));
        }

        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(9)]
        [InlineData(10)]
        [InlineData(11)]
        public void Vertical_OneRun(int length)
        {
            var pipes = new HashSet<Location>();
            AddPipes(pipes, minX: 1, maxX: 1, minY: 1, maxY: length);
            var context = GetContext(pipes);

            Run(context, pipes);

            Assert.Equal(2, context.Grid.EntityToLocation.Count);
            Assert.All(context.Grid.EntityToLocation.Keys, e => Assert.IsType<UndergroundPipe>(e));

            var up = Assert.IsType<UndergroundPipe>(context.Grid[new Location(1, 1)]);
            Assert.Equal(Direction.Up, up.Direction);
            var down = Assert.IsType<UndergroundPipe>(context.Grid[new Location(1, length)]);
            Assert.Equal(Direction.Down, down.Direction);
        }


        [Theory]
        [InlineData(12)]
        [InlineData(13)]
        public void Vertical_OneRun_WithExtra(int length)
        {
            var pipes = new HashSet<Location>();
            AddPipes(pipes, minX: 1, maxX: 1, minY: 1, maxY: length);
            var context = GetContext(pipes);

            Run(context, pipes);

            Assert.Equal(2 + (length - 11), context.Grid.EntityToLocation.Count);
            Assert.All(context.Grid.EntityToLocation.Keys, e => Assert.IsAssignableFrom<Pipe>(e));
            Assert.Equal(2, context.Grid.EntityToLocation.Keys.Count(e => e is UndergroundPipe));
            Assert.Equal(2 + (length - 11), context.Grid.EntityToLocation.Keys.Count(e => e is Pipe));

            var up = Assert.IsType<UndergroundPipe>(context.Grid[new Location(1, 1)]);
            Assert.Equal(Direction.Up, up.Direction);
            var down = Assert.IsType<UndergroundPipe>(context.Grid[new Location(1, 11)]);
            Assert.Equal(Direction.Down, down.Direction);

            Assert.All(Enumerable.Range(12, length - 11), y => Assert.IsType<Pipe>(context.Grid[new Location(1, y)]));
        }

        [Theory]
        [InlineData(14)]
        [InlineData(15)]
        [InlineData(16)]
        [InlineData(17)]
        [InlineData(18)]
        [InlineData(19)]
        [InlineData(20)]
        [InlineData(21)]
        [InlineData(22)]
        public void Vertical_TwoRuns(int length)
        {
            var pipes = new HashSet<Location>();
            AddPipes(pipes, minX: 1, maxX: 1, minY: 1, maxY: length);
            var context = GetContext(pipes);

            Run(context, pipes);

            Assert.Equal(4, context.Grid.EntityToLocation.Count);
            Assert.All(context.Grid.EntityToLocation.Keys, e => Assert.IsAssignableFrom<Pipe>(e));
            Assert.Equal(4, context.Grid.EntityToLocation.Keys.Count(e => e is UndergroundPipe));

            var upA = Assert.IsType<UndergroundPipe>(context.Grid[new Location(1, 1)]);
            Assert.Equal(Direction.Up, upA.Direction);
            var downA = Assert.IsType<UndergroundPipe>(context.Grid[new Location(1, 11)]);
            Assert.Equal(Direction.Down, downA.Direction);

            var upB = Assert.IsType<UndergroundPipe>(context.Grid[new Location(1, 12)]);
            Assert.Equal(Direction.Up, upB.Direction);
            var downB = Assert.IsType<UndergroundPipe>(context.Grid[new Location(1, length)]);
            Assert.Equal(Direction.Down, downB.Direction);
        }

        [Theory]
        [InlineData(23)]
        [InlineData(24)]
        public void Vertical_TwoRuns_WithExtra(int length)
        {
            var pipes = new HashSet<Location>();
            AddPipes(pipes, minX: 1, maxX: 1, minY: 1, maxY: length);
            var context = GetContext(pipes);

            Run(context, pipes);

            Assert.Equal(4 + (length - 22), context.Grid.EntityToLocation.Count);
            Assert.All(context.Grid.EntityToLocation.Keys, e => Assert.IsAssignableFrom<Pipe>(e));
            Assert.Equal(4, context.Grid.EntityToLocation.Keys.Count(e => e is UndergroundPipe));
            Assert.Equal(4 + (length - 22), context.Grid.EntityToLocation.Keys.Count(e => e is Pipe));

            var upA = Assert.IsType<UndergroundPipe>(context.Grid[new Location(1, 1)]);
            Assert.Equal(Direction.Up, upA.Direction);
            var downA = Assert.IsType<UndergroundPipe>(context.Grid[new Location(1, 11)]);
            Assert.Equal(Direction.Down, downA.Direction);

            var upB = Assert.IsType<UndergroundPipe>(context.Grid[new Location(1, 12)]);
            Assert.Equal(Direction.Up, upB.Direction);
            var downB = Assert.IsType<UndergroundPipe>(context.Grid[new Location(1, 22)]);
            Assert.Equal(Direction.Down, downB.Direction);

            Assert.All(Enumerable.Range(23, length - 22), y => Assert.IsType<Pipe>(context.Grid[new Location(1, y)]));
        }

        [Fact]
        public void Intersection_Plus()
        {
            var pipes = new HashSet<Location>();
            AddPipes(pipes, minX: 1, maxX: 11, minY: 6, maxY: 6);
            AddPipes(pipes, minX: 6, maxX: 6, minY: 1, maxY: 11);
            var context = GetContext(pipes);

            Run(context, pipes);

            Assert.Equal(9, context.Grid.EntityToLocation.Count);
            Assert.All(context.Grid.EntityToLocation.Keys, e => Assert.IsAssignableFrom<Pipe>(e));

            Assert.Equal(Direction.Left, Assert.IsType<UndergroundPipe>(context.Grid[new Location(1, 6)]).Direction);
            Assert.Equal(Direction.Right, Assert.IsType<UndergroundPipe>(context.Grid[new Location(5, 6)]).Direction);

            Assert.Equal(Direction.Left, Assert.IsType<UndergroundPipe>(context.Grid[new Location(7, 6)]).Direction);
            Assert.Equal(Direction.Right, Assert.IsType<UndergroundPipe>(context.Grid[new Location(11, 6)]).Direction);

            Assert.Equal(Direction.Up, Assert.IsType<UndergroundPipe>(context.Grid[new Location(6, 1)]).Direction);
            Assert.Equal(Direction.Down, Assert.IsType<UndergroundPipe>(context.Grid[new Location(6, 5)]).Direction);

            Assert.Equal(Direction.Up, Assert.IsType<UndergroundPipe>(context.Grid[new Location(6, 7)]).Direction);
            Assert.Equal(Direction.Down, Assert.IsType<UndergroundPipe>(context.Grid[new Location(6, 11)]).Direction);

            Assert.IsType<Pipe>(context.Grid[new Location(6, 6)]);
        }

        [Fact]
        public void Intersection_T()
        {
            var pipes = new HashSet<Location>();
            AddPipes(pipes, minX: 1, maxX: 11, minY: 6, maxY: 6);
            AddPipes(pipes, minX: 11, maxX: 11, minY: 1, maxY: 11);
            var context = GetContext(pipes);

            Run(context, pipes);

            Assert.Equal(7, context.Grid.EntityToLocation.Count);
            Assert.All(context.Grid.EntityToLocation.Keys, e => Assert.IsAssignableFrom<Pipe>(e));

            Assert.Equal(Direction.Left, Assert.IsType<UndergroundPipe>(context.Grid[new Location(1, 6)]).Direction);
            Assert.Equal(Direction.Right, Assert.IsType<UndergroundPipe>(context.Grid[new Location(10, 6)]).Direction);

            Assert.Equal(Direction.Up, Assert.IsType<UndergroundPipe>(context.Grid[new Location(11, 1)]).Direction);
            Assert.Equal(Direction.Down, Assert.IsType<UndergroundPipe>(context.Grid[new Location(11, 5)]).Direction);

            Assert.IsType<Pipe>(context.Grid[new Location(11, 6)]);

            Assert.Equal(Direction.Up, Assert.IsType<UndergroundPipe>(context.Grid[new Location(11, 7)]).Direction);
            Assert.Equal(Direction.Down, Assert.IsType<UndergroundPipe>(context.Grid[new Location(11, 11)]).Direction);
        }

        [Fact]
        public void RunIsInterruptedByTerminal()
        {
            var pipes = new HashSet<Location>();
            AddPipes(pipes, minX: 1, maxX: 11, minY: 4, maxY: 4);
            var context = GetContext(pipes);
            AddPumpjack(context, new Location(7, 2), Direction.Down);

            Run(context, pipes);

            Assert.Equal(5, context.Grid.EntityToLocation.Keys.Count(e => e is Pipe));

            Assert.Equal(Direction.Left, Assert.IsType<UndergroundPipe>(context.Grid[new Location(1, 4)]).Direction);
            Assert.Equal(Direction.Right, Assert.IsType<UndergroundPipe>(context.Grid[new Location(5, 4)]).Direction);

            Assert.IsType<Terminal>(context.Grid[new Location(6, 4)]);
            
            Assert.Equal(Direction.Left, Assert.IsType<UndergroundPipe>(context.Grid[new Location(7, 4)]).Direction);
            Assert.Equal(Direction.Right, Assert.IsType<UndergroundPipe>(context.Grid[new Location(11, 4)]).Direction);
        }

        [Fact]
        public void RunIsAdjacentToTerminal()
        {
            var pipes = new HashSet<Location>();
            AddPipes(pipes, minX: 1, maxX: 11, minY: 4, maxY: 4);
            AddPipe(pipes, x: 5, y: 3);
            var context = GetContext(pipes);
            AddPumpjack(context, new Location(7, 2), Direction.Left);

            Run(context, pipes);

            Assert.Equal(6, context.Grid.EntityToLocation.Keys.Count(e => e is Pipe));

            Assert.Equal(Direction.Left, Assert.IsType<UndergroundPipe>(context.Grid[new Location(1, 4)]).Direction);
            Assert.Equal(Direction.Right, Assert.IsType<UndergroundPipe>(context.Grid[new Location(4, 4)]).Direction);

            Assert.IsType<Terminal>(context.Grid[new Location(5, 3)]);
            Assert.IsType<Pipe>(context.Grid[new Location(5, 4)]);

            Assert.Equal(Direction.Left, Assert.IsType<UndergroundPipe>(context.Grid[new Location(6, 4)]).Direction);
            Assert.Equal(Direction.Right, Assert.IsType<UndergroundPipe>(context.Grid[new Location(11, 4)]).Direction);
        }

        [Fact]
        public void RunEndsWithTerminal()
        {
            var pipes = new HashSet<Location>();
            AddPipes(pipes, minX: 1, maxX: 5, minY: 3, maxY: 3);
            var context = GetContext(pipes, width: 10);
            AddPumpjack(context, new Location(7, 2), Direction.Left);

            Run(context, pipes);

            Assert.Equal(2, context.Grid.EntityToLocation.Keys.Count(e => e is Pipe));

            Assert.Equal(Direction.Left, Assert.IsType<UndergroundPipe>(context.Grid[new Location(1, 3)]).Direction);
            Assert.Equal(Direction.Right, Assert.IsType<UndergroundPipe>(context.Grid[new Location(5, 3)]).Direction);
        }

        [Fact]
        public void RunStartsWithTerminal()
        {
            var pipes = new HashSet<Location>();
            AddPipes(pipes, minX: 4, maxX: 8, minY: 1, maxY: 1);
            var context = GetContext(pipes, height: 5);
            AddPumpjack(context, new Location(2, 2), Direction.Right);

            Run(context, pipes);

            Assert.Equal(2, context.Grid.EntityToLocation.Keys.Count(e => e is Pipe));

            Assert.Equal(Direction.Left, Assert.IsType<UndergroundPipe>(context.Grid[new Location(4, 1)]).Direction);
            Assert.Equal(Direction.Right, Assert.IsType<UndergroundPipe>(context.Grid[new Location(8, 1)]).Direction);
        }

        [Fact]
        public void RunStartsAndEndsWithTerminal()
        {
            var pipes = new HashSet<Location>();
            AddPipes(pipes, minX: 4, maxX: 8, minY: 3, maxY: 3);
            var context = GetContext(pipes, width: 13, height: 7);
            AddPumpjack(context, new Location(2, 4), Direction.Right);
            AddPumpjack(context, new Location(10, 2), Direction.Left);

            Run(context, pipes);

            Assert.Equal(2, context.Grid.EntityToLocation.Keys.Count(e => e is Pipe));

            Assert.Equal(Direction.Left, Assert.IsType<UndergroundPipe>(context.Grid[new Location(4, 3)]).Direction);
            Assert.Equal(Direction.Right, Assert.IsType<UndergroundPipe>(context.Grid[new Location(8, 3)]).Direction);
        }

        private static void Run(Context context, HashSet<Location> pipes)
        {
            var undergroundPipes = PlanUndergroundPipes.Execute(context, pipes);

            Assert.Equal(0, context.Grid.EntityToLocation.Keys.Count(e => e is Pipe));
            Assert.All(undergroundPipes.Keys, p => Assert.Contains(p, pipes));

            AddPipeEntities.Execute(context, pipes, undergroundPipes);
        }

        private static void AddPipes(HashSet<Location> pipes, int minX, int maxX, int minY, int maxY)
        {
            for (var x = minX; x <= maxX; x++)
            {
                for (var y = minY; y <= maxY; y++)
                {
                    AddPipe(pipes, x, y);
                }
            }
        }

        private static void AddPipe(HashSet<Location> pipes, int x, int y)
        {
            var pipe = new Location(x, y);
            pipes.Add(pipe);
        }

        private static Context GetContext(HashSet<Location> pipes, int? width = null, int? height = null)
        {
            width = width ?? (pipes.Max(l => l.X) + 2);
            height = height ?? (pipes.Max(l => l.Y) + 2);

            var context = InitializeContext.GetEmpty(OilFieldOptions.ForMediumElectricPole, width.Value, height.Value);

            return context;
        }
    }
}
