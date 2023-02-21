﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Seatpicker.Domain;

namespace Seatpicker.Infrastructure.Adapters.Database;

public class SeatpickerContext : DbContext
{
    public DbSet<SeatDao> Seat { get; set;  }

    public DbSet<FloorplanDao> Floorplans { get; set;  }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("InMemory");
    }
}

public class FloorplanDao
{
    [Key]
    public int Id { get; set; }

    public byte[] Backdrop { get; set; }
}

public class SeatDao
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(Floorplan))]
    public int FloorplanId { get; set; }

    public string Nick { get; set; }

    public string DiscordId { get; set; }

    public string DiscordAvatar { get; set; }

    public FloorplanDao Floorplan;
}
