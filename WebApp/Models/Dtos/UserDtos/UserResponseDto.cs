﻿namespace WebApp.Models.Dtos.UserDtos;

public class UserResponseDto : IDto
{
    public Guid Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
}