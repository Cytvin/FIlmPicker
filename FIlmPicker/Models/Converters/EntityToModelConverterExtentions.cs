using Microsoft.AspNetCore.Identity;
using FIlmPicker.Data.Models;

namespace FIlmPicker.Models.Converters
{
    public static class EntityToModelConverterExtentions
    {
        public static User ToModel(this IdentityUser user)
        {
            return new User(user.Id, user.UserName ?? "Unknown");
        }

        public static RoomSettings ToModel(this RoomSettingsEntity roomSettings)
        {
            List<Genre>? genres = roomSettings.Genres?.Select(g => g.ToModel()).ToList();

            return new RoomSettings(roomSettings.Id, roomSettings.RoomId, roomSettings.MinKpRating, 
                roomSettings.MaxKpRating, roomSettings.MinYear, roomSettings.MaxYear,
                roomSettings.TypeNumber, roomSettings.MoviesReceived, roomSettings.MinVotes, genres);
        }

        public static Genre ToModel(this GenreEntity genre)
        {
            return new Genre(genre.Id, genre.Name);
        }

        public static Room ToModel(this RoomEntity room)
        {
            User owner = room.Owner.ToModel();
            User guest = room.Guest.ToModel();
            RoomSettings roomSettings = room.RoomSetting.ToModel();

            return new Room(room.Id, owner, guest, room.InviteAccepted, room.OwnerIsOut, room.GuestIsOut, roomSettings);
        }

        public static Movie ToModel(this RoomMovieEntity roomMovieEntity)
        {
            List<Genre> genres = roomMovieEntity.Movie.Genres.Select(g => g.ToModel()).ToList();

            return new Movie(roomMovieEntity.MovieId, roomMovieEntity.RoomId, roomMovieEntity.Movie.Name, roomMovieEntity.Movie.Description,
                roomMovieEntity.Movie.TypeId, roomMovieEntity.Movie.MovieLength, roomMovieEntity.Movie.Year, roomMovieEntity.Movie.KpRaiting,
                roomMovieEntity.Movie.ImdbRating, roomMovieEntity.Movie.Poster, (UserScore)roomMovieEntity.OwnerScore, (UserScore)roomMovieEntity.GuestScore, genres);
        }
    }
}
