using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeltaBitPacker.Tests
{
    struct ServerPacket
    {
        public uint Sequence;
        public float Time;
        public byte PlayerId;
        public GameState GameState;
    }

    struct GameState
    {
        public byte CurrentMap;
        public byte ScoreToWin;
        public byte PreferredNumberOfPlayers;
        public byte MatchState;
        public Player[] Players;
        public Powerup[] Powerups;
        public ConquestFlag[] ConquestFlags;
    }

    struct ConquestFlag
    {
        public byte Team;
    }

    struct Powerup
    {
        public bool Visible;
    }

    struct Player
    {
        public byte PlayerId;
        public float X;
        public float Y;
        public string Name;
        public bool Throttle;
        public bool Fire;
        public bool EngineMalfunction;
        public bool Dead;
        public byte Team;
        public byte Kills;
        public byte Deaths;
        public byte CurrentWeapon;
    }

    struct ClientPacket
    {
        public UInt32 AckSequence;
        public ClientState ClientState;
        public Command[] Commands;
    }

    struct Command
    {
        public byte Type;
    }

    struct ClientState
    {
        public float X;
        public float Y;
        public bool Fire;
        public bool Throttle;
    }
}
