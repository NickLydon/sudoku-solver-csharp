using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SudokuSolver;

internal class Peers : IEnumerable<(int x, int y)>
{
    private readonly int size;
    private readonly (int x, int y) pos;

    public Peers(int size, (int x, int y) pos)
    {
        this.size = size;
        this.pos = pos;
    }

    private IEnumerable<(int x, int y)> AllPeers()
    {
        var (x, y) = pos;
        var squares = Enumerable.Range(0, size);

        IEnumerable<(int x, int y)> InnerGrid()
        {
            var innerGridSize = (int)System.Math.Sqrt(size);
            var start = (x: x - x % innerGridSize, y: y - y % innerGridSize);
            var innerGridCount = Enumerable.Range(0, innerGridSize);
            foreach (var row in innerGridCount)
                foreach (var col in innerGridCount)
                    yield return (start.x + col, start.y + row);
        }

        return squares.Select(col => ((x: col, y)))
            .Concat(squares.Select(row => ((x: x, y: row))))
            .Concat(InnerGrid())
            .Where(xy => xy.x != x || xy.y != y);
    }

    public IEnumerator<(int x, int y)> GetEnumerator()
    {
        var (x, y) = pos;
        var squares = Enumerable.Range(0, size);

        IEnumerable<(int x, int y)> InnerGrid()
        {
            var innerGridSize = (int)System.Math.Sqrt(size);
            var start = (x: x - x % innerGridSize, y: y - y % innerGridSize);
            var innerGridCount = Enumerable.Range(0, innerGridSize);
            foreach (var row in innerGridCount)
                foreach (var col in innerGridCount)
                    yield return (start.x + col, start.y + row);
        }

        return squares.Select(col => ((x: col, y)))
            .Concat(squares.Select(row => ((x: x, y: row))))
            .Concat(InnerGrid())
            .Where(xy => xy.x != x || xy.y != y)
            .GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
