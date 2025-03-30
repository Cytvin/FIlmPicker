using FIlmPicker.Data;
using FIlmPicker.Data.Models;
using FIlmPicker.Models;
using FIlmPicker.Services.DatabaseServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace FIlmPicker.Tests
{
    public class RoomServiceTests
    {
        private readonly ITestOutputHelper _output;
        private readonly Mock<ILogger<RoomService>> _loggerMock;

        public RoomServiceTests(ITestOutputHelper output)
        {
            _loggerMock = new Mock<ILogger<RoomService>>();
            _output = output;
        }

        private ApplicationDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var context = new ApplicationDbContext(options);

            return context;
        }

        [Fact]
        public async Task GetRoomAsync_ReturnsRoom_WhenRoomExists()
        {
            // Arrange
            var context = GetInMemoryDbContext("GetRoomAsyncDb");
            var roomService = new RoomService(_loggerMock.Object, context);

            var roomEntity = new RoomEntity { Id = "1", OwnerId = "Owner1", GuestId = "Guest1", InviteAccepted = true };
            var ownerEntity = new IdentityUser { Id = "Owner1", UserName = "OwnerName" };
            var guestEntity = new IdentityUser { Id = "Guest1", UserName = "GuestName" };
            var roomSettingsEntity = new RoomSettingsEntity { RoomId = "1" };
            roomEntity.RoomSetting = roomSettingsEntity;

            var roomEntity2 = new RoomEntity { Id = "2", OwnerId = "Owner2", GuestId = "Guest2", InviteAccepted = true };
            var ownerEntity2 = new IdentityUser { Id = "Owner2", UserName = "OwnerName" };
            var guestEntity2 = new IdentityUser { Id = "Guest2", UserName = "GuestName" };
            var roomSettingsEntity2 = new RoomSettingsEntity { RoomId = "2" };
            roomEntity2.RoomSetting = roomSettingsEntity2;

            context.Users.AddRange(ownerEntity, guestEntity, ownerEntity2, guestEntity2);
            context.Rooms.AddRange(roomEntity, roomEntity2);
            await context.SaveChangesAsync();

            // Act
            var result = await roomService.GetRoomAsync("1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1", result.Id);
        }

        [Fact]
        public async Task GetRoomAsync_ReturnsNull_WhenRoomDoesNotExist()
        {
            // Arrange
            var context = GetInMemoryDbContext("GetRoomAsyncDb_NotExist");
            var roomService = new RoomService(_loggerMock.Object, context);

            // Act
            var result = await roomService.GetRoomAsync("1");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateRoomAsync_CreatesNewRoom()
        {
            // Arrange
            var context = GetInMemoryDbContext("CreateRoomAsyncDb");
            var roomService = new RoomService(_loggerMock.Object, context);

            // Act
            await roomService.CreateRoomAsync("Owner1", "Guest1");

            // Assert
            var createdRoom = await context.Rooms.FirstOrDefaultAsync(r => r.OwnerId == "Owner1" && r.GuestId == "Guest1");
            Assert.NotNull(createdRoom);
            Assert.Equal("Owner1", createdRoom.OwnerId);
            Assert.Equal("Guest1", createdRoom.GuestId);
        }

        [Fact]
        public async Task UpdateRoomAsync_UpdatesRoom()
        {
            // Arrange
            var context = GetInMemoryDbContext("UpdateRoomAsyncDb");
            var roomService = new RoomService(_loggerMock.Object, context);

            var roomEntity = new RoomEntity { Id = "1", OwnerId = "Owner1", GuestId = "Guest1", InviteAccepted = false };
            context.Rooms.Add(roomEntity);
            await context.SaveChangesAsync();

            var room = new Room("1", new User("Owner1", "OwnerName"), new User("Guest1", "GuestName"), true, false, false);

            // Act
            await roomService.UpdateRoomAsync(room);

            // Assert
            var updatedRoom = await context.Rooms.FirstOrDefaultAsync(r => r.Id == "1");
            Assert.NotNull(updatedRoom);
            Assert.True(updatedRoom.InviteAccepted);
        }

        [Fact]
        public async Task DeleteRoomAsync_DeletesRoom()
        {
            // Arrange
            var context = GetInMemoryDbContext("DeleteRoomAsyncDb");
            var roomService = new RoomService(_loggerMock.Object, context);

            var roomEntity = new RoomEntity { Id = "1", OwnerId = "Owner1", GuestId = "Guest1" };
            context.Rooms.Add(roomEntity);
            await context.SaveChangesAsync();

            // Act
            await roomService.DeleteRoomAsync("1");

            // Assert
            var deletedRoom = await context.Rooms.FirstOrDefaultAsync(r => r.Id == "1");
            Assert.Null(deletedRoom);
        }

        [Fact]
        public async Task AddMovieListToRoomAsync_AddsMoviesToRoom()
        {
            // Arrange
            var context = GetInMemoryDbContext("AddMovieListToRoomAsyncDb");
            var roomService = new RoomService(_loggerMock.Object, context);

            var roomEntity = new RoomEntity { Id = "1", OwnerId = "Owner1", GuestId = "Guest1" };
            context.Rooms.Add(roomEntity);
            await context.SaveChangesAsync();

            var movies = new List<Movie>
            {
                new Movie(1, "Room1", "Movie1", "Description1", 1, 120, 2021, 8.5, 7.5, "Poster1", UserScore.None, UserScore.None, new List<Genre> { new Genre("Action") }),
                new Movie(2, "Room1", "Movie2", "Description2", 1, 130, 2022, 8.0, 7.0, "Poster2", UserScore.None, UserScore.None, new List<Genre> { new Genre("Comedy") })
            };

            // Act
            await roomService.AddMovieListToRoomAsync("1", movies);

            // Assert
            var roomMovies = await context.RoomMovies.Where(rm => rm.RoomId == "1").ToListAsync();
            Assert.Equal(2, roomMovies.Count);
            Assert.Contains(roomMovies, rm => rm.MovieId == 1);
            Assert.Contains(roomMovies, rm => rm.MovieId == 2);
        }

        [Fact]
        public async Task IsRoomWithUsersExistAsync_ReturnsRoom_WhenRoomExists()
        {
            // Arrange
            var context = GetInMemoryDbContext("IsRoomWithUsersExistAsyncDb");
            var roomService = new RoomService(_loggerMock.Object, context);

            var roomEntity = new RoomEntity { Id = "1", OwnerId = "Owner1", GuestId = "Guest1", InviteAccepted = true };
            var ownerEntity = new IdentityUser { Id = "Owner1", UserName = "OwnerName" };
            var guestEntity = new IdentityUser { Id = "Guest1", UserName = "GuestName" };
            var roomSettingsEntity = new RoomSettingsEntity { RoomId = "1" };

            roomEntity.RoomSetting = roomSettingsEntity;
            context.Users.AddRange(ownerEntity, guestEntity);
            context.Rooms.Add(roomEntity);
            await context.SaveChangesAsync();

            // Act
            var result = await roomService.IsRoomWithUsersExistAsync("Owner1", "Guest1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1", result.Id);
        }

        [Fact]
        public async Task IsRoomWithUsersExistAsync_ReturnsNull_WhenRoomDoesNotExist()
        {
            // Arrange
            var context = GetInMemoryDbContext("IsRoomWithUsersExistAsyncDb_NotExist");
            var roomService = new RoomService(_loggerMock.Object, context);

            // Act
            var result = await roomService.IsRoomWithUsersExistAsync("Owner1", "Guest1");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserOwnRoomsAsync_ReturnsUserOwnRooms()
        {
            // Arrange
            var context = GetInMemoryDbContext("GetUserOwnRoomsAsyncDb");
            var roomService = new RoomService(_loggerMock.Object, context);

            var roomEntity1 = new RoomEntity { Id = "1", OwnerId = "Owner1", GuestId = "Guest1", InviteAccepted = true, OwnerIsOut = false };
            var ownerEntity1 = new IdentityUser { Id = "Owner1", UserName = "OwnerName" };
            var guestEntity1 = new IdentityUser { Id = "Guest1", UserName = "GuestName" };
            var roomSettingsEntity1 = new RoomSettingsEntity { RoomId = "1" };
            roomEntity1.RoomSetting = roomSettingsEntity1;

            var roomEntity2 = new RoomEntity { Id = "2", OwnerId = "Owner1", GuestId = "Guest2", InviteAccepted = true, OwnerIsOut = false };
            var guestEntity2 = new IdentityUser { Id = "Guest2", UserName = "OwnerName" };
            var roomSettingsEntity2 = new RoomSettingsEntity { RoomId = "2" };
            roomEntity2.RoomSetting = roomSettingsEntity2;

            var roomEntity3 = new RoomEntity { Id = "3", OwnerId = "Owner1", GuestId = "Guest3", InviteAccepted = true, OwnerIsOut = true };
            var guestEntity3 = new IdentityUser { Id = "Guest3", UserName = "OwnerName" };
            var roomSettingsEntity3 = new RoomSettingsEntity { RoomId = "3" };
            roomEntity3.RoomSetting = roomSettingsEntity3;

            var roomEntity4 = new RoomEntity { Id = "4", OwnerId = "Owner1", GuestId = "Guest4", InviteAccepted = false, OwnerIsOut = false };
            var guestEntity4 = new IdentityUser { Id = "Guest4", UserName = "OwnerName" };
            var roomSettingsEntity4 = new RoomSettingsEntity { RoomId = "4" };
            roomEntity4.RoomSetting = roomSettingsEntity4;

            context.Rooms.AddRange(roomEntity1, roomEntity2, roomEntity3, roomEntity4);
            context.Users.AddRange(ownerEntity1, guestEntity1, guestEntity2, guestEntity3, guestEntity4);
            await context.SaveChangesAsync();

            // Act
            var result = await roomService.GetUserOwnRoomsAsync("Owner1");

            // Assert
            Assert.Equal(3, result.Count());
            Assert.Contains(result, r => r.Id == "1");
            Assert.Contains(result, r => r.Id == "2");
            Assert.Contains(result, r => r.Id == "4");
        }

        [Fact]
        public async Task GetUserGuestRoomsAsync_ReturnsUserGuestRooms()
        {
            // Arrange
            var context = GetInMemoryDbContext("GetUserGuestRoomsAsyncDb");
            var roomService = new RoomService(_loggerMock.Object, context);

            var roomEntity1 = new RoomEntity { Id = "1", OwnerId = "Owner1", GuestId = "Guest1", InviteAccepted = true, GuestIsOut = false };
            var ownerEntity1 = new IdentityUser { Id = "Owner1", UserName = "OwnerName" };
            var guestEntity1 = new IdentityUser { Id = "Guest1", UserName = "GuestName" };
            var roomSettingsEntity1 = new RoomSettingsEntity { RoomId = "1" };
            roomEntity1.RoomSetting = roomSettingsEntity1;

            var roomEntity2 = new RoomEntity { Id = "2", OwnerId = "Owner2", GuestId = "Guest1", InviteAccepted = true, GuestIsOut = false };
            var ownerEntity2 = new IdentityUser { Id = "Owner2", UserName = "OwnerName" };
            var roomSettingsEntity2 = new RoomSettingsEntity { RoomId = "2" };
            roomEntity2.RoomSetting = roomSettingsEntity2;

            var roomEntity3 = new RoomEntity { Id = "3", OwnerId = "Owner3", GuestId = "Guest1", InviteAccepted = true, GuestIsOut = true };
            var ownerEntity3 = new IdentityUser { Id = "Owner3", UserName = "OwnerName" };
            var roomSettingsEntity3 = new RoomSettingsEntity { RoomId = "3" };
            roomEntity3.RoomSetting = roomSettingsEntity3;

            var roomEntity4 = new RoomEntity { Id = "4", OwnerId = "Owner4", GuestId = "Guest1", InviteAccepted = false, GuestIsOut = false };
            var ownerEntity4 = new IdentityUser { Id = "Owner4", UserName = "OwnerName" };
            var roomSettingsEntity4 = new RoomSettingsEntity { RoomId = "4" };
            roomEntity4.RoomSetting = roomSettingsEntity4;

            context.Rooms.AddRange(roomEntity1, roomEntity2, roomEntity3, roomEntity4);
            context.Users.AddRange(ownerEntity1, ownerEntity2, ownerEntity3, ownerEntity4, guestEntity1);
            await context.SaveChangesAsync();

            // Act
            var result = await roomService.GetUserGuestRoomsAsync("Guest1");

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, r => r.Id == "1");
            Assert.Contains(result, r => r.Id == "2");
        }

        [Fact]
        public async Task GetInvitationsAsync_ReturnsInvitations()
        {
            // Arrange
            var context = GetInMemoryDbContext("GetInvitationsAsyncDb");
            var roomService = new RoomService(_loggerMock.Object, context);

            var roomEntity1 = new RoomEntity { Id = "1", OwnerId = "Owner1", GuestId = "Guest1", InviteAccepted = false };
            var ownerEntity1 = new IdentityUser { Id = "Owner1", UserName = "OwnerName" };
            var guestEntity1 = new IdentityUser { Id = "Guest1", UserName = "GuestName" };
            var roomSettingsEntity1 = new RoomSettingsEntity { RoomId = "1" };
            roomEntity1.RoomSetting = roomSettingsEntity1;

            var roomEntity2 = new RoomEntity { Id = "2", OwnerId = "Owner2", GuestId = "Guest1", InviteAccepted = false };
            var ownerEntity2 = new IdentityUser { Id = "Owner2", UserName = "OwnerName" };
            var roomSettingsEntity2 = new RoomSettingsEntity { RoomId = "2" };
            roomEntity2.RoomSetting = roomSettingsEntity2;

            var roomEntity3 = new RoomEntity { Id = "3", OwnerId = "Owner3", GuestId = "Guest1", InviteAccepted = true };
            var ownerEntity3 = new IdentityUser { Id = "Owner3", UserName = "OwnerName" };
            var roomSettingsEntity3 = new RoomSettingsEntity { RoomId = "3" };
            roomEntity3.RoomSetting = roomSettingsEntity3;

            context.Users.AddRange(ownerEntity1, ownerEntity2, ownerEntity3, guestEntity1);
            context.Rooms.AddRange(roomEntity1, roomEntity2, roomEntity3);
            await context.SaveChangesAsync();

            // Act
            var result = await roomService.GetInvitationsAsync("Guest1");

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, r => r.Id == "1");
            Assert.Contains(result, r => r.Id == "2");
        }

        [Fact]
        public async Task GetInviteCountAsync_ReturnsInviteCount()
        {
            // Arrange
            var context = GetInMemoryDbContext("GetInviteCountAsyncDb");
            var roomService = new RoomService(_loggerMock.Object, context);

            var roomEntity1 = new RoomEntity { Id = "1", OwnerId = "Owner1", GuestId = "Guest1", InviteAccepted = false };
            var roomEntity2 = new RoomEntity { Id = "2", OwnerId = "Owner2", GuestId = "Guest1", InviteAccepted = false };
            context.Rooms.AddRange(roomEntity1, roomEntity2);
            await context.SaveChangesAsync();

            // Act
            var result = await roomService.GetInviteCountAsync("Guest1");

            // Assert
            Assert.Equal(2, result);
        }
    }
}