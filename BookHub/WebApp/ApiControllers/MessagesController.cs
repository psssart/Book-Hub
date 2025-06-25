using System.Net;
using App.Contracts.BLL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using App.DAL.EF;
using App.Domain.Identity;
using App.DTO.v1_0;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using WebApp.Helpers;
using Message = App.Domain.Entities.Message;

namespace WebApp.ApiControllers
{
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class MessagesController : ControllerBase
{
    private readonly AppDbContext _context; 
    private readonly IAppBLL _bll;
    private readonly UserManager<AppUser> _userManager;
    private readonly PublicDTOBllMapper<App.DTO.v1_0.Message, App.BLL.DTO.Message> _mapper;

    public MessagesController(AppDbContext context, IAppBLL bll, UserManager<AppUser> userManager, IMapper autoMapper)
    {
        _context = context;
        _bll = bll;
        _userManager = userManager;
        _mapper = new PublicDTOBllMapper<App.DTO.v1_0.Message, App.BLL.DTO.Message>(autoMapper);
    }
    
    /// <summary>
    /// Return all messages
    /// </summary>
    /// <returns>list of Messages</returns>
    [HttpGet]
    [ProducesResponseType<IEnumerable<App.DTO.v1_0.Message>>((int) HttpStatusCode.OK)]
    [ProducesResponseType((int) HttpStatusCode.Unauthorized)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<List<App.DTO.v1_0.Message>>> GetMessages()
    {
        var res = (await _bll.Messages.GetAllIncludeAll())
            .Select(e => _mapper.Map(e))
            .ToList();
        return Ok(res);
    }
    
    /// <summary>
    /// Return message by id
    /// </summary>
    /// <param name="id">Message ID.</param>
    /// <returns>single Message</returns>
    // GET: api/Messages/5
    [HttpGet("{id}")]
    [ProducesResponseType<App.Domain.Entities.Message>((int) HttpStatusCode.OK)]
    [ProducesResponseType((int) HttpStatusCode.NotFound)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<App.Domain.Entities.Message>> GetMessage(Guid id)
    {
        var message = await _context.Messages.FindAsync(id);

        if (message == null)
        {
            return NotFound();
        }

        return message;
    }
    
    /// <summary>
    /// Replace Message with specific ID
    /// </summary>
    /// <param name="updateInfo">Message ID and Content.</param>
    /// <returns>nothing</returns>
    // PUT: api/Messages/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    [ProducesResponseType((int) HttpStatusCode.NoContent)]
    [ProducesResponseType((int) HttpStatusCode.BadRequest)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> PutMessage(
        [FromBody]
        MessageUpdateInfo updateInfo)
    {
        var user = await _userManager.GetUserAsync(User);

        var message = new Message()
        {
            Id = updateInfo.Id,
            AppUserId = user!.Id,
            TopicId = _context.Topics.First().Id,
            Content = updateInfo.Content,
            CreationTime = DateTime.Now.ToUniversalTime()
        };
        
        _context.Messages.Update(message);

        await _context.SaveChangesAsync();

        return NoContent();
    }
    
    /// <summary>
    /// Add new message
    /// </summary>
    /// <param name="createInfo">Message content.</param>
    /// <returns>Newly added message</returns>
    // POST: api/Messages
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    [ProducesResponseType<App.Domain.Entities.Message>((int) HttpStatusCode.Created)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<App.Domain.Entities.Message>> PostMessage(
        [FromBody]
        MessageCreateInfo createInfo)
    {
        var user = await _userManager.GetUserAsync(User);

        var message = new Message()
                  {
                      AppUserId = user!.Id,
                      TopicId = _context.Topics.First().Id,
                      Content = createInfo.Content,
                      CreationTime = DateTime.Now.ToUniversalTime()
                  };
        
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetMessage", new
        {
            version = HttpContext.GetRequestedApiVersion()?.ToString(),
            id = message.Id
        }, message);
    }
    
    /// <summary>
    /// Delete Message by ID
    /// </summary>
    /// <param name="id">Message ID.</param>
    /// <returns>nothing</returns>
    // DELETE: api/Messages/5
    [ProducesResponseType((int) HttpStatusCode.NoContent)]
    [ProducesResponseType((int) HttpStatusCode.NotFound)]
    [HttpDelete("{id}")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> DeleteMessage(Guid id)
    {
        var message = await _context.Messages.FindAsync(id);
        if (message == null)
        {
            return NotFound();
        }

        _context.Messages.Remove(message);
        await _context.SaveChangesAsync();

        return NoContent();
    }
    
    [ApiExplorerSettings(IgnoreApi = true)]
    private bool MessageExists(Guid id)
    {
        return _context.Messages.Any(e => e.Id == id);
    }
}
}