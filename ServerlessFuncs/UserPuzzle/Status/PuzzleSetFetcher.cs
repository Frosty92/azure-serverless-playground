using System;
using Azure;
using System.Collections.Generic;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using ServerlessFuncs.PuzzleNS;
using System.Threading.Tasks;
using ServerlessFuncs.Puzzles;
using System.Linq;

namespace ServerlessFuncs.UserPuzzle.Status
{
    public class PuzzleSetFetcher
    {

        public static readonly int PUZZLES_PER_PAGE = 10;
        public static readonly int START_INDEX = -1;
        private static readonly int MAX_LEVEL = 6;

        private TableClient PuzzlesTable { get;}
        
       
        public PuzzleSetFetcher(TableClient PuzzlesTable)
        {
            this.PuzzlesTable = PuzzlesTable;
        }

        public async Task<PuzzleSet> FetchPuzzleSet(
            int levelNum,
            int lastCompletedIndex,
            string currentPageToken,
            string NextPageToken
        )
        {

            PuzzleSet puzzleSet = lastCompletedIndex == PUZZLES_PER_PAGE - 1
                ? await GetPuzzleSet(levelNum, START_INDEX, NextPageToken)
                : await GetPuzzleSet(levelNum, lastCompletedIndex, currentPageToken);

            if (puzzleSet.Puzzles.Count > 0 || levelNum + 1 > MAX_LEVEL) return puzzleSet;
            else return await GetPuzzleSet(levelNum + 1, START_INDEX, null);
        }


        private async Task<PuzzleSet> GetPuzzleSet(int levelNum,int lastCompletedIndex,string paginationToken)
        {
            var puzzleSet = new PuzzleSet();
            puzzleSet.LastCompletedPuzzleIndex = lastCompletedIndex;

            string filter = $"partitionKey eq '{levelNum}'";
            await foreach (Page<PuzzleEntity> page in PuzzlesTable.QueryAsync<PuzzleEntity>(
                    e => e.PartitionKey == levelNum.ToString()
                    ).AsPages(paginationToken, PUZZLES_PER_PAGE))
            {
                List<PuzzleEntity> puzzlesPage = page.Values.ToList();

                puzzleSet.CurrentPageToken = paginationToken;
                puzzleSet.NextPageToken = page.ContinuationToken;

                puzzleSet.LevelNum = levelNum;

                int index = lastCompletedIndex + 1;
                for (int i = index; i < puzzlesPage.Count; i++)
                {
                    var puzzle = puzzlesPage[i];
                    puzzleSet.Puzzles.Add(puzzle.ToPuzzle());
                }
                break;
            }
            return puzzleSet;
        }
    }
}

