﻿using System;
using System.Threading.Tasks;
using DotNetCore.CAP.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DotNetCore.CAP.EntityFrameworkCore
{
    /// <summary>
    /// Represents a new instance of a persistence store for the specified message types.
    /// </summary>
    /// <typeparam name="TContext">The type of the data context class used to access the store.</typeparam>
    public class CapMessageStore<TContext> : ICapMessageStore where TContext : DbContext
    {
        /// <summary>
        /// Constructs a new instance of <see cref="TContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="DbContext"/>.</param>
        public CapMessageStore(TContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public TContext Context { get; private set; }

        private DbSet<CapSentMessage> SentMessages => Context.Set<CapSentMessage>();

        private DbSet<CapReceivedMessage> ReceivedMessages => Context.Set<CapReceivedMessage>();

        /// <summary>
        /// Creates the specified <paramref name="message"/> in the cap message store.
        /// </summary>
        /// <param name="message">The message to create.</param>
        public async Task<OperateResult> StoreSentMessageAsync(CapSentMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            Context.Add(message);
            await Context.SaveChangesAsync();
            return OperateResult.Success;
        }

        public async Task<OperateResult> ChangeSentMessageStateAsync(CapSentMessage message, string status,
            bool autoSaveChanges = true)
        {
            Context.Attach(message);
            message.LastRun = DateTime.Now;
            message.StatusName = status;
            try
            {
                if (autoSaveChanges)
                {
                    await Context.SaveChangesAsync();
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return OperateResult.Failed(
                    new OperateError()
                    {
                        Code = "DbUpdateConcurrencyException",
                        Description = ex.Message
                    });
            }
            return OperateResult.Success;
        }

        /// <summary>
        /// First Enqueued Message.
        /// </summary>
        public async Task<CapSentMessage> GetNextSentMessageToBeEnqueuedAsync()
        {
            return await SentMessages.FirstOrDefaultAsync(x => x.StatusName == StatusName.Enqueued);
        }

        /// <summary>
        /// Updates a message in a store as an asynchronous operation.
        /// </summary>
        /// <param name="message">The message to update in the store.</param>
        public async Task<OperateResult> UpdateSentMessageAsync(CapSentMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            Context.Attach(message);
            message.LastRun = DateTime.Now;
            Context.Update(message);

            try
            {
                await Context.SaveChangesAsync();
                return OperateResult.Success;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return OperateResult.Failed(new OperateError()
                {
                    Code = "DbUpdateConcurrencyException",
                    Description = ex.Message
                });
            }
        }

        /// <summary>
        ///  Deletes the specified <paramref name="message"/> from the consistency message store.
        /// </summary>
        /// <param name="message">The message to delete.</param>
        public async Task<OperateResult> RemoveSentMessageAsync(CapSentMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            Context.Remove(message);
            try
            {
                await Context.SaveChangesAsync();
                return OperateResult.Success;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return OperateResult.Failed(new OperateError()
                {
                    Code = "DbUpdateConcurrencyException",
                    Description = ex.Message
                });
            }
        }

        /// <summary>
        /// Creates the specified <paramref name="message"/> in the consistency message store.
        /// </summary>
        /// <param name="message">The message to create.</param>
        public async Task<OperateResult> StoreReceivedMessageAsync(CapReceivedMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            Context.Add(message);
            await Context.SaveChangesAsync();
            return OperateResult.Success;
        }

        public async Task<OperateResult> ChangeReceivedMessageStateAsync(CapReceivedMessage message, string status,
            bool autoSaveChanges = true)
        {
            Context.Attach(message);
            message.LastRun = DateTime.Now;
            message.StatusName = status;
            try
            {
                if (autoSaveChanges)
                {
                    await Context.SaveChangesAsync();
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return OperateResult.Failed(new OperateError()
                {
                    Code = "DbUpdateConcurrencyException",
                    Description = ex.Message
                });
            }
            return OperateResult.Success;
        }

        public async Task<CapReceivedMessage> GetNextReceivedMessageToBeExcuted()
        {
            return await ReceivedMessages.FirstOrDefaultAsync(x => x.StatusName == StatusName.Enqueued);
        }

        /// <summary>
        /// Updates the specified <paramref name="message"/> in the message store.
        /// </summary>
        /// <param name="message">The message to update.</param>
        public async Task<OperateResult> UpdateReceivedMessageAsync(CapReceivedMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            Context.Attach(message);
            message.LastRun = DateTime.Now;
            Context.Update(message);

            try
            {
                await Context.SaveChangesAsync();
                return OperateResult.Success;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return OperateResult.Failed(new OperateError()
                {
                    Code = "DbUpdateConcurrencyException",
                    Description = ex.Message
                });
            }
        }
    }
}