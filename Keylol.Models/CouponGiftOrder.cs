﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keylol.Models
{
    public class CouponGiftOrder
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string UserId { get; set; }

        public KeylolUser User { get; set; }

        [Required]
        public string GiftId { get; set; }

        public CouponGift Gift { get; set; }

        [Index]
        public DateTime RedeemTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 用户额外录入的信息，JSON 格式，按照 <see cref="CouponGift"/> AcceptedFields 录入
        /// </summary>
        [Required]
        public string Extra { get; set; } = "{}";

        public bool Finished { get; set; } = false;
    }
}