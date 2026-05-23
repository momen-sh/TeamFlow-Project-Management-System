using TeamFlow.Entities;
using TeamFlow.Repositories.Interfaces;
using TeamFlow.Services.Interfaces;
using System.Text.RegularExpressions;

namespace TeamFlow.Services.Implementations
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;

        public CommentService(
            ICommentRepository commentRepository,
            IUserRepository userRepository,
            INotificationService notificationService)
        {
            _commentRepository = commentRepository;
            _userRepository = userRepository;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<Comment>> GetAllAsync()
            => await _commentRepository.GetAllAsync();

        public async Task<Comment?> GetByIdAsync(int id)
            => await _commentRepository.GetByIdAsync(id);

        public async Task<IEnumerable<Comment>> GetByTaskIdAsync(int taskId)
            => await _commentRepository.GetByTaskIdAsync(taskId);

        public async Task<Comment> CreateAsync(Comment comment, IEnumerable<int>? mentionedUserIds = null)
        {
            var parsedMentionUsers = await _userRepository.SearchMentionTargetsAsync(ParseMentionTokens(comment.Content));
            var parsedMentionIds = parsedMentionUsers.Select(user => user.Id);
            var distinctMentionIds = (mentionedUserIds ?? Enumerable.Empty<int>())
                .Concat(parsedMentionIds)
                .Distinct()
                .ToList();

            if (distinctMentionIds.Count > 0)
            {
                var existingUserIds = await _userRepository.GetExistingUserIdsAsync(distinctMentionIds);
                comment.Mentions = existingUserIds
                    .Select(userId => new CommentMention { UserId = userId })
                    .ToList();
            }

            await _commentRepository.AddAsync(comment);
            await _commentRepository.SaveAsync();
            await _notificationService.NotifyMentionedUsersAsync(comment, comment.Mentions.Select(x => x.UserId), comment.UserId);
            return comment;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _commentRepository.GetByIdAsync(id);
            if (entity is null) return false;

            _commentRepository.Delete(entity);
            await _commentRepository.SaveAsync();
            return true;
        }

        private static IEnumerable<string> ParseMentionTokens(string content)
        {
            return Regex.Matches(content, @"(?<!\w)@([\w.\-]+@[\w.\-]+\.\w+|[\w.\-]+)")
                .Select(match => match.Groups[1].Value);
        }
    }
}
