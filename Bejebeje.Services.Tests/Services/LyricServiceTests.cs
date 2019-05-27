﻿namespace Bejebeje.Services.Tests.Services
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Bejebeje.Common.Exceptions;
  using Bejebeje.Domain;
  using Bejebeje.Services.Services;
  using Bejebeje.Services.Services.Interfaces;
  using Bejebeje.Services.Tests.Helpers;
  using Bejebeje.ViewModels.Lyric;
  using FluentAssertions;
  using Moq;
  using NUnit.Framework;

  [TestFixture]
  public class LyricServiceTests : DatabaseTestBase
  {
    private Mock<IArtistsService> artistsServiceMock;

    private LyricsService lyricsService;

    public List ListCardViewModel { get; private set; }

    [SetUp]
    public void Setup()
    {
      SetupDataContext();

      artistsServiceMock = new Mock<IArtistsService>(MockBehavior.Strict);

      lyricsService = new LyricsService(artistsServiceMock.Object, Context);
    }

    [Test]
    public async Task GetLyricsAsync_WhenArtistDoesNotExist_ThrowsAnArtistNotFoundException()
    {
      // arrange
      string artistSlug = "john-doe";

      artistsServiceMock
        .Setup(x => x.GetArtistIdAsync(artistSlug))
        .ThrowsAsync(new ArtistNotFoundException(artistSlug));

      // act
      Func<Task> action = async () => await lyricsService.GetLyricsAsync(artistSlug);

      // assert
      await action.Should().ThrowAsync<ArtistNotFoundException>();
    }

    [Test]
    public async Task GetLyricsAsync_WhenArtistDoesExistButHasNoLyrics_ReturnsAnEmptyListOfLyrics()
    {
      // arrange
      int artistId = 1;
      string artistSlug = "fats-waller";
      string artistFirstName = "Fats";
      string artistLastName = "Waller";

      string lyricSlug = "write-myself-a-letter";
      string lyricTitle = "Write Myself A Letter";
      string lyricBody = "Song lyrics";

      Lyric writeMyselfALetterSong = new Lyric
      {
        Title = lyricTitle,
        Body = lyricBody,
        Slugs = new List<LyricSlug>
        {
          new LyricSlug
          {
            Name = lyricSlug,
            CreatedAt = DateTime.UtcNow,
            IsPrimary = true
          }
        }
      };

      Artist fatsWaller = new Artist
      {
        Id = artistId,
        FirstName = artistFirstName,
        LastName = artistLastName,
        Slugs = new List<ArtistSlug>
        {
          new ArtistSlug
          {
            Name = artistSlug,
            CreatedAt = DateTime.UtcNow,
            IsPrimary = true
          }
        },
        Lyrics = new List<Lyric> { writeMyselfALetterSong }
      };

      Context.Artists.Add(fatsWaller);
      Context.SaveChanges();

      artistsServiceMock
        .Setup(x => x.GetArtistIdAsync(artistSlug))
        .ReturnsAsync(artistId);

      // act
      IList<LyricCardViewModel> result = await lyricsService.GetLyricsAsync(artistSlug);

      // assert
      result.Should().NotBeEmpty();
      result.First().Title.Should().Be(lyricTitle);
      result.First().Slug.Should().Be(lyricSlug);
    }

    [Test]
    public async Task SearchLyricsAsync_WhenNoLyricsMatch_ReturnsAnEmptyListOfLyricCardViewModels()
    {
      // arrange
      string lyricTitle = "window";
      string seedLyricTitle = "TNT";
      string seedLyricSlug = "tnt";
      DateTime seedLyricCreatedAt = DateTime.UtcNow;

      Lyric tnt = new Lyric
      {
        Title = seedLyricTitle,
        CreatedAt = seedLyricCreatedAt,
        Slugs = new List<LyricSlug>
        {
          new LyricSlug
          {
            Name = seedLyricSlug,
            IsPrimary = true,
            CreatedAt = seedLyricCreatedAt,
          }
        }
      };

      Context.Lyrics.Add(tnt);
      Context.SaveChanges();

      // act
      IList<LyricCardViewModel> result = await lyricsService.SearchLyricsAsync(lyricTitle);

      // assert
      result.Should().BeOfType<List<LyricCardViewModel>>();
      result.Should().BeEmpty();
    }

    [Test]
    public async Task SearchLyricsAsync_WhenThereIsMatchOnTitleOnly_ReturnsAPopulatedListOfLyricCardViewModels()
    {
      // arrange
      string lyricTitle = "tnt";
      string seedLyricTitle = "TNT";
      string unmatchedLyricSlug = "door";
      DateTime seedLyricCreatedAt = DateTime.UtcNow;

      Lyric tnt = new Lyric
      {
        Title = seedLyricTitle,
        CreatedAt = seedLyricCreatedAt,
        Slugs = new List<LyricSlug>
        {
          new LyricSlug
          {
            Name = unmatchedLyricSlug,
            IsPrimary = true,
            CreatedAt = seedLyricCreatedAt
          }
        }
      };

      Context.Lyrics.Add(tnt);
      Context.SaveChanges();

      // act
      IList<LyricCardViewModel> result = await lyricsService.SearchLyricsAsync(lyricTitle);

      // assert
      result.Should().BeOfType<List<LyricCardViewModel>>();
      result.Should().NotBeEmpty();
      result.Should().HaveCount(1);
      result.First().Title.Should().Be(seedLyricTitle);
      result.First().Slug.Should().Be(unmatchedLyricSlug);
    }

    [Test]
    public async Task SearchLyricsAsync_WhenThereIsMatchOnLyricSlugOnly_ReturnsAPopulatedListOfLyricCardViewModels()
    {
      // arrange
      string lyricTitle = "oen";
      string seedLyricTitle = "TNT";
      string uniqueLyricSlug = "uioenkl";
      DateTime seedLyricCreatedAt = DateTime.UtcNow;

      Lyric tnt = new Lyric
      {
        Title = seedLyricTitle,
        CreatedAt = seedLyricCreatedAt,
        Slugs = new List<LyricSlug>
        {
          new LyricSlug
          {
            Name = uniqueLyricSlug,
            IsPrimary = true,
            CreatedAt = seedLyricCreatedAt
          }
        }
      };

      Context.Lyrics.Add(tnt);
      Context.SaveChanges();

      // act
      IList<LyricCardViewModel> result = await lyricsService.SearchLyricsAsync(lyricTitle);

      // assert
      result.Should().BeOfType<List<LyricCardViewModel>>();
      result.Should().NotBeEmpty();
      result.Should().HaveCount(1);
      result.First().Title.Should().Be(seedLyricTitle);
      result.First().Slug.Should().Be(uniqueLyricSlug);
    }

    [Test]
    public async Task GetSingleLyricAsync_WhenArtistDoesNotExist_ThrowsAnArtistNotFoundException()
    {
      // arrange
      string artistSlug = "john-doe";
      string lyricSlug = "test-song";

      artistsServiceMock
        .Setup(x => x.GetArtistIdAsync(artistSlug))
        .ThrowsAsync(new ArtistNotFoundException(artistSlug));

      // act
      Func<Task> action = async () => await lyricsService.GetSingleLyricAsync(artistSlug, lyricSlug);

      // assert
      await action.Should().ThrowAsync<ArtistNotFoundException>();
    }

    [Test]
    public async Task GetSingleLyricAsync_WhenTheLyricDoesNotExist_ThrowsALyricNotFoundException()
    {
      // arrange
      int artistId = 1;
      string artistSlug = "ada-brown";
      string lyricSlug = "test-song";

      string artistFirstName = "Ada";
      string artistLastName = "Brown";

      Artist adaBrown = new Artist
      {
        Id = artistId,
        FirstName = artistFirstName,
        LastName = artistLastName,
        Slugs = new List<ArtistSlug>
        {
          new ArtistSlug
          {
            Name = artistSlug,
            CreatedAt = DateTime.UtcNow,
            IsPrimary = true
          }
        }
      };

      Context.Artists.Add(adaBrown);
      Context.SaveChanges();

      artistsServiceMock
        .Setup(x => x.GetArtistIdAsync(artistSlug))
        .ReturnsAsync(artistId);

      // act
      Func<Task> action = async () => await lyricsService.GetSingleLyricAsync(artistSlug, lyricSlug);

      // assert
      await action.Should().ThrowAsync<LyricNotFoundException>();
    }

    [Test]
    public async Task GetSingleLyricAsync_WhenBothArtistAndLyricExist_ReturnsTheLyric()
    {
      // arrange
      int artistId = 1;
      string artistSlug = "ada-brown";
      string lyricSlug = "test-song";

      string artistFirstName = "Ada";
      string artistLastName = "Brown";

      string lyricTitle = "Evil Mama Blues";
      string lyricBody = "song lyrics";

      Lyric evilMamaBlue = new Lyric
      {
        Title = lyricTitle,
        Body = lyricBody,
        CreatedAt = DateTime.UtcNow,
        Slugs = new List<LyricSlug>
        {
          new LyricSlug
          {
            Name = lyricSlug,
            CreatedAt = DateTime.UtcNow,
            IsPrimary = true
          }
        }
      };

      Artist adaBrown = new Artist
      {
        Id = artistId,
        FirstName = artistFirstName,
        LastName = artistLastName,
        Slugs = new List<ArtistSlug>
        {
          new ArtistSlug
          {
            Name = artistSlug,
            CreatedAt = DateTime.UtcNow,
            IsPrimary = true
          }
        },
        Lyrics = new List<Lyric> { evilMamaBlue }
      };

      Context.Artists.Add(adaBrown);
      Context.SaveChanges();

      artistsServiceMock
        .Setup(x => x.GetArtistIdAsync(artistSlug))
        .ReturnsAsync(artistId);

      // act
      LyricViewModel result = await lyricsService.GetSingleLyricAsync(artistSlug, lyricSlug);

      // assert
      result.Should().NotBeNull();
      result.Title.Should().Be(lyricTitle);
      result.Body.Should().Be(lyricBody);
    }
  }
}
