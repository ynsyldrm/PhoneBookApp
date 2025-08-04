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
    var options = new DbContextOptionsBuilder<ContactDbContext>()
        .UseInMemoryDatabase(databaseName: "TestDb")
        .Options;

    using var context = new ContactDbContext(options);
    var logger = new Mock<ILogger<ContactsController>>();
    var controller = new ContactsController(context, logger.Object);

    var request = new CreateContactRequest("Name 1", "Last Name 1", "Test Company 1");

    var result = await controller.CreateContact(request);
    var actionResult = Assert.IsType<ActionResult<Contact>>(result);

    if (actionResult.Result != null)
    {
      var createdAtResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
      var contact = Assert.IsType<Contact>(createdAtResult.Value);
      Assert.Equal("Name 1", contact.FirstName);
    }
    else
    {
      var contact = Assert.IsType<Contact>(actionResult.Value);
      Assert.Equal("Name 1", contact.FirstName);
    }
  }

  [Fact]
  public async Task DeleteContact_ShouldReturnNoContent_WhenContactExists()
  {
    var options = new DbContextOptionsBuilder<ContactDbContext>()
        .UseInMemoryDatabase(databaseName: "TestDb")
        .Options;

    var testContactId = Guid.NewGuid();
    using (var context = new ContactDbContext(options))
    {
      context.Contacts.Add(new Contact
      {
        Id = testContactId,
        FirstName = "Test",
        LastName = "User"
      });
      await context.SaveChangesAsync();
    }

    using (var context = new ContactDbContext(options))
    {
      var logger = new Mock<ILogger<ContactsController>>();
      var controller = new ContactsController(context, logger.Object);

      var result = await controller.DeleteContact(testContactId);

      Assert.IsType<NoContentResult>(result);

      var deletedContact = await context.Contacts.FindAsync(testContactId);
      Assert.Null(deletedContact);

      logger.Verify(x => x.Log(
          LogLevel.Information,
          It.IsAny<EventId>(),
          It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Contact deleted with ID: {testContactId}")),
          null,
          It.IsAny<Func<It.IsAnyType, Exception, string>>()),
          Times.Once);
    }
  }

  [Fact]
  public async Task DeleteContact_ShouldReturnNotFound_WhenContactDoesNotExist()
  {
    var options = new DbContextOptionsBuilder<ContactDbContext>()
        .UseInMemoryDatabase(databaseName: "TestDb")
        .Options;

    using var context = new ContactDbContext(options);
    var logger = new Mock<ILogger<ContactsController>>();
    var controller = new ContactsController(context, logger.Object);

    var nonExistentId = Guid.NewGuid();

    var result = await controller.DeleteContact(nonExistentId);

    Assert.IsType<NotFoundResult>(result);
  }

  [Fact]
  public async Task GetContacts_ShouldReturnContacts_WhenContactsExist()
  {
    var options = new DbContextOptionsBuilder<ContactDbContext>()
        .UseInMemoryDatabase(databaseName: "GetContactsTestDb")
        .Options;

    using (var context = new ContactDbContext(options))
    {
      context.Contacts.AddRange(
          new Contact { Id = Guid.NewGuid(), FirstName = "Test1", LastName = "TestLast1" },
          new Contact { Id = Guid.NewGuid(), FirstName = "Test2", LastName = "TestLast2" });
      await context.SaveChangesAsync();
    }

    using (var context = new ContactDbContext(options))
    {
      var logger = new Mock<ILogger<ContactsController>>();
      var controller = new ContactsController(context, logger.Object);

      var result = await controller.GetContacts();

      var actionResult = Assert.IsType<ActionResult<List<Contact>>>(result);
      var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
      var contacts = Assert.IsType<List<Contact>>(okResult.Value);
      Assert.Equal(2, contacts.Count);
      Assert.Equal("Test1", contacts[0].FirstName);
    }
  }

  [Fact]
  public async Task GetContacts_ShouldReturnEmptyList_WhenNoContactsExist()
  {
    var options = new DbContextOptionsBuilder<ContactDbContext>()
        .UseInMemoryDatabase(databaseName: "EmptyContactsTestDb")
        .Options;

    using var context = new ContactDbContext(options);
    var logger = new Mock<ILogger<ContactsController>>();
    var controller = new ContactsController(context, logger.Object);

    var result = await controller.GetContacts();

    var actionResult = Assert.IsType<ActionResult<List<Contact>>>(result);
    var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
    var contacts = Assert.IsType<List<Contact>>(okResult.Value);
    Assert.Empty(contacts);
  }

    [Fact]
    public async Task GetContact_ShouldReturnContact_WhenContactExists()
    {
      var options = new DbContextOptionsBuilder<ContactDbContext>()
          .UseInMemoryDatabase(databaseName: "GetContactTestDb")
          .Options;

      var testContactId = Guid.NewGuid();
      using (var context = new ContactDbContext(options))
      {
        context.Contacts.Add(new Contact
        {
          Id = testContactId,
          FirstName = "Test",
          LastName = "User",
          ContactInfos = new List<ContactInfo>
                {
                    new ContactInfo { Id = Guid.NewGuid(), InfoType = ContactInfoType.EmailAddress, InfoContent = "test@example.com" }
                }
        });
        await context.SaveChangesAsync();
      }

      using (var context = new ContactDbContext(options))
      {
        var logger = new Mock<ILogger<ContactsController>>();
        var controller = new ContactsController(context, logger.Object);

        var result = await controller.GetContact(testContactId);

        var actionResult = Assert.IsType<ActionResult<Contact>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var contact = Assert.IsType<Contact>(okResult.Value);
        Assert.Equal(testContactId, contact.Id);
        Assert.Single(contact.ContactInfos);
      }
    }

    [Fact]
    public async Task GetContact_ShouldReturnNotFound_WhenContactDoesNotExist()
    {
      var options = new DbContextOptionsBuilder<ContactDbContext>()
          .UseInMemoryDatabase(databaseName: "GetContactNotFoundTestDb")
          .Options;

      using var context = new ContactDbContext(options);
      var logger = new Mock<ILogger<ContactsController>>();
      var controller = new ContactsController(context, logger.Object);

      var result = await controller.GetContact(Guid.NewGuid());

      var actionResult = Assert.IsType<ActionResult<Contact>>(result);
      Assert.IsType<NotFoundResult>(actionResult.Result);
    }
  

}