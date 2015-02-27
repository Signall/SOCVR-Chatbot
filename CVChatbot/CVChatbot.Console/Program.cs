﻿using CVChatbot.Bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCL.Extensions;

namespace CVChatbot.Console
{
    class Program
    {
        static RoomManager mng;
        static bool shutdownOrderGiven = false;

        static void Main(string[] args)
        {
            WriteToConsole("Starting program");

            mng = new RoomManager();
            mng.ShutdownOrderGiven += mng_ShutdownOrderGiven;
            mng.InformationMessageBroadcasted += mng_InformationMessageBroadcasted;

            WriteToConsole("Gathering settings");

            var settings = new InstallationSettings()
            {
                ChatRoomUrl = SettingsFileAccessor.GetSettingValue<string>("ChatRoomUrl"),
                Email = SettingsFileAccessor.GetSettingValue<string>("LoginEmail"),
                Password = SettingsFileAccessor.GetSettingValue<string>("LoginPassword"),
                StartUpMessage = SettingsFileAccessor.GetSettingValue<string>("StartUpMessage"),
                StopMessage = SettingsFileAccessor.GetSettingValue<string>("StopMessage"),
                MaxReviewLengthHours = SettingsFileAccessor.GetSettingValue<int>("MaxReviewLengthHours"),
                DefaultCompletedTagsPeopleThreshold = SettingsFileAccessor.GetSettingValue<int>("DefaultCompletedTagsPeopleThreshold"),
                MaxTagsToFetch = SettingsFileAccessor.GetSettingValue<int>("MaxFetchTags"),
                DatabaseConnectionString = SettingsFileAccessor.GetSettingValue<string>("DatabaseConnectionString"),
                PingReviewersDaysBackThreshold = SettingsFileAccessor.GetSettingValue<int>("PingReviewersDaysBackThreshold"),
                DefaultNextTagCount = SettingsFileAccessor.GetSettingValue<int>("DefaultNextTagCount"),
            };

            WriteToConsole("Joining room");
            mng.JoinRoom(settings);

            WriteToConsole("Running wait loop");
            while (!shutdownOrderGiven)
            {
                //thumbs be moving
            }
        }

        static void mng_InformationMessageBroadcasted(string message)
        {
            WriteToConsole(message);
        }

        static void mng_ShutdownOrderGiven(object sender, EventArgs e)
        {
            WriteToConsole("Shutdown order given.");
            shutdownOrderGiven = true;
        }

        private static object writeToConsoleLockObject = new object();
        private static void WriteToConsole(string message)
        {
            lock (writeToConsoleLockObject)
            {
                // [2000-01-01 00:00:00.00] [<profile id>] (<Message Type>) <message>
                var formattedMessage = "[{0}] {1}".FormatInline(DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss.ff zzz"), message);
                System.Console.WriteLine(formattedMessage);
            }
        }
    }
}