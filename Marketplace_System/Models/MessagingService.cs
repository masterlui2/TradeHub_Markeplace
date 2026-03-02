using System;
using System.Collections.Generic;
using System.Linq;
using Marketplace_System.Data;
using Marketplace_System.Models;

namespace Marketplace_System.Services
{
    public static class MessagingService
    {
        public static int? EnsureThread(int currentUserId, int otherUserId)
        {
            if (currentUserId <= 0 || otherUserId <= 0 || currentUserId == otherUserId)
            {
                return null;
            }

            int userOneId = Math.Min(currentUserId, otherUserId);
            int userTwoId = Math.Max(currentUserId, otherUserId);

            using AppDbContext dbContext = new();
            MessageThread? existingThread = dbContext.MessageThreads
                .FirstOrDefault(t => t.UserOneId == userOneId && t.UserTwoId == userTwoId);

            if (existingThread is not null)
            {
                return existingThread.Id;
            }

            MessageThread thread = new()
            {
                UserOneId = userOneId,
                UserTwoId = userTwoId,
                UpdatedAt = DateTime.UtcNow
            };

            dbContext.MessageThreads.Add(thread);
            dbContext.SaveChanges();
            return thread.Id;
        }

        public static bool SendMessage(int currentUserId, int otherUserId, string body)
        {
            string cleanBody = body.Trim();
            if (string.IsNullOrWhiteSpace(cleanBody))
            {
                return false;
            }

            int? threadId = EnsureThread(currentUserId, otherUserId);
            if (threadId is null)
            {
                return false;
            }

            using AppDbContext dbContext = new();
            MessageThread? thread = dbContext.MessageThreads.FirstOrDefault(t => t.Id == threadId.Value);
            if (thread is null)
            {
                return false;
            }

            dbContext.ChatMessages.Add(new ChatMessage
            {
                ThreadId = thread.Id,
                SenderUserId = currentUserId,
                ReceiverUserId = otherUserId,
                Body = cleanBody,
                CreatedAt = DateTime.UtcNow
            });

            thread.UpdatedAt = DateTime.UtcNow;
            dbContext.SaveChanges();
            return true;
        }

        public static List<(MessageThread Thread, User OtherUser, ChatMessage? LastMessage)> GetThreadsForUser(int userId)
        {
            using AppDbContext dbContext = new();

            var threads = dbContext.MessageThreads
                .Where(t => t.UserOneId == userId || t.UserTwoId == userId)
                .OrderByDescending(t => t.UpdatedAt)
                .ToList();

            List<(MessageThread, User, ChatMessage?)> result = new();
            foreach (MessageThread thread in threads)
            {
                int otherId = thread.UserOneId == userId ? thread.UserTwoId : thread.UserOneId;
                User? other = dbContext.Users.FirstOrDefault(u => u.Id == otherId);
                if (other is null)
                {
                    continue;
                }

                ChatMessage? last = dbContext.ChatMessages
                    .Where(m => m.ThreadId == thread.Id)
                    .OrderByDescending(m => m.CreatedAt)
                    .FirstOrDefault();

                result.Add((thread, other, last));
            }

            return result;
        }

        public static List<ChatMessage> GetMessages(int threadId)
        {
            using AppDbContext dbContext = new();
            return dbContext.ChatMessages
                .Where(m => m.ThreadId == threadId)
                .OrderBy(m => m.CreatedAt)
                .ToList();
        }
    }
}