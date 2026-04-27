using ComunicacaoEmRedesApi.Domain.Models;
using ComunicacaoEmRedesApi.Domain.Repositories;
using ComunicacaoEmRedesApi.Domain.Services;
using ComunicacaoEmRedesApi.Domain.Services.Interfaces;

namespace ComunicacaoEmRedesApi.Api.Sockets;

using System.Net.WebSockets;
using System.Text;

public class ChatWebSocketHandler
{
    private readonly IChatService _chatService;
    private readonly IMessageRepository _messageRepository;

    public ChatWebSocketHandler(IChatService chatService, IMessageRepository messageRepository)
    {
        _chatService = chatService;
        _messageRepository = messageRepository;
    }

    public async Task HandleAsync(HttpContext context)
    {
        var socket = await context.WebSockets.AcceptWebSocketAsync();
        var buffer = new byte[4096];

        // receive auth
        var result = await socket.ReceiveAsync(buffer, CancellationToken.None);
        var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

        var auth = System.Text.Json.JsonSerializer.Deserialize<AuthDto>(json)!;

        var response = await _chatService.GetChatById(auth.ChatId);

        if (!response.IsSuccess)
        {
            var chat = new Chat { Title = "Default", Id = auth.ChatId };
            await _chatService.SaveChatAsync(chat);
        }

        await _chatService.AddClientAsync(auth.UserId, socket);
        ((ChatService)_chatService).AddUserToChat(auth.ChatId, auth.UserId);

        var history = await _messageRepository.GetMessagesByChatIdAsync(auth.ChatId);

        foreach (var msg in history)
        {
            if (socket.State != WebSocketState.Open)
            {
                break;
            }
            
            var text = $"{msg.UserId}: {msg.Content}";
            var bytes = Encoding.UTF8.GetBytes(text);

            await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        try
        {
            while (socket.State == WebSocketState.Open)
            {
                var receive = await socket.ReceiveAsync(buffer, CancellationToken.None);

                if (receive.MessageType == WebSocketMessageType.Close)
                    break;

                var content = Encoding.UTF8.GetString(buffer, 0, receive.Count);
                var message = new Message
                {
                    UserId = auth.UserId,
                    ChatId = auth.ChatId,
                    Content = content,
                    CreatedAt = DateTime.UtcNow
                };

                await _messageRepository.SaveMessageAsync(message);
                
                await _chatService.SendMessageToChatAsync(
                    auth.ChatId,
                    content,
                    auth.UserId
                );
            }
        }
        finally
        {
            await _chatService.RemoveClientAsync(auth.UserId);

            await socket.CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                "Closed",
                CancellationToken.None
            );
        }
    }

    private record AuthDto(Guid UserId, string Email, Guid ChatId);
}