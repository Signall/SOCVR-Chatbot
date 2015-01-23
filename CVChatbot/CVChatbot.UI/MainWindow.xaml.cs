﻿using CVChatbot.Bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CVChatbot.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        RoomManager mng;

        public MainWindow()
        {
            InitializeComponent();
            mng = new RoomManager();

            lblCurrentStatus.Content = "Disconnected";
        }

        private void btnQuit_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private async void btnStartStop_Click(object sender, RoutedEventArgs e)
        {
            if (btnStartStop.Content.ToString() == "Start Bot")
            {
                

                RoomManagerSettings settings = new RoomManagerSettings()
                {
                    ChatRoomUrl = SettingsAccessor.GetSettingValue<string>("ChatRoomUrl"),
                    Username = SettingsAccessor.GetSettingValue<string>("LoginUsername"),
                    Email = SettingsAccessor.GetSettingValue<string>("LoginEmail"),
                    Password = SettingsAccessor.GetSettingValue<string>("LoginPassword"),
                    StartUpMessage = SettingsAccessor.GetSettingValue<string>("StartUpMessage"),
                };

                lblCurrentStatus.Content = "Joining...";
                btnStartStop.IsEnabled = false;
                await Task.Run(() => mng.JoinRoom(settings));

                btnStartStop.IsEnabled = true;
                lblCurrentStatus.Content = "Connected";
                btnStartStop.Content = "Stop Bot";
            }
            else
            {
                var stopMessage = SettingsAccessor.GetSettingValue<string>("StopMessage");

                mng.LeaveRoom(stopMessage);
                mng = new RoomManager();
                lblCurrentStatus.Content = "Disconnected";
                btnStartStop.Content = "Start Bot";

                //current html code only works once, once that is fixed then I can start the bot again
                //without having to relaunch the program
                btnStartStop.IsEnabled = false;
            }
        }
    }
}
