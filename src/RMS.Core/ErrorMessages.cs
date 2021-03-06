﻿namespace RMS.Core
{
    public static class ErrorMessages
    {
        public const string RequestNotFound = "REQUEST_NOT_FOUND";
        public const string RequestStatusNotFound = "REQUEST_STATUS_NOT_FOUND";
        public const string NameShouldNotBeEmpty = "NAME_SHOULD_NOT_BE_EMPTY";

        public const string InvalidId = "INVALID_ID";
        public const string GenericUniqueError = @"{0} should be unique";
        public const string GenericCombinationUniqueError = @"Combination of {0} should be unique";
    }
}
