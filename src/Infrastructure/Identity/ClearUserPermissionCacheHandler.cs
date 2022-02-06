﻿using FSH.WebApi.Application.Common.Events;
using FSH.WebApi.Application.Identity.Users;
using FSH.WebApi.Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace FSH.WebApi.Infrastructure.Identity;

internal class ClearUserPermissionCacheHandler :
    INotificationHandler<EventNotification<ApplicationUserUpdatedEvent>>,
    INotificationHandler<EventNotification<ApplicationRoleUpdatedEvent>>
{
    private readonly IUserService _userService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ClearUserPermissionCacheHandler(IUserService userService, UserManager<ApplicationUser> userManager) =>
        (_userService, _userManager) = (userService, userManager);

    public async Task Handle(EventNotification<ApplicationUserUpdatedEvent> notification, CancellationToken cancellationToken)
    {
        if (notification.Event.RolesUpdated)
        {
            await _userService.ClearPermissionCacheAsync(notification.Event.UserId, cancellationToken);
        }
    }

    public async Task Handle(EventNotification<ApplicationRoleUpdatedEvent> notification, CancellationToken cancellationToken)
    {
        if (notification.Event.PermissionsUpdated)
        {
            foreach (var user in await _userManager.GetUsersInRoleAsync(notification.Event.RoleName))
            {
                await _userService.ClearPermissionCacheAsync(user.Id, cancellationToken);
            }
        }
    }
}