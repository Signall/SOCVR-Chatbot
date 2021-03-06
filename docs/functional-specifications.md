# Chatbot for the SO Close Vote Reviews Chat Room Functional Specifications

This document describes the functionality of the chatbot that resides in the [SO Close Vote Reviewers chat room](http://chat.stackoverflow.com/rooms/41570/so-close-vote-reviewers).

<!-- TOC depthFrom:2 depthTo:6 withLinks:1 updateOnSave:1 orderedList:0 -->

- [Background](#background)
- [What does this program plan to solve?](#what-does-this-program-plan-to-solve)
- [History of v1](#history-of-v1)
- [Goals for v2](#goals-for-v2)
- [V2 Member Workflow](#v2-member-workflow)
- [V2 Command List](#v2-command-list)
	- [Commands for all users](#commands-for-all-users)
	- [Commands for Reviewer permission group](#commands-for-reviewer-permission-group)
	- [Commands for Room Owner permission group](#commands-for-room-owner-permission-group)
	- [Commands for Users in any permission group](#commands-for-users-in-any-permission-group)
- [Commands - Descriptive](#commands-descriptive)
	- [Alive](#alive)
	- [Commands](#commands)
	- [Help](#help)
	- [Running Commands](#running-commands)
	- [Status](#status)
	- [Reviews Today](#reviews-today)
	- [Total reviews today](#total-reviews-today)
	- [Request permission to group](#request-permission-to-group)
	- [Membership](#membership)
	- [Opt-in and Opt-out](#opt-in-and-opt-out)
	- [Audit stats](#audit-stats)
	- [Current Tag and Next # tags](#current-tag-and-next-tags)
	- [Refresh tags](#refresh-tags)
	- [Stats](#stats)
	- [Current Review Count](#current-review-count)
	- [Reviews Today](#reviews-today)
	- [Start Event](#start-event)
	- [Ping Reviewers](#ping-reviewers)
	- [Stop Bot](#stop-bot)
	- [Reboot Bot](#reboot-bot)
	- [Approve and Reject Request #](#approve-and-reject-request-)
	- [Add User To Group](#add-user-to-group)
	- [Remove User From Group](#remove-user-from-group)
	- [View Requests](#view-requests)
- [Command Interpretation](#command-interpretation)
- [Permission system](#permission-system)
	- [Permission Request](#permission-request)
		- [Asking for permission](#asking-for-permission)
		- [Viewing Requests](#viewing-requests)
	- [Handling Requests](#handling-requests)
- [User Tracking](#user-tracking)
	- [Bot Messages](#bot-messages)
- [Docker](#docker)
- [Configuration](#configuration)
- [Database](#database)

<!-- /TOC -->

## Background

Grab a chair and sit down. Let me tell you a tale of how things used to be.

Back when I (gunr2171) joined SOCVR, the main members were rene and TGMCians. That's just about it. Some people popped in from time to time, but for the most part the only people consistently helping on the CV queue were those two. It took me some time to get used to the queue but once I was acclimated I joined in.

With such a small crew, the only way we could make a dent in the queue would be to all work on the same tag. This way, there would be a high chance we would be reviewing the same post, and ensuring that more review items were completed.

The best way we had to keep track of what we were working on was manually. We would need to ping each other (usually rene) to say when we're done with a tag, or other such status updates. With only 3 members, this wasn't a problem.

Audits were a different story. We wrote "passed c# audit" not to keep track of it, but to just tell the other person that we were still here and doing our part. It was a "hey look, I'm still going strong."

All of the commands in version 1 are formatted to accept the way manual messages were made. It's not the cleanest, but it works.

## What does this program plan to solve?

Instead of 3 regulars we've got about 10 or more, depending on the day. I developed the bot primarily as a way of keeping track of what our chat room has accomplished so we know what to work on next.

The questions this bot is designed to solve are:

> The other chat members and I are in the CV queue right now, what should we work on to maximize the number of review items that get completed?  
> How am I doing in the CV queue? (stats)

## History of v1

Version 1 was a really good stab. It got everything we really needed to have the bot do. Of course, there are a lot of things we wish it _could_ do. Version 2 is mostly improvement to the current system.

Areas of Version 1 that we have problems with:

* You needed to type chat commands for anything to happen.
* It only knew about things you told it about.

## Goals for v2

The high level goals for version 2 are:

* Runs as a Linux Docker image - running the bot from Jenkins is not what Jenkins is designed for, so the software will be built to run as a service. Jenkins will still be used for testing the software and deploying it to test/production environments.
* Minimum interactions needed from chat to operate - bot will work in the background to gather information, without needing it from chat.
* Store review sessions by UTC day, not by individual sessions - in v1 a person can have multiple sessions per day, which doesn't make a lot of sense and can be confusing. In v2, the chatbot will just record all reviews done within a UTC day and do computations from there.

## V2 Member Workflow

Here's how a chat member should expect to use the bot in v2:

1. A UTC day starts.
2. The user does a single review.
3. The bot pings the user to say:
  > I see you have started reviewing @[username]. Good luck!"

4. The user may stop and start (human phrasing) multiple times, the bot will not care (this includes taking an hour between reviews).
5. If the user passes an audit it will be posted to the chat room.
6. As soon as the user completes 40 review items in a single UTC day, the bot will ping the user saying:
  > You've completed 40 CV review items today, thanks! The time between your first and last review today was X minutes, averaging to a review every Y minutes.

7. The user waits for the next UTC day to start.

## V2 Command List

Commands for this version will focus on stats. Some commands will stick around in case manual intervention is needed.

**General rules for commands:**

* "Public" commands are primarily to test if the bot is running or get general information from the bot.
* Moderators have access to all commands.
* Commands and arguments are case-insensitive

The following is a summary table of all commands. Extended details are in the next sections.

### Commands for all users

<!-- use https://ozh.github.io/ascii-tables/ to create this -->

Anyone in chat can run these commands.

| Command                            | Description                                                                        |
|------------------------------------|------------------------------------------------------------------------------------|
| Alive                              | Tests if the bot is running and listening to chat.                                 |
| Commands                           | Shows the list of commands to control the bot.                                     |
| Help                               | Prints information about the bot.                                                  |
| Running Commands                   | Displays a list of commands that the bot is currently executing.                   |
| Status                             | Displays how long it has been running for and what version is running.             |
| Request permission to group        | Submits a request for the user to be added to a given permission group.            |
| Membership                         | Shows a list of all permission groups, and the members in those permission groups. |

### Commands for Reviewer permission group

| Command              | Description                                                                                                                                        |
|----------------------|----------------------------------------------------------------------------------------------------------------------------------------------------|
| Audit stats          | Shows the user how many audits of each tag they have passed.                                                                                       |
| Current Tag          | Fetches the tag that has the most amount of manageable close queue items from the SEDE query.                                                      |
| Next # tags          | Displays the first X tags from the SEDE query to focus on.                                                                                         |
| Refresh tags         | Forces a refresh of the tags obtained from the SEDE query.                                                                                         |
| Stats                | Shows the stats at the top of the /review/close/stats page.                                                                                        |
| Reviews today        | Shows user's stats for the reviews they have made in the current UTC day. Adding "Details" will print a table of those reviews.                    |
| Total reviews today  | Shows summary information and a table of the people who have completed reviews today.                                                              |
| opt out and opt in   | (also "opt-out" / "opt-in") Allows a user to be temporarily removed from the tracking system, or resume being tracked.                             |

### Commands for Room Owner permission group

| Command        | Description                                                                                                   |
|----------------|---------------------------------------------------------------------------------------------------------------|
| Start event    | The Start Event command is a combination of the Next [3] Tags and Stats command.                              |
| Ping reviewers | Sends a message which includes an @reply to all users in the reviewers group that have done reviews recently. |
| Stop bot       | Leaves the chat room and quits the running application.                                                       |
| Reboot bot     | Shuts down then starts back up.                                                                               |

### Commands for Users in any permission group

| Command                       | Description                                                          |
|-------------------------------|----------------------------------------------------------------------|
| [approve/reject] request [#]  | Approves or rejects a request for a user to join a permission group. |
| add user to [group name]      | Manually adds a user to the given permission group.                  |
| remove user from [group name] | Manually removes a user from the given permission group.             |
| view requests                 | Shows a list of all pending permission requests                      |

## Commands - Descriptive

Below are the descriptive actions for each command.

### Alive
This is just a quick command to test if the program is operational. When activated, the bot will respond with one of the following lines:

> I'm alive and kicking!  
> Still here you guys!  
> I'm not dead yet!  
> I feel... happy!  
> I feel fine.

### Commands
This command is used for showing the user what commands they can use to control the bot.

By default, the bot will display an alphabetical list of commands that the user is allowed to run.

> (in reply) Here is a list of commands you have permission to run:  
> [command 1 usage] - [command 1 description]  
> [command 2 usage] - [command 2 description]  
> [command 3 usage] - [command 3 description]  

Running `commands full` will show all commands the bot listens for, whether the user can run them or not. Commands will be grouped according to their permission group.

The bot will make two messages. The first will be a reply to the command. The second will be a multilined message containing the list of commands in this format:

<pre>
Below is a list of commands for the SOCVR Chatbot:
[group name]
    [command 1 usage] - [command 1 description]
    [command 2 usage] - [command 2 description]
    [command 3 usage] - [command 3 description]

[group name]
    [command 1 usage] - [command 1 description]
    [command 2 usage] - [command 2 description]
    [command 3 usage] - [command 3 description]
</pre>

### Help
The bot will post a general message in the following format:

> This is a chatbot for the SO Close Vote Reviewers chat room, developed by members of the SO Close Vote Reviewers chat room. For more information see the [github page](https://github.com/SO-Close-Vote-Reviewers/SOCVR-Chatbot), or reply with `commands` to learn what you can do.

### Running Commands
This is the "task manager" for the bot. All commands will run on their own threads, so the bot will keep track of which commands are currently being processed.

An example output:

<pre>
| Command          | For User           | Started       |
|-------------------------------------------------------|
| Running Commands | gunr2171 (1043380) | 0 seconds ago |
</pre>

### Status

This command has the bot display what version of code it is currently running along with the time it has been running for.

> SOCVR Chatbot [deployment type] version [git version] (commited [date/time of commit]), running for [timespan].

* `[deployment type]` is the "purpose" name of this running version of the bot. Expect values like "production", "branch-test", or "development".
* `[git version]` will be the first 8 characters of the SHA-1 commit id that the bot is currently running.
* `[timespan]` will be the amount of time that the bot has been running for.

 An example:

> SOCVR Chatbot production version abcd1234 (commited 2015-01-02 00:00:00 UTC), running for 4 hours, 2 minutes, and 23 seconds.

### Reviews Today
This command is used to show summary stats for the reviews a user has completed in the current UTC day. The syntax is:

    reviews today [details]

This is an example reply:

> Today you have completed 30 review items, 2 of which were audits. The time between your first and last review item was 31 minutes and 23 seconds, averaging a review every 14 seconds.

If the user adds `details` to the end of the command, the following table will be displayed under the bot's given stats. Adding `details` is optional.

<pre>
+---------+------------+--------+-------------------------+
| Item Id | Action     | Audit  | Completed At            |
+---------+------------+--------+-------------------------+
| 12345   | Closed     |        | 2015-01-02 03:23:11 UTC |
| 23456   | Edit       | Passed | 2015-01-02 03:23:11 UTC |
| 34567   | Leave Open | Failed | 2015-01-02 03:23:11 UTC |
+---------+------------+--------+-------------------------+
</pre>

The `Item Id` is the number in the URL for that review item. If the review item is not an audit then the Audit cell will be blank. Order this table by `Completed At` ascending.

### Total reviews today
This command shows summary information about all reviews done in the current day along with a breakdown table of reviews by user. The table will be sorted by `Review Items Today` descending. Only members that have reviewed at least one item today will be used in these stats.

<pre>
Today, 4 members have reviewed a total of 60 items. They are 55% of the way to processing all review items for the day.
+----------+--------------------+
| User     | Review Items Today |
+----------+--------------------+
| person 1 | 20                 |
| person 3 | 14                 |
| person 2 | 4                  |
+----------+--------------------+
</pre>

### Request permission to group
If a person wants to join a permission group, this is one method of requesting access. See Permission System / Asking For Permission / Second Method for more details.

### Membership
A user will want to know what permissions he/she currently has, and the permission groups of other people in the room (to see who to contact about things, for example). This command lists out each permission group.

The first of the two messages the bot will post will be a reply to the command:

> Below is a listing of the people in each permission group:

Then followed by a multilined message in the following format:

<pre>
[group name]
	[display name] [user id]
	[display name] [user id]
[group name]
	[display name] [user id]
	[display name] [user id]
</pre>

The groups shown are shown, in this order:

1. Reviewers
2. Bot Owners

### Opt-in and Opt-out
By default, when a user joins the Reviews permission group, they are opted-in to the tracking system. A Reviewer might want to be ignored by the tracking system for a period of time (this could be hours, days, months, etc). The Opt-out command will allow that user to be ignored until they run the opt-in command. The bot will also record the date of the last "opt-" change.

Running opt-out while opted-in:

> (in reply) You have been opted-out from tracking, and will remain that way until you run `opt-in`.

Running opt-in while opted-out:

> (in reply) You have been opted-in to tracking, and will remain that way until you run `opt-out`.

Running either command while you are already in that state:

> (in reply) You are already opted-[in/out] [of/to] tracking, and have been in this state for [time since last opt- change]. You may switch your preference by running [opposite command of state currently in].

### Audit stats
A user will want to know how many audits they have passed per tag and how many audits they have passed in the current day.

This command will output two messages. The first will be a reply to the command.

> Below are your audit stats:

The second message will be a multilined message formatted like this:

<pre>
Audits passed today:
| Tag     | Count |
|---------|-------|
| c#      | 3     |
| c++     | 1     |
| haskell | 1     |

All tracked audits by tag:
| Tag     | Count | %      |
|---------|-------|--------|
| haskell | 14    | 45.40% |
| c++     | 4     | 12.40% |
| c#      | 1     | 4.60%  |
</pre>

Both tables will be ordered by the Count column descending.

### Current Tag and Next # tags
The bot will run a pre-determined SEDE query to figure out what tags the room should be working on.

The Current Tag command will display the first one from the list.

> The current tag to work on is [tag] with [#] known review items.

The Next Tags command will retrieve the top _n_ tags from that list.

> The top [#] tags to work on are [tag] `[#]`, [tag] `[#]`, ...

If the value given as the argument for the Next Tags command is not an integer between 1 and 15 inclusive, the bot will reply with:

> Please give me a number between 1 and 15.

### Refresh tags
The bot holds on to the current list of SEDE tags for a predetermined cache time (configurable). This command forces a refresh of those values.

The bot will post one or two messages (depending on the timing). The first message will be a reply to the command:

> Refreshing the tag listing, please wait.

Once the refresh has completed, the bot will attempt to edit the first message to include.

> Refresh complete, took [time it took to do refresh].

If the bot is not able to edit the first message (usualy because it took too much time), the bot will post another message instead, also as a reply to the command:

> Tag refresh complete, took [time it took to do refresh].

### Stats
The "/review/close/stats" page holds general queue stats. This command displays those values.

> In the CV queue, [# need review] need review, [# reviews today] reviews have been completed today, and [# all time] reviews have been completed overall.

### Current Review Count
As a user reviews CV items in the queue, the number of reviews they’ve completed gradually increases. This command prints exactly how many reviews the user has completed so far. Example,

> Of the reviews I have tracked, you've completed [number of reviews today] reviews today, and [number of reviews overall] reviews overall.

### Reviews Today
A user may want more detailed information than what has already been provided from the Current Review Count command, such as whether or not a particular review was an audit. All this information will be displayed as a table, for example:

| Item Id | Action     | Audit  | Completed At            |
|---------|------------|--------|-------------------------|
| 12345   | Close      |        | 2015-01-02 03:23:11 UTC |
| 23456   | Edit       | Passed | 2015-01-02 03:23:11 UTC |
| 34567   | Leave Open | Failed | 2015-01-02 03:23:11 UTC |

The Item Id is the number in the URL for that review item. If the review item is not an audit then the Audit cell will be blank. Order this table by Completed At ascending.

This is the only place in the entire program which will display failed audits.

### Start Event
The Start Event command is a combination of the Next [3] Tags and Queue Stats command. This is used to formally start a weekly review event.

The bot will output two messages. The first message will be the output of the Queue Stats command. The second message will be the output of the Next [3] Tags command, except the text before the tag list will be "The tags to work on are".

### Ping Reviewers
This command allows the user to ping everyone in the Reviewer permissions group with a custom message.

In order for a user to appear in the user list they must have reviewed at least X reviews within the last Y days (configurable). Default is 50 reviews within 3 days.

The person giving the command will not appear in the list. The user list will be sorted alphabetically. Ensure that people with non-alphanumeric charters in their names have those characters removed so pings work correctly. The list of users is always placed after the message.

Here is an example:

> User A: @Bot Ping reviewers It’s time to review!  
> Bot: It’s time to review! @UserB @UserC @UserD @UserE

### Stop Bot
When executed, this command will cause the bot to leave the chatroom and then terminate the current process. This will end the Docker container with a 0 exit code.

### Reboot Bot
This command does the same as Stop Bot, but causes the bot to start back up again after successfully shutting down. This will not stop the Docker container.

### Approve and Reject Request #
Depending on the message, this command will approve or reject a request from a user to join a chat command permission group.

The command will be in on of these formats:

> approve request 1234  
> reject request 1234

The person who handled the request and the time it was handled will be recorded in the database, but it will not be publicly shown.

If the given request cannot be found:

> I can't find that permission request. Run `View Requests` to see the current list.

If the given request has already been handled (note that there are no details given about how the request was handled);

> That request has already been handled.

If command is valid, and the process was successful, the bot will reply to the command message with:

> Request processed successfully.

If the request was approved, the message will append the following:

> @[requesting user name] has been added to the [permission group] group.

### Add User To Group
The previous command is for add a user to a permission group if he/she has already requested it. However, chat members will want to add users without needing a request from the recipient.

The command syntax is:

    add [user id] to [group name]

Group name is the name of any permission group, so "reviewers" or "bot owner".

When a user is successfully added to a group using this command, the following message will be said as a reply to the command:

> I've added @(user) to the [group name] group.

If the user running the command is not in the permission group specified, the bot responds with

> You need to be in the [group name] group in order to add people to it.

If the target user is already in the mentioned group

> (username) is already in the [group name] group.

If the user cannot be added to the group because of restrictions

> I can't add (username) to the [group name] because (restriction message).

### Remove User From Group
This is the mirror of the `Add user to` command.

    remove [user id] from [group name]

Use the opposite logic as the last command.

### View Requests
This command shows all pending permission requests in the system. The user can then use any of the other permission commands to process them.

See the "Viewing Requests" section for more details.

## Command Interpretation
The bot will make an attempt to correct a user if they type in a wrong command (such as spelling).

The bot will look at the input and see if it within a configurable % of a known command (if multiple commands match the one with the highest percent match will be chosen) and suggest to the user if they meant that:

> Did you mean to run "(command name)"? Reply "yes" to run it.

If the user replies yes to the message then the bot will attempt to run the command as normal. Remember that permissions and restrictions still need to be checked.

If no command matched the % threshold, then the bot will ignore the message.

## Permission system

The new permission system will be made of 2 groups:

* Reviewers
* Bot Owners

These groups are independent of each other. A person can be in multiple groups at once. Members of each group can add users to that group.

Restrictions on joining a group:
* To join the Reviewers group you must have at least 3000 reputation.
* To join the Bot Owners group you must be in the Reviewers group.

Restrictions on approving or rejecting a request:
* To approve or reject a request for the Reviewers group you must be in that group for at least 1 week.
* To approve or reject a request for the Reviewers group you must have done at least 100 reviews in the last 7 days, including the current UTC day.

### Permission Request

In version 1, only bot owners could add users to the track list. This was fine most of the time, but there are some drawbacks:
* If there are no bot owners present then the user has to come back later or make a ping.
* Even if the bot owner is around they might not see the request.

Either way, it's easy for a request to go unseen. This new permission system will allow normal members to handle requests (in the same spirit as community moderation on Stack Overflow). More eyes means quicker request handling.

Note: a user may only have one request per permission group active at a time (active means it is waiting to be approved or rejected).

#### Asking for permission

A user has 2 methods to request permission to a group.
1. Attempt to run a command they don't have access to, then respond "yes" to the prompt.
2. Run the `request permission for [group name]` command.

Exception: you _can not_ use the first method to request access to Bot Owner. These messages will be ignored by the bot.

**First method**

If a user does not have the correct permissions to run a command the bot will respond with:

> Sorry, you are not in the [name of group] permission group. Do you want to request access? (reply with "yes")

A permission request will be inserted into the request queue the user replies "yes" (or limited variations) to the bot's message.

The above is the standard reply, assuming there are no issues with the user creating a request. The following are alternate replies the bot can make depending on if the user can make a request.

If the user tries to run a command where they do not have permission to do so, and they have an active request for that permission, the bot will respond with:

> Sorry, you are not in the [group name] permission group. There is already a request to get you this permission, please be patient.

If the user tries to run a command where they do not have permission to do so, the latest request for that permission has been denied, and that denial was within the last 48 hours, then the bot will ignore the message. Once 48 hours has ellapsed, the bot will allow them to ask for permission again.

*Note, there should not be a need to increase the amount of time a reject will incur.*

There is a already a "kick" and "ban" system in chat which which room owners should use if it gets to this point.
If the user tries to run a Reviewers command where they do not have permission to do so, and the user has less than 3000 reputation, the bot will respond with:

> Sorry, this command requires that you have 3000 reputation and are a part of the Reviewers permission group.

**Second method**

A user can run the `request permission for [group name]` command.

* If the user is already in that group:
  > You are already in the [group name] group.

* If the user's last request for this permission was denied and that denial was less than 48 hours ago:
  > Sorry, your latest request for this permission was denied. Please wait [time until user can request again] to request again.

  Note that any member of the requested group may add the user to the group during this "cool down" time.

* If there is already an active request for this permission group:
  > There is already a request to get you this permission, please be patent.

* If none of the above are true, create the request in the system and reply:
  > I'm created a request (#[request number]) to get you in the [group name] group.

#### Viewing Requests

A user that is in at least one permission group user can run the `View requests` command to see the full list. This is an example output:

| Request # | Display Name | User Id | Requesting | Requested at            |
|-----------|--------------|---------|------------|-------------------------|
| 1         | gunr2171     | 12345   | Reviewer   | 2015-01-01 00:00:00 UTC |
| 2         | rene         | 23456   | Bot Owner  | 2015-01-01 00:00:00 UTC |
| 3         | sam          | 34567   | Reviewer   | 2015-01-01 00:00:00 UTC |

The table will be ordered by "Requested at" ascending (oldest requests first).

If there are no active requests the bot will reply:

> There are no users requesting access to a permission group.

In this version, there will be no way to view requests that have already been handled (other than searching the transcript).

Non-public non-bot-owners must run the command to see this request list. The bot will send a general message (not a reply to any user) if:
 * A Bot Owner has posted 3 messages within 5 minutes.
 * There is one or more request in the system.
 * This message has not been displayed in the last 6 hours.

### Handling Requests

A group member has two methods to approve or reject a request.
1. Run `approve request <#>` or `reject request <#>`. The request number can be found out by running `view requests`.
2. The user manually adds the user to the group (this will approve the request at the same time).

## User Tracking

As previously stated, v1 primarily relies on information fed from chat users in the room. This worked to a degree, but allows users to supply [inaccurate data](http://chat.stackoverflow.com/transcript/41570?m=24910842#24910842) and can become tiresome after a while. Thus we’ve introduced automated review data<sup>§</sup> gathering for:

* Passed audits,
* Failed audits,
* Tags reviewed (experimental),
* Items reviewed.

<sup>§</sup> <sub>Data of posts that have been deleted are not detected (I.e., audits).</sub>

A user will only be eligible for tracking if:
* The user is in the Reviewers permission group.
* The user is currently opted-in to tracking.
* The user is not a moderator.

### Bot Messages
The bot will write messages in chat when it detects particular events from the user tracking system. However, a message will only be posted if the user is currently in the chat room.

For this section, a "tracked user" is a person in the Reviewers permission group and is currently opted-in.

If a user completes the first review item of the day:

> I see you have started reviewing @[username]. Good luck!

If a tracked user passes an audit:

> @[username] has passed a[n] [tag] audit.

If a tracked user has completed 40 review items:

> @[username], You've completed 40 CV review items today, thanks! The time between your first and last review today was X minutes, averaging to a review every Y [minutes|seconds].

If the user appears to be working on a new set of tags (this message will only be said once per day per user)<sup>^</sup>:

> Yesterday you were working on [tag], are you done with that?

<sup><sup>^</sup> Additional discussion needed (as a user may review multiple tag sets per day).</sup>

## Docker

Docker is a "virtual machine" system for linux. We should use the following methods when using docker.
* Environment variables before config file - If a setting can be found as an environment variable, use it. If it can't, try and find it in the configuration file. If it still can't be found, use a built-in default.
* Database in its own container, which is linked.

## Configuration
This program is designed to be very configurable.

Some values used in the program have built-in defaults. However some values won't have built-in defaults. If these values are not configured from either the configuration file or environment variables then the bot should reply that the value(s) have not been configured.

> This operation can't be performed because the following configuration values have not been set and no default exits: `value 1`, `value 2`, ...

The order of operations for finding a configuration value is:

1. If there is a configuration file, and the value is defined in the file, use this.
2. If the value is defined in an environment variable, use this.
3. Use built-in defaults if available

## Database
Information that the bot gets will be stored in a database.

When the bot starts up it will check if the database can be found (using the provided connection string). If the database can't be found the program will create a new empty database and add all Room Owners to the Bot Owners permission group.
