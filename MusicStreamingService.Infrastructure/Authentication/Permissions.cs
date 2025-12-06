namespace MusicStreamingService.Infrastructure.Authentication;

public static class Permissions
{
    public const string ViewSongsPermission = "mss.songs.view";
    public const string ManageSongsPermission = "mss.songs.manage";
    public const string PlaySongsPermission = "mss.songs.playback";
    public const string FavoriteSongsPermission = "mss.songs.favorite";
    public const string AdministrateSongsPermission = "mss.songs.admin";
    
    public const string ViewAlbumsPermission = "mss.albums.view";
    public const string FavoriteAlbumsPermission = "mss.albums.favorite";
    public const string ManageAlbumsPermission = "mss.albums.manage";
    public const string AdministrateAlbumsPermission = "mss.albums.admin";
    
    public const string ViewUsersPermission = "mss.users.view";
    public const string ManageUsersPermission = "mss.users.manage";
    public const string AdministrateUsersPermission = "mss.users.admin";
    
    public const string ViewSubscriptionsPermission = "mss.subscriptions.view";
    public const string ManageSubscriptionsPermission = "mss.subscriptions.manage";
    public const string AdministrateSubscriptionsPermission = "mss.subscriptions.admin";
    
    public const string ViewPlaylistsPermission = "mss.playlists.view";
    public const string FavoritePlaylistsPermission = "mss.playlists.favorite";
    public const string ManagePlaylistsPermission = "mss.playlists.manage";
    public const string AdministratePlaylistsPermission = "mss.playlists.admin";
    
    public const string ViewGenresPermission = "mss.genres.view";
    public const string AdministrateGenresPermission = "mss.genres.admin";
    
    public const string ViewRegionsPermission = "mss.regions.view";
    public const string AdministrateRegionsPermission = "mss.regions.admin";
    
    public const string ViewStreamingEventsPermission = "mss.streaming-events.view";
    public const string ManageStreamingEventsPermission = "mss.streaming-events.manage";
    public const string AdministrateStreamingEventsPermission = "mss.streaming-events.admin";
    
    public const string ViewPaymentsPermission = "mss.payments.view";
    public const string ManagePaymentsPermission = "mss.payments.manage";
    public const string AdministratePaymentsPermission = "mss.payments.admin";
}