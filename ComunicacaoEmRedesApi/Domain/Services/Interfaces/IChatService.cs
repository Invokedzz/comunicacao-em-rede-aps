using System.Net.WebSockets;
using ComunicacaoEmRedesApi.Domain.Models;
using ComunicacaoEmRedesApi.Domain.Results;

namespace ComunicacaoEmRedesApi.Domain.Services.Interfaces;

public interface IChatService
{
    Task AddClientAsync(Guid userId, WebSocket socket);
    Task RemoveClientAsync(Guid userId);
    Task SendMessageToChatAsync(Guid chatId, string message, Guid senderId);
    
    Task SaveChatAsync(Chat chat);
    Task<Result<Chat>> GetChatById(Guid chatId);
    Task<bool> DoesChatExists(Guid chatId);
}