﻿namespace Bejebeje.Mvc.Controllers
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Bejebeje.Models.Artist;
  using Bejebeje.Services.Services.Interfaces;
  using Extensions;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;

  public class ArtistController : Controller
  {
    private readonly IArtistsService _artistsService;

    public ArtistController(
      IArtistsService artistsService)
    {
      _artistsService = artistsService;
    }

    [Route("artists")]
    public async Task<IActionResult> Index()
    {
      IDictionary<char, List<LibraryArtistViewModel>> viewModel = await _artistsService
        .GetAllArtistsAsync();

      return View(viewModel);
    }

    [Route("artists/new")]
    [Authorize]
    public IActionResult Create()
    {
      CreateArtistViewModel viewModel = new CreateArtistViewModel();

      return View(viewModel);
    }

    [Route("artists/new")]
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(
      CreateArtistViewModel viewModel)
    {
      try
      {
        viewModel.UserId = User
          .GetUserId()
          .ToString();

        ArtistCreationResult result = await _artistsService
          .AddArtistAsync(viewModel);

        return RedirectToAction("ArtistLyrics", "Lyric", new { artistSlug = result.PrimarySlug });
      }
      catch
      {
        return View(viewModel);
      }
    }
  }
}
