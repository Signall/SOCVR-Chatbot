﻿using System;

namespace SOCVR.Chatbot.Database
{
    class CompletedTag
    {
        public string TagName { get; set; }
        public int PeopleWhoCompletedTag { get; set; }
        public DateTimeOffset LastEntryTs { get; set; }
    }
}
