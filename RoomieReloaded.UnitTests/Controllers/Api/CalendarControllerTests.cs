﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RoomieReloaded.Controllers.Api;
using RoomieReloaded.Models;
using RoomieReloaded.Services;
using Xunit;

namespace RoomieReloaded.Tests.Controllers.Api
{
	public class CalendarControllerTests
	{
		[Fact]
		public async Task Test_NoRoomFound_ReturnsBadRequest()
		{
			var sut = CreateController();

			const string roomMail = "some not found room";
			var result = await sut.Index(roomMail, null, null);

			var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
			Assert.Contains(roomMail, badRequestResult.Value.ToString());
		}

		[Fact]
		public async Task Test_RoomMailIsSentToRoomService()
		{
			var roomService = CreateRoomServiceMock();
			var sut = CreateController(roomService);

			const string roomMail = "some room mail";
			await sut.Index(roomMail, null, null);

			roomService.Verify(m => m.GetRoomByNameAsync(roomMail));
		}

		[Fact]
		public async Task Test_DateStartInvalid_ReturnsBadRequest()
		{
			var roomService = CreateRoomServiceMock();
			var sut = CreateController(roomService);

			var dateStart = "funny start date";
			var result = await sut.Index(null, dateStart, null);

			var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
			Assert.Contains("dateStart", badRequestResult.Value.ToString());
		}

		[Fact]
		public async Task Test_DateEndInvalid_ReturnsBadRequest()
		{
			var roomService = CreateRoomServiceMock();
			var sut = CreateController(roomService);

			var dateEnd = "funny end date";
			var result = await sut.Index(null, DateTime.Now.ToString("s"), dateEnd);

			var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
			Assert.Contains("dateEnd", badRequestResult.Value.ToString());
		}

		[Fact]
		public async Task Test_InputDataIsSentToCalendarService()
		{
			const string expectedRoomName = "some expected RoomName";
			var roomService = CreateRoomServiceMock(expectedRoomName);
			var calendarService = new Mock<ICalendarService>
			{
				DefaultValue = DefaultValue.Mock
			};

			var sut = CreateController(roomService, calendarService);

			var start = DateTime.Today;
			var end = start.AddDays(1);
			await sut.Index(null, start.ToString("s"), end.ToString("s"));

			calendarService.Verify(m => m.GetCalendarEventsAsync(expectedRoomName, start, end ));
		}

		[Fact]
		public async Task Test_CalendarServiceOutputIsReturned()
		{
			const string expectedRoomName = "some expected RoomName";
			var roomService = CreateRoomServiceMock(expectedRoomName);
			var calendarService = new Mock<ICalendarService>
			{
				DefaultValue = DefaultValue.Mock
			};

			var calendarEvent = new Mock<ICalendarEvent>();
			calendarEvent.Setup(m => m.Organizer).Returns("Organizer");
			calendarEvent.Setup(m => m.From).Returns(DateTime.Now);
			calendarEvent.Setup(m => m.To).Returns(DateTime.Now.AddHours(1));
			var calendarEvents = new List<ICalendarEvent>
			{
				calendarEvent.Object
			};

			calendarService.Setup(m => m.GetCalendarEventsAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
				.ReturnsAsync(calendarEvents);

			var sut = CreateController(roomService, calendarService);

			var start = DateTime.UtcNow;
			var end = start.AddDays(1);
			var result = await sut.Index(null, start.ToString("s"), end.ToString("s"));

			var json = Assert.IsType<JsonResult>(result);

			var obj = Assert.IsType<CalendarRoomModel>(json.Value);
			Assert.Equal(obj.Room, expectedRoomName + "Nice");
			Assert.Single(obj.Events);

			var expectedEvent = calendarEvent.Object;
			var actualEvent = obj.Events[0];
			Assert.Equal(expectedEvent.Organizer, actualEvent.Organizer);
			Assert.Equal(expectedEvent.From, actualEvent.Start);
			Assert.Equal(expectedEvent.To, actualEvent.End);
		}

		private Mock<IRoomService> CreateRoomServiceMock(string roomName = null, string mail = null)
		{
			var room = new Mock<IRoom>();
			room.Setup(m => m.Name).Returns(roomName);
			room.Setup(m => m.NiceName).Returns(roomName + "Nice");
			room.Setup(m => m.Mail).Returns(mail);

			var roomService = new Mock<IRoomService>();
			roomService.Setup(m => m.GetRoomByNameAsync(It.IsAny<string>()))
				.ReturnsAsync(room.Object);

			return roomService;
		}

		private CalendarController CreateController(IMock<IRoomService> roomServiceMock = null,
			IMock<ICalendarService> calendarServiceMock = null)
		{
			return new CalendarController(
				roomServiceMock?.Object ?? Mock.Of<IRoomService>(),
				calendarServiceMock?.Object ?? Mock.Of<ICalendarService>()
			);
		}
	}
}