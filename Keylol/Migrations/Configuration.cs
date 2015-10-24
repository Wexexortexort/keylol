﻿using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using Keylol.DAL;
using Keylol.Models;
using Microsoft.AspNet.Identity;

namespace Keylol.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<KeylolDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(KeylolDbContext context)
        {
            context.ArticleTypes.AddOrUpdate(type => type.Name,
                new ArticleType()
                {
                    Name = "评",
                    Description = "感悟、心得、体验、报告",
                    AllowVote = true
                },
                new ArticleType()
                {
                    Name = "研",
                    Description = "攻略、技术、成就、教程"
                },
                new ArticleType()
                {
                    Name = "讯",
                    Description = "新闻、购物、更新、竞技"
                },
                new ArticleType()
                {
                    Name = "谈",
                    Description = "聊天、灌水、吐槽、杂文"
                },
                new ArticleType()
                {
                    Name = "档",
                    Description = "声画、模组、插件、汉化"
                });

            context.NormalPoints.AddOrUpdate(point => point.IdCode,
                new NormalPoint
                {
                    AvatarImage = "keylol://avatars/d6fa208df2f1a15d2d14324cd1f3004c.jpg",
                    ChineseName = "军团要塞2",
                    EnglishName = "Team Fortress 2",
                    PreferedName = PreferedNameType.Chinese,
                    ChineseAliases = "絕地要塞2",
                    EnglishAliases = "tf2,tf,tfc",
                    IdCode = "TMFT2",
                    Type = NormalPointType.Game,
                    StoreLink = "http://store.steampowered.com/app/440/",
                    BackgroundImage = "d6fa208df2f1a15d2d14324cd1f3004c.jpg"
                });

#if !DEBUG
            var credentials =
                @"keylol_bot_1 YrXF9LGfHkTJYW8HE4GE8YpJ|keylol_bot_2 NWG8SUTuXKGBkK7g4dXHCWdU|keylol_bot_3 EXc897fp5cg2akUpzazwRCk2|keylol_bot_4 LNFLNCvmSmr2EJHqRNHjpNVt|keylol_bot_5 AVw9sFHWQuZ9jx4xc8cwA5ny"
                    .Split('|').Select(s =>
                    {
                        var parts = s.Split(' ');
                        return new
                        {
                            UserName = parts[0],
                            Password = parts[1]
                        };
                    });

            foreach (var credential in credentials)
            {
                context.SteamBots.AddOrUpdate(bot => bot.SteamUserName, new Models.SteamBot
                {
                    SteamUserName = credential.UserName,
                    SteamPassword = credential.Password,
                    FriendUpperLimit = 200
                });
            }
#endif

            //            var hasher = new PasswordHasher();
            //            var profilePoint = new ProfilePoint
            //            {
            //                OwnedEntries = new List<Entry>()
            //            };
            //            var normalPoint = new NormalPoint
            //            {
            //                Name = "测试据点",
            //                AlternativeName = "Test Point",
            //                UrlFriendlyName = "test-point",
            //                Type = NormalPointType.Game
            //            };
            //            var user = new KeylolUser
            //            {
            //                UserName = "stackia",
            //                PasswordHash = hasher.HashPassword("test"),
            //                Email = "jsq2627@gmail.com",
            //                ProfilePoint = profilePoint,
            //                SecurityStamp = "hahaha",
            //                SubscribedPoints = new List<Point> {normalPoint},
            //                ModeratedPoints = new List<NormalPoint> {normalPoint}
            //            };
            //            var articleType = new ArticleType
            //            {
            //                Category = ArticleTypeCategory.Topic,
            //                Name = "测试",
            //                UrlFriendlyName = "test"
            //            };
            //            var article = new Article
            //            {
            //                Title = "测试文章",
            //                Content = "哈哈哈哈哈",
            //                Type = articleType,
            //                AttachedPoints = new List<Point> {profilePoint, normalPoint}
            //            };
            //            profilePoint.OwnedEntries.Add(article);
            //            context.Users.Add(user);
            //            context.SaveChanges();
        }
    }
}