// Global using directives

global using System.Diagnostics;
global using System.Text.Json;
global using System.Web;
global using MassTransit;
global using MassTransit.Mediator;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.ChangeTracking;
global using Microsoft.Extensions.Caching.Memory;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using SpinRallyBot.BackNavigations;
global using SpinRallyBot.Commands;
global using SpinRallyBot.Events;
global using SpinRallyBot.Events.CommandReceivedConsumers.Base;
global using SpinRallyBot.Models;
global using SpinRallyBot.PipelineStateMachine;
global using SpinRallyBot.Queries;
global using SpinRallyBot.Subscriptions;
global using SpinRallyBot.Utils;
global using Telegram.Bot;
global using Telegram.Bot.Polling;
global using Telegram.Bot.Types;
global using Telegram.Bot.Types.Enums;
global using Telegram.Bot.Types.ReplyMarkups;