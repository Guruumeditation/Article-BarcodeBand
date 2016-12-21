using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Band.Tiles;

namespace BarcodeBand
{
    public sealed partial class MainPage : INotifyPropertyChanged
    {
        private BandHelper _bandHelper;

        private readonly List<Movie> _movies = new List<Movie> {new Movie
            {
                Poster = "assassins_creed.jpg",
                Time = DateTime.Today.AddDays(1).AddHours (14),
                Title = "Assassin's Creed"
            },
            new Movie
            {
                Poster = "assassins_creed.jpg",
                Time = DateTime.Today.AddDays(1).AddHours (17),
                Title = "Assassin's Creed"
            },
            new Movie
            {
                Poster = "Captain-America-Civil-War.jpg",
                Time = DateTime.Today.AddDays(1).AddHours (14),
                Title = "Captain America : Civil War"
            },
            new Movie
            {
                Poster = "Captain-America-Civil-War.jpg",
                Time = DateTime.Today.AddDays(1).AddHours (20),
                Title = "Captain America : Civil War"
            },
            new Movie
            {
                Poster = "star_wars_rogue_one.jpg",
                Time = DateTime.Today.AddDays(1).AddHours (19),
                Title = "Star Wars Rogue One"
            },
            new Movie
            {
                Poster = "star_wars_rogue_one.jpg",
                Time = DateTime.Today.AddDays(1).AddHours (21.5),
                Title = "Star Wars Rogue One"
            }
            };

        private bool _isBusy;
        private string _message;

        public List<Movie> Movies => _movies;

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
               OnPropertyChanged();
            }
        }

        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        public MainPage () {
            InitializeComponent ();
            DataContext = this;

            SetBusy (true, "Connecting to Band");
        }

        protected override void OnNavigatedTo (NavigationEventArgs e) {
            base.OnNavigatedTo (e);
            ConnectToBandAsync ();
        }

        private async Task ConnectToBandAsync ()
        {
            _bandHelper = await BandHelper.GetBandHelperAsync();

            if (!await _bandHelper.CheckIfTileAlreadyInstalledAsync())
            {
                Message = "Adding Tile";
                await _bandHelper.AddNewTileAsync (); 
            }
            SetBusy (false);
        }

        private async void ListViewBase_OnItemClick(object sender, ItemClickEventArgs e)
        {
            SetBusy(true, "Adding movie");

            var movie = e.ClickedItem as Movie;
            await _bandHelper.SetPageDataAsync(movie);

            SetBusy (false);
        }

        private void SetBusy(bool isbusy, string message = "")
        {
            IsBusy = isbusy;
            ProgessStackPanel.Visibility = isbusy ? Visibility.Visible : Visibility.Collapsed;
            MovieListView.Opacity = isbusy ? 0.2 :  1.0;
            Message = message;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum PageElementEnum : short
    {
        MovieName = 1,
        MovieTime = 2,
        MovieBarcode = 3
    }
}
