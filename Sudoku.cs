using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SudokuSolver;

internal class Sudoku
{
    private readonly ImmutableDictionary<(int, int), ImmutableHashSet<int>> grid;
    private readonly int size;
    private readonly ImmutableDictionary<(int x, int y), (int x, int y)[]> peers;

    public Sudoku(ImmutableDictionary<(int, int), int?> grid, int size)
    {
        this.grid = grid.Select(kvp =>
            (kvp.Key, kvp.Value.HasValue
                ? ImmutableHashSet.Create(kvp.Value.Value)
                : Enumerable.Range(1, size).ToImmutableHashSet())
        ).ToImmutableDictionary(k => k.Key, v => v.Item2);
        this.size = size;
        this.peers =
            Enumerable.Range(0, size).SelectMany(x =>
                Enumerable.Range(0, size).Select(y => ((x, y), new Peers(size, (x, y)).ToArray())))
            .ToImmutableDictionary(k => k.Item1, v => v.Item2);
    }

    private IEnumerable<ImmutableDictionary<(int, int), ImmutableHashSet<int>>> Go(
        ImmutableDictionary<(int, int), ImmutableHashSet<int>> current)
    {
        if (current.All(x => x.Value.Count == 1))
        {
            yield return current;
            yield break;
        }

        foreach (var ((x, y), values) in
            current
                .Where(kvp => kvp.Value.Count > 1)
                .Select(kvp => (kvp.Key, kvp.Value))
                .OrderBy(x => x.Value.Count)
                .Take(1))
            foreach (var (updated, _) in values.Select(value => SetTileValue(current, (x, y), value)).Where(tup => !tup.Item2))
                foreach (var completed in Go(updated))
                    yield return completed;
    }

    private (ImmutableDictionary<(int, int), ImmutableHashSet<int>> updated, bool contradicts) SetTileValue(
        ImmutableDictionary<(int, int), ImmutableHashSet<int>> current, (int x, int y) pos, int value)
    {
        current = current.SetItem(pos, ImmutableHashSet.Create(value));
        foreach (var peer in peers[pos].Where(xy => current[(xy.x, xy.y)].Contains(value)))
        {
            var oldVal = current[peer];
            var newVal = oldVal.Remove(value);
            current = current.SetItem(peer, newVal);
            if (newVal.IsEmpty) return (current, true);
            if (newVal.Count == 1)
            {
                var (updated, contradicts) = SetTileValue(current, peer, newVal.Single());
                if (contradicts) return (updated, contradicts);
                current = updated;
            }
        }

        return (current, false);
    }

    public IEnumerable<ImmutableDictionary<(int, int), int>> Solve()
    {
        var grid = this.grid;
        foreach (var (pos, val) in grid.Where(x => x.Value.Count == 1))
        {
            var (updated, contradicts) = SetTileValue(grid, pos, val.Single());
            if (contradicts) return Enumerable.Empty<ImmutableDictionary<(int, int), int>>();
            grid = updated;
        }

        return Go(grid)
            .Select(x =>
                x.Select(kvp => (kvp.Key, kvp.Value.Single()))
                .ToImmutableDictionary(k => k.Key, v => v.Item2));
    }
}
