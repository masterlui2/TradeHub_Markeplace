using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Marketplace_System.Services;

namespace Marketplace_System.Views
{
    public partial class InboxPanelView : UserControl
    {
        private int? _activeThreadId;
        private int _activeOtherUserId;
        private string _activeOtherUserName = "";

        public InboxPanelView()
        {
            InitializeComponent();
            Loaded += InboxPanelView_Loaded;
        }

        private void InboxPanelView_Loaded(object sender, RoutedEventArgs e)
        {
            LoadThreads();
        }

        private void LoadThreads()
        {
            try
            {
                var threads = MessagingService.GetThreadsForUser(SessionManager.CurrentUserId)
                    .Select(t => new ThreadItemViewModel
                    {
                        ThreadId = t.Thread.Id,
                        OtherUserId = t.OtherUser.Id,
                        OtherUserName = t.OtherUser.FullName,
                        Preview = t.LastMessage?.Body ?? "Start a conversation",
                        TimeText = t.LastMessage?.CreatedAt.ToLocalTime().ToString("MMM d, h:mm tt") ?? ""
                    })
                    .ToList();

                ThreadsItemsControl.ItemsSource = threads;

                if (_activeThreadId is null && threads.Count > 0)
                {
                    SetActiveThread(threads[0].ThreadId, threads[0].OtherUserId, threads[0].OtherUserName);
                }
            }
            catch
            {
                ThreadsItemsControl.ItemsSource = Array.Empty<ThreadItemViewModel>();
            }
        }

        private void ThreadButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: int threadId, DataContext: ThreadItemViewModel vm })
            {
                return;
            }

            SetActiveThread(threadId, vm.OtherUserId, vm.OtherUserName);
        }

        private void SetActiveThread(int threadId, int otherUserId, string otherUserName)
        {
            _activeThreadId = threadId;
            _activeOtherUserId = otherUserId;
            _activeOtherUserName = otherUserName;
            ActiveThreadText.Text = $"Chat with {otherUserName}";
            LoadMessages();
        }

        private void LoadMessages()
        {
            if (_activeThreadId is null)
            {
                MessagesItemsControl.ItemsSource = Array.Empty<MessageItemViewModel>();
                return;
            }

            List<MessageItemViewModel> messages = MessagingService.GetMessages(_activeThreadId.Value)
                .Select(m => new MessageItemViewModel
                {
                    Body = m.Body,
                    TimeText = m.CreatedAt.ToLocalTime().ToString("h:mm tt"),
                    MessageAlignment = m.SenderUserId == SessionManager.CurrentUserId ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                    LeftProfileVisibility = m.SenderUserId == SessionManager.CurrentUserId ? Visibility.Collapsed : Visibility.Visible,
                    RightProfileVisibility = m.SenderUserId == SessionManager.CurrentUserId ? Visibility.Visible : Visibility.Collapsed,
                    BubbleColor = m.SenderUserId == SessionManager.CurrentUserId
                        ? new SolidColorBrush(Color.FromRgb(230, 244, 236))
                        : new SolidColorBrush(Color.FromRgb(243, 244, 246))
                })
                .ToList();

            MessagesItemsControl.ItemsSource = messages;
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string message = NewMessageTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(message) || _activeOtherUserId <= 0)
            {
                return;
            }

            try
            {
                if (MessagingService.SendMessage(SessionManager.CurrentUserId, _activeOtherUserId, message))
                {
                    NewMessageTextBox.Clear();
                    LoadThreads();
                    if (_activeThreadId is not null)
                    {
                        SetActiveThread(_activeThreadId.Value, _activeOtherUserId, _activeOtherUserName);
                    }
                }
            }
            catch
            {
                MessageBox.Show("Unable to send message right now.", "Message Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private sealed class ThreadItemViewModel
        {
            public int ThreadId { get; init; }
            public int OtherUserId { get; init; }
            public string OtherUserName { get; init; } = string.Empty;
            public string Preview { get; init; } = string.Empty;
            public string TimeText { get; init; } = string.Empty;
        }

        private sealed class MessageItemViewModel
        {
            public string Body { get; init; } = string.Empty;
            public string TimeText { get; init; } = string.Empty;
            public HorizontalAlignment MessageAlignment { get; init; }
            public Visibility LeftProfileVisibility { get; init; } = Visibility.Visible;
            public Visibility RightProfileVisibility { get; init; } = Visibility.Collapsed;
            public Brush BubbleColor { get; init; } = Brushes.White;
        }
    }
}