﻿using NewsTrack.Identity.Results;

namespace NewsTrack.WebApi.Dtos
{
    public class CreateIdentityResponseDto : ResponseBaseDto
    {
        public enum FailureReason
        {
            InvalidEmailPattern = 1,
            PasswordsDontMatch = 2,
            InvalidUsername = 3,
            InvalidEmail = 4
        }

        public CreateIdentityResponseDto() { }

        private CreateIdentityResponseDto(SaveIdentityResult.ResultType result)
        {
            IsSuccessful = result == SaveIdentityResult.ResultType.Ok;
            if (!IsSuccessful)
            {
                Failure = (FailureReason)result;
            }
        }

        public FailureReason Failure { get; protected set; }


        public static CreateIdentityResponseDto Create(SaveIdentityResult.ResultType result)
        {
            return new CreateIdentityResponseDto(result);
        }
    }
}