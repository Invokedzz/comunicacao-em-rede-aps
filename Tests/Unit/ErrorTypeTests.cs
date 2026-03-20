using ComunicacaoEmRedesApi.Domain.Enums;

namespace Tests.Unit;

public class ErrorTypeTests
{
    [Fact]
    public void Should_Get_404_For_NotFound()
    {
        Assert.Equal(404, (int)ErrorType.NotFound);
    }

    [Fact]
    public void Should_Get_400_For_BadRequest()
    {
        Assert.Equal(400, (int)ErrorType.BadRequest);
    }

    [Fact]
    public void Should_Get_409_For_Conflict()
    {
        Assert.Equal(409, (int)ErrorType.Conflict);
    }
    
    [Fact]
    public void Should_Get_0_For_NoError()
    {
        Assert.Equal(0, (int)ErrorType.NoError);
    }
}