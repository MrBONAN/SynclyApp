using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core;
using The49.Maui.BottomSheet;
using CommunityToolkit.Maui.Views;
using Infrastructure.API.SpotifyAPI;

namespace ProfileBottomSheet;

public partial class Sheet : BottomSheet
{
    public Sheet()
    {
        InitializeComponent();
        BindingContext = new ProfileBottomSheetViewModel();
    }
}

public class ProfileBottomSheetViewModel
{
    public ObservableCollection<Track> Tracks { get; set; }
    public ObservableCollection<Artist> Artists { get; set; }

    public ProfileBottomSheetViewModel()
    {
        Tracks = new ObservableCollection<Track>
        {
            new Track { Title = "Smells Like Teen Spirit", Artist = "Nirvana", AlbumCover = "nirvana_cover.jpg" },
            new Track { Title = "Тесно", Artist = "BUSHIDO ZHO" },
            new Track { Title = "Тесно", Artist = "BUSHIDO ZHO" },
            new Track { Title = "Мяу", Artist = "Heronwater" }
        };
        
        Artists = new ObservableCollection<Artist>
        {
            new Artist { Name = "Nirvana" },
            new Artist { Name = "BUSHIDO ZHO" },
            new Artist { Name = "Heronwater" },
            new Artist { Name = "FORTUNA 812" }
        };
    }
}

public class Track
{
    public string Title { get; set; }
    public string Artist { get; set; }
    public string AlbumCover { get; set; }
}

public class Artist
{
    public string Name { get; set; }
    public string Cover { get; set; }
}