using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
  private readonly AuctionDbContext _context;
  private readonly IMapper _mapper;

  public AuctionsController(AuctionDbContext context, IMapper mapper)
  {
      _context = context;
      _mapper = mapper;
      
  }

  [HttpGet]
  public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions()
  {
    var auctions = await _context.Auctions
        .Include(x => x.Item)
        .OrderBy(x => x.Item.Make)
        .ToListAsync();

    return _mapper.Map<List<AuctionDto>>(auctions);
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
  {
    var auction = await _context.Auctions
        .Include(x => x.Item)
        .FirstOrDefaultAsync(x => x.Id == id);

    if(auction == null) return NotFound();

    return _mapper.Map<AuctionDto>(auction);
    
  }
  

  [HttpPost]
  public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
  {
    var auction = _mapper.Map<Auction>(auctionDto);

    auction.Seller = User.Identity.Name;

    _context.Auctions.Add(auction);

    var result = await _context.SaveChangesAsync() > 0;

    if (!result) return BadRequest("Could not save changes to the DB");

    //When a client calls CreateAuction endpoint and successfully creates an auction, this line ensures that 
    //the client receives a 201 status code, A "Location" header in the response pointing to where the newly 
    //created auction can be accessed (via the GetAuctionById endpoint) and the data of the newly created auction 
    //in the format of AuctionDto in the response body.
    return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, _mapper.Map<AuctionDto>(auction));
  }


  [HttpPut("{id}")]
  public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
  {
    var auction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);

    if (auction == null) return NotFound();

    if (auction.Seller != User.Identity.Name) return Forbid();

    auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
    auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
    auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
    auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
    auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

    var result = await _context.SaveChangesAsync() > 0;

    if (result) return Ok();

    return BadRequest("Problem saving changes");
  }

  
  [HttpDelete("{id}")]
  public async Task<ActionResult> DeleteAuction(Guid id)
  {
    var auction = await _context.Auctions.FindAsync(id);

    if (auction == null) return NotFound();

    _context.Auctions.Remove(auction);

    var result = await _context.SaveChangesAsync() > 0;
    
    if (!result) return BadRequest("Could not update DB");

    return Ok();
  }
  

}

