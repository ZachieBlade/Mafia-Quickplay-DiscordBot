﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord_Mafia_Bot.Core;
using Discord_Mafia_Bot.Util;
using DiscordBot.Game;
using DiscordBot.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Mafia_Bot.Commands
{

    public class Commands : ModuleBase
    {
        /// <summary>
        /// TODO: change up the mention system so it accepts non-mentions.
        /// </summary>
        #region MiscCommands
        public class MiscCommands : ModuleBase
        {
            [Command("howtoplay"), Summary("Explains how to use the Mafia Bot and play a game!")]
            public async Task Ping()
            {
                await ReplyAsync("**How to play quickplay mafia games, with ME**\n\nYou can join a game by typing `!join` in the chat, once you're ready you'll have to type `!ready`.\nThe game won't start before __5__ people have joined!\nEveryone must be ready before the game starts.\n\nOnce the game starts you get your role PM. Any actions will be explained to you in your role PM.\nIn the game chat you can vote by typing `VOTE: @playername`\nFor a list of commands type `!help`.");
            }

            [Command("bug"), Summary("What to do when you find a bug.")]
            public async Task Bug()
            {
                await ReplyAsync("Found a bug? Or wanna recommend something be added/removed/changed?\nSend SoaringDylan a PM about it,\nPost it on our github https://github.com/dylanpiera/Mafia-Quickplay-DiscordBot \nor join our development server https://discord.gg/Tu82eWU");
            }

            [Command("cookie"), Summary("Gives you a cookie!")]
            public async Task Cookie()
            {
                await Context.Message.AddReactionAsync(new Emoji("🍪"));
            }

            [Command("inviteLink"), DiscordbotAdminPrecon(), Hidden()]
            public async Task inviteLink()
            {

                await ReplyAsync(Sneaky.botInvite);
            }
        }
        #endregion

        #region DebugCommands
        [Group("debug")]
        public class DebugCommands : ModuleBase
        {
            [Command("ping"), Summary("Returns with Pong!")]
            public async Task Ping()
            {
                await ReplyAsync("Pong!");
            }
        }
        #endregion

        #region JoinCommand
        public class JoinCommand : ModuleBase
        {
            [Command("join"), Summary("")]
            public async Task Join()
            {
                if (!Program.Servers[Context.Guild].gameRunning)
                {
                    if (!Program.Servers[Context.Guild].inGame(Context.User as IGuildUser))
                    {
                        Program.Servers[Context.Guild].Add(Context.User as IGuildUser);
                        await ReplyAsync("", false, new EmbedBuilder() { Title = "Player Joined!", Color = Color.Blue, Description = $"{Context.User.Mention} has joined the game! :white_check_mark:", Footer = new EmbedFooterBuilder() { Text = $"Current amount of players : {Program.Servers[Context.Guild].Objects.Count}" } });

                        return;
                    }
                    else
                    {
                        await ReplyAsync("", false, new EmbedBuilder() { Title = "Already in Game!", Color = Color.DarkRed, Description = $"{Context.User.Mention} you're already in the game! :x:", Footer = new EmbedFooterBuilder() { Text = $"Current amount of players : {Program.Servers[Context.Guild].Objects.Count}" } });
                    }
                }
                else
                {
                    await ReplyAsync("", false, new EmbedBuilder() { Title = "Ongoing Game!", Color = Color.DarkRed, Description = $"{Context.User.Mention} I'm sorry, but the game has already started. :no_entry_sign:", Footer = new EmbedFooterBuilder() { Text = $"Current amount of players alive: {Program.Servers[Context.Guild].TownAlive + Program.Servers[Context.Guild].MafiaAlive}/{Program.Servers[Context.Guild].Objects.Count}" } });
                }
            }

            [Command("join"), Summary("(optional) argument: User | (make user) join the game."), RequireUserPermission(GuildPermission.Administrator)]
            public async Task Join(params IGuildUser[] user)
            {
                if (!Program.Servers[Context.Guild].gameRunning)
                {
                    if (Context.Message.MentionedUserIds.Count == 0) await ReplyAsync("", false, new EmbedBuilder() { Title = "Ongoing Game!", Color = Color.Orange, Description = $"{Context.User.Mention} You need to mention a user. :x:" });
                    foreach (ulong id in Context.Message.MentionedUserIds)
                    {
                        IGuildUser mentionedUser = await Context.Guild.GetUserAsync(id);
                        if (!mentionedUser.IsBot || Context.Message.Content.Contains("--force") && mentionedUser != Context.Client.CurrentUser)
                        {
                            if (!Program.Servers[Context.Guild].inGame(mentionedUser as IGuildUser))
                            {
                                Program.Servers[Context.Guild].Add(mentionedUser as IGuildUser);
                                await ReplyAsync("", false, new EmbedBuilder() { Title = "Player Added!", Color = Color.Blue, Description = $"{mentionedUser.Mention} was added to the game by {Context.User.Mention}! :white_check_mark:", Footer = new EmbedFooterBuilder() { Text = $"Current amount of players : {Program.Servers[Context.Guild].Objects.Count}" } });
                            }
                            else
                            {
                                await ReplyAsync("", false, new EmbedBuilder() { Title = "Already in Game!", Color = Color.DarkRed, Description = $"{Context.User.Mention}, {mentionedUser.Mention} already is in the game! :x:", Footer = new EmbedFooterBuilder() { Text = $"Current amount of players : {Program.Servers[Context.Guild].Objects.Count}" } });
                            }
                        }
                        else
                        {
                            await ReplyAsync("", false, new EmbedBuilder() { Title = "Bots can't join!", Color = Color.DarkRed, Description = $"{Context.User.Mention} Bots aren't allowed to play, in particular ME. :no_entry_sign:" });
                        }
                    }
                }
                else
                {
                    await ReplyAsync("", false, new EmbedBuilder() { Title = "Ongoing Game!", Color = Color.DarkRed, Description = $"{Context.User.Mention} I'm sorry, but the game has already started. :no_entry_sign:", Footer = new EmbedFooterBuilder() { Text = $"Current amount of players alive: {Program.Servers[Context.Guild].TownAlive + Program.Servers[Context.Guild].MafiaAlive}/{Program.Servers[Context.Guild].Objects.Count}" } });
                }

            }
            [Command("join"), Hidden(), Priority(-1), RequireUserPermission(GuildPermission.Administrator)]
            public async Task Join(params string[] s)
            {
                await ReplyAsync("", false, new EmbedBuilder() { Title = "Missing Mention!", Color = Color.Orange, Description = $"{Context.User.Mention} You need to mention a user. :x:" });
            }
        }
        #endregion

        #region LeaveCommand
        public class LeaveCommand : ModuleBase
        {
            [Command("leave"), Summary("Leave the current game signups.")]
            public async Task Leave()
            {
                if (!Program.Servers[Context.Guild].gameRunning)
                {
                    if (Program.Servers[Context.Guild].inGame(Context.User as IGuildUser))
                    {
                        Program.Servers[Context.Guild].Remove(Context.User as IGuildUser);
                        await ReplyAsync("", false, new EmbedBuilder() { Title = "Player Left!", Color = Color.DarkGrey, Description = $"{Context.User.Mention} has left the game! :heavy_check_mark:", Footer = new EmbedFooterBuilder() { Text = $"Current amount of players : {Program.Servers[Context.Guild].Objects.Count}" } });
                    }
                    else
                    {
                        await ReplyAsync("", false, new EmbedBuilder() { Title = "Not in Game!", Color = Color.DarkRed, Description = $"{Context.User.Mention} you ain't in the game! :x:" });
                    }
                }
                else
                {
                    await ReplyAsync("", false, new EmbedBuilder() { Title = "Not in Game!", Color = Color.DarkRed, Description = $"{Context.User.Mention} You can not leave a game in progress. :no_entry_sign:" });
                }
            }

            [Command("leave"), Summary("Force (mentioned) to leave the game."), RequireUserPermission(GuildPermission.Administrator), Alias("kick")]
            public async Task Leave(params IGuildUser[] users)
            {
                if (!Program.Servers[Context.Guild].gameRunning)
                {
                    if (Context.Message.MentionedUserIds.Count == 0) await ReplyAsync("", false, new EmbedBuilder() { Title = "Ongoing Game!", Color = Color.Orange, Description = $"{Context.User.Mention} You need to mention a user. :x:" });
                    foreach (ulong id in Context.Message.MentionedUserIds)
                    {
                        IGuildUser mentionedUser = await Context.Guild.GetUserAsync(id);
                        if (Program.Servers[Context.Guild].inGame(mentionedUser as IGuildUser))
                        {
                            Program.Servers[Context.Guild].Remove(mentionedUser as IGuildUser);
                            if (!Context.Message.Content.Contains("--silent"))
                                await ReplyAsync("", false, new EmbedBuilder() { Title = "Player Left!", Color = Color.DarkGrey, Description = $"{Context.User.Mention} removed {mentionedUser.Mention} from the game! :heavy_check_mark:", Footer = new EmbedFooterBuilder() { Text = $"Current amount of players : {Program.Servers[Context.Guild].Objects.Count}" } });
                        }
                        else
                        {
                            if (!Context.Message.Content.Contains("--silent"))
                                await ReplyAsync("", false, new EmbedBuilder() { Title = "Not in Game!", Color = Color.DarkRed, Description = $"{Context.User.Mention}, {mentionedUser.Mention} is not in the game! :x:" });
                        }
                    }
                    #region Possibility2
                    //
                    //foreach (IGuildUser mentionedUser in users)
                    //{
                    //    if (Program.Servers[Context.Guild].inGame(mentionedUser as IGuildUser))
                    //    {
                    //        Program.Servers[Context.Guild].Remove(mentionedUser as IGuildUser);
                    //        if (!Context.Message.Content.Contains("--silent"))
                    //            await ReplyAsync("", false, new EmbedBuilder() { Title = "Player Left!", Color = Color.DarkGrey, Description = $"{Context.User.Mention} removed {mentionedUser.Mention} from the game! :heavy_check_mark:", Footer = new EmbedFooterBuilder() { Text = $"Current amount of players : {Program.Servers[Context.Guild].PlayerAmount}" } });
                    //    }
                    //    else
                    //    {
                    //        if (!Context.Message.Content.Contains("--silent"))
                    //            await ReplyAsync("", false, new EmbedBuilder() { Title = "Not in Game!", Color = Color.DarkRed, Description = $"{Context.User.Mention}, {mentionedUser.Mention} is not in the game! :x:" });
                    //    }
                    //}
                    #endregion
                }
            }
            [Command("leave"), Hidden(), Priority(-1), RequireUserPermission(GuildPermission.Administrator), Alias("kick")]
            public async Task Leave(params string[] s)
            {
                await ReplyAsync("", false, new EmbedBuilder() { Title = "Missing Mention!", Color = Color.Orange, Description = $"{Context.User.Mention} You need to mention a user. :x:" });
            }

        }
        #endregion

        #region ListCommand
        public class ListCommand : ModuleBase
        {
            [Command("list"), Summary("Get a list of people currently in the mafia game on the current server.")]
            public async Task List()
            {
                if (!Program.Servers[Context.Guild].gameRunning)
                {
                    if (Program.Servers[Context.Guild].Objects.Count > 0)
                    {
                        EmbedBuilder builder = new EmbedBuilder()
                        {
                            Color = Color.LighterGrey,
                            Title = "Player List:"
                        };
                        int i = 0;
                        foreach (Player player in Program.Servers[Context.Guild].Objects)
                        {
                            i++;
                            builder.Description += $"{i}. {player.User.Username}\n";
                        }
                        await ReplyAsync("", false, builder.Build());
                        return;
                    }
                    else
                    {
                        await ReplyAsync("", false, new EmbedBuilder() { Title = "Empty Game!", Color = Color.DarkRed, Description = $"{Context.User.Mention} the game is empty! :x:" });
                    }
                }
                else
                {
                    EmbedBuilder builder = new EmbedBuilder()
                    {
                        Color = Color.LighterGrey,
                        Title = "Player list with votes:"
                    };
                    int i = 0;
                    foreach (Player player in Program.Servers[Context.Guild].Objects)
                    {
                        i++;
                        try
                        {
                            builder.Description += $"{i}. {player.User.Mention} + votes: **{player.LynchTarget.User.Mention}**\n";
                        }
                        catch (Exception)
                        {
                            builder.Description += $"{i}. {player.User.Mention} + votes: -\n";
                        }
                        await ReplyAsync("", false, builder.Build());
                        return;
                    }
                }
            }

            [Command("votecount"),Alias("tally"),Summary("Shows the total votes on all players.")]
            public async Task VoteCount()
            {
                GamePlayerList game = Program.Servers[Context.Guild];
                if (game.Phase == Phases.Day)
                {
                    countVotes(game);
                    int i = 0;
                    EmbedBuilder builder = new EmbedBuilder() {Color = Color.LightGrey,Title = $"{game.Phase.ToString()} {game.PhaseCounter} vote tally:" };

                    List<Player> SortedList = game.Objects.Where(x => x.Alive && x.VotesOn > 0).OrderByDescending(o => o.VotesOn).ToList();
                    if (SortedList.Count != 0)
                    {
                        foreach (Player player in SortedList)
                        {
                            i++;
                            try
                            {
                                builder.Description += $"{i}. {player.User.Mention} {player.VotesOn}: {votedFor(game.Objects.Where(x => x.Alive).ToList(), player)}";
                            }
                            catch (Exception) { }
                        }
                    }
                    else
                    {
                        builder.Description += "There are no votes.";
                    }

                    await ReplyAsync("", false, builder.Build());
                }
            }

            public static string votedFor(List<Player> sortedList, Player lynchee)
            {
                string s = "";

                foreach (Player player in sortedList)
                {
                    if (player.LynchTarget == lynchee)
                        s += player.User.Mention + ", ";
                }
                s.Remove(s.Length - 2);
                return s;
            }

            public static void countVotes(GamePlayerList game)
            {
                foreach (Player player in game.Objects)
                {
                    player.VotesOn = 0;
                }
                foreach (Player player in game.Objects)
                {
                    try
                    {
                        player.LynchTarget.VotesOn++;
                    }
                    catch (Exception) { }
                }
            }
        }
        #endregion

        #region ReadyCommand
        public class ReadyCommand : ModuleBase
        {
            [Command("ready",RunMode = RunMode.Async), Summary("Ready up for the game to start!") ]
            public async Task Ready()
            {
                GamePlayerList game = Program.Servers[Context.Guild];

                if (!game.gameRunning)
                {
                    if(game.inGame(Context.User as IGuildUser))
                    {
                        Player player = game.Find(Context.User as IGuildUser);
                        if (!player.Ready)
                        {
                            bool everyoneReady = player.readyUp(game);
                            await ReplyAsync("", false, new EmbedBuilder() { Title = "Player Ready!", Color = Color.Green, Description = $"{Context.User.Mention} is ready! :white_check_mark:", Footer = new EmbedFooterBuilder() { Text = $"Ready players: {game.Objects.Where(x => x.Ready).Count()}/{game.Objects.Count}" } });

                            if(everyoneReady && game.Objects.Count > 4)
                            {
                                await ReplyAsync("", false, new EmbedBuilder() { Title = "Game Start!", Color = Color.Green, Description = $"@everyone is ready! Starting up the game..."});
                                //game.gameRunning = true; //Should be moved to startGame()
                                await Task.Delay(TimeConverter.SecToMS(2));
                                GameManager.startGame(Context, game);
                            }
                            else if (everyoneReady)
                            {
                                await ReplyAsync("", false, new EmbedBuilder() { Title = "Too little players!", Color = Color.Orange, Description = $"@everyone is ready, but we don't have enough players. ", Footer = new EmbedFooterBuilder() {Text = $"[{game.Objects.Count}/5] required." } });
                            }
                        }
                        else
                        {
                            player.Ready = false;
                            await ReplyAsync("", false, new EmbedBuilder() { Title = "Player no longer ready!", Color = Color.Red, Description = $"{Context.User.Mention} is no longer ready! :x:", Footer = new EmbedFooterBuilder() { Text = $"Ready players: {game.Objects.Where(x => x.Ready).Count()}/{game.Objects.Count}" } });
                        }
                    }
                    else
                    {
                        await ReplyAsync("", false, new EmbedBuilder() { Title = "Not in game!", Color = Color.Red, Description = $"{Context.User.Mention} you're not in the game! Please join first by typing !join :no_entry_sign:" });
                    }
                }
            }

            [Command("startgame"), Summary("Admin only: Force game to start"), RequireUserPermission(GuildPermission.Administrator)]
            public async Task startGame()
            {
                GamePlayerList game = Program.Servers[Context.Guild];

                if (!game.gameRunning && game.Objects.Count > 4)
                {
                    await ReplyAsync("", false, new EmbedBuilder() { Title = "Game Forced Start!", Color = Color.DarkGreen, Description = $"The game has been started by a Moderator @everyone, Starting up the game..." });
                    //game.gameRunning = true; //Should be moved to startGame()
                    await Task.Delay(TimeConverter.SecToMS(2));
                    GameManager.startGame(Context, game);
                }
                else if (!game.gameRunning && game.Objects.Count <= 4)
                {
                    await ReplyAsync("", false, new EmbedBuilder() { Title = "Failed to start!", Color = Color.DarkOrange, Description = $"{Context.User.Mention} you can not force launch the game, it has less than 5 players. :no_entry_sign:", Footer = new EmbedFooterBuilder() { Text = $"[{game.Objects.Count}/5] required."}});
                }
            }
        }
        #endregion
    }
}