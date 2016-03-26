﻿using System;
using System.Data.Entity;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.Models.ViewModels;
using Keylol.Provider;
using Keylol.Services;
using Keylol.Services.Contracts;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.Like
{
    public partial class LikeController
    {
        /// <summary>
        ///     创建一个认可
        /// </summary>
        /// <param name="vm">认可相关属性</param>
        [Route]
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof (int))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "文章或评论被封存，当前登录用户无权创建认可")]
        public async Task<IHttpActionResult> CreateOne(LikeVM vm)
        {
            if (vm == null)
            {
                ModelState.AddModelError("vm", "Invalid view model.");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var operatorId = User.Identity.GetUserId();
            var @operator = await DbContext.Users.SingleAsync(u => u.Id == operatorId);

            Models.Like like;
            switch (vm.Type)
            {
                case LikeVM.LikeType.ArticleLike:
                {
                    var existLike = await DbContext.ArticleLikes.SingleOrDefaultAsync(
                        l => l.ArticleId == vm.TargetId && l.OperatorId == operatorId && l.Backout == false);
                    if (existLike != null)
                    {
                        ModelState.AddModelError("vm.TargetId", "不能对同一篇文章重复认可。");
                        return BadRequest(ModelState);
                    }
                    var article = await DbContext.Articles.FindAsync(vm.TargetId);
                    if (article == null)
                    {
                        ModelState.AddModelError("vm.TargetId", "指定文章不存在。");
                        return BadRequest(ModelState);
                    }
                    if (article.PrincipalId == operatorId)
                    {
                        ModelState.AddModelError("vm.TargetId", "不能认可自己发表的文章。");
                        return BadRequest(ModelState);
                    }
                    if (article.Archived != ArchivedState.None)
                        return Unauthorized();
                    var articleLike = DbContext.ArticleLikes.Create();
                    articleLike.ArticleId = vm.TargetId;
                    like = articleLike;
                    if (!article.IgnoreNewLikes)
                    {
                        var articleAuthor = await DbContext.Users.Include(u => u.SteamBot)
                            .SingleAsync(u => u.Id == article.PrincipalId);

                        // 邮政中心
                        var message = DbContext.Messages.Create();
                        message.Type = MessageType.ArticleLike;
                        message.OperatorId = operatorId;
                        message.ReceiverId = articleAuthor.Id;
                        message.ArticleId = article.Id;
                        DbContext.Messages.Add(message);

                        // Steam 通知
                        ISteamBotCoodinatorCallback callback;
                        if (articleAuthor.SteamNotifyOnArticleLiked && articleAuthor.SteamBot.SessionId != null &&
                            SteamBotCoodinator.Clients.TryGetValue(articleAuthor.SteamBot.SessionId, out callback))
                        {
                            callback.SendMessage(articleAuthor.SteamBotId, articleAuthor.SteamId,
                                $"@{@operator.UserName} 认可了你的文章 《{article.Title}》：\nhttps://www.keylol.com/article/{articleAuthor.IdCode}/{article.SequenceNumberForAuthor}");
                        }
                    }
                    break;
                }

                case LikeVM.LikeType.CommentLike:
                {
                    var existLike = await DbContext.CommentLikes.SingleOrDefaultAsync(
                        l => l.CommentId == vm.TargetId && l.OperatorId == operatorId && l.Backout == false);
                    if (existLike != null)
                    {
                        ModelState.AddModelError("vm.TargetId", "不能对同一篇评论重复认可。");
                        return BadRequest(ModelState);
                    }
                    var comment =
                        await DbContext.Comments.Include(c => c.Article).SingleOrDefaultAsync(c => c.Id == vm.TargetId);
                    if (comment == null)
                    {
                        ModelState.AddModelError("vm.TargetId", "指定评论不存在。");
                        return BadRequest(ModelState);
                    }
                    if (comment.CommentatorId == operatorId)
                    {
                        ModelState.AddModelError("vm.TargetId", "不能认可自己发表的评论。");
                        return BadRequest(ModelState);
                    }
                    if (comment.Archived != ArchivedState.None || comment.Article.Archived != ArchivedState.None)
                        return Unauthorized();
                    var commentLike = DbContext.CommentLikes.Create();
                    commentLike.CommentId = vm.TargetId;
                    like = commentLike;
                    if (!comment.IgnoreNewLikes)
                    {
                        var commentAuthor = await DbContext.Users.Include(u => u.SteamBot)
                            .SingleAsync(u => u.Id == comment.CommentatorId);
                        var articleAuthor = await DbContext.Users.SingleAsync(u => u.Id == comment.Article.PrincipalId);

                        // 邮政中心
                        var message = DbContext.Messages.Create();
                        message.Type = MessageType.CommentLike;
                        message.OperatorId = operatorId;
                        message.ReceiverId = commentAuthor.Id;
                        message.CommentId = comment.Id;
                        DbContext.Messages.Add(message);

                        // Steam 通知
                        ISteamBotCoodinatorCallback callback;
                        if (commentAuthor.SteamNotifyOnCommentLiked && commentAuthor.SteamBot.SessionId != null &&
                            SteamBotCoodinator.Clients.TryGetValue(commentAuthor.SteamBot.SessionId, out callback))
                        {
                            callback.SendMessage(commentAuthor.SteamBotId, commentAuthor.SteamId,
                                $"@{@operator.UserName} 认可了你在 《{comment.Article.Title}》 下的评论：\nhttps://www.keylol.com/article/{articleAuthor.IdCode}/{comment.Article.SequenceNumberForAuthor}#{comment.SequenceNumberForArticle}");
                        }
                    }
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
            like.OperatorId = operatorId;
            DbContext.Likes.Add(like);
            await DbContext.SaveChangesAsync();
            return Created($"like/{like.Id}", "Liked!");
        }
    }
}