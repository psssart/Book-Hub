using App.BLL;
using App.BLL.DTO;
using App.Contracts.BLL;
using App.Contracts.DAL;
using App.Contracts.DAL.Repositories;
using App.DAL.EF;
using App.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebApp.ApiControllers;
using NSubstitute;

namespace App.Test;

public class RepositoryAndServiceTest
{
    private readonly AppDbContext _ctx;
    private readonly IAppBLL _bll;
    private readonly IAppUnitOfWork _uow;
    private readonly UserManager<AppUser> _userManager;
    private readonly MessagesController _mesController;

    private IMessageRepository _messageRepository;

    public RepositoryAndServiceTest()
    {
        // set up mock database - inmemory
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // use random guid as db instance id5
        optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
        
        _ctx = new AppDbContext(optionsBuilder.Options);

        var configUow =
            new MapperConfiguration(cfg => cfg.CreateMap<App.Domain.Entities.Message, DAL.DTO.Message>().ReverseMap());
        var mapperUow = configUow.CreateMapper();
        
        _uow = new AppUOW(_ctx, mapperUow);
        _messageRepository = _uow.Messages;
        
        var configBll = new MapperConfiguration(cfg => cfg.CreateMap<DAL.DTO.Message, BLL.DTO.Message>().ReverseMap());
        var mapperBll = configBll.CreateMapper();
        _bll = new AppBLL(_uow, mapperBll);
        
        var configWeb = new MapperConfiguration(cfg => cfg.CreateMap<BLL.DTO.Message, DTO.v1_0.Message>().ReverseMap());
        var mapperWeb = configWeb.CreateMapper();
        
        var storeStub = Substitute.For<IUserStore<AppUser>>();
        var optionsStub = Substitute.For<IOptions<IdentityOptions>>();
        var hasherStub = Substitute.For<IPasswordHasher<AppUser>>();
        
        var validatorStub = Substitute.For<IEnumerable<IUserValidator<AppUser>>>();
        var passwordStup = Substitute.For<IEnumerable<IPasswordValidator<AppUser>>>();
        var lookupStub = Substitute.For<ILookupNormalizer>();
        var errorStub = Substitute.For<IdentityErrorDescriber>();
        var serviceStub = Substitute.For<IServiceProvider>();
        var loggerStub = Substitute.For<ILogger<UserManager<AppUser>>>();
        
        _userManager = Substitute.For<UserManager<AppUser>>(
            storeStub, 
            optionsStub, 
            hasherStub,
            validatorStub, passwordStup, lookupStub, errorStub, serviceStub, loggerStub
        );
      
        _mesController = new MessagesController(_ctx, _bll, _userManager, mapperWeb);
        _userManager.GetUserId(_mesController.User).Returns(Guid.NewGuid().ToString());
    }
    
    [Fact]
    public async Task CreateUser()
    {
        var userId = _userManager.GetUserId(_mesController.User);
        _ctx.Users.Add(new AppUser()
        {
            Id = Guid.Parse(userId),
            FirstName = "Admin",
            LastName = "Adminian",
            Email = "admin@admin.ee",
            UserName = "admin@admin.ee",
        });
        await _ctx.SaveChangesAsync();
        Assert.NotEmpty(_ctx.Users);
    }
    
    [Fact]
    public async void CreateMessage()
    {
        await CreateUser();
        var user = _ctx.Users.First();
        var message = new Message()
        {
            TopicId = Guid.NewGuid(),
            AppUserId = user.Id,
            Content = "Nothing",
            CreationTime = DateTime.Now
        };
        _bll.Messages.Add(message);
        await _bll.SaveChangesAsync();
        //Untrack created entity. Important for future gets
        _ctx.ChangeTracker.Clear();      
        Assert.NotEmpty(await _bll.Messages.GetAllAsync());
    }
    
    [Fact]
    public async Task UpdateMessageTest()
    {
        var userId = _userManager.GetUserId(_mesController.User);
        CreateMessage();
        var message = (await _bll.Messages.GetAllAsync(Guid.Parse(userId))).FirstOrDefault(e => e.Content.Equals("Nothing"));
        message!.Content = "Something";
      
        _bll.Messages.Update(message);
        await _bll.SaveChangesAsync();
      
        var changedMessage = (await _bll.Messages.GetAllAsync()).FirstOrDefault(e => e.Content.Equals("Something"));
      
        Assert.True(changedMessage!.Content == "Something");
    }

    [Fact]
    public async Task GetAllMessagesTest()
    {
        var userId = _userManager.GetUserId(_mesController.User);
        CreateMessage();
        var messages = _bll.Messages.GetAll(Guid.Parse(userId));
        Assert.NotEmpty(messages);
    }
    
    [Fact]
    public async Task ExistsTest()
    {
        var userId = _userManager.GetUserId(_mesController.User);
        CreateMessage();
        var message = (await _bll.Messages.GetAllAsync()).FirstOrDefault(e => e.Content.Equals("Nothing"));
        var exists = _messageRepository.Exists(message!.Id);
        Assert.True(exists);
        var existsAsync = await _messageRepository.ExistsAsync(message.Id);
        Assert.True(existsAsync);
    }

    [Fact]
    public async void CreateRandomMessage()
    {
        var message = new Message()
        {
            TopicId = Guid.NewGuid(),
            AppUserId = Guid.NewGuid(),
            Content = "Nothing",
            CreationTime = DateTime.Now
        };
        _bll.Messages.Add(message);
        await _bll.SaveChangesAsync();
        //Untrack created entity. Important for future gets
        _ctx.ChangeTracker.Clear();      
        Assert.NotEmpty(await _bll.Messages.GetAllAsync());
    }
    
    [Fact]
    public async Task GetAllSortedAsyncTest()
    {
        var userId = _userManager.GetUserId(_mesController.User);
        CreateMessage();
        CreateRandomMessage();
        var messages = await _bll.Messages.GetAllSortedAsync(Guid.Parse(userId));
        Assert.True(messages.ToList().Count == 1);
    }
    
    /*[Fact]
    public async Task Remove()
    {
        // Arrange
        var userId = _userManager.GetUserId(_mesController.User);
        CreateMessage();

        // Act
        var messages = await _bll.Messages.GetAllAsync();
        var message = messages.FirstOrDefault(e => e.Content.Equals("Nothing"));
        if (message != null)
        {
            _bll.Messages.Remove(message);
            await _bll.SaveChangesAsync();
        }

        // Assert
        messages = await _bll.Messages.GetAllAsync();
        Assert.Empty(messages);
    }*/
}