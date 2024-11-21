using Microsoft.AspNetCore.Mvc.Rendering;

namespace FIlmPicker.Models
{
    public class RoomSettingsViewModel
    {
        public string RoomId { get; set; }
        public RoomSettings RoomSettings { get; set; }
        public IEnumerable<Genre> Genres { get; set; }
        public string SecondUserName { get; set; }
        public string? StatusMessage { get; set; }
        public List<SelectListItem> Types { get; set; } = new List<SelectListItem>
        {
            new SelectListItem{ Value = "1", Text = "Фильм"},
            new SelectListItem{ Value = "2", Text = "Сериал"},
            new SelectListItem{ Value = "3", Text = "Мульфильм"},
            new SelectListItem{ Value = "4", Text = "Аниме"},
            new SelectListItem{ Value = "5", Text = "Мультсериал"}
        };
    }
}
