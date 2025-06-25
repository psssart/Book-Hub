using System.Net;
using App.Contracts.BLL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using App.DAL.EF;
using App.Domain.Entities;
using App.Domain.Identity;
using App.DTO.v1_0;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using WebApp.Helpers;
using Topic = App.DTO.v1_0.Topic;

namespace WebApp.ApiControllers{

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class TopicsController : ControllerBase
{
    private readonly AppDbContext _context; 
    private readonly IAppBLL _bll;
    private readonly UserManager<AppUser> _userManager;
    private readonly PublicDTOBllMapper<App.DTO.v1_0.Topic, App.BLL.DTO.Topic> _mapper;

    public TopicsController(AppDbContext context, IAppBLL bll, UserManager<AppUser> userManager, IMapper autoMapper)
    {
        _context = context;
        _bll = bll;
        _userManager = userManager;
        _mapper = new PublicDTOBllMapper<App.DTO.v1_0.Topic, App.BLL.DTO.Topic>(autoMapper);
    }
    
    /// <summary>
    /// Return all topics
    /// </summary>
    /// <returns>list of Topics</returns>
    [HttpGet]
    [ProducesResponseType<IEnumerable<App.BLL.DTO.Topic>>((int) HttpStatusCode.OK)]
    [ProducesResponseType((int) HttpStatusCode.Unauthorized)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<List<App.DTO.v1_0.Topic>>> GetTopics()
    {
        var res = (await _bll.Topics.GetAllIncludeAll())
            .Select(e => _mapper.Map(e))
            .ToList();
        return Ok(res);
    }
    
    /// <summary>
    /// Return topic by id
    /// </summary>
    /// <param name="id">Topic ID.</param>
    /// <returns>single Topic</returns>
    // GET: api/Topics/5
    [HttpGet("{id}")]
    [ProducesResponseType<App.Domain.Entities.Topic>((int) HttpStatusCode.OK)]
    [ProducesResponseType((int) HttpStatusCode.NotFound)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<App.Domain.Entities.Topic>> GetTopic(Guid id)
    {
        var topic = await _context.Topics.FindAsync(id);

        if (topic == null)
        {
            return NotFound();
        }

        return topic;
    }
    
    /// <summary>
    /// Replace Topic with specific ID
    /// </summary>
    /// <param name="updateInfo">Topic ID, Tittle and Content</param>
    /// <returns>nothing</returns>
    // PUT: api/Topics/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    [ProducesResponseType((int) HttpStatusCode.NoContent)]
    [ProducesResponseType((int) HttpStatusCode.BadRequest)]
    [ProducesResponseType((int) HttpStatusCode.NotFound)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> PutTopic(
        [FromBody]
        TopicUpdateInfo updateInfo)
    {
        var user = await _userManager.GetUserAsync(User);
        
        var topic = new App.Domain.Entities.Topic()
        {
            Id = updateInfo.Id,
            AppUserId = user!.Id,
            DiscussionId = _context.Discussions.First().Id,
            Tittle = updateInfo.Tittle,
            Content = updateInfo.Content,
            CreationTime = DateTime.UtcNow
        };
        
        _context.Topics.Update(topic);

        await _context.SaveChangesAsync();

        return NoContent();
    }
    
    /// <summary>
    /// Add new Topic
    /// </summary>
    /// <param name="createInfo">Topic Tittle and Content.</param>
    /// <returns>Newly added topic</returns>
    // POST: api/Topics
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    [ProducesResponseType<App.Domain.Entities.Topic>((int) HttpStatusCode.Created)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<App.Domain.Entities.Topic>> PostTopic(
        [FromBody]
        TopicCreateInfo createInfo)
    {
        var user = await _userManager.GetUserAsync(User);
        
        var topic = new App.Domain.Entities.Topic()
        {
            AppUserId = user!.Id,
            DiscussionId = _context.Discussions.First().Id,
            Tittle = createInfo.Tittle,
            Content = createInfo.Content,
            CreationTime = DateTime.UtcNow
        };
        
        _context.Topics.Add(topic);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetTopic", new
        {
            version = HttpContext.GetRequestedApiVersion()?.ToString(),
            id = topic.Id
        }, topic);
    }
    
    /// <summary>
    /// Delete Topic by ID
    /// </summary>
    /// <param name="id">Topic ID.</param>
    /// <returns>nothing</returns>
    // DELETE: api/Topics/5
    [ProducesResponseType((int) HttpStatusCode.NoContent)]
    [ProducesResponseType((int) HttpStatusCode.NotFound)]
    [HttpDelete("{id}")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> DeleteTopic(Guid id)
    {
        var topic = await _context.Topics.FindAsync(id);
        if (topic == null)
        {
            return NotFound();
        }

        _context.Topics.Remove(topic);
        await _context.SaveChangesAsync();

        return NoContent();
    }
    
    [ApiExplorerSettings(IgnoreApi = true)]
    private bool TopicExists(Guid id)
    {
        return _context.Topics.Any(e => e.Id == id);
    }
}
}