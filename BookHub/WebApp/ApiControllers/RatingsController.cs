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
using Rating = App.DTO.v1_0.Rating;

namespace WebApp.ApiControllers
{
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class RatingsController : ControllerBase
{
    private readonly AppDbContext _context; 
    private readonly IAppBLL _bll;
    private readonly UserManager<AppUser> _userManager;
    private readonly PublicDTOBllMapper<App.DTO.v1_0.Rating, App.BLL.DTO.Rating> _mapper;

    public RatingsController(AppDbContext context, IAppBLL bll, UserManager<AppUser> userManager, IMapper autoMapper)
    {
        _context = context;
        _bll = bll;
        _userManager = userManager;
        _mapper = new PublicDTOBllMapper<App.DTO.v1_0.Rating, App.BLL.DTO.Rating>(autoMapper);
    }
    
    /// <summary>
    /// Return all ratings visible to current user
    /// </summary>
    /// <returns>list of Ratings</returns>
    [HttpGet]
    [ProducesResponseType<IEnumerable<App.BLL.DTO.Rating>>((int) HttpStatusCode.OK)]
    [ProducesResponseType((int) HttpStatusCode.Unauthorized)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<List<App.DTO.v1_0.Rating>>> GetRatings()
    {
        var res = (await _bll.Ratings.GetAllSortedAsync(Guid.Parse(_userManager.GetUserId(User))))
            .Select(e => _mapper.Map(e))
            .ToList();
        return Ok(res);
    }
    
    /// <summary>
    /// Return rating by id
    /// </summary>
    /// <param name="id">Rating ID.</param>
    /// <returns>single Rating</returns>
    // GET: api/Ratings/5
    [HttpGet("{id}")]
    [ProducesResponseType<App.Domain.Entities.Rating>((int) HttpStatusCode.OK)]
    [ProducesResponseType((int) HttpStatusCode.NotFound)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<App.Domain.Entities.Rating>> GetRating(Guid id)
    {
        var rating = await _context.Ratings.FindAsync(id);

        if (rating == null)
        {
            return NotFound();
        }

        return rating;
    }
    
    /// <summary>
    /// Replace Rating with specific ID
    /// </summary>
    /// <param name="updateInfo">Rating ID, Value and Comment.</param>
    /// <returns>nothing</returns>
    // PUT: api/Ratings/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    [ProducesResponseType((int) HttpStatusCode.NoContent)]
    [ProducesResponseType((int) HttpStatusCode.BadRequest)]
    [ProducesResponseType((int) HttpStatusCode.NotFound)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> PutRating(
        [FromBody]
        RatingUpdateInfo updateInfo)
    {
        var user = await _userManager.GetUserAsync(User);
        
        var rating = new App.Domain.Entities.Rating()
        {
            Id = updateInfo.Id,
            AppUserId = user!.Id,
            BookId = _context.Books.First().Id,
            Comment = updateInfo.Comment,
            Value = float.Parse(updateInfo.Value)
        };
        
        _context.Ratings.Update(rating);

        await _context.SaveChangesAsync();

        return NoContent();
    }
    
    /// <summary>
    /// Add new Rating
    /// </summary>
    /// <param name="createInfo">Rating Value and Comment.</param>
    /// <returns>Newly added Rating</returns>
    // POST: api/Ratings
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    [ProducesResponseType<App.Domain.Entities.Rating>((int) HttpStatusCode.Created)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<App.Domain.Entities.Rating>> PostRating(
        [FromBody] 
        RatingCreateInfo createInfo)
    {
        var user = await _userManager.GetUserAsync(User);
        
        var rating = new App.Domain.Entities.Rating()
        {
            AppUserId = user!.Id,
            BookId = _context.Books.First().Id,
            Comment = createInfo.Comment,
            Value = float.Parse(createInfo.Value)
        };
        
        _context.Ratings.Add(rating);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetRating", new
        {
            version = HttpContext.GetRequestedApiVersion()?.ToString(),
            id = rating.Id
        }, rating);
    }
    
    /// <summary>
    /// Delete Rating by ID
    /// </summary>
    /// <param name="id">Rating ID.</param>
    /// <returns>Newly added Rating</returns>
    // DELETE: api/Ratings/5
    [ProducesResponseType((int) HttpStatusCode.NoContent)]
    [ProducesResponseType((int) HttpStatusCode.NotFound)]
    [HttpDelete("{id}")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> DeleteRating(Guid id)
    {
        var rating = await _context.Ratings.FindAsync(id);
        if (rating == null)
        {
            return NotFound();
        }

        _context.Ratings.Remove(rating);
        await _context.SaveChangesAsync();

        return NoContent();
    }
    
    [ApiExplorerSettings(IgnoreApi = true)]
    private bool RatingExists(Guid id)
    {
        return _context.Ratings.Any(e => e.Id == id);
    }
}
}