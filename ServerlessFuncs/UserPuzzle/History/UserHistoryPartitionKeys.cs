using System;
namespace ServerlessFuncs.UserPuzzle.History
{
    public static class UserHistoryPartitionKeys
    {
        public static string GetForWrong(string userID)
        {
            return $"{userID}_WRONG";
        }

        public static string GetForMarked(string userID)
        {
            return $"{userID}_MARKED";
        }
    }
}

