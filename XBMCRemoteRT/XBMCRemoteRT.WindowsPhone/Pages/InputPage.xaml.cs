﻿using XBMCRemoteRT.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using XBMCRemoteRT.RPCWrappers;
using Newtonsoft.Json.Linq;
using XBMCRemoteRT.Helpers;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Animation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace XBMCRemoteRT.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InputPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        private bool isVolumeSetProgrammatically;
        int[] Speeds = { -32, -16, -8, -4, -2, -1, 1, 2, 4, 8, 16, 32 };

        public InputPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            DataContext = GlobalVariables.CurrentPlayerState;
            PopulateFlyout();
        }        

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            GlobalVariables.CurrentTracker.SendView("InputPage");
            this.navigationHelper.OnNavigatedTo(e);
            ShowButtons();
        }

        private void ShowButtons()
        {
            string[] buttons = ((string)SettingsHelper.GetValue("ButtonsToShow", "GoBack, Home, TextInput")).Split(',');
            foreach (string button in buttons)
            {
                Button btn = this.FindName(button.Trim() + "Button") as Button;
                btn.Visibility = Visibility.Visible;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
        
        #region Remote Keys
        private void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            Input.ExecuteAction(InputCommands.Left);
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            Input.ExecuteAction(InputCommands.Up);
        }

        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            Input.ExecuteAction(InputCommands.Right);
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            Input.ExecuteAction(InputCommands.Down);
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            Input.ExecuteAction(InputCommands.Home);
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            Input.ExecuteAction(InputCommands.ContextMenu);
        }

        private void OSDButton_Click(object sender, RoutedEventArgs e)
        {
            Input.ExecuteAction(InputCommands.ShowOSD);
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            Input.ExecuteAction(InputCommands.Info);
        }

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            Input.ExecuteAction(InputCommands.Select);
        }

        private void GoBackButton_Click(object sender, RoutedEventArgs e)
        {
            Input.ExecuteAction(InputCommands.Back);
        }

        #endregion

        private async void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            await Player.GoTo(GlobalVariables.CurrentPlayerState.PlayerType, GoTo.Next);
            await PlayerHelper.RefreshPlayerState();
        }

        private async void SpeedDownButton_Click(object sender, RoutedEventArgs e)
        {
            string backwardCommand = (string)SettingsHelper.GetValue("BackwardButtonCommand", "SmallBackward");
            if (backwardCommand == "DecreaseSpeed")
            {
                int speed = GlobalVariables.CurrentPlayerState.CurrentPlayerProperties.Speed;

                if (speed != 0 && speed != -32)
                {
                    int index = Array.IndexOf(Speeds, speed);
                    int newSpeed = Speeds[index - 1];
                    await Player.SetSpeed(GlobalVariables.CurrentPlayerState.PlayerType, newSpeed);
                    await PlayerHelper.RefreshPlayerState();
                }
            }
            else
            {
                Player.Seek(GlobalVariables.CurrentPlayerState.PlayerType, backwardCommand.ToLower());
            }
        }

        private async void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            await Player.PlayPause(GlobalVariables.CurrentPlayerState.PlayerType);
            await PlayerHelper.RefreshPlayerState();
        }

        private async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            await Player.Stop(GlobalVariables.CurrentPlayerState.PlayerType);
            await PlayerHelper.RefreshPlayerState();
        }

        private async void SpeedUpButton_Click(object sender, RoutedEventArgs e)
        {
            string forwardCommand = (string)SettingsHelper.GetValue("ForwardButtonCommand", "SmallForward");
            if (forwardCommand == "IncreaseSpeed")
            {
                int speed = GlobalVariables.CurrentPlayerState.CurrentPlayerProperties.Speed;

                if (speed != 0 && speed != 32)
                {
                    int index = Array.IndexOf(Speeds, speed);
                    int newSpeed = Speeds[index + 1];
                    await Player.SetSpeed(GlobalVariables.CurrentPlayerState.PlayerType, newSpeed);
                    await PlayerHelper.RefreshPlayerState();
                }
            }
            else
            {
                Player.Seek(GlobalVariables.CurrentPlayerState.PlayerType, forwardCommand.ToLower());
            }
        }

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            await Player.GoTo(GlobalVariables.CurrentPlayerState.PlayerType, GoTo.Next);
            await PlayerHelper.RefreshPlayerState();
        }

        private async void VolumeSlider_Loaded(object sender, RoutedEventArgs e)
        {
            int volume = await Applikation.GetVolume();
            SetVolumeSliderValue(volume);
        }        

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            Applikation.Quit();
        }

        private DispatcherTimer timer;
        private void VolumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (isVolumeSetProgrammatically)
            {
                isVolumeSetProgrammatically = false;
                return;
            }
            else
            {
                if (timer != null)
                    timer.Stop();

                timer = new DispatcherTimer();

                timer.Interval = new TimeSpan(0, 0, 1);
                timer.Tick += timer_Tick;

                timer.Start();
            }
        }

        void timer_Tick(object sender, object e)
        {
            int value = (int)Math.Round(VolumeSlider.Value);
            Applikation.SetVolume(value);

            timer.Stop();
            timer.Tick -= timer_Tick;
        }

        private async void VolumeDownWrapper_Tapped(object sender, TappedRoutedEventArgs e)
        {
            int currentVolume = await Applikation.GetVolume();
            Applikation.SetVolume(--currentVolume);
            SetVolumeSliderValue(currentVolume);
        }

        private async void VolumeUpWrapper_Tapped(object sender, TappedRoutedEventArgs e)
        {
            int currentVolume = await Applikation.GetVolume();
            Applikation.SetVolume(++currentVolume);
            SetVolumeSliderValue(currentVolume);
        }

        private void SetVolumeSliderValue(int value)
        {
            isVolumeSetProgrammatically = true;
            VolumeSlider.Value = value;
        }

        private void SendTextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                Input.SendText(SendTextBox.Text, true);
                SendTextBox.Text = string.Empty;
                LoseFocus(sender);
            }
        }

        private void LoseFocus(object sender)
        {
            var control = sender as Control;
            var isTabStop = control.IsTabStop;
            control.IsTabStop = false;
            control.IsEnabled = false;
            control.IsEnabled = true;
            control.IsTabStop = isTabStop;
        }

        private void TextInputButton_Click(object sender, RoutedEventArgs e)
        {
            SendTextBox.Visibility = Visibility.Visible;
            (this.Resources["ShowSendTextBox"] as Storyboard).Begin();
            SendTextBox.Focus(FocusState.Keyboard);
        }

        private void SendTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ((this.Resources["HideSendTextBox"]) as Storyboard).Begin();
        }

        private void SubtitlesButton_Click(object sender, RoutedEventArgs e)
        {
            Input.ExecuteAction("nextsubtitle");
        }

        private void AdvancedButton_Click(object sender, RoutedEventArgs e)
        {
            AdvancedMenuFlyout.SelectedItem = null;
        }

        
        private string audioLibUpdate;// = "update audio library";
        private string videoLibUpdate;// = "update video library";
        private string audioLibClean;// = "clean audio library";
        private string videoLibClean;// ="clean video library";
        private string showSubtitleSerach;// = "download subtitles";

        private void PopulateFlyout()
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            audioLibUpdate = loader.GetString("UpdateAudioLibrary");
            videoLibUpdate = loader.GetString("UpdateVideoLibrary");
            audioLibClean = loader.GetString("CleanAudioLibrary");
            videoLibClean = loader.GetString("CleanVideoLibrary");
            showSubtitleSerach = loader.GetString("DownloadSubtitles");

            AdvancedMenuFlyout.ItemsSource = new List<string> { audioLibUpdate, videoLibUpdate, audioLibClean, videoLibClean, showSubtitleSerach };
        }

        private void AdvancedMenuFlyout_ItemsPicked(ListPickerFlyout sender, ItemsPickedEventArgs args)
        {
            string pickedCommand = (string)AdvancedMenuFlyout.SelectedItem;
            
            if (pickedCommand == audioLibUpdate)
                AudioLibrary.Scan();
            else if (pickedCommand == videoLibUpdate)
                VideoLibrary.Scan();
            else if (pickedCommand == audioLibClean)
                AudioLibrary.Clean();
            else if (pickedCommand == videoLibClean)
                VideoLibrary.Clean();
            else if (pickedCommand == showSubtitleSerach)
                GUI.ShowSubtitleSearch();
        }
    }
}
