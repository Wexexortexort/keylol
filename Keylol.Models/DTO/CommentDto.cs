﻿using System;
using System.Runtime.Serialization;

namespace Keylol.Models.DTO
{
    /// <summary>
    ///     Comment DTO
    /// </summary>
    [DataContract]
    public class CommentDto
    {
        /// <summary>
        ///     创建空 DTO，需要手动填充
        /// </summary>
        public CommentDto()
        {
        }

        /// <summary>
        ///     创建 DTO 并自动填充部分数据
        /// </summary>
        /// <param name="comment"><see cref="Comment" /> 对象</param>
        /// <param name="includeContent">是否包含评论内容</param>
        /// <param name="truncateContentTo">评论内容截取长度，0 表示不截取</param>
        public CommentDto(Comment comment, bool includeContent = true, int truncateContentTo = 0)
        {
            Id = comment.Id;
            if (includeContent)
            {
                Content = truncateContentTo > 0 && truncateContentTo < comment.Content.Length
                    ? comment.Content.Substring(0, truncateContentTo)
                    : comment.Content;
            }
            PublishTime = comment.PublishTime;
            SequenceNumberForArticle = comment.SequenceNumberForArticle;
        }

        /// <summary>
        ///     Id
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        ///     内容
        /// </summary>
        [DataMember]
        public string Content { get; set; }

        /// <summary>
        ///     发布时间
        /// </summary>
        [DataMember]
        public DateTime PublishTime { get; set; }

        /// <summary>
        ///     评论人
        /// </summary>
        [DataMember]
        public UserDto Commentator { get; set; }

        /// <summary>
        ///     在文章中的楼层号
        /// </summary>
        [DataMember]
        public int? SequenceNumberForArticle { get; set; }

        /// <summary>
        ///     所属文章作者识别码
        /// </summary>
        [DataMember]
        public string ArticleAuthorIdCode { get; set; }

        /// <summary>
        ///     所属文章是作者的第几篇文章
        /// </summary>
        [DataMember]
        public int? ArticleSequenceNumberForAuthor { get; set; }

        /// <summary>
        ///     认可数
        /// </summary>
        [DataMember]
        public int? LikeCount { get; set; }

        /// <summary>
        ///     当前登录用户是否认可过
        /// </summary>
        [DataMember]
        public bool? Liked { get; set; }

        /// <summary>
        ///     封存状态
        /// </summary>
        [DataMember]
        public ArchivedState? Archived { get; set; }

        /// <summary>
        ///     警告状态
        /// </summary>
        [DataMember]
        public bool? Warned { get; set; }
    }
}