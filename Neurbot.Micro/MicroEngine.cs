using Neurbot.Brain;
using Neurbot.Generic;
using Neurbot.Micro.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Neurbot.Micro
{
    public sealed class MicroEngine : Engine<GameState>
    {
        private static readonly Random random = new Random();

        private readonly string historyFileName;
        private readonly Brain.Brain brain;

        public MicroEngine(string brainFileName, string historyFileName)
        {
            this.historyFileName = historyFileName;

            brain = Brain.Brain.LoadFromFile(brainFileName);
        }

        public override void Response(List<GameState> gameStates)
        {
            try
            {
                DoResponse(gameStates);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Response failure: " + ex.Message);
            }
        }

        private static UfoAction SelectUfoAction(int actionId)
        {
            switch (actionId)
            {
                case 0: return new UfoAction() { Move = new Move() { Direction = 0, Speed = 10 } };
                case 1: return new UfoAction() { Move = new Move() { Direction = 45, Speed = 10 } };
                case 2: return new UfoAction() { Move = new Move() { Direction = 90, Speed = 10 } };
                case 3: return new UfoAction() { Move = new Move() { Direction = 135, Speed = 10 } };
                case 4: return new UfoAction() { Move = new Move() { Direction = 180, Speed = 10 } };
                case 5: return new UfoAction() { Move = new Move() { Direction = 225, Speed = 10 } };
                case 6: return new UfoAction() { Move = new Move() { Direction = 270, Speed = 10 } };
                case 7: return new UfoAction() { Move = new Move() { Direction = 315, Speed = 10 } };

                case 8: return new UfoAction() { Shoot = new Shoot() { Direction = 0 } };
                case 9: return new UfoAction() { Shoot = new Shoot() { Direction = 45 } };
                case 10: return new UfoAction() { Shoot = new Shoot() { Direction = 90, } };
                case 11: return new UfoAction() { Shoot = new Shoot() { Direction = 135 } };
                case 12: return new UfoAction() { Shoot = new Shoot() { Direction = 180 } };
                case 13: return new UfoAction() { Shoot = new Shoot() { Direction = 225 } };
                case 14: return new UfoAction() { Shoot = new Shoot() { Direction = 270 } };
                case 15: return new UfoAction() { Shoot = new Shoot() { Direction = 315 } };
            }

            return new UfoAction();
        }

        private void DoResponse(List<GameState> gameStates)
        {
            try
            {
                var gameState = gameStates.Last();

                var me = Helpers.GetPlayerByName(gameState.Players, gameState.PlayerName);

                //time += 0.1;
                //
                //var ufos = mePlayer.Ufos;
                //var targetUfo = GetUfosWithHitPoints(GetOtherUfos(gameState, playerName)).FirstOrDefault();
                //
                //if (targetUfo == default(Protocol.Ufo))
                //    return;
                //
                //foreach (var ufo in ufos)
                //{
                //    WriteMessage(new GameResponse
                //    {
                //        Commands = new List<UfoAction>
                //        {
                //            new UfoAction
                //            {
                //                Id = ufo.Id,
                //                Move = new Move { Direction = Sin(time * 2) * 70, Speed = Sin(time * 0.2) * 4 },
                //                ShootAt = new ShootAt { X = targetUfo.Position.X, Y = targetUfo.Position.Y },
                //            }
                //        },
                //    });
                //}

                // TODO: Doesn't work correctly, probs are really small
                int actionId = brain.GetRandomAction(gameState.ToNeuralNetInput());

                actionId = random.Next(0, 15);

                brain.SaveHistory(historyFileName);

                var action = SelectUfoAction(actionId);
                action.Id = me.Ufos.First().Id;

                WriteMessage(new GameResponse
                {
                    Commands = new List<UfoAction> { action }
                });
            }
            catch (Exception ex)
            {
                Logger.Log("Exception '{0}': {1}", ex.GetType(), ex.Message);
            }
        }
    }
}