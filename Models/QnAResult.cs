﻿using System;
namespace QnABot.Models
{
    public class QnAResult
    {
        public string[] Questions { get; set; }

        public string Answer { get; set; }

        public double Score { get; set; }

        public int Id { get; set; }

        public string Source { get; set; }

        public QnAMetadata[] Metadata { get; set; }

        public QnAContext Context { get; set; }
    }
}
