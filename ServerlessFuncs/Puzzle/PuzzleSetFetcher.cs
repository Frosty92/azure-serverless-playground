using System;
using Azure;
using System.Collections.Generic;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using ServerlessFuncs.PuzzleNS;
using System.Threading.Tasks;
using ServerlessFuncs.Puzzles;
using System.Linq;
using System.Diagnostics;

namespace ServerlessFuncs.PuzzleNS
{
    public class PuzzleSetFetcher
    {

        public static readonly int PUZZLES_PER_PAGE = 15;
        public static readonly int START_INDEX = -1;
        private static readonly int MAX_LEVEL = 6;

        public static readonly int PUZZLE_COUNT_LVL_1 = 300;
        public static readonly int PUZZLE_COUNT_LVL_2 = 300;
        public static readonly int PUZZLE_COUNT_LVL_3 = 350;
        public static readonly int PUZZLE_COUNT_LVL_4 = 400;
        public static readonly int PUZZLE_COUNT_LVL_5 = 500;
        public static readonly int PUZZLE_COUNT_LVL_6 = 600;

        private TableClient PuzzlesTable { get;}
        
       
        public PuzzleSetFetcher(TableClient PuzzlesTable)
        {
            this.PuzzlesTable = PuzzlesTable;
        }

        public async Task<PuzzleSet> FetchPuzzleSet(
            int levelNum,
            int subLevel,
            int lastCompletedIndex
        )
        {

            PuzzleSet puzzleSet = lastCompletedIndex == PUZZLES_PER_PAGE - 1
                ? await GetPuzzleSet(levelNum, subLevel + 1, START_INDEX)
                : await GetPuzzleSet(levelNum, subLevel, lastCompletedIndex);

            if (puzzleSet.Puzzles.Count > 0 || levelNum + 1 > MAX_LEVEL) return puzzleSet;
            else return await GetPuzzleSet(levelNum + 1, 1, START_INDEX);
        }


        private async Task<PuzzleSet> GetPuzzleSet(int levelNum, int subLevel ,int lastCompletedIndex)
        {
            var puzzleSet = new PuzzleSet();
            puzzleSet.LastCompletedPuzzleIndex = lastCompletedIndex;

            string partitionKey = $"{levelNum}_{subLevel}";
            await foreach (Page<PuzzleEntity> page in PuzzlesTable.QueryAsync<PuzzleEntity>(
                    e => e.PartitionKey == partitionKey
                    ).AsPages(null, PUZZLES_PER_PAGE))
            {
                List<PuzzleEntity> puzzlesPage = page.Values.ToList();

                puzzleSet.SubLevel = subLevel;
                puzzleSet.LevelNum = levelNum;
                puzzleSet.LevelPuzzleCount = GetPuzzleCountForLevel(levelNum);

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

        private int GetPuzzleCountForLevel(int levelNum)
        {
            var dict = new Dictionary<int, int>();
            dict[1] = PUZZLE_COUNT_LVL_1;
            dict[2] = PUZZLE_COUNT_LVL_2;
            dict[3] = PUZZLE_COUNT_LVL_3;
            dict[4] = PUZZLE_COUNT_LVL_4;
            dict[5] = PUZZLE_COUNT_LVL_5;
            dict[6] = PUZZLE_COUNT_LVL_6;

            if (dict.ContainsKey(levelNum) == false)
            {
                throw new ArgumentOutOfRangeException($"LevelNum: {levelNum} has no PuzzleCount stat");
            }

            return dict[levelNum];
        }
    }
}

