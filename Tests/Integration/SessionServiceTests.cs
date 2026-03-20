using ComunicacaoEmRedesApi.Application.Dtos;
using ComunicacaoEmRedesApi.Domain.Models;
using ComunicacaoEmRedesApi.Domain.Repositories;
using ComunicacaoEmRedesApi.Domain.Services;
using ComunicacaoEmRedesApi.Infrastructure.Security.Interfaces;
using Moq;

namespace Tests.Integration;

public class SessionServiceTests
{
    private readonly Mock<IUserRepository> _userRepo = new(MockBehavior.Default);
    private readonly Mock<IPasswordEncryption> _encryption = new(MockBehavior.Default);

    [Fact]
    public async Task Should_Return_Ok_When_Register_Is_Successful()
    {
        _userRepo.Setup(e => e.DoesEmailExists(It.IsAny<string>())).ReturnsAsync(false);
        _encryption.Setup(e => e.HashPassword(It.IsAny<User>(), It.IsAny<string>())).Returns("hashed");
        _userRepo.Setup(e => e.SaveUserAsync(It.IsAny<User>())).Verifiable();
        
        var service = new SessionService(_userRepo.Object, _encryption.Object);
        var req = new RegisterRequestDto { Email = "aps@gmail.com", Password = "1234ab@c" };

        var response = await service.Register(req);

        Assert.NotNull(response.Value);
        Assert.True(response.IsSuccess);
        Assert.IsType<Guid>(response.Value.OperationId);
        Assert.Equal("aps@gmail.com", response.Value.Email);
        Assert.Equal(DateOnly.FromDateTime(DateTime.Today), response.Value.CreatedAt);
        
        _userRepo.VerifyAll();
        _encryption.Verify(e => e.HashPassword(It.IsAny<User>(), It.IsAny<string>()), Times.Once());
    }

    [Fact]
    public async Task Should_Return_BadRequest_When_Multiple_Domain_Errors_Happen()
    {
        var service = new SessionService(_userRepo.Object, _encryption.Object);
        var req = new RegisterRequestDto { Email = "aps@net.com", Password = "1234" };

        var response = await service.Register(req);
        
        Assert.False(response.IsSuccess);
        Assert.Equal(3, response.Errors.Count);

        Assert.Contains(response.Errors, e => e is { Code: "INVALID_EMAIL", Message: "Email domain is not valid!" });
        Assert.Contains(response.Errors, e => e is { Code: "INVALID_PASSWORD", Message: "Password length must be between 8 and 15 characters!" });
        Assert.Contains(response.Errors, e => e is { Code: "INVALID_PASSWORD", Message: "Password must contain letters, numbers and a special character!" });
    }
    
    [Fact]
    public async Task Should_Return_Conflict_When_Email_Already_Exists()
    {
        _userRepo.Setup(e => e.DoesEmailExists(It.IsAny<string>())).ReturnsAsync(true);
        
        var service = new SessionService(_userRepo.Object, _encryption.Object);
        var req = new RegisterRequestDto { Email = "aps@gmail.com", Password = "1234ab@c" };

        var response = await service.Register(req);

        Assert.Single(response.Errors);
        Assert.False(response.IsSuccess);
        Assert.Equal(409, (int)response.ErrorType);
        Assert.Equal("EMAIL_ALREADY_EXISTS", response.Errors[0].Code);
        Assert.Equal("This email already exists!", response.Errors[0].Message);
        
        _userRepo.Verify(e => e.DoesEmailExists(It.IsAny<string>()), Times.Once);
    }
    
    [Fact]
    public async Task Should_Return_BadRequest_When_Email_Does_Not_Have_A_Valid_Provider()
    {
        var service = new SessionService(_userRepo.Object, _encryption.Object);
        var req = new RegisterRequestDto { Email = "email@random.com", Password = "1234ab@c" };
        
        var response = await service.Register(req);
        
        Assert.Single(response.Errors);
        Assert.False(response.IsSuccess);
        Assert.Equal(400, (int)response.ErrorType);
        Assert.Equal("INVALID_EMAIL", response.Errors[0].Code);
        Assert.Equal("Email domain is not valid!", response.Errors[0].Message);
    }
    
    [Fact]
    public async Task Should_Return_BadRequest_When_Email_Does_Not_Have_A_Valid_Domain()
    {
        var service = new SessionService(_userRepo.Object, _encryption.Object);
        var req = new RegisterRequestDto { Email = "email@gmail.net", Password = "1234ab@c" };
        
        var response = await service.Register(req);
        
        Assert.Single(response.Errors);
        Assert.False(response.IsSuccess);
        Assert.Equal(400, (int)response.ErrorType);
        Assert.Equal("INVALID_EMAIL", response.Errors[0].Code);
        Assert.Equal("Email domain is not valid!", response.Errors[0].Message);
    }
    
    [Fact]
    public async Task Should_Return_BadRequest_When_Password_Contain_Only_Letters()
    {
        var service = new SessionService(_userRepo.Object, _encryption.Object);
        var req = new RegisterRequestDto { Email = "email@gmail.com", Password = "abcdefgbe" };
        
        var response = await service.Register(req);
        
        Assert.Single(response.Errors);
        Assert.False(response.IsSuccess);
        Assert.Equal(400, (int)response.ErrorType);
        Assert.Equal("INVALID_PASSWORD", response.Errors[0].Code);
        Assert.Equal("Password must contain letters, numbers and a special character!", response.Errors[0].Message);
    }
    
    [Fact]
    public async Task Should_Return_BadRequest_When_Password_Contain_Only_Numbers()
    {
        var service = new SessionService(_userRepo.Object, _encryption.Object);
        var req = new RegisterRequestDto { Email = "email@gmail.com", Password = "12345678" };
        
        var response = await service.Register(req);
        
        Assert.Single(response.Errors);
        Assert.False(response.IsSuccess);
        Assert.Equal(400, (int)response.ErrorType);
        Assert.Equal("INVALID_PASSWORD", response.Errors[0].Code);
        Assert.Equal("Password must contain letters, numbers and a special character!", response.Errors[0].Message);
    }
    
    [Fact]
    public async Task Should_Return_BadRequest_When_Password_Length_Is_Lower_Than_8()
    {
        var service = new SessionService(_userRepo.Object, _encryption.Object);
        var req = new RegisterRequestDto { Email = "email@gmail.com", Password = "1@a4" };
        
        var response = await service.Register(req);
        
        Assert.Single(response.Errors);
        Assert.False(response.IsSuccess);
        Assert.Equal(400, (int)response.ErrorType);
        Assert.Equal("INVALID_PASSWORD", response.Errors[0].Code);
        Assert.Equal("Password length must be between 8 and 15 characters!", response.Errors[0].Message);
    }
    
    [Fact]
    public async Task Should_Return_BadRequest_When_Password_Length_Is_Higher_Than_15()
    {
        var service = new SessionService(_userRepo.Object, _encryption.Object);
        var req = new RegisterRequestDto { Email = "email@gmail.com", Password = "1@a4@ahdn123@ksj" };
        
        var response = await service.Register(req);
        
        Assert.Single(response.Errors);
        Assert.False(response.IsSuccess);
        Assert.Equal(400, (int)response.ErrorType);
        Assert.Equal("INVALID_PASSWORD", response.Errors[0].Code);
        Assert.Equal("Password length must be between 8 and 15 characters!", response.Errors[0].Message);
    }
}