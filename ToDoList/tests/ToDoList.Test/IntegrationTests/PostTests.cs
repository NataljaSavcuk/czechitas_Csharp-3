namespace ToDoList.Test;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using ToDoList.Domain.Models;
using ToDoList.Domain.DTO;
using ToDoList.WebApi;
using ToDoList.Persistence.Repository;
public class PostTests
{
    private readonly IRepository<ToDoItem> repositoryMock;
    private readonly ToDoItemsController controller;

    public PostTests()
    {
        repositoryMock = Substitute.For<IRepository<ToDoItem>>();
        controller = new ToDoItemsController(repositoryMock);
    }

    [Fact]
    public void Post_CreateValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        var request = new ToDoItemCreateRequestDto("Jmeno", "Popis", false, "test", Domain.Types.Priority.Low);

        var toDoItem = new ToDoItem
        {
            ToDoItemId = 1,
            Name = request.Name,
            Description = request.Description,
            IsCompleted = request.IsCompleted
        };

        repositoryMock.When(r => r.Create(Arg.Any<ToDoItem>()))
                      .Do(callInfo => callInfo.Arg<ToDoItem>().ToDoItemId = toDoItem.ToDoItemId);

        repositoryMock.ReadById(toDoItem.ToDoItemId).Returns(toDoItem);

        // Act
        var result = controller.Create(request);
        var createdAtResult = result as CreatedAtActionResult;
        var value = createdAtResult?.Value as ToDoItem;

        // Assert
        Assert.IsType<CreatedAtActionResult>(createdAtResult);
        Assert.NotNull(value);

        Assert.Equal(request.Description, value.Description);
        Assert.Equal(request.IsCompleted, value.IsCompleted);
        Assert.Equal(request.Name, value.Name);
        Assert.Equal(toDoItem.ToDoItemId, value.ToDoItemId);

        // Verify that Create was called on the repository
        repositoryMock.Received(1).Create(Arg.Is<ToDoItem>(item =>
            item.Name == request.Name &&
            item.Description == request.Description &&
            item.IsCompleted == request.IsCompleted
        ));
    }

    [Fact]
    public void Post_CreateUnhandledException_ReturnsInternalServerError()
    {
        // Arrange
        var request = new ToDoItemCreateRequestDto("Jmeno", "Popis", false, "test", Domain.Types.Priority.Low);

        repositoryMock.When(r => r.Create(Arg.Any<ToDoItem>()))
                        .Do(call => throw new Exception("Chyba prihlaseni do databaze"));

        // Act
        var result = controller.Create(request);
        var errorResult = result as ObjectResult;

        //Assert
        Assert.IsType<ObjectResult>(errorResult);
        Assert.Equal(StatusCodes.Status500InternalServerError, errorResult.StatusCode);
    }
}
