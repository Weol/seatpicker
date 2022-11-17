﻿namespace Seatpicker.Domain;

public record User(
    string Id, 
    string Nick, 
    string Avatar,
    string Name,
    IEnumerable<Role> Roles, 
    DateTimeOffset CreatedAt);

public record UnregisteredUser(
    string Id,
    string Nick,
    string Avatar,
    string Name);
