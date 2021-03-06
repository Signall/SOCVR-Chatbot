﻿using SOCVR.Chatbot.Database;
using System;
using System.Linq;
using ChatExchangeDotNet;
using Microsoft.Data.Entity;
using TCL.Extensions;
using SOCVR.Chatbot.Configuration;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Permission
{
    class AddUserToGroup : PermissionUserCommand
    {
        public override string ActionDescription => "Manually adds a user to the given permission group.";

        public override string ActionName => "Add User To Group";

        public override string ActionUsage => "add [user id] to [group name]";

        public override PermissionGroup? RequiredPermissionGroup => null;

        public override bool UserMustBeInAnyPermissionGroupToRun => true;

        protected override string RegexMatchingPattern => @"^add (\d{1,9}) to ([\w ]+)$";

        public override void RunAction(Message incomingChatMessage, Room chatRoom)
        {
            var targetUserId = GetRegexMatchingObject()
                .Match(incomingChatMessage.Content)
                .Groups[1]
                .Value
                .Parse<int>();

            if (!ChatExchangeDotNet.User.Exists(chatRoom.Meta, targetUserId))
            {
                chatRoom.PostReplyOrThrow(incomingChatMessage, "Sorry, I couldn't find a user with that ID.");
                return;
            }

            //get the permission group from the chat message
            var rawRequestingPermissionGroup = GetRegexMatchingObject()
                .Match(incomingChatMessage.Content)
                .Groups[2]
                .Value;

            //parse the string into the enum
            var requestingPermissionGroup = MatchInputToPermissionGroup(rawRequestingPermissionGroup);

            if (requestingPermissionGroup == null)
            {
                //we don't know what that permission group is
                chatRoom.PostReplyOrThrow(incomingChatMessage, "I don't know what that permission group is. Run `Membership` to see a list of permission groups.");
                return;
            }

            using (var db = new DatabaseContext())
            {
                //lookup the processing user
                var processingUser = db.Users
                    .Include(x => x.Permissions)
                    .Single(x => x.ProfileId == incomingChatMessage.Author.ID);

                //lookup the target user
                var targetUser = db.Users
                    .Include(x => x.Permissions)
                    .Include(x => x.PermissionsRequested)
                    .SingleOrDefault(x => x.ProfileId == targetUserId);

                //if the user has never said a message in chat, the user will not exist in the database
                //add this user
                if (targetUser == null)
                {
                    targetUser = new Database.User()
                    {
                        ProfileId = targetUserId
                    };
                    db.Users.Add(targetUser);
                    db.SaveChanges();
                }

                //lookup the chat target user
                var chatTargetUser = chatRoom.GetUser(targetUserId);

                //check restrictions on processing user
                var processingUserAbilityStatus = CanUserModifyMembershipForGroup(requestingPermissionGroup.Value, processingUser.ProfileId);

                if (processingUserAbilityStatus != PermissionGroupModifiableStatus.CanModifyGroupMembership)
                {
                    switch (processingUserAbilityStatus)
                    {
                        case PermissionGroupModifiableStatus.NotInGroup:
                            chatRoom.PostReplyOrThrow(incomingChatMessage, $"You need to be in the {requestingPermissionGroup.Value} group in order to add people to it.");
                            break;
                        case PermissionGroupModifiableStatus.Reviewer_NotInGroupLongEnough:
                        {
                            var days = ConfigurationAccessor.DaysInReviewersGroupBeforeProcessingRequests;
                            chatRoom.PostReplyOrThrow(incomingChatMessage, $"You need to be in the Reviewer group for at least {days} day{(days > 1 ? "s" : "")} before you can process requests.");
                            break;
                        }
                    }

                    return;
                }
                //else, there was no problems with using this processing user

                //check restrictions on target user
                var canJoinStatus = CanTargetUserJoinPermissionGroup(requestingPermissionGroup.Value, targetUserId, chatRoom);

                if (canJoinStatus != PermissionGroupJoinabilityStatus.CanJoinGroup)
                {
                    switch (canJoinStatus)
                    {
                        case PermissionGroupJoinabilityStatus.AlreadyInGroup:
                            chatRoom.PostReplyOrThrow(incomingChatMessage, $"{chatTargetUser.Name} is already in the {requestingPermissionGroup.Value} group.");
                            break;
                        case PermissionGroupJoinabilityStatus.BotOwner_NotInReviewerGroup:
                            chatRoom.PostReplyOrThrow(incomingChatMessage, $"{chatTargetUser.Name} needs to be in the Reviewer group before they can join the Bot Owners group.");
                            break;
                        case PermissionGroupJoinabilityStatus.Reviewer_NotEnoughRep:
                            chatRoom.PostReplyOrThrow(incomingChatMessage, $"{chatTargetUser.Name} needs at least {ConfigurationAccessor.RepRequirementToJoinReviewers} rep to join the Reviewer group");
                            break;
                    }

                    return;
                }
                //else, user can join group, continue on

                //passed all checked, add the user to the group and approve any pending requests for that user/group
                targetUser.Permissions.Add(new UserPermission
                {
                    PermissionGroup = requestingPermissionGroup.Value,
                    JoinedOn = DateTimeOffset.UtcNow
                });

                var pendingRequestsForTargetUserAndGroup = targetUser
                    .PermissionsRequested
                    .Where(x => x.Accepted == null)
                    .Where(x => x.RequestedPermissionGroup == requestingPermissionGroup.Value);

                foreach (var pendingRequest in pendingRequestsForTargetUserAndGroup)
                {
                    //approve it
                    pendingRequest.Accepted = true;
                    pendingRequest.ReviewingUser = processingUser;
                }

                //if you're add the user to the Reviewer group, also set them to opt-in
                if (requestingPermissionGroup.Value == PermissionGroup.Reviewer)
                {
                    targetUser.OptInToReviewTracking = true;
                    targetUser.LastTrackingPreferenceChange = DateTimeOffset.UtcNow;
                }

                db.SaveChanges();

                chatRoom.PostReplyOrThrow(incomingChatMessage, $"I've added @{chatTargetUser.Name.Replace(" ", "")} to the {requestingPermissionGroup} group.");
            }
        }
    }
}
