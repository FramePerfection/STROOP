using StroopNext.Platform;

namespace StroopNext.Core.Tests;

public class CodeCacheLocatorTests
{
    private static byte[] Table(int offset = 0, int extra = 0)
    {
        var bytes = new byte[offset + CodeCacheLocator.Pages + extra];
        Array.Fill(bytes, (byte)1, offset, CodeCacheLocator.Pages);
        for (int page = 0x80246; page < 0x80266; page++)
            bytes[offset + page] = 0;
        return bytes;
    }

    [Fact]
    public void A_table_with_compiled_game_pages_is_recognised()
    {
        Assert.True(CodeCacheLocator.IsTable(Table(), 0));
        Assert.True(CodeCacheLocator.IsTable(Table(offset: 64, extra: 64), 64));
    }

    [Fact]
    public void Zero_memory_or_other_data_is_not_a_table()
    {
        Assert.False(CodeCacheLocator.IsTable(new byte[CodeCacheLocator.Pages], 0));

        var noCode = Table();
        Array.Fill(noCode, (byte)1);
        Assert.False(CodeCacheLocator.IsTable(noCode, 0));

        var other = Table();
        other[0x90000] = 7;
        Assert.False(CodeCacheLocator.IsTable(other, 0));

        var user = Table();
        user[0x1234] = 0;
        Assert.False(CodeCacheLocator.IsTable(user, 0));

        Assert.False(CodeCacheLocator.IsTable(Table(), 1));
    }
}
