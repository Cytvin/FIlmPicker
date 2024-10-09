using FIlmPicker.Models;
using FIlmPicker.Models.DTO;

namespace FIlmPicker.Converters
{
    public static class ToDTOConvertersExtentions
    {
        public static RoomDTO ToDTO(this Room room)
        {
            RoomDTO dto = new RoomDTO()
            {
                Id = room.Id,
                Owner = room.Owner.ToDTO(),
                Guest = room.Guest.ToDTO(),
                InviteAccepted = room.InviteAccepted,
                Settings = room.RoomSettings.ToDTO()
            };

            return dto;
        }

        public static UserDTO ToDTO(this User user)
        {
            UserDTO dto = new UserDTO()
            {
                Id = user.Id,
                UserName = user.UserName
            };

            return dto;
        }

        public static RoomSettingsDTO ToDTO(this RoomSettings roomSettings) 
        {
            RoomSettingsDTO dto = new RoomSettingsDTO()
            {
                Id = roomSettings.Id,
                RoomId = roomSettings.RoomId,
                MinKpRaitings = roomSettings.MinKpRating,
                MaxKpRaitings = roomSettings.MaxKpRating,
                MinYear = roomSettings.MinYear,
                MaxYear = roomSettings.MaxYear,
                TypeNumber = roomSettings.TypeNumber
            };

            return dto;
        }

        public static MovieDTO ToDTO(this Movie movie)
        {
            MovieDTO dto = new MovieDTO()
            {
                Id = movie.Id,
                RoomId = movie.RoomId,
                Name = movie.Name,
                Description = movie.Description,
                Year = movie.Year,
                MovieLength = movie.MovieLength,
                ImdbRaiting = movie.ImdbRaiting,
                KpRaiting = movie.KpRaiting,
                TypeNumber = movie.TypeNumber,
                Poster = movie.Poster,
                OwnerScore = (int)movie.OwnerScore,
                GuestScore = (int)movie.GuestScore,
                Genres = movie.Genres.Select(g => g.ToDTO()).ToList()
            };

            return dto;
        }

        public static GenreDTO ToDTO(this Genre genre)
        {
            GenreDTO dto = new GenreDTO()
            {
                Id = genre.Id,
                Name = genre.Name
            };

            return dto;
        }
    }
}
