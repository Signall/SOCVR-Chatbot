﻿using SOCVR.Chatbot.Database;
using System;
using System.Reflection;
using System.Linq;
using SOCVR.Chatbot.Configuration;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Utilities
{
    internal class Status : UserCommand
    {
        public override string ActionDescription =>
            "Tests if the chatbot is alive and shows simple info about it.";

        public override string ActionName => "Status";

        public override string ActionUsage => "status";

        public override PermissionGroup? RequiredPermissionGroup => null;

        public override bool UserMustBeInAnyPermissionGroupToRun => false;

        protected override string RegexMatchingPattern => @"^status\??$";

        public override void RunAction(ChatExchangeDotNet.Message incomingChatMessage, ChatExchangeDotNet.Room chatRoom)
        {
            var tracker = (UserTracking)typeof(Program).GetField("watcher", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            var avgLat = Math.Round(tracker.WatchedUsers.Values.Average(x => x.DetectionLatency.TotalMilliseconds));

            var elapsedTime = DateTime.Now - ChatBotStats.LoginDate;
            var sha = ThisAssembly.Git.Sha.Substring(0, 8);
            var branch = ThisAssembly.Git.Branch;
            var location = ConfigurationAccessor.InstallationLocation;
            var commitUrl = $"https://github.com/SO-Close-Vote-Reviewers/SOCVR-Chatbot/commit/{ThisAssembly.Git.Sha}";

            var message = $"SOCVR Chatbot, running at {location}, version [`{sha}`]({commitUrl}) on {branch}, running for {elapsedTime.ToUserFriendlyString()} (tracking latency `{avgLat}`ms).";

            chatRoom.PostMessageOrThrow(message);
        }
    }
}
