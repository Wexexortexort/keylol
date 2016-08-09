﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.StateTreeManager;
using Keylol.Utilities;

namespace Keylol.States.Aggregation.User.Dossier.Point
{
    /// <summary>
    /// 用户订阅的据点列表
    /// </summary>
    public class PointList : List<Point>
    {
        private PointList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 获取用户订阅列表
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="page">搜索页码</param>
        /// <param name="recordsPerPage">每页显示文章数量</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        public static async Task<PointList> Get(string userId, int page, int recordsPerPage,
            [Injected] KeylolDbContext dbContext, [Injected] CachedDataProvider cachedData)
        {
            return
                (await
                    CreateAsync(userId, StateTreeHelper.GetCurrentUserId(), page, recordsPerPage, false, dbContext,
                        cachedData)).Item1;
        }

        /// <summary>
        /// 创建用用户订阅列表
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="currentUserId">当前用户 ID</param>
        /// <param name="page">搜索页码</param>
        /// <param name="recordsPerPage">每页显示文章数量</param>
        /// <param name="returnCount">是否返回据点总数</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns>Item1 表示 <see cref="PointList"/>，Item2 表示总数</returns>
        public static async Task<Tuple<PointList, int>> CreateAsync(string userId, string currentUserId, int page,
            int recordsPerPage, bool returnCount, KeylolDbContext dbContext, CachedDataProvider cachedData)
        {
            var conditionQuery = from subscription in dbContext.Subscriptions
                where subscription.SubscriberId == userId && subscription.TargetType == SubscriptionTargetType.Point
                select subscription;

            var queryResult = await (from subscription in conditionQuery
                join point in dbContext.Points on subscription.TargetId equals point.Id
                orderby subscription.Sid descending
                select new
                {
                    point.Id,
                    point.Type,
                    point.IdCode,
                    point.AvatarImage,
                    point.ChineseName,
                    point.EnglishName,
                    point.SteamAppId,
                    Count = returnCount ? conditionQuery.Count() : 1
                }).TakePage(page, recordsPerPage).ToListAsync();

            var result = new PointList(queryResult.Count);
            foreach (var p in queryResult)
            {
                result.Add(new Point
                {
                    Type = p.Type,
                    IdCode = p.IdCode,
                    AvatarImage = p.AvatarImage,
                    ChineseName = p.ChineseName,
                    EnglishName = p.EnglishName,
                    InLibrary = string.IsNullOrWhiteSpace(currentUserId) || p.SteamAppId == null
                        ? (bool?) null
                        : await cachedData.Users.IsSteamAppInLibraryAsync(currentUserId, p.SteamAppId.Value),
                    Subscribed =
                        string.IsNullOrWhiteSpace(currentUserId)
                            ? await cachedData.Subscriptions.IsSubscribedAsync(currentUserId, p.Id,
                                SubscriptionTargetType.Point)
                            : (bool?) null,
                    SubscriberCount =
                        await cachedData.Subscriptions.GetSubscriberCountAsync(p.Id, SubscriptionTargetType.Point),
                    ArticleCount = await (from subscription in conditionQuery
                        join articles in dbContext.Articles on subscription.TargetId equals articles.TargetPointId
                        select articles.Id).CountAsync(),
                    ActivityCount = await (from subscription in conditionQuery
                        join activities in dbContext.Activities on subscription.TargetId equals activities.TargetPointId
                        select activities.Id).CountAsync()
                });
            }
            var firstRecord = queryResult.FirstOrDefault();
            return new Tuple<PointList, int>(result, firstRecord?.Count ?? 0);
        }
    }

    /// <summary>
    /// 用户订阅的据点
    /// </summary>
    public class Point
    {
        /// <summary>
        /// 中文名
        /// </summary>
        public string ChineseName { get; set; }

        /// <summary>
        /// 英文名
        /// </summary>
        public string EnglishName { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string AvatarImage { get; set; }

        /// <summary>
        /// 识别码
        /// </summary>
        public string  IdCode { get; set; }

        /// <summary>
        /// 据点类型
        /// </summary>
        public PointType Type { get; set; }

        /// <summary>
        /// 读者数量
        /// </summary>
        public long? SubscriberCount { get; set; }

        /// <summary>
        /// 文章数
        /// </summary>
        public int? ArticleCount { get; set; }

        /// <summary>
        /// 动态数
        /// </summary>
        public int? ActivityCount { get; set; }

        /// <summary>
        /// 是否被订阅
        /// </summary>
        public bool? Subscribed { get; set; }

        /// <summary>
        /// 是否入库
        /// </summary>
        public bool? InLibrary { get; set; }
    }
}