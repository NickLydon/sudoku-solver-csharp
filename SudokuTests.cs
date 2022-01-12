using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SudokuSolver;

public class SudokuTests
{
    public static IEnumerable<object[]> Values() =>
        Directory.GetFiles("SampleGrids", "*.txt").Select(input => new object[] { input, input + ".solved" });        

    [Theory]
    [MemberData(nameof(Values))]
    public async Task ShouldSolveGrid(string input, string output)
    {
        var inputGrid = await ReadGrid(input);
        var expected = (await ReadGrid(output)).ToImmutableDictionary(k => k.Key, v => v.Value.Value);
        var sut = new Sudoku(inputGrid, 9);
        var finalResult = sut.Solve().Last();
        Assert.Equal(expected, finalResult);
    }

    private static async Task<ImmutableDictionary<(int rowNum, int colNum), int?>> ReadGrid(string path)
    {
        var rows = await File.ReadAllLinesAsync(path);
        var grid = rows
            .SelectMany((row, rowNum) => row
                .Where(char.IsDigit)
                .Select((col, colNum) =>
                    ((rowNum, colNum), int.Parse(col.ToString()) switch { 0 => (int?)null, var x => x })))
            .ToImmutableDictionary(k => k.Item1, v => v.Item2);
        return grid;
    }
}