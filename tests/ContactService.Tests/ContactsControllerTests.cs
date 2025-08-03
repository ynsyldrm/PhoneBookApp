using ContactService.Controllers;
using ContactService.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Models;

public class ContactsControllerTests
{
  [Fact]
  public async Task CreateContact_ShouldReturnContact_WhenValidRequest()
  {
    // Arrange
    var options = new DbContextOptionsBuilder<ContactDbContext>()
        .UseInMemoryDatabase(databaseName: "TestDb")
        .Options;

    using var context = new ContactDbContext(options);
    var logger = new Mock<ILogger<ContactsController>>();
    var controller = new ContactsController(context, logger.Object);

    var request = new CreateContactRequest("Name 1", "Last Name 1", "Test Company 1");

    // Act
    var result = await controller.CreateContact(request);

    // Assert
    var actionResult = Assert.IsType<ActionResult<Contact>>(result);
    var contact = Assert.IsType<Contact>(actionResult.Value);
    Assert.Equal("John", contact.FirstName);
  }
}