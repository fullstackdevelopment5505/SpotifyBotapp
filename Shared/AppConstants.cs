using System;
using System.Collections.Generic;
using System.Text;

namespace SpotifyBot.Shared
{
    class AppConstants
    {

        public static class Messages
        {
            public const string Not_Permitted = "The operation is not permitted";
            public const string Validation_failed = "Validatation has failed.";
            public const string Internal_error = "Internal error has occurred.";
            public const string No_content_found = "Content not found.";

        }

        public static class Configurations
        {
            public const int DefaultAccessTokenExpirationInDays = 365;

            public const int MinimumTrackPlayingDurationInSeconds = 33;

            public const int DefaultPageValue = 1;

            public const int MasterAccountId = 0;
        }

        /// <summary>
        /// Lists all the user roles in the system
        /// </summary>
        public static class SystemUserRoleNames
        {
            public const string Admin = "Admin";
            public const string FileManager = "Client";
            public const string ManageFolders = "Operator";
        }
    }
}
