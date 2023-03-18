﻿using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.Pesto;
using BotNet.Services.Pesto.Exceptions;
using BotNet.Services.Pesto.Models;
using BotNet.Services.Piston;
using BotNet.Services.Piston.Models;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.Services.BotCommands {
	public static class Exec {
		public static async Task ExecAsync(ITelegramBotClient botClient, IServiceProvider serviceProvider, Message message, string language, CancellationToken cancellationToken) {
			if (message.Entities?.FirstOrDefault() is { Type: MessageEntityType.BotCommand, Offset: 0, Length: int commandLength }
				&& message.Text![commandLength..].Trim() is string commandArgument) {
				if (commandArgument.Length > 0) {
					// See if the language is supported on Pesto
					bool pestoSupportedLanguage =
						Enum.TryParse(CultureInfo.InvariantCulture.TextInfo.ToTitleCase(language),
							out Language pestoLanguage);

					if (pestoSupportedLanguage) {
						// Use Pesto
						try {
							CodeResponse result = await serviceProvider.GetRequiredService<PestoClient>()
							                                           .ExecuteAsync(pestoLanguage, commandArgument,
								                                           cancellationToken);

							if (result.Compile is { ExitCode: not 0 }) {
								await botClient.SendTextMessageAsync(
									chatId: message.Chat.Id,
									text: $"<code>{WebUtility.HtmlEncode(result.Compile.Stderr)}</code>",
									parseMode: ParseMode.Html,
									replyToMessageId: message.MessageId,
									cancellationToken: cancellationToken);
							} else if (result.Runtime is { ExitCode: not 0 }) {
								await botClient.SendTextMessageAsync(
									chatId: message.Chat.Id,
									text: $"<code>{WebUtility.HtmlEncode(result.Runtime.Stderr)}</code>",
									parseMode: ParseMode.Html,
									replyToMessageId: message.MessageId,
									cancellationToken: cancellationToken);
							} else if (result.Runtime.Output.Length > 1000) {
								await botClient.SendTextMessageAsync(
									chatId: message.Chat.Id,
									text: "<code>Output is too long.</code>",
									parseMode: ParseMode.Html,
									replyToMessageId: message.MessageId,
									cancellationToken: cancellationToken);
							} else {
								await botClient.SendTextMessageAsync(
									chatId: message.Chat.Id,
									text:
									$"Code:\n<code>{WebUtility.HtmlEncode(commandArgument)}</code>\n\nOutput:\n<code>{WebUtility.HtmlEncode(result.Runtime.Output)}</code>",
									parseMode: ParseMode.Html,
									replyToMessageId: message.MessageId,
									cancellationToken: cancellationToken);
							}
						} catch (Exception exception) when (exception is not PestoAPIException
							                                    or PestoMonthlyLimitExceededException
							                                    or PestoRuntimeNotFoundException
							                                    or PestoServerRateLimitedException) {
							// Rethrow exception, just because
							throw;
						} catch {
							// Suppress error, and retry code execution using Piston.
						}
					}

					// Use Piston
					try {
						ExecuteResult result = await serviceProvider.GetRequiredService<PistonClient>()
						                                            .ExecuteAsync(language.ToLowerInvariant(),
							                                            commandArgument, cancellationToken);

						if (result.Compile is { Code: not 0 }) {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: $"<code>{WebUtility.HtmlEncode(result.Compile.Stderr)}</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						} else if (result.Run.Code != 0) {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: $"<code>{WebUtility.HtmlEncode(result.Run.Stderr)}</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						} else if (result.Run.Output.Length > 1000) {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: "<code>Output is too long.</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						} else {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: $"Code:\n<code>{WebUtility.HtmlEncode(commandArgument)}</code>\n\nOutput:\n<code>{WebUtility.HtmlEncode(result.Run.Output)}</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						}
#pragma warning disable CS0618 // Type or member is obsolete
					} catch (ExecutionEngineException exc) {
#pragma warning restore CS0618 // Type or member is obsolete
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: "<code>" + WebUtility.HtmlEncode(exc.Message ?? "Unknown error") + "</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					} catch (OperationCanceledException) {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: "<code>Timeout exceeded.</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					}
				} else if (message.ReplyToMessage?.Text is string repliedToMessage) {
					// See if the language is supported on Pesto
					bool pestoSupportedLanguage =
						Enum.TryParse(CultureInfo.InvariantCulture.TextInfo.ToTitleCase(language),
							out Language pestoLanguage);

					if (pestoSupportedLanguage) {
						// Use Pesto
						try {
							CodeResponse result = await serviceProvider.GetRequiredService<PestoClient>()
							                                           .ExecuteAsync(pestoLanguage, commandArgument,
								                                           cancellationToken);

							if (result.Compile is { ExitCode: not 0 }) {
								await botClient.SendTextMessageAsync(
									chatId: message.Chat.Id,
									text: $"<code>{WebUtility.HtmlEncode(result.Compile.Stderr)}</code>",
									parseMode: ParseMode.Html,
									replyToMessageId: message.MessageId,
									cancellationToken: cancellationToken);
							} else if (result.Runtime is { ExitCode: not 0 }) {
								await botClient.SendTextMessageAsync(
									chatId: message.Chat.Id,
									text: $"<code>{WebUtility.HtmlEncode(result.Runtime.Stderr)}</code>",
									parseMode: ParseMode.Html,
									replyToMessageId: message.MessageId,
									cancellationToken: cancellationToken);
							} else if (result.Runtime.Output.Length > 1000) {
								await botClient.SendTextMessageAsync(
									chatId: message.Chat.Id,
									text: "<code>Output is too long.</code>",
									parseMode: ParseMode.Html,
									replyToMessageId: message.MessageId,
									cancellationToken: cancellationToken);
							} else {
								await botClient.SendTextMessageAsync(
									chatId: message.Chat.Id,
									text:
									$"Code:\n<code>{WebUtility.HtmlEncode(commandArgument)}</code>\n\nOutput:\n<code>{WebUtility.HtmlEncode(result.Runtime.Output)}</code>",
									parseMode: ParseMode.Html,
									replyToMessageId: message.MessageId,
									cancellationToken: cancellationToken);
							}
						} catch (Exception exception) when (exception is not PestoAPIException
							                                    and not PestoMonthlyLimitExceededException
							                                    and not PestoRuntimeNotFoundException
							                                    and not PestoServerRateLimitedException) {
							// Rethrow exception, just because
							throw;
						} catch (Exception) {
							// Suppress error, and retry code execution using Piston.
						}
					}
					
					try {
						ExecuteResult result = await serviceProvider.GetRequiredService<PistonClient>()
						                                            .ExecuteAsync(language.ToLowerInvariant(),
							                                            repliedToMessage, cancellationToken);

						if (result.Compile is { Code: not 0 }) {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: $"<code>{WebUtility.HtmlEncode(result.Compile.Stderr)}</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						} else if (result.Run.Code != 0) {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: $"<code>{WebUtility.HtmlEncode(result.Run.Stderr)}</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						} else if (result.Run.Output.Length > 1000) {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: "<code>Output is too long.</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						} else {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: $"Code:\n<code>{WebUtility.HtmlEncode(repliedToMessage)}</code>\n\nOutput:\n<code>{WebUtility.HtmlEncode(result.Run.Output)}</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.ReplyToMessage.MessageId,
								cancellationToken: cancellationToken);
						}
#pragma warning disable CS0618 // Type or member is obsolete
					} catch (ExecutionEngineException exc) {
#pragma warning restore CS0618 // Type or member is obsolete
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: "<code>" + WebUtility.HtmlEncode(exc.Message ?? "Unknown error") + "</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: message.ReplyToMessage.MessageId,
							cancellationToken: cancellationToken);
					} catch (OperationCanceledException) {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: "<code>Timeout exceeded.</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					}
				} else {
					await botClient.SendTextMessageAsync(
						chatId: message.Chat.Id,
						text: $"Untuk mengeksekusi program, silakan ketik {message.Text![..commandLength].Trim()} diikuti code.",
						parseMode: ParseMode.Html,
						replyToMessageId: message.MessageId,
						cancellationToken: cancellationToken);
				}
			}
		}
	}
}
