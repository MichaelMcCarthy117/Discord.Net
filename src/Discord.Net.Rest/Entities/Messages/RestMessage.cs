﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Model = Discord.API.Message;

namespace Discord.Rest
{
    public abstract class RestMessage : RestEntity<ulong>, IMessage, IUpdateable
    {
        internal readonly IGuild _guild;
        private long _timestampTicks;

        public IMessageChannel Channel { get; }
        public RestUser Author { get; }

        public string Content { get; private set; }

        public DateTimeOffset CreatedAt => DateTimeUtils.FromSnowflake(Id);
        public virtual bool IsTTS => false;
        public virtual bool IsPinned => false;
        public virtual DateTimeOffset? EditedTimestamp => null;
        public virtual IReadOnlyCollection<Attachment> Attachments => ImmutableArray.Create<Attachment>();
        public virtual IReadOnlyCollection<Embed> Embeds => ImmutableArray.Create<Embed>();
        public virtual IReadOnlyCollection<ulong> MentionedChannelIds => ImmutableArray.Create<ulong>();
        public virtual IReadOnlyCollection<ulong> MentionedRoleIds => ImmutableArray.Create<ulong>();
        public virtual IReadOnlyCollection<RestUser> MentionedUsers => ImmutableArray.Create<RestUser>();
        public virtual IReadOnlyCollection<ITag> Tags => ImmutableArray.Create<ITag>();
        public virtual IReadOnlyCollection<IReaction> Reactions => ImmutableArray.Create<RestReaction>();
        public virtual ulong? WebhookId => null;
        public bool IsWebhook => WebhookId != null;

        public DateTimeOffset Timestamp => DateTimeUtils.FromTicks(_timestampTicks);

        internal RestMessage(BaseDiscordClient discord, ulong id, IMessageChannel channel, RestUser author, IGuild guild)
            : base(discord, id)
        {
            Channel = channel;
            Author = author;
            _guild = guild;
        }
        internal static RestMessage Create(BaseDiscordClient discord, IGuild guild, Model model)
        {
            if (model.Type == MessageType.Default)
                return RestUserMessage.Create(discord, guild, model);
            else
                return RestSystemMessage.Create(discord, guild, model);
        }
        internal virtual void Update(Model model)
        {
            if (model.Timestamp.IsSpecified)
                _timestampTicks = model.Timestamp.Value.UtcTicks;

            if (model.Content.IsSpecified)
                Content = model.Content.Value;
        }

        public async Task UpdateAsync(RequestOptions options = null)
        {
            var model = await Discord.ApiClient.GetChannelMessageAsync(Channel.Id, Id, options).ConfigureAwait(false);
            Update(model);
        }
        public Task DeleteAsync(RequestOptions options = null)
            => MessageHelper.DeleteAsync(this, Discord, options);

        public override string ToString() => Content;

        MessageType IMessage.Type => MessageType.Default;
        IUser IMessage.Author => Author;
        IReadOnlyCollection<IAttachment> IMessage.Attachments => Attachments;
        IReadOnlyCollection<IEmbed> IMessage.Embeds => Embeds;
        IReadOnlyCollection<ulong> IMessage.MentionedUserIds => MentionedUsers.Select(x => x.Id).ToImmutableArray();
        IReadOnlyCollection<IReaction> IMessage.Reactions => Reactions;
    }
}
