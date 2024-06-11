﻿using FluentValidation;

namespace WebApp.Models.Dtos.RoleDtos;

public class RoleCreateDto : IDto
{
    public string Name { get; set; } = null!;
}


public class RoleCreateDtoValidator : AbstractValidator<RoleCreateDto>
{
    public RoleCreateDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}