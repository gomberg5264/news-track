﻿using System;
using System.Dynamic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NewsTrack.Common.Events;
using NewsTrack.Common.Validations;
using NewsTrack.Identity.Encryption;
using NewsTrack.Identity.Repositories;
using NewsTrack.Identity.Results;
using static NewsTrack.Identity.Results.SaveIdentityResult.ResultType;
using static NewsTrack.Common.Events.NotificationEventArgs.NotificationType;

namespace NewsTrack.Identity.Services
{
    public class IdentityService : IIdentityService
    {
        private const string EmailPattern =
            @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
            @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$";

        private readonly IIdentityRepository _identityRepository;
        private readonly ICryptoManager _cryptoManager;

        private event EventHandler<NotificationEventArgs> SendNotificationEvent;

        public IdentityService(IIdentityRepository identityRepository, ICryptoManager cryptoManager, EventHandler<NotificationEventArgs> handler)
        {
            _identityRepository = identityRepository;
            _cryptoManager = cryptoManager;
            SendNotificationEvent = handler;
        }

        public async Task<SaveIdentityResult> Save(string username, string email, IdentityTypes type)
        {
            string password = Guid.NewGuid().ToString("N").Substring(0, 8);
            var result = await Create(username, email, password, password, type);
            if (result.Type == Ok)
            {
                var args = To(AccountCreated, result.Identity);
                args.Model.Password = password;
                OnSendNotification(args);
            }
            return result;
        }

        public async Task<SaveIdentityResult> Save(string username, string email, string password1, string password2, IdentityTypes type)
        {
            var result = await Create(username, email, password1, password2, type);
            if (result.Type == Ok)
            {
                OnSendNotification(AccountCreated, result.Identity);
            }
            return result;
        }

        public async Task<AuthenticateResult> Authenticate(string email, string password)
        {
            email.CheckIfNull(nameof(email));
            password.CheckIfNull(nameof(password));

            var identity = await _identityRepository.GetByEmail(email);

            if (identity == null || !identity.IsEnabled)
            {
                return AuthenticateResult.Failed;
            }
            if (identity.LockoutEnd > DateTime.UtcNow)
            {
                return AuthenticateResult.Lockout;
            }
            if (!_cryptoManager.CheckPassword(password, identity.Password))
            {
                var status = AuthenticateResult.Failed;
                identity.AccessFailedCount++;                
                if (identity.AccessFailedCount > 5)
                {                    
                    identity.LockoutEnd = DateTime.UtcNow.AddMinutes(5);
                    status = AuthenticateResult.Lockout;
                    OnSendNotification(AccountLockout, identity);
                }

                await _identityRepository.Update(identity);
                return status;
            }

            if (identity.AccessFailedCount > 0)
            {
                identity.AccessFailedCount = 0;
                identity.LockoutEnd = null;
                await _identityRepository.Update(identity);
            }
            
            return AuthenticateResult.Ok;
        }

        public async Task<bool> Confirm(string email, string securityStamp)
        {
            var identity = await _identityRepository.GetByEmail(email);
            if (identity?.IsEnabled == false && identity.SecurityStamp == securityStamp)
            {
                OnSendNotification(AccountConfirmed, identity);
                identity.IsEnabled = true;
                await _identityRepository.Update(identity);
                return true;
            }

            return false;
        }

        public async Task<ChangePasswordResult> ChangePassword(
            Guid id, 
            string currentPassword, 
            string password1,
            string password2)
        {
            var identity = await _identityRepository.Get(id);
            if (!_cryptoManager.CheckPassword(currentPassword, identity.Password))
            {
                return ChangePasswordResult.InvalidCurrentPassword;
            }
            if (password1 != password2)
            {
                return ChangePasswordResult.PasswordsDontMatch;
            }

            identity.Password = _cryptoManager.HashPassword(password1);
            await _identityRepository.Update(identity);
            return ChangePasswordResult.Ok;
        }

        private async Task<SaveIdentityResult> Create(
            string username, 
            string email, 
            string password1, 
            string password2, 
            IdentityTypes type)
        {
            username.CheckIfNull(nameof(username));
            email.CheckIfNull(nameof(email));
            password1.CheckIfNull(nameof(password1));
            password2.CheckIfNull(nameof(password2));

            if (!Regex.IsMatch(email, EmailPattern, RegexOptions.IgnoreCase))
            {
                return SaveIdentityResult.Create(InvalidEmail);
            }
            if (password1 != password2)
            {
                return SaveIdentityResult.Create(PasswordsDontMatch);
            }
            if (await _identityRepository.ExistsByUsername(username))
            {
                return SaveIdentityResult.Create(InvalidUsername);
            }
            if (await _identityRepository.ExistsByEmail(email))
            {
                return SaveIdentityResult.Create(InvalidEmail);
            }

            var identity = new Identity
            {
                Username = username,
                Email = email,
                IdType = type,
                Password = _cryptoManager.HashPassword(password1),
                SecurityStamp = Guid.NewGuid().ToString()
            };

            await _identityRepository.Save(identity);
            return SaveIdentityResult.Create(identity, Ok);
        }

        private void OnSendNotification(NotificationEventArgs.NotificationType type, Identity identity)
        {
            var args = To(type, identity);
            OnSendNotification(args);
        }

        private void OnSendNotification(NotificationEventArgs args)
        {
            if (SendNotificationEvent != null && args != null)
            {
                SendNotificationEvent.Invoke(this, args);
            }
        }

        private NotificationEventArgs To(NotificationEventArgs.NotificationType type, Identity identity)
        {
            var args =  new NotificationEventArgs
            {
                Type = type,
                To = identity.Email,
                Username = identity.Username
            };

            if (type == AccountCreated)
            {
                args.Model = new ExpandoObject();
                args.Model.Email = identity.Email;
                args.Model.SecurityStamp = identity.SecurityStamp;
            }

            return args;
        }
    }
}