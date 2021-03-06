﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using log4net.Config;
using MaterialDesignThemes.Wpf;
using PogoLocationFeeder.Common;
using PogoLocationFeeder.Config;
using PogoLocationFeeder.GUI.Common;
using PogoLocationFeeder.GUI.Models;
using PogoLocationFeeder.GUI.Properties;
using PropertyChanged;

//using POGOProtos.Enums;

namespace PogoLocationFeeder.GUI.ViewModels {
    [ImplementPropertyChanged]
    public class MainWindowViewModel {
        private Visibility _colVisibility;

        public MainWindowViewModel() {
            Instance = this;
            Pokemons = new ReadOnlyObservableCollection<SniperInfoModel>(GlobalVariables.PokemonsInternal);
            SettingsComand = new ActionCommand(ShowSettings);
            StartStopCommand = new ActionCommand(Startstop);
            DebugComand = new ActionCommand(ShowDebug);
            RemovePathCommand = new ActionCommand(RemovePath);
            SaveCommand = new ActionCommand(SaveClick);
            PayPalCommand = new ActionCommand(OpenPaypal);
            BitcoinCommand = new ActionCommand(OpenBitcoin);
            Settings.Default.DebugOutput = "";
            //var poke = new SniperInfo {
            //    Id = PokemonId.Missingno,
            //    Latitude = 45.99999,
            //    Longitude = 66.6677,
            //    ExpirationTimestamp = DateTime.Now
            //};
            //var y = new SniperInfoModel {
            //    Info = poke,
            //    Icon = new BitmapImage(new Uri(Path.Combine(iconPath, $"{(int) poke.Id}.png")))
            //};
            //GlobalVariables.PokemonsInternal.Add(y);
            GlobalSettings.Gui = true;
            XmlConfigurator.Configure(
                Assembly.GetExecutingAssembly().GetManifestResourceStream("PogoLocationFeeder.GUI.App.config"));
            GlobalSettings.Output = new Output();
            var p = new Program();
            var a = new Thread(p.Start) {IsBackground = true};
            a.Start();
        }

        public static MainWindowViewModel Instance { get; private set; }

        public int TransitionerIndex { get; set; }

        public PackIconKind PausePlayButtonIcon { get; set; } = PackIconKind.Pause;
        public ReadOnlyObservableCollection<SniperInfoModel> Pokemons { get; }

        public ICommand SettingsComand { get; }
        public ICommand DebugComand { get; }
        public ICommand StartStopCommand { get; }
        public ICommand RemovePathCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand PayPalCommand { get; }
        public ICommand BitcoinCommand { get; }

        public string CustomIp { get; set; } = "localhost";

        public int CustomPort { get; set; }

        public string Status { get; set; } = "Connected to pogo-feed.mmoex.com";

        public string ThreadStatus { get; set; } = "[Running]";

        public int ShowLimit { get; set; }

        public string Sniper2Exe { get; set; }

        public string RemoveMinutes { get; set; }

        public Visibility ColVisibility {
            get {
                _colVisibility = GlobalSettings.IsOneClickSnipeSupported() ? Visibility.Visible : Visibility.Collapsed;
                return _colVisibility;
            }
            set { _colVisibility = value; }
        }

        public void RemovePath() {
            Sniper2Exe = "";
        }

        public void SetStatus(string status) {
            Status = status;
        }

        public void ShowSettings() {
            if (TransitionerIndex != 0) {
                TransitionerIndex = 0;
                return;
            }
            GlobalSettings.Load();
            ShowLimit = GlobalSettings.ShowLimit;
            CustomPort = GlobalSettings.Port;
            Sniper2Exe = GlobalSettings.PokeSnipers2Exe;
            ShowLimit = GlobalSettings.ShowLimit;
            RemoveMinutes = GlobalSettings.RemoveAfter.ToString();
            TransitionerIndex = 1;

        }

        public void SaveClick() {
            if(Sniper2Exe != null && Sniper2Exe.Contains(".exe")) {
                ColVisibility = Visibility.Visible;
            }
            if (Sniper2Exe == null || Sniper2Exe.Equals("")) {
                Sniper2Exe = "";
                ColVisibility = Visibility.Collapsed;
            }
            GlobalSettings.ShowLimit = Math.Max(ShowLimit, 1);
            GlobalSettings.Port = CustomPort;
            GlobalSettings.PokeSnipers2Exe = Sniper2Exe;
            GlobalSettings.ShowLimit = ShowLimit;
            GlobalSettings.RemoveAfter = int.Parse(RemoveMinutes);
            GlobalSettings.Save();

            GlobalSettings.Output.RemoveListExtras();
        }

        public void ShowDebug() {
            if (TransitionerIndex != 0) {
                TransitionerIndex = 0;
                return;
            }
            TransitionerIndex = 2;
        }

        private void Startstop() {
            var status = GlobalSettings.ThreadPause;
            if (status) {
                GlobalSettings.ThreadPause = false;
                ThreadStatus = "[Running]";
                PausePlayButtonIcon = PackIconKind.Pause;
                return;
            }
            GlobalSettings.ThreadPause = true;
            ThreadStatus = "[Paused]";
            PausePlayButtonIcon = PackIconKind.Play;
        }

        public void OpenPaypal() {
            try {
                Process.Start("https://www.paypal.com/en_US/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=QZCKGUUQ9RYPY");

            } catch (Exception) {
                //ignore
            }
        }
        public void OpenBitcoin() {
            try {
                Process.Start("bitcoin:1FeederpUZXQN6F45M5cpYuYP6MzE2huPp?label=PogoLocationFeeder");

            } catch (Exception) {
                //ignore
            }
        }
    }

    public class BindingProxy : Freezable {
            #region Overrides of Freezable

            protected override Freezable CreateInstanceCore() {
                return new BindingProxy();
            }

            #endregion

            public object Data {
                get { return (object) GetValue(DataProperty); }
                set { SetValue(DataProperty, value); }
            }

            public static readonly DependencyProperty DataProperty =
                DependencyProperty.Register("Data", typeof(object),
                    typeof(BindingProxy));
        }
    }
